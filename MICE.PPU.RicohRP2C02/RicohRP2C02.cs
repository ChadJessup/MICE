﻿using MICE.Common.Helpers;
using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;
using System.Collections.Generic;
using System.Threading;

namespace MICE.PPU.RicohRP2C02
{
    // Some notes: http://problemkaputt.de/everynes.htm#pictureprocessingunitppu

    public class RicohRP2C02 : IMicroprocessor
    {
        public static class Constants
        {
            public const int ScanlinesPerFrame = 262;
            public const int CyclesPerScanline = 341;
            public const int NTSCWidth = 256;
            public const int NTSCHeight = 224;
        }

        private byte registerLatch;

        public RicohRP2C02(PPUMemoryMap memoryMap, PPURegisters registers, IMemoryMap cpuMemoryMap, IList<byte[]> chrBanks)
        {
            this.Registers = registers;
            this.MemoryMap = memoryMap;
            this.ScrollHandler = new ScrollHandler(this.Registers, this.InternalRegisters);

            this.BackgroundHandler = new BackgroundHandler(this.MemoryMap, this.Registers, this.InternalRegisters, this.ScrollHandler, cpuMemoryMap, chrBanks);
            this.SpriteHandler = new SpriteHandler(this.MemoryMap, this.Registers, cpuMemoryMap, chrBanks);
            this.PixelMuxer = new PixelMuxer(this.Registers);
        }

        public byte[] ScreenData { get; private set; } = new byte[256 * 240];

        /// <summary>
        /// Gets the Primary Object Attribute Memory (OAM) memory.
        /// This holds up to 64 sprites for the frame.
        /// </summary>
        public OAM PrimaryOAM { get; } = new OAM(256);

        /// <summary>
        /// Gets the Secondary Object Attribute Memory (OAM) memory.
        /// This holds up to 8 sprites for the current scanline.
        /// </summary>
        public OAM SecondaryOAM { get; } = new OAM(32);

        public BackgroundHandler BackgroundHandler { get; private set; }
        public SpriteHandler SpriteHandler { get; private set; }
        public ScrollHandler ScrollHandler { get; private set; }
        public PixelMuxer PixelMuxer { get; private set; }

        public bool ShouldNMInterrupt { get; set; }

        public PPUMemoryMap MemoryMap { get; private set; }

        /// <summary>
        /// Gets the externally accessible PPU Registers. These are manipulated via the CPU.
        /// </summary>
        public PPURegisters Registers { get; private set; }

        /// <summary>
        /// Gets the internal-only PPU Registers. These are manipulated only via the PPU.
        /// </summary>
        public PPUInternalRegisters InternalRegisters { get; } = new PPUInternalRegisters();

        /// <summary>
        /// Gets or sets the VRAM Address Increment.
        /// False = add 1, going across.
        /// True = add 32, going down.
        /// </summary>
        public VRAMAddressIncrements VRAMAddressIncrement
        {
            get => this.Registers.PPUCTRL.GetBit(2) ? VRAMAddressIncrements.Down32 : VRAMAddressIncrements.Across1;
            set => this.Registers.PPUCTRL.SetBit(2, value == VRAMAddressIncrements.Down32 ? true : false);
        }

        /// <summary>
        /// Gets or sets a value when the PPU is set to master/external mode. Not really used.
        /// </summary>
        public bool IsPPUMaster
        {
            get => this.Registers.PPUCTRL.GetBit(6);
            set => this.Registers.PPUCTRL.SetBit(6, value);
        }

        /// <summary>
        /// Gets or sets a value when a Nonmaskable Interrupt was requested by the CPU.
        /// </summary>
        public bool WasNMIRequested
        {
            get => this.Registers.PPUCTRL.GetBit(7);
            set => this.Registers.PPUCTRL.SetBit(7, value);
        }

        /// <summary>
        /// Gets or sets a value when in a VBlank period.
        /// </summary>
        public bool IsVBlank
        {
            get => this.Registers.PPUSTATUS.GetBit(7);
            set => this.Registers.PPUSTATUS.SetBit(7, value);
        }

        /// <summary>
        /// Gets the current scan line that the PPU is working on.
        /// </summary>
        public int ScanLine { get; private set; }

        /// <summary>
        /// Gets the current cycle (or tick) of the PPU on the current scanline.
        /// There should be 3 PPU cycles per CPU cycle.
        /// Each PPU cycle is a single pixel.
        /// The cycle resets to zero after a scanline is complete.
        /// </summary>
        public int Cycle { get; private set; }

        /// <summary>
        /// Number of the current frame being rendered.
        /// </summary>
        public long FrameNumber { get; private set; }

        /// <summary>
        /// Gets a value indicating if the Frame is even or odd.
        /// </summary>
        public bool IsFrameEven => this.FrameNumber % 2 == 0;

        public bool IsRenderingEnabled => this.BackgroundHandler.ShowBackground;

        public void PowerOn(CancellationToken cancellationToken)
        {
            this.Registers.PPUADDR.AfterWriteAction = (address, value) =>
            {
                this.registerLatch = value;
                this.UpdatePPUAddress(value);
            };

            this.Registers.PPUADDR.ReadByteInsteadAction = (address, value) => this.registerLatch;

            this.Registers.PPUSCROLL.AfterWriteAction = this.ScrollHandler.PPUScrollWrittenTo;

            this.Registers.PPUDATA.AfterReadAction = (address, value) => this.InternalRegisters.v += (ushort)this.VRAMAddressIncrement;
            this.Registers.PPUCTRL.AfterWriteAction = (address, value) =>
            {
                this.ShouldNMInterrupt = false;
                this.InternalRegisters.t = (ushort)((this.InternalRegisters.t & 0xF3FF) | ((value & 0x03) << 10));
            };

            this.Registers.PPUDATA.AfterWriteAction = (address, value) =>
            {
                this.registerLatch = value;

                this.MemoryMap.Write(this.InternalRegisters.v, value);
                this.InternalRegisters.v += (ushort)this.VRAMAddressIncrement;
            };

            this.Registers.PPUSTATUS.AfterReadAction = (_, value) =>
            {
                this.InternalRegisters.w = true;
                this.IsVBlank = false;
            };

            this.Registers.PPUDATA.ReadShortInsteadAction = (_, value) => this.InternalRegisters.v;

            byte bufferedRead = 0;
            this.Registers.PPUDATA.ReadByteInsteadAction = (address, value) =>
            {
                if (this.InternalRegisters.v >= 0x3F00 && this.InternalRegisters.v <= 0x3FFF)
                {
                    bufferedRead = this.MemoryMap.ReadByte(this.InternalRegisters.v);
                    return bufferedRead;
                }

                var temp = bufferedRead;
                bufferedRead = this.MemoryMap.ReadByte(this.InternalRegisters.v);

                return temp;
            };

            this.Restart(cancellationToken);
        }

        public void Restart(CancellationToken cancellationToken)
        {
            this.Registers.PPUCTRL.Write(0);
            this.Registers.PPUMASK.Write(0);
            this.Registers.PPUSTATUS.Write(0);

            this.Registers.OAMADDR.Write(0);
            this.Registers.PPUSCROLL.Write(0);
            // this.Registers.PPUDATA.Write(0);

            this.InternalRegisters.w = true;
            this.InternalRegisters.v = 0;

            this.FrameNumber = 0;
            this.ScanLine = 0;
            this.Cycle = 0;
        }

        public int Step()
        {
            // Prerender scanline
            if (this.ScanLine == -1)
            {
                this.HandlePrerenderScanline();
            }
            else if (this.ScanLine >= 0 && this.ScanLine <= 239)
            {
                this.HandleVisibleScanlines();
            }
            else if (this.ScanLine == 240)
            {
                this.HandlePostRenderScanline();
            }
            else if (this.ScanLine >= 241 && this.ScanLine <= 260)
            {
                this.HandleVerticalBlankLines();
            }

            if (this.ScanLine == 261)
            {
                this.HandleFinalScanline();
            }

            if (this.Cycle > 0 && this.Cycle % 8 == 0)
            {
                this.ScrollHandler.IncrementCoarseX();
            }

            if (this.Cycle == 256)
            {
                if (this.IsRenderingEnabled)
                {
                    this.ScrollHandler.IncrementCoarseY();
                }
            }
            else if (this.Cycle == 257)
            {
                // if rendering is enabled copy all bits related to horizontal position from t to v.
                if(this.IsRenderingEnabled)
                {
                    this.ScrollHandler.CopyHorizontalBits();
                }
            }

            this.Cycle++;

            if (this.OnFinalCycleOnLine())
            {
                this.ScanLine++;
                this.Cycle = 0;
            }

            return 0;
        }

        private bool OnFinalCycleOnLine() =>
            this.BackgroundHandler.ShowBackground && !this.IsFrameEven && this.ScanLine == -1
                ? this.Cycle == 340
                : this.Cycle == 341;

        private void HandleFinalScanline()
        {
            this.ScanLine = -1;
            this.FrameNumber++;
        }

        private void HandlePrerenderScanline()
        {
            if (this.Cycle == 0)
            {
                // Idle cycle
            }
            else if (this.Cycle >= 1 && this.Cycle <= 256)
            {
                if (this.Cycle == 1)
                {
                    this.IsVBlank = false;
                    this.SpriteHandler.WasSprite0Hit = false;
                    this.SpriteHandler.WasSpriteOverflow = false;
                }

                // Current scanline data fetch cycles
            }
            else if (this.Cycle >= 257 && this.Cycle <= 320)
            {
                this.Registers.OAMADDR.Write(00);
                // Tile Data fetch for next scanline
            }
            else if (this.Cycle >= 321 && this.Cycle <= 336)
            {
                // Tile fetch for next scanline
            }
            else if (this.Cycle >= 337 && this.Cycle <= 340)
            {
                // For unknown reason, 2-bytes fetched which are fetched again later.
            }

            if (this.Cycle >= 280 && this.Cycle <= 304)
            {
                // If rendering is enabled, at the end of vblank,
                // shortly after the horizontal bits are copied from t to v at dot 257,
                // the PPU will repeatedly copy the vertical bits from t to v from dots
                // 280 to 304, completing the full initialization of v from t
                this.ScrollHandler.CopyVerticalBits();
            }
        }

        private void HandleVisibleScanlines()
        {
            if (this.Cycle > 0 && this.Cycle <= 256)
            {
                if (this.IsRenderingEnabled && SpriteHandler.ShowSprites)
                {
                    var pixelX = this.Cycle - 1;
                    
                    (byte backgroundPixel, Tile tile) drawnTile = BackgroundHandler.GetBackgroundPixel(pixelX, this.ScanLine);
                    (byte spritePixel, Sprite sprite) drawnSprite = SpriteHandler.GetSpritePixel(pixelX, this.ScanLine, this.PrimaryOAM);

                    byte muxedPixel = PixelMuxer.MuxPixel(drawnSprite, drawnTile);
                    this.ScreenData[(this.ScanLine * Constants.NTSCWidth) + pixelX] = muxedPixel;

                    this.HandleSprite0Hit(drawnTile, drawnSprite);
                }
            }
            else if (this.Cycle >= 257 && this.Cycle <= 320)
            {
                // this.Registers.OAMADDR.Write(00);
                SpriteHandler.EvaluateSpritesOnScanline(this.PrimaryOAM, this.ScanLine + 1);
            }
            else if (this.Cycle >= 321 && this.Cycle <= 336)
            {
            }
        }

        byte[] oam_temp = new byte[8];

        private void HandlePostRenderScanline()
        {
        }

        private void HandleVerticalBlankLines()
        {
            if (this.ScanLine == 241 && this.Cycle == 1)
            {
                this.IsVBlank = true;
                this.SpriteHandler.ClearSprites();

                if (this.WasNMIRequested)
                {
                    this.ShouldNMInterrupt = true;
                }
            }
        }

        private void HandleSprite0Hit((byte backgroundPixel, Tile tile) drawnTile, (byte spritePixel, Sprite sprite) drawnSprite)
        {
            // Sprite0 hit flag gets reset in cycle 1 of prerender line.
            if (this.SpriteHandler.WasSprite0Hit)
            {
                return;
            }

            this.SpriteHandler.WasSprite0Hit =
                drawnSprite.sprite?.IsSpriteZero ?? false
                && (drawnTile.backgroundPixel != 0x00
                && drawnSprite.spritePixel != 0x00);
        }

        private void UpdatePPUAddress(byte value)
        {
            this.registerLatch = value;

            if (this.InternalRegisters.w)
            {
                this.InternalRegisters.t = (ushort)((this.InternalRegisters.t & 0x80FF) | ((value & 0x3F) << 8));
                this.InternalRegisters.t.SetBit(14, false);
            }
            else
            {
                this.InternalRegisters.t = (ushort)((this.InternalRegisters.t & 0xFF00) | value);
                this.InternalRegisters.v = this.InternalRegisters.t;
            }

            this.InternalRegisters.w = !this.InternalRegisters.w;
        }
    }
}

﻿using MICE.Common.Helpers;
using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;
using MICE.PPU.RicohRP2C02.Handlers;
using System;
using System.Runtime.CompilerServices;

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

        public RicohRP2C02(
            IPPUMemoryMap memoryMap,
            PPURegisters registers,
            PPUInternalRegisters internalRegisters,
            ScrollHandler scrollHandler,
            PaletteHandler paletteHandler,
            BackgroundHandler backgroundHandler,
            PixelMuxer pixelMuxer,
            SpriteHandler spriteHandler)
        {
            this.InternalRegisters = internalRegisters;

            this.Registers = registers;
            this.MemoryMap = (PPURawMemoryMap)memoryMap;
            this.ScrollHandler = scrollHandler;

            this.PaletteHandler = paletteHandler;
            this.BackgroundHandler = backgroundHandler;
            this.SpriteHandler = spriteHandler;
            this.PixelMuxer = pixelMuxer;
        }

        public byte RegisterLatch { get; set; }
        public bool IsPowered { get; private set; }
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
        public PaletteHandler PaletteHandler { get; private set; }
        public SpriteHandler SpriteHandler { get; private set; }
        public ScrollHandler ScrollHandler { get; private set; }
        public PixelMuxer PixelMuxer { get; private set; }

        public IPPUMemoryMap MemoryMap { get; private set; }

        /// <summary>
        /// Gets the externally accessible PPU Registers. These are manipulated via the CPU.
        /// </summary>
        public PPURegisters Registers { get; private set; }

        /// <summary>
        /// Gets the internal-only PPU Registers. These are manipulated only via the PPU.
        /// </summary>
        public PPUInternalRegisters InternalRegisters { get; private set; }

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

        public bool ShouldNMInterrupt { get; set; }

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

        public void ClearOutput()
        {
            for (int i = 0; i < this.ScreenData.Length; i++)
            {
                this.ScreenData[i] = 0x0F;
            }
        }

        /// <summary>
        /// Gets a value indicating if the Frame is even or odd.
        /// </summary>
        public bool IsFrameEven => this.FrameNumber % 2 == 0;

        public bool IsRenderingEnabled => this.BackgroundHandler.ShowBackground || this.SpriteHandler.ShowSprites;

        public bool IsPreRenderLine => this.ScanLine == -1;
        public bool IsVisibleLine => this.ScanLine >= 0 && this.ScanLine <= 239;
        public bool IsVisibleCycle => this.Cycle >= 1 && this.Cycle <= 256;
        public bool IsPostRenderLine => this.ScanLine == 240;
        public bool IsRenderLine => this.IsVisibleLine || this.IsPreRenderLine;

        public bool IsFetchLine => this.IsRenderLine;
        public bool IsFetchCycle => this.IsVisibleCycle || (this.Cycle >= 321 && this.Cycle <= 336);
        public bool ShouldFetch => this.IsRenderingEnabled && this.IsFetchLine && this.IsFetchCycle;

        public bool ShouldIncrementHorizontal => this.IsRenderingEnabled && this.IsRenderLine && this.IsFetchCycle && this.Cycle % 8 == 0;
        public bool ShouldIncrementVertical => this.IsRenderingEnabled && this.IsRenderLine && this.Cycle == 256;

        public bool ShouldCopyVertical => this.IsRenderingEnabled && this.IsPreRenderLine && (this.Cycle >= 280 && this.Cycle <= 304);
        public bool ShouldCopyHorizontalBits => this.IsRenderingEnabled && this.IsRenderLine && this.Cycle == 257;

        public bool ShouldSetVBlank => this.IsPostRenderLine && this.Cycle == 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Step()
        {
            ++this.Cycle;

            if (this.OnFinalCycleOnLine())
            {
                this.ScanLine++;

                this.Cycle = 0;

                if (this.ScanLine == 240)
                {
                    this.FrameNumber++;
                }
                else if (this.ScanLine == 261)
                {
                    this.ScanLine = -1;
                }
                else if (this.ScanLine == 0)
                {
                    this.Cycle = this.IsRenderingEnabled
                     ? this.FrameNumber % 2 == 0 ? 0 : 1
                     : 0;
                }
            }

            if (this.IsPreRenderLine && this.Cycle == 1)
            {
                this.IsVBlank = false;
                this.SpriteHandler.WasSprite0Hit = false;
                this.SpriteHandler.WasSpriteOverflow = false;
            }

            if (this.IsVisibleLine)
            {
                if (this.IsRenderingEnabled && this.IsVisibleCycle)
                {
                    this.DrawPixel(this.Cycle - 1, this.ScanLine);
                }

                if (this.Cycle == 64)
                {
                    this.SecondaryOAM.Data.Clear(0xFF);
                }

                if (this.Cycle == 140 && (this.BackgroundHandler.ShowBackground || this.SpriteHandler.ShowSprites))
                {
                    this.SpriteHandler.EvaluateSpritesOnScanline(this.PrimaryOAM, this.ScanLine, this.Cycle);
                }
            }

            if (this.ScanLine == 241 && this.Cycle == 0)
            {
                this.HandleVerticalBlankLines();
            }

            if (this.ScanLine <= 240 && this.Cycle >= 257 && this.Cycle <= 320)
            {
                this.Registers.OAMADDR.WriteInternal(0);
            }

            if (this.ShouldFetch)
            {
                this.Fetch();
            }

            if (this.ShouldIncrementHorizontal)
            {
                this.ScrollHandler.IncrementCoarseX();
            }

            if (this.ShouldIncrementVertical)
            {
                this.ScrollHandler.IncrementCoarseY();
            }

            if (this.ShouldCopyHorizontalBits)
            {
                this.ScrollHandler.CopyHorizontalBits();
            }

            if (this.ShouldCopyVertical)
            {
                this.ScrollHandler.CopyVerticalBits();
            }

            return 0;
        }

        public void PowerOn(Action cycleComplete)
        {
            this.Registers.PPUADDR.ReadByteInsteadAction = (_, value) => this.RegisterLatch;
            this.Registers.PPUADDR.AfterWriteAction = (_, value) =>
            {
                this.RegisterLatch = value;
                this.UpdatePPUAddress(value);
            };


            this.Registers.PPUMASK.ReadByteInsteadAction = (_, value) => this.RegisterLatch;
            this.Registers.PPUMASK.AfterWriteAction = (address, value) => this.RegisterLatch = value;


            this.Registers.PPUSCROLL.ReadByteInsteadAction = (_, value) => this.RegisterLatch;
            this.Registers.PPUSCROLL.AfterWriteAction = (address, value) =>
            {
                this.RegisterLatch = (byte)value;
                this.ScrollHandler.PPUScrollWrittenTo(address, value);
            };


            this.Registers.PPUCTRL.ReadByteInsteadAction = (_, value) => this.RegisterLatch;
            this.Registers.PPUCTRL.AfterWriteAction = (address, value) =>
            {
                this.RegisterLatch = value;
                this.InternalRegisters.t = (ushort)((this.InternalRegisters.t & 0xF3FF) | ((value & 0x03) << 10));
            };


            this.Registers.PPUDATA.ReadShortInsteadAction = (_, value) => this.InternalRegisters.v;
            this.Registers.PPUDATA.AfterReadAction = (_, value) => this.InternalRegisters.v += (ushort)this.VRAMAddressIncrement;
            this.Registers.PPUDATA.AfterWriteAction = (_, value) =>
            {
                this.RegisterLatch = value;

                this.MemoryMap.Write(this.InternalRegisters.v, value);
                this.InternalRegisters.v += (ushort)this.VRAMAddressIncrement;
            };

            this.Registers.PPUDATA.ReadByteInsteadAction = (_, value) =>
            {
                if (this.InternalRegisters.v >= 0x3F00 && this.InternalRegisters.v <= 0x3FFF)
                {
                    var paletteByte = this.MemoryMap.ReadByte(this.InternalRegisters.v);
                    this.MemoryMap.ReadBuffer = this.MemoryMap.ReadByte((ushort)(this.InternalRegisters.v - 0x1000));

                    return paletteByte;
                }

                var temp = this.MemoryMap.ReadBuffer;
                this.MemoryMap.ReadBuffer = this.MemoryMap.ReadByte(this.InternalRegisters.v);

                return temp;
            };


            this.Registers.PPUSTATUS.AfterWriteAction = (_, value) => this.RegisterLatch = value;
            this.Registers.PPUSTATUS.AfterReadAction = (_, value) =>
            {
                this.InternalRegisters.w = true;
                this.IsVBlank = false;
            };

            this.Registers.PPUSTATUS.ReadByteInsteadAction = (_, value) =>
            {
                byte result = (byte)(this.RegisterLatch & 0b00011111);

                if (this.SpriteHandler.WasSpriteOverflow)
                {
                    result |= 0b00100000;
                }

                if (this.SpriteHandler.WasSprite0Hit)
                {
                    result |= 0b01000000;
                }

                if (this.IsVBlank)
                {
                    result |= 0b10000000;
                }

                return (byte)result;
            };

            this.Registers.OAMADDR.ReadByteInsteadAction = (_, value) => this.RegisterLatch;
            this.Registers.OAMADDR.AfterWriteAction = (_, value) => this.RegisterLatch = value;

            this.Registers.OAMDATA.ReadByteInsteadAction = (_, value) => this.PrimaryOAM[this.Registers.OAMADDR.ReadInternal()];
            this.Registers.OAMDATA.AfterWriteAction = (_, value) =>
            {
                this.RegisterLatch = value;

                byte oamADDR = this.Registers.OAMADDR.ReadInternal();
                this.PrimaryOAM[this.Registers.OAMADDR.ReadInternal()] = value;
                this.Registers.OAMADDR.WriteInternal(++oamADDR);
            };

            this.Registers.OAMDMA.ReadByteInsteadAction = (_, value) => this.RegisterLatch;

            this.IsPowered = true;
            this.Restart();
        }

        public void Restart()
        {
            this.Registers.PPUCTRL.Write(0);
            this.Registers.PPUMASK.Write(0);
            this.Registers.PPUSTATUS.Write(0);

            this.Registers.OAMADDR.Write(0);
            this.Registers.PPUSCROLL.WriteInternal(0);

            this.InternalRegisters.w = true;

            this.FrameNumber = 1;
            this.ScanLine = -1;
            this.Cycle = 340;

            this.ClearOutput();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool OnFinalCycleOnLine()
        {
            return this.Cycle > 339;

            bool isFinalCycle = this.ScanLine == 261 && this.BackgroundHandler.ShowBackground && !this.IsFrameEven
                ? this.Cycle == 340
                : this.Cycle == 341;

            if (isFinalCycle)
            {

            }

            return isFinalCycle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DrawPixel(int x, int y)
        {
            var backgroundPixel = BackgroundHandler.GetBackgroundPixel(x, y);
            (byte spritePixel, Sprite sprite) drawnSprite = SpriteHandler.GetSpritePixel(x, y, this.PrimaryOAM);

            byte muxedPixel = PixelMuxer.MuxPixel(drawnSprite, backgroundPixel, BackgroundHandler.CurrentTile);

            this.ScreenData[(y * Constants.NTSCWidth) + x] = muxedPixel;

            this.HandleSprite0Hit(x, y, backgroundPixel, drawnSprite);
        }

        private void Fetch()
        {
            this.BackgroundHandler.NextCycle();

            switch (this.Cycle % 8)
            {
                case 1:
                    this.BackgroundHandler.FetchNametableByte();
                    break;
                case 3:
                    this.BackgroundHandler.FetchAttributeByte();
                    break;
                case 5:
                    this.BackgroundHandler.FetchLowBGTile();
                    break;
                case 7:
                    this.BackgroundHandler.FetchHighBGTile();
                    break;
                case 0:
                    this.BackgroundHandler.StoreTileData();
                    break;
            }
        }

        private void HandleVerticalBlankLines()
        {
            this.IsVBlank = true;
            this.SpriteHandler.ClearSprites();

            if (this.WasNMIRequested)
            {
                this.ShouldNMInterrupt = true;
            }
        }

        private void HandleSprite0Hit(int x, int y, byte backgroundPixel, (byte spritePixel, Sprite sprite) drawnSprite)
        {
            if (this.SpriteHandler.WasSprite0Hit || x == 255)
            {
                return;
            }

            if (!this.BackgroundHandler.ShowBackground || !this.SpriteHandler.ShowSprites)
            {
                return;
            }

            if (BackgroundHandler.CurrentTile?.IsTransparentPixel ?? false || drawnSprite.spritePixel % 4 == 0)
            {
                return;
            }

            if (x <= 7 && this.Registers.PPUMASK.ReadInternal() != 0x1e)
            {
                return;
            }

            this.SpriteHandler.WasSprite0Hit =
                drawnSprite.sprite?.IsSpriteZero ?? false
                && (backgroundPixel != 0x00
                && drawnSprite.spritePixel != 0x00);
        }

        private void UpdatePPUAddress(byte value)
        {
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

        public void PowerOff()
        {
        }
    }
}

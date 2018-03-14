using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            public static uint[] RGBAPalette = {
                0x7C7C7CFF,0x0000FCFF,0x0000BCFF,0x4428BCFF,
                0x940084FF,0xA80020FF,0xA81000FF,0x881400FF,
                0x503000FF,0x007800FF,0x006800FF,0x005800FF,
                0x004058FF,0x000000FF,0x000000FF,0x000000FF,
                0xBCBCBCFF,0x0078F8FF,0x0058F8FF,0x6844FCFF,
                0xD800CCFF,0xE40058FF,0xF83800FF,0xE45C10FF,
                0xAC7C00FF,0x00B800FF,0x00A800FF,0x00A844FF,
                0x008888FF,0x000000FF,0x000000FF,0x000000FF,
                0xF8F8F8FF,0x3CBCFCFF,0x6888FCFF,0x9878F8FF,
                0xF878F8FF,0xF85898FF,0xF87858FF,0xFCA044FF,
                0xF8B800FF,0xB8F818FF,0x58D854FF,0x58F898FF,
                0x00E8D8FF,0x787878FF,0x000000FF,0x000000FF,
                0xFCFCFCFF,0xA4E4FCFF,0xB8B8F8FF,0xD8B8F8FF,
                0xF8B8F8FF,0xF8A4C0FF,0xF0D0B0FF,0xFCE0A8FF,
                0xF8D878FF,0xD8F878FF,0xB8F8B8FF,0xB8F8D8FF,
                0x00FCFCFF,0xF8D8F8FF,0x000000FF,0x000000FF
            };
        }

        private Stopwatch stepSW;
        private Stopwatch frameSW;

        public RicohRP2C02(PPUMemoryMap memoryMap, PPURegisters registers, IMemoryMap cpuMemoryMap, IList<byte[]> chrBanks)
        {
            this.Registers = registers;
            this.MemoryMap = memoryMap;
            this.BackgroundHandler = new BackgroundHandler(this.MemoryMap, this.Registers, cpuMemoryMap, chrBanks);
            this.SpriteHandler = new SpriteHandler(this.MemoryMap, this.Registers, cpuMemoryMap);
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
        public PixelMuxer PixelMuxer { get; private set; }

        private ushort ppuAddress = 0;
        private bool hasWrittenToggle = false;

        public bool ShouldNMInterrupt { get; set; }

        public PPUMemoryMap MemoryMap { get; private set; }

        public PPURegisters Registers { get; private set; }

        public int BaseNametableAddress => (this.Registers.PPUCTRL.GetBit(0) ? 1 : 0) | (this.Registers.PPUCTRL.GetBit(1) ? 1 : 0) << 2;

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

        public bool IsPPUMaster
        {
            get => this.Registers.PPUCTRL.GetBit(6);
            set => this.Registers.PPUCTRL.SetBit(6, value);
        }

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
        /// Current frame number being rendered.
        /// </summary>
        public long FrameNumber { get; private set; }

        public bool IsFrameEven => this.FrameNumber % 2 == 0;

        public void PowerOn(CancellationToken cancellationToken)
        {
            this.stepSW = new Stopwatch();
            this.frameSW = new Stopwatch();

            this.Registers.PPUMASK.AfterWriteAction = (value) =>
             {

             };

            this.Registers.PPUADDR.AfterWriteAction = (value) =>
            {
                this.ppuAddress = this.hasWrittenToggle
                ? (ushort)(this.ppuAddress | value)
                : (ushort)(value << 8);

                this.hasWrittenToggle = !this.hasWrittenToggle;
            };

            this.Registers.PPUDATA.AfterReadAction = () =>
            {
                this.ppuAddress += (ushort)this.VRAMAddressIncrement;
            };

            this.Registers.PPUDATA.AfterReadAction = () =>
            {

            };

            this.Registers.PPUDATA.AfterWriteAction = (value) =>
            {
                this.MemoryMap.Write(this.ppuAddress, value);
                this.ppuAddress += (ushort)this.VRAMAddressIncrement;
            };

            this.Registers.PPUSTATUS.AfterReadAction = () =>
            {
                this.Registers.PPUSCROLL.Write(0);
                this.Registers.PPUADDR.Write(0);
                this.ppuAddress = 0;
                this.hasWrittenToggle = false;

                this.IsVBlank = false;
            };

            this.Registers.PPUCTRL.AfterWriteAction = (value) =>
            {
                this.ShouldNMInterrupt = false;
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
            this.Registers.PPUDATA.Write(0);

            this.hasWrittenToggle = false;
            this.ppuAddress = 0;

            this.FrameNumber = 0;
            this.ScanLine = 0;
            this.Cycle = 0;
        }

        public int Step()
        {
            stepSW.Start();

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

            this.Cycle++;

            if (this.OnFinalCycleOnLine())
            {
                this.ScanLine++;
                this.Cycle = 0;
            }

            stepSW.Stop();

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

            this.frameSW.Stop();
            this.frameSW.Start();
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
        }

        private void SetPixel(int x, int y)
        {
            if (this.BackgroundHandler.ShowBackground)
            {
                this.BackgroundHandler.DrawBackgroundPixel(x, y);
            }

            if (this.SpriteHandler.ShowSprites)
            {
                this.SpriteHandler.DrawSpritePixel(x, y);
            }
        }

        private void HandleVisibleScanlines()
        {
            if (this.Cycle > 0 && this.Cycle <= 256)
            {
                if (BackgroundHandler.ShowBackground && SpriteHandler.ShowSprites)
                {
                    var pixelX = this.Cycle - 1;
                    byte backgroundPixel = BackgroundHandler.DrawBackgroundPixel(pixelX, this.ScanLine);
                    byte spritePixel = SpriteHandler.DrawSpritePixel(pixelX, this.ScanLine);

                    byte muxedPixel = PixelMuxer.MuxPixel(spritePixel, backgroundPixel);
                    this.ScreenData[256 * this.ScanLine + pixelX] = muxedPixel;
                }
            }
            else if (this.Cycle >= 257 && this.Cycle <= 320)
            {
                this.Registers.OAMADDR.Write(00);
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

                if (this.WasNMIRequested)
                {
                    // TODO: do NMI.
                    this.ShouldNMInterrupt = true;
                }
            }
        }
    }
}

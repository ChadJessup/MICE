using MICE.Common.Interfaces;
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
            public const int NTSC_WIDTH = 256;
            public const int NTSC_HEIGHT = 224;
        }

        public RicohRP2C02()
        {
            this.BackgroundHandler = new BackgroundHandler(this.Registers);
            this.SpriteHandler = new SpriteHandler(this.Registers);
            this.PixelMuxer = new PixelMuxer(this.Registers);
        }

        /// <summary>
        /// Gets the Primary Object Attribute Memory (OAM) byte array.
        /// This holds up to 64 sprites for the frame.
        /// </summary>
        public byte[] PrimaryOAM { get; } = new byte[256];

        /// <summary>
        /// Gets the Secondary Object Attribute Memory (OAM) byte array.
        /// This holds up to 8 sprites for the current scanline.
        /// </summary>
        public byte[] SecondaryOAM { get; } = new byte[32];

        public BackgroundHandler BackgroundHandler { get; private set; }
        public SpriteHandler SpriteHandler { get; private set; }
        public PixelMuxer PixelMuxer { get; private set; }

        private ushort ppuAddress = 0;
        private bool hasWrittenToggle = false;

        public PPUMemoryMap MemoryMap { get; } = new PPUMemoryMap();
        public Registers Registers { get; } = new Registers();

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
        /// Current frame being rendered.
        /// </summary>
        public long Frame { get; private set; }

        public bool IsFrameEven => this.Frame % 2 == 0;

        public void PowerOn(CancellationToken cancellationToken)
        {
            this.Registers.PPUADDR.AfterWriteAction = (value) =>
            {
                if (!this.hasWrittenToggle)
                {
                    this.ppuAddress = (ushort)(value << 8);
                }
                else
                {
                    this.ppuAddress = (ushort)(this.ppuAddress | value);
                }

                this.hasWrittenToggle = !this.hasWrittenToggle;
            };

            this.Registers.PPUDATA.AfterReadAction = () =>
            {
                this.ppuAddress += (ushort)this.VRAMAddressIncrement;
            };

            this.Registers.PPUDATA.AfterWriteAction = (value) =>
            {
                this.ppuAddress += (ushort)this.VRAMAddressIncrement;
            };

            this.Registers.PPUCTRL.AfterWriteAction = (value) =>
            {
                if (this.WasNMIRequested)
                {

                }
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

            this.Frame = 0;
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

            this.Cycle++;

            if (this.OnFinalCycleOnLine())
            {
                this.ScanLine++;
                this.Cycle = 0;
            }

            return 0;
        }

        private bool OnFinalCycleOnLine()
        {
            if (this.BackgroundHandler.ShowBackground && !this.IsFrameEven && this.ScanLine == -1)
            {
                return this.Cycle == 340;
            }

            return this.Cycle == 341;
        }

        private void HandleFinalScanline()
        {
            this.ScanLine = -1;
            this.Frame++;
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
                // this.SetPixel(this.Cycle, this.ScanLine);
                uint backgroundPixel = BackgroundHandler.DrawBackgroundPixel(this.Cycle, this.ScanLine);
                uint spritePixel = SpriteHandler.DrawSpritePixel(this.Cycle, this.ScanLine);

                uint muxedPixel = PixelMuxer.MuxPixel(spritePixel, backgroundPixel);
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
                }
            }
        }
    }
}

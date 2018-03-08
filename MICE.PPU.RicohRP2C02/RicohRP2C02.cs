using MICE.Common.Interfaces;
using MICE.Components.CPU;
using System;
using System.Threading;

namespace MICE.PPU.RicohRP2C02
{
    // Some notes: http://problemkaputt.de/everynes.htm#pictureprocessingunitppu

    public class RicohRP2C02 : IMicroprocessor
    {
        private static class Constants
        {
            public const int ScanlinesPerFrame = 262;
            public const int CyclesPerScanline = 341;
            public const int NTSC_WIDTH = 256;
            public const int NTSC_HEIGHT = 224;
        }

        public byte[] OAM { get; } = new byte[1024];
        private int spriteCount = 0;

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

        public bool IsSpritePatternTableAddress1000
        {
            get => this.Registers.PPUCTRL.GetBit(3);
            set => this.Registers.PPUCTRL.SetBit(3, value);
        }

        public bool IsBackgroundPatternTableAddress1000
        {
            get => this.Registers.PPUCTRL.GetBit(4);
            set => this.Registers.PPUCTRL.SetBit(4, value);
        }

        public bool IsSmallSprites
        {
            get => this.Registers.PPUCTRL.GetBit(5);
            set => this.Registers.PPUCTRL.SetBit(5, value);
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
        /// Gets or sets a value indicating whether or not to output gray scale.
        /// </summary>
        public bool IsGrayScale
        {
            get => this.Registers.PPUMASK.GetBit(0);
            set => this.Registers.PPUMASK.SetBit(0, value);
        }

        public bool DrawLeft8BackgroundPixels
        {
            get => this.Registers.PPUMASK.GetBit(1);
            set => this.Registers.PPUMASK.SetBit(1, value);
        }

        public bool DrawLeft8SpritePixels
        {
            get => this.Registers.PPUMASK.GetBit(2);
            set => this.Registers.PPUMASK.SetBit(2, value);
        }

        public bool ShowBackground
        {
            get => this.Registers.PPUMASK.GetBit(3);
            set => this.Registers.PPUMASK.SetBit(3, value);
        }

        public bool ShowSprites
        {
            get => this.Registers.PPUMASK.GetBit(4);
            set => this.Registers.PPUMASK.SetBit(4, value);
        }

        public bool EmphasizeRed
        {
            get => this.Registers.PPUMASK.GetBit(5);
            set => this.Registers.PPUMASK.SetBit(5, value);
        }

        public bool EmphasizeGreen
        {
            get => this.Registers.PPUMASK.GetBit(6);
            set => this.Registers.PPUMASK.SetBit(6, value);
        }

        public bool EmphasizeBlue
        {
            get => this.Registers.PPUMASK.GetBit(7);
            set => this.Registers.PPUMASK.SetBit(7, value);
        }

        /// <summary>
        /// Gets or sets a value indicating if there was sprite over (more than 8 sprites on scanline).
        /// Note, there was a hardware bug on this, so behavior might be difficult to understand once fully implemented.
        /// </summary>
        public bool WasSpriteOverflow
        {
            get => this.Registers.PPUSTATUS.GetBit(5);
            set => this.Registers.PPUSTATUS.SetBit(5, value);
        }

        /// <summary>
        /// Gets or sets a value when Sprite 0 was hit.
        /// </summary>
        public bool WasSprite0Hit
        {
            get => this.Registers.PPUSTATUS.GetBit(6);
            set => this.Registers.PPUSTATUS.SetBit(6, value);
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

            this.Registers.PPUMASK.AfterReadAction = () =>
            {

            };

            this.Registers.PPUMASK.AfterWriteAction = (value) =>
            {

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
            if (this.ShowBackground && !this.IsFrameEven && this.ScanLine == -1)
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
                    this.WasSprite0Hit = false;
                    this.WasSpriteOverflow = false;
                }

                // Current scanline data fetch cycles
                this.SetPixel(this.Cycle, this.ScanLine);
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
            if (this.ShowBackground)
            {
                this.DrawBackgroundPixel(x, y);
            }

            if (this.ShowSprites)
            {
                this.DrawSpritePixel(x, y);
            }
        }

        private void HandleVisibleScanlines()
        {
            if (this.Cycle >= 257 && this.Cycle <= 320)
            {
                this.Registers.OAMADDR.Write(00);
                this.FetchSpriteTileData(this.ScanLine + 1);
            }
            else if (this.Cycle >= 321 && this.Cycle <= 336)
            {

            }
        }

        private void FetchSpriteTileData(int scanline)
        {
            this.spriteCount = 0;

            for (byte oam_index = 0; oam_index < 64 && this.spriteCount < 8; ++oam_index)
            {
            //    byte sprite_y = (byte)this.MemoryMap.ReadByte()
            }
        }

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

        private void DrawBackgroundPixel(int x, int y)
        {
            if (x <= 8 && !this.DrawLeft8BackgroundPixels)
            {
                return;
            }

            if (x <= 8 && !this.DrawLeft8SpritePixels)
            {
                return;
            }


        }

        private void DrawSpritePixel(int x, int y)
        {

        }
    }
}

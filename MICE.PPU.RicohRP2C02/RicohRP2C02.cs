using MICE.Common.Interfaces;
using MICE.Components.CPU;
using System;
using System.Threading;

namespace MICE.PPU.RicohRP2C02
{
    public class RicohRP2C02 : IMicroprocessor
    {
        private static class Constants
        {
            public const int ScanlinesPerFrame = 262;
            public const int CyclesPerScanline = 341;
            public const int NTSC_WIDTH = 256;
            public const int NTSC_HEIGHT = 224;
        }

        public PPUMemoryMap MemoryMap { get; } = new PPUMemoryMap();

        // Registers for the PPU that the CPU has memory mapped to particular locations.
        // The memory mapping happens when the CPU is being initialized.

        /// <summary>
        /// The PPU Control register contains various bits that controls how the PPU behaves. Sometimes called PPU Control Register 1.
        /// This register is memory mapped to the CPU at $2000.
        /// </summary>
        public Register8Bit PPUCTRL = new Register8Bit("PPUCTRL");

        public int BaseNametableAddress => (this.PPUCTRL.GetBit(0) ? 1 : 0) | (this.PPUCTRL.GetBit(1) ? 1 : 0) << 2;

        public bool IsVRAMAddressIncrement1
        {
            get => this.PPUCTRL.GetBit(2);
            set => this.PPUCTRL.SetBit(2, value);
        }

        public bool IsSpritePatternTableAddress1000
        {
            get => this.PPUCTRL.GetBit(3);
            set => this.PPUCTRL.SetBit(3, value);
        }

        public bool IsBackgroundPatternTableAddress1000
        {
            get => this.PPUCTRL.GetBit(4);
            set => this.PPUCTRL.SetBit(4, value);
        }

        public bool IsSmallSprites
        {
            get => this.PPUCTRL.GetBit(5);
            set => this.PPUCTRL.SetBit(5, value);
        }

        public bool IsPPUMaster
        {
            get => this.PPUCTRL.GetBit(6);
            set => this.PPUCTRL.SetBit(6, value);
        }

        public bool WasNMIRequested
        {
            get => this.PPUCTRL.GetBit(7);
            set => this.PPUCTRL.SetBit(7, value);
        }

        /// <summary>
        /// Various bits that enables masking of certain features of the PPU. Sometimes called PPU Control Register 2.
        /// This register is memory mapped to the CPU at $2001.
        /// </summary>
        public Register8Bit PPUMASK = new Register8Bit("PPUMASK");

        /// <summary>
        /// Gets or sets a value indicating whether or not to output gray scale.
        /// </summary>
        public bool IsGrayScale
        {
            get => this.PPUMASK.GetBit(0);
            set => this.PPUMASK.SetBit(0, value);
        }

        public bool ShowBackground
        {
            get => this.PPUMASK.GetBit(3);
            set => this.PPUMASK.SetBit(3, value);
        }

        public bool ShowSprites
        {
            get => this.PPUMASK.GetBit(4);
            set => this.PPUMASK.SetBit(4, value);
        }

        public bool DrawLeft8BackgroundPixels
        {
            get => this.PPUMASK.GetBit(1);
            set => this.PPUMASK.SetBit(1, value);
        }

        public bool DrawLeft8SpritePixels
        {
            get => this.PPUMASK.GetBit(2);
            set => this.PPUMASK.SetBit(2, value);
        }

        /// <summary>
        /// Status bits of the current state of the PPU.
        /// This register is memory mapped to the CPU at $2002.
        /// </summary>
        public Register8Bit PPUSTATUS = new Register8Bit("PPUSTATUS");

        public bool IsVBlank
        {
            get => this.PPUSTATUS.GetBit(7);
            set => this.PPUSTATUS.SetBit(7, value);
        }

        /// <summary>
        /// The OAM read/write address.
        /// This register is memory mapped to the CPU at $2003.
        /// </summary>
        public Register8Bit OAMADDR = new Register8Bit("OAMADDR");

        /// <summary>
        /// The OAM data read/write.
        /// This register is memory mapped to the CPU at $2004.
        /// </summary>
        public Register8Bit OAMDATA = new Register8Bit("OAMDATA");

        /// <summary>
        /// Fine control of the PPU's scroll position (X, Y).
        /// This register is memory mapped to the CPU at $2005.
        /// </summary>
        public Register8Bit PPUSCROLL = new Register8Bit("PPUSCROLL");

        /// <summary>
        /// PPU read/write address.
        /// This register is memory mapped to the CPU at $2006.
        /// </summary>
        public Register8Bit PPUADDR = new Register8Bit("PPUADDR");

        /// <summary>
        /// PPU data read/write.
        /// This register is memory mapped to the CPU at $2007.
        /// </summary>
        public Register8Bit PPUDATA = new Register8Bit("PPUDATA");

        /// <summary>
        /// The OAM DMA high address.
        /// This register is memory mapped to the CPU at $4014.
        /// </summary>
        public Register8Bit OAMDMA = new Register8Bit("OAMDMA");

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
            this.PPUSTATUS.AfterReadAction = () =>
            {
                this.IsVBlank = false;
            };
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
                // Current scanline data fetch cycles
                this.SetPixel(this.Cycle, this.ScanLine);
            }
            else if (this.Cycle >= 257 && this.Cycle <= 320)
            {
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

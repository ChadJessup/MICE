using System;

namespace MICE.PPU.RicohRP2C02
{
    public class BackgroundHandler
    {
        private readonly Registers registers;
        public BackgroundHandler(Registers registers)
        {
            this.registers = registers;
        }

        public bool DrawLeft8BackgroundPixels
        {
            get => this.registers.PPUMASK.GetBit(1);
            set => this.registers.PPUMASK.SetBit(1, value);
        }

        public bool ShowBackground
        {
            get => this.registers.PPUMASK.GetBit(3);
            set => this.registers.PPUMASK.SetBit(3, value);
        }

        public bool IsBackgroundPatternTableAddress1000
        {
            get => this.registers.PPUCTRL.GetBit(4);
            set => this.registers.PPUCTRL.SetBit(4, value);
        }

        public void DrawBackgroundPixel(int x, int y)
        {
            if (x <= 8 && !this.DrawLeft8BackgroundPixels)
            {
                return;
            }

            int ppu_scroll_x = (this.registers.PPUSCROLL >> 8) & 0xFF;
            int ppu_scroll_y = this.registers.PPUSCROLL & 0xFF;

            ppu_scroll_x += ((this.registers.PPUCTRL >> 0) & 1) * 256;
            ppu_scroll_y += ((this.registers.PPUCTRL >> 1) & 1) * 240;


        }
    }
}
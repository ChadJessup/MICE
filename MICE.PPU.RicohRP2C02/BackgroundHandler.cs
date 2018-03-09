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

        public uint DrawBackgroundPixel(int x, int y)
        {
            if (x <= 8 && !this.DrawLeft8BackgroundPixels)
            {
                return 0;
            }

            int ppu_scroll_x = (this.registers.PPUSCROLL >> 8) & 0xFF;
            int ppu_scroll_y = this.registers.PPUSCROLL & 0xFF;

            ppu_scroll_x += ((this.registers.PPUCTRL >> 0) & 1) * 256;
            ppu_scroll_y += ((this.registers.PPUCTRL >> 1) & 1) * 240;

            int which_nametable = 0;
            int nametable_x = (ppu_scroll_x + x) % 512;

            if (nametable_x >= 256)
            {
                which_nametable += 1;
                nametable_x -= 256;
            }

            int nametable_y = (ppu_scroll_y + py) % 480;
            if (nametable_y >= 240)
            {
                which_nametable += 2;
                nametable_y -= 240;
            }

            int nametable_tile_x = nametable_x / 8;
            int nametable_tile_y = nametable_y / 8;

            int attribute_x = nametable_tile_x / 4;
            int attribute_y = nametable_tile_y / 4;


            return 0;
        }
    }
}
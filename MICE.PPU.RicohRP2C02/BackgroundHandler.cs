using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;

namespace MICE.PPU.RicohRP2C02
{
    public class BackgroundHandler
    {
        private readonly PPURegisters registers;
        private readonly IMemoryMap ppuMemoryMap;
        private readonly IMemoryMap cpuMemoryMap;

        private Nametable nameTable0;
        private Nametable nameTable1;
        private Nametable nameTable2;
        private Nametable nameTable3;

        private Nametable currentNameTable;

        private Palette imagePalette;

        public BackgroundHandler(IMemoryMap ppuMemoryMap, PPURegisters registers, IMemoryMap cpuMemoryMap)
        {
            this.ppuMemoryMap = ppuMemoryMap;
            this.registers = registers;
            this.cpuMemoryMap = cpuMemoryMap;

            this.CacheMemorySegments(ppuMemoryMap);
        }

        // Cache memory segments that are used heavily....
        // TODO: Find a better way, this sucks.
        private void CacheMemorySegments(IMemoryMap ppuMemoryMap)
        {
            this.nameTable0 = ppuMemoryMap.GetMemorySegment<Nametable>("Name Table 0");
            this.nameTable1 = ppuMemoryMap.GetMemorySegment<Nametable>("Name Table 1");
            this.nameTable2 = ppuMemoryMap.GetMemorySegment<Nametable>("Name Table 2");
            this.nameTable3 = ppuMemoryMap.GetMemorySegment<Nametable>("Name Table 3");

            this.imagePalette = ppuMemoryMap.GetMemorySegment<Palette>("Image Palette");
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

        public byte DrawBackgroundPixel(int x, int y)
        {
            if (x <= 8 && !this.DrawLeft8BackgroundPixels)
            {
                return 0;
            }

            int ppu_scroll_x = (this.registers.PPUSCROLL >> 8) & 0xFF;
            int ppu_scroll_y = this.registers.PPUSCROLL & 0xFF;

            ppu_scroll_x += ((this.registers.PPUCTRL >> 0) & 1) * 256;
            ppu_scroll_y += ((this.registers.PPUCTRL >> 1) & 1) * 240;

            ushort bg_pattern_base = this.IsBackgroundPatternTableAddress1000
                ? (ushort)0x1000
                : (ushort)0x0000;

            int which_nametable = 0;
            int nametable_x = (ppu_scroll_x + x) % 512;

            if (nametable_x >= 256)
            {
                which_nametable += 1;
                nametable_x -= 256;
            }

            int nametable_y = (ppu_scroll_y + y) % 480;
            if (nametable_y >= 240)
            {
                which_nametable += 2;
                nametable_y -= 240;
            }

            this.SetTables(which_nametable);

            int nametable_tile_x = nametable_x / 8;
            int nametable_tile_y = nametable_y / 8;

            int attribute_x = nametable_tile_x / 4;
            int attribute_y = nametable_tile_y / 4;

            int nametable_start = 0x2000 + 0x400 * which_nametable;

            int attribute_table_base = nametable_start + 0x3C0;
            int attributeTableIndex = attribute_y * 8 + attribute_x;
            byte attributeTableEntry = this.currentNameTable.ReadByte(((ushort)(attribute_table_base + attributeTableIndex)));

            // which pallete to derive the color from
            int which_palette = 0;
            which_palette |= nametable_tile_x % 4 >= 2 ? 2 : 0;
            which_palette |= nametable_tile_y % 4 >= 2 ? 4 : 0;

            // Use the previous value to select the two-bit palette number
            byte palette_num = (byte)((attributeTableEntry >> which_palette) & 3);

            // Compute color index

            int nt_index = nametable_tile_y * 32 + nametable_tile_x;
            ushort pt_index = this.currentNameTable.ReadByte((ushort)(nametable_start + nt_index));

            int tile_x = nametable_x % 8;
            int tile_y = nametable_y % 8;

            byte lowBits = this.ppuMemoryMap.ReadByte((ushort)(bg_pattern_base + pt_index * 16 + tile_y));
            byte highBits = this.ppuMemoryMap.ReadByte((ushort)(bg_pattern_base + pt_index * 16 + tile_y + 8));

            byte lowBit = (byte)((lowBits >> (7 - tile_x)) & 1);
            byte highBit = (byte)((highBits >> (7 - tile_x)) & 1);
            byte color_index = (byte)(lowBit + highBit * 2);

            // If it's the background color, look at palette 0 (Universally-shared BG color)
            if (color_index == 0)
            {
                palette_num = 0;
            }

            //backgroundCHRValues[256 * y + x] = color_index;

            byte RGB_index = this.imagePalette.ReadByte((ushort)(0x3F00 + 4 * palette_num + color_index));
            return RGB_index;
//            uint pixelColor = RicohRP2C02.Constants.RGBAPalette[RGB_index & 0x3F];

//            return pixelColor;
        }

        private void SetTables(int nametableId)
        {
            switch (nametableId)
            {
                case 0:
                    this.currentNameTable = this.nameTable0;
                    break;
                case 1:
                    this.currentNameTable = this.nameTable1;
                    break;
                case 2:
                    this.currentNameTable = this.nameTable2;
                    break;
                case 3:
                    this.currentNameTable = this.nameTable3;
                    break;
            }
        }
    }
}
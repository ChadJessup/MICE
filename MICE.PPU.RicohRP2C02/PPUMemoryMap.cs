using MICE.Common.Misc;
using MICE.Components.Memory;
using MICE.PPU.RicohRP2C02.Components;
using System.IO;

namespace MICE.PPU.RicohRP2C02
{
    /// <summary>
    /// A class that represents how the NES PPU's memory is mapped out.
    /// </summary>
    public class PPUMemoryMap : MemoryMapper
    {
        public PPUMemoryMap(StreamWriter sw) : base(sw)
        {
            // http://nesdev.com/NESDoc.pdf - Figure 3-1 PPU memory map

            // Pattern Tables
            this.Add(new PatternTable(0x0000, 0x0FFF, "Pattern Table 0"));
            this.Add(new PatternTable(0x1000, 0x1FFF, "Pattern Table 1"));

            // Name Tables
            this.Add(new Nametable(0x2000, 0x23FF, "Name Table 0"));
            this.Add(new Nametable(0x2400, 0x27FF, "Name Table 1"));
            this.Add(new Nametable(0x2800, 0x2BFF, "Name Table 2"));
            this.Add(new Nametable(0x2C00, 0x2FFF, "Name Table 3"));

            this.Add(new MirroredMemory(0X3000, 0x3EFF, 0x2000, 0x2FFF, this, "Mirrored Name Tables"));

            // Palettes
            this.Add(new Palette(0x3F00, 0x3F0F, "Image Palette"));
            this.Add(new Palette(0x3F10, 0x3F1F, "Sprite Palette"));

            this.Add(new MirroredMemory(0x3F20, 0x3FFF, 0x3F00, 0x3F1F, this, "Mirrored Palettes"));

            // Mirrors
            this.Add(new MirroredMemory(0x4000, 0xFFFF, 0x0000, 0x3FFF, this, "Mirrored PPU"));
        }
    }
}
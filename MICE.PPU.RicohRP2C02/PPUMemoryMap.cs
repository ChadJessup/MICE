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
        public PPUMemoryMap() : base()
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
            this.Add(new Palette(0x3F00, 0x3F03, "Background palette 0"));
            this.Add(new Palette(0x3F04, 0x3F07, "Background palette 1"));
            this.Add(new Palette(0x3F08, 0x3F0B, "Background palette 2"));
            this.Add(new Palette(0x3F0C, 0x3F0F, "Background palette 3"));

            this.Add(new MirroredMemory(0x3F10, 0x3F10, 0x3F00, 0x3F00, this, "Mirrored Universal background color byte"));
            this.Add(new MirroredMemory(0x3F14, 0x3F14, 0x3F04, 0x3F04, this, "Mirrored Background palette 0 byte"));
            this.Add(new MirroredMemory(0x3F18, 0x3F18, 0x3F08, 0x3F08, this, "Mirrored Background palette 1 byte"));
            this.Add(new MirroredMemory(0x3F1C, 0x3F1C, 0x3F0C, 0x3F0C, this, "Mirrored Background palette 2 byte"));

            this.Add(new Palette(0x3F11, 0x3F13, "Sprite palette 0"));
            this.Add(new Palette(0x3F15, 0x3F17, "Sprite palette 1"));
            this.Add(new Palette(0x3F19, 0x3F1B, "Sprite palette 2"));
            this.Add(new Palette(0x3F1D, 0x3F1F, "Sprite palette 3"));

            this.Add(new MirroredMemory(0x3F20, 0x3FFF, 0x3F00, 0x3F1F, this, "Mirrored Palettes"));

            // Mirrors
            this.Add(new MirroredMemory(0x4000, 0xFFFF, 0x0000, 0x3FFF, this, "Mirrored PPU"));
        }
    }
}
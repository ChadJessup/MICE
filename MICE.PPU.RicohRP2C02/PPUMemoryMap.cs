using MICE.Common.Misc;
using MICE.Components.Memory;

namespace MICE.PPU.RicohRP2C02
{
    /// <summary>
    /// A class that represents how the NES PPU's memory is mapped out.
    /// </summary>
    public class PPUMemoryMap : MemoryMapper
    {
        public PPUMemoryMap()
        {
            // http://nesdev.com/NESDoc.pdf - Figure 3-1 PPU memory map

            // Pattern Tables
            this.Add(new VRAM(0x0000, 0x0FFF, "Pattern Table 0"));
            this.Add(new VRAM(0x1000, 0x1FFF, "Pattern Table 1"));

            // Name Tables
            this.Add(new VRAM(0x2000, 0x23BF, "Name Table 0"));
            this.Add(new VRAM(0x23C0, 0x23FF, "Attribute Table 0"));
            this.Add(new VRAM(0x2400, 0x27BF, "Name Table 1"));
            this.Add(new VRAM(0x27C0, 0x27FF, "Attribute Table 1"));
            this.Add(new VRAM(0x2800, 0x2BBF, "Name Table 2"));
            this.Add(new VRAM(0x2BC0, 0x2BFF, "Attribute Table 2"));
            this.Add(new VRAM(0x2C00, 0x2FBF, "Name Table 3"));
            this.Add(new VRAM(0x2FC0, 0x2FFF, "Attribute Table 3"));

            this.Add(new MirroredMemory(0X3000, 0x3EFF, 0x2000, 0x2FFF, "Mirrored Name Tables"));

            // Palettes
            this.Add(new VRAM(0x3F00, 0x3F0F, "Image Palette"));
            this.Add(new VRAM(0x3F10, 0x3F1F, "Sprite Palette"));

            this.Add(new MirroredMemory(0x3F20, 0x3FFF, 0x3F00, 0x3FFF, "Mirrored Palettes"));

            // Mirrors
            this.Add(new MirroredMemory(0x4000, 0xFFFF, 0x0000, 0x3FFF, "Mirrored PPU"));
        }
    }
}
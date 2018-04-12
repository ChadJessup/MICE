using MICE.Components.Memory;
using System;

namespace MICE.PPU.RicohRP2C02.Components
{
    // https://wiki.nesdev.com/w/index.php/PPU_palettes
    public class Palette : VRAM
    {
        public Palette(int lowerIndex, int upperIndex, string name, Memory<byte> memory = default)
            : base(lowerIndex, upperIndex, name, memory)
        {
        }

        public byte GetColor(int index) => this.Memory.Span[index];
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using MICE.PPU.RicohRP2C02.Components;

namespace MICE.PPU.RicohRP2C02.Handlers
{
    public class PaletteHandler
    {
        private List<Palette> palettes = new List<Palette>();
        private Palette BackgroundPalette0;

        public PaletteHandler(PPUMemoryMap memoryMap)
        {
            var memorySegments = memoryMap.GetSegmentsInRange(0x3F00, 0x3FFF);

            this.BackgroundPalette0 = memoryMap.GetMemorySegment<Palette>("Background palette 0");
            this.palettes.Add(this.BackgroundPalette0);

            this.palettes.Add(memoryMap.GetMemorySegment<Palette>("Background palette 1"));
            this.palettes.Add(memoryMap.GetMemorySegment<Palette>("Background palette 2"));
            this.palettes.Add(memoryMap.GetMemorySegment<Palette>("Background palette 3"));

            this.palettes.Add(memoryMap.GetMemorySegment<Palette>("Sprite palette 0"));
            this.palettes.Add(memoryMap.GetMemorySegment<Palette>("Sprite palette 1"));
            this.palettes.Add(memoryMap.GetMemorySegment<Palette>("Sprite palette 2"));
            this.palettes.Add(memoryMap.GetMemorySegment<Palette>("Sprite palette 3"));
        }

        public byte GetBackgroundColor(int paletteId, byte colorIndex)
        {
            var address = (ushort)(0x3f00 + 4 * paletteId + colorIndex);

            if (colorIndex == 0)
            {
                return this.BackgroundPalette0.GetColor(0);
            }

            var palette = this.palettes.First(p => p.IsIndexInRange(address));

            return palette.GetColor(colorIndex % 4);
        }
    }
}

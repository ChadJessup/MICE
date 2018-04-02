using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;
using System.Collections.Generic;
using System.Linq;

namespace MICE.PPU.RicohRP2C02.Handlers
{
    public class PaletteHandler
    {
        private List<IMemorySegment> palettes = new List<IMemorySegment>();
        private Palette BackgroundPalette0;

        public PaletteHandler(PPUMemoryMap memoryMap)
        {
            var memorySegments = memoryMap.GetSegmentsInRange(0x3F00, 0x3FFF);

            this.BackgroundPalette0 = memoryMap.GetMemorySegment<Palette>("Background palette 0");
            this.palettes.AddRange(memoryMap.GetSegmentsInRange(0x3F00, 0x3F1F));
        }

        public byte GetBackgroundColor(int paletteId, byte colorIndex)
        {
            var address = (ushort)(0x3f00 + 4 * paletteId + colorIndex);

            if (colorIndex == 0)
            {
                return this.BackgroundPalette0.GetColor(0);
            }

            var palette = this.palettes.First(p => p.IsIndexInRange(address));
            return palette.ReadByte(address);
        }

        public byte GetSpriteColor(Sprite sprite)
        {
            if (sprite.ColorIndex == 0)
            {
                return 0;
            }
            sprite.PaletteAddress = (ushort)(0x3f10 + 4 * sprite.PaletteNumber + sprite.ColorIndex);

            var palette = this.palettes.First(p => p.IsIndexInRange(sprite.PaletteAddress));
            return palette.ReadByte(sprite.PaletteAddress);
        }
    }
}

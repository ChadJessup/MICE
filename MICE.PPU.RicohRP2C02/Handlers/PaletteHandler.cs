using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;
using Ninject;
using System.Collections.Generic;
using System.Linq;

namespace MICE.PPU.RicohRP2C02.Handlers
{
    public class PaletteHandler
    {
        private List<IMemorySegment> palettes = new List<IMemorySegment>();
        private Palette BackgroundPalette0;
        private readonly PPUMemoryMap memoryMap;

        public PaletteHandler([Named("PPU")] IMemoryMap memoryMap)
        {
            this.memoryMap = (PPUMemoryMap)memoryMap;

            this.BackgroundPalette0 = this.memoryMap.GetMemorySegment<Palette>("Background palette 0");
            this.palettes.AddRange(this.memoryMap.GetSegmentsInRange(0x3F00, 0x3F1F));
        }

        public byte UniversalBackgroundColor => this.BackgroundPalette0.GetColor(0);

        public byte GetBackgroundColor(int paletteId, byte colorIndex)
        {
            var address = (ushort)(0x3f00 + 4 * paletteId + colorIndex);

            if (colorIndex == 0)
            {
                this.memoryMap.ReadBuffer = this.UniversalBackgroundColor;
            }
            else
            {
                //var palette = this.palettes.First(p => p.IsIndexInRange(address));
                this.memoryMap.ReadBuffer = this.memoryMap.ReadByte(address);
            }

            return this.memoryMap.ReadBuffer;
        }

        public byte GetSpriteColor(Sprite sprite)
        {
            if (sprite.ColorIndex == 0)
            {
                this.memoryMap.ReadBuffer = this.UniversalBackgroundColor;
            }
            else
            {
                sprite.PaletteAddress = (ushort)(0x3f10 + 4 * sprite.PaletteNumber + sprite.ColorIndex);

               // var palette = this.palettes.First(p => p.IsIndexInRange(sprite.PaletteAddress));
                this.memoryMap.ReadBuffer = this.memoryMap.ReadByte(sprite.PaletteAddress); //palette.ReadByte(sprite.PaletteAddress);
            }

            return this.memoryMap.ReadBuffer;
        }
    }
}

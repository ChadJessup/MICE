using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MICE.PPU.RicohRP2C02.Handlers
{
    public class PaletteHandler
    {
        private List<IMemorySegment> palettes = new List<IMemorySegment>();
        private Palette backgroundPalette0;
        private Palette backgroundPalette1;
        private Palette backgroundPalette2;
        private Palette backgroundPalette3;

        private Palette spritePalette0;
        private Palette spritePalette1;
        private Palette spritePalette2;
        private Palette spritePalette3;

        private readonly PPURawMemoryMap memoryMap;

        public PaletteHandler([Named("PPU")] IMemoryMap memoryMap)
        {
            this.memoryMap = (PPURawMemoryMap)memoryMap;

            var memorySegments = this.memoryMap.GetSegmentsInRange(0x3F00, 0x3FFF);

            this.backgroundPalette0 = this.memoryMap.GetMemorySegment<Palette>("Background palette 0");
            this.backgroundPalette1 = this.memoryMap.GetMemorySegment<Palette>("Background palette 1");
            this.backgroundPalette2 = this.memoryMap.GetMemorySegment<Palette>("Background palette 2");
            this.backgroundPalette3 = this.memoryMap.GetMemorySegment<Palette>("Background palette 3");

            this.spritePalette0 = this.memoryMap.GetMemorySegment<Palette>("Sprite palette 0");
            this.spritePalette1 = this.memoryMap.GetMemorySegment<Palette>("Sprite palette 1");
            this.spritePalette2 = this.memoryMap.GetMemorySegment<Palette>("Sprite palette 2");
            this.spritePalette3 = this.memoryMap.GetMemorySegment<Palette>("Sprite palette 3");
        }

        public byte UniversalBackgroundColor => this.backgroundPalette0.GetColor(0);

        public byte GetBackgroundColor(int paletteId, byte colorIndex)
        {
            var address = (ushort)(0x3f00 + 4 * paletteId + colorIndex);

            this.memoryMap.ReadBuffer = colorIndex == 0
             ? this.UniversalBackgroundColor
             : this.GetColorFromAddress(address);

            return this.memoryMap.ReadBuffer;
        }

        public byte GetSpriteColor(Sprite sprite)
        {
            sprite.PaletteAddress = (ushort)(0x3f10 + 4 * sprite.PaletteNumber + sprite.ColorIndex);

            this.memoryMap.ReadBuffer = sprite.ColorIndex == 0
             ? this.UniversalBackgroundColor
             : this.GetColorFromAddress(sprite.PaletteAddress);

            return this.memoryMap.ReadBuffer;
        }

        private byte GetColorFromAddress(ushort address)
        {
            Palette currentPalette;

            if (this.backgroundPalette0.IsIndexInRange(address))
            {
                currentPalette = this.backgroundPalette0;
            }
            else if (this.backgroundPalette1.IsIndexInRange(address))
            {
                currentPalette = this.backgroundPalette1;
            }
            else if (this.backgroundPalette2.IsIndexInRange(address))
            {
                currentPalette = this.backgroundPalette2;
            }
            else if (this.backgroundPalette3.IsIndexInRange(address))
            {
                currentPalette = this.backgroundPalette3;
            }
            else if (this.spritePalette0.IsIndexInRange(address))
            {
                currentPalette = this.spritePalette0;
            }
            else if (this.spritePalette1.IsIndexInRange(address))
            {
                currentPalette = this.spritePalette1;
            }
            else if (this.spritePalette2.IsIndexInRange(address))
            {
                currentPalette = this.spritePalette2;
            }
            else if (this.spritePalette3.IsIndexInRange(address))
            {
                currentPalette = this.spritePalette3;
            }
            else
            {
                throw new InvalidOperationException();
            }

            return currentPalette.ReadByte(address);
        }
    }
}

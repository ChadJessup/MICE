using System;
using MICE.PPU.RicohRP2C02.Components;

namespace MICE.PPU.RicohRP2C02.Handlers
{
    public class PaletteHandler
    {
        public PaletteHandler(PPUMemoryMap memoryMap)
        {
            var memorySegments = memoryMap.GetSegmentsInRange(0x3F00, 0x3FFF);
            this.UniversalBackgroundColor = memoryMap.GetMemorySegment<Palette>("Universal background color");
            this.BackgroundPalette0 = memoryMap.GetMemorySegment<Palette>("Background palette 0");
            this.BackgroundPalette1 = memoryMap.GetMemorySegment<Palette>("Background palette 1");
            this.BackgroundPalette2 = memoryMap.GetMemorySegment<Palette>("Background palette 2");
            this.BackgroundPalette3 = memoryMap.GetMemorySegment<Palette>("Background palette 3");

            this.SpritePalette0 = memoryMap.GetMemorySegment<Palette>("Sprite palette 0");
            this.SpritePalette1 = memoryMap.GetMemorySegment<Palette>("Sprite palette 1");
            this.SpritePalette2 = memoryMap.GetMemorySegment<Palette>("Sprite palette 2");
            this.SpritePalette3 = memoryMap.GetMemorySegment<Palette>("Sprite palette 3");

            /*
            MirroredMemory(0x3F10, 0x3F10, 0x3F00, 0x3F00, this, "Mirrored Universal background color byte"));
            MirroredMemory(0x3F14, 0x3F14, 0x3F04, 0x3F04, this, "Mirrored Background palette 0 byte"));
            MirroredMemory(0x3F18, 0x3F18, 0x3F08, 0x3F08, this, "Mirrored Background palette 1 byte"));
            MirroredMemory(0x3F1C, 0x3F1C, 0x3F0C, 0x3F0C, this, "Mirrored Background palette 2 byte"));
            MirroredMemory(0x3F20, 0x3FFF, 0x3F00, 0x3F1F, this, "Mirrored Palettes"));
            */
        }

        public Palette UniversalBackgroundColor { get; private set; }
        public Palette BackgroundPalette0 { get; private set; }
        public Palette BackgroundPalette1 { get; private set; }
        public Palette BackgroundPalette2 { get; private set; }
        public Palette BackgroundPalette3 { get; private set; }

        public Palette SpritePalette0 { get; private set; }
        public Palette SpritePalette1 { get; private set; }
        public Palette SpritePalette2 { get; private set; }
        public Palette SpritePalette3 { get; private set; }

        public byte GetBackgroundColor(byte paletteId, byte colorIndex)
        {
            switch (paletteId)
            {
                case 0:
                    return this.UniversalBackgroundColor.GetColor(colorIndex % 4);
                case 1:
                    return this.BackgroundPalette0.GetColor(colorIndex % 4);
                case 2:
                    return this.BackgroundPalette1.GetColor(colorIndex % 4);
                case 3:
                    return this.BackgroundPalette2.GetColor(colorIndex % 4);
                case 4:
                    return this.BackgroundPalette3.GetColor(colorIndex % 4);
                default:
                    throw new InvalidOperationException("Unknown paletteId requested: " + paletteId);
            }
        }
    }
}

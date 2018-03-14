using System;

namespace MICE.PPU.RicohRP2C02.Components
{
    // https://wiki.nesdev.com/w/index.php/PPU_OAM
    public class OAM
    {
        private static class Constants
        {
            public const int SizeOfSprite = 4;
        }

        public OAM(int size)
        {
            this.Data = new byte[size];
            this.MaxSpritesCount = size / 4;

            if (this.MaxSpritesCount <= 0)
            {
                throw new InvalidOperationException("OAM size is not big enough to contain any sprites.");
            }

            if (size % Constants.SizeOfSprite != 0)
            {
                throw new InvalidOperationException("OAM initialized with size that isn't a multiple of a sprite size (4)");
            }
        }

        public int MaxSpritesCount { get; private set; } = 0;
        public int CurrentSpriteCount { get; private set; } = 0;
        public byte[] Data { get; private set; }

        public byte this[int index]
        {
            get => this.Data[index];
            set => this.Data[index] = value;
        }
    }
}

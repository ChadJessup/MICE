using MICE.Common.Helpers;

namespace MICE.PPU.RicohRP2C02.Components
{
    // https://wiki.nesdev.com/w/index.php/PPU_OAM
    public class Sprite
    {
        private byte yPosition;
        private byte tileIndex;
        private byte attributes;
        private byte xPosition;

        public Sprite(int spriteIndex, byte yPosition, byte tileIndex, byte attributes, byte xPosition)
        {
            this.SpriteIndex = spriteIndex;

            this.yPosition = yPosition;
            this.tileIndex = tileIndex;
            this.attributes = attributes;
            this.xPosition = xPosition;
        }

        public int SpriteIndex { get; private set; }
        public int TileIndex => this.tileIndex;
        public (int X, int Y) Position => (X: this.xPosition, Y: this.yPosition);

        public ushort PaletteAddress { get; set; }
        public ushort TileAddress { get; set; }

        public byte PaletteNumber => (byte)(this.attributes & 3);

        public bool IsBehindBackground
        {
            get => this.attributes.GetBit(5);
            set => this.attributes.SetBit(5, value);
        }

        public bool IsFlippedHorizontally
        {
            get => this.attributes.GetBit(6);
            set => this.attributes.SetBit(6, value);
        }

        public bool IsFlippedVertically
        {
            get => this.attributes.GetBit(7);
            set => this.attributes.SetBit(7, value);
        }

        public bool IsSpriteZero => this.SpriteIndex == 0;
        public bool IsVisible { get; set; }
    }
}

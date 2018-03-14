using MICE.Common.Helpers;

namespace MICE.PPU.RicohRP2C02.Components
{
    // https://wiki.nesdev.com/w/index.php/PPU_OAM
    public class Sprite
    {
        private byte byte0;
        private byte byte1;
        private byte byte2;
        private byte byte3;

        public int PositionY { get; private set; }
        public int PositionX { get; private set; }

        public int Index { get; private set; }

        public bool IsSpriteZero { get; set; }

        public bool IsBehindBackground
        {
            get => this.byte2.GetBit(5);
            set => this.byte2.SetBit(5, value);
        }

        public bool IsFlippedHorizontally
        {
            get => this.byte2.GetBit(6);
            set => this.byte2.SetBit(6, value);
        }

        public bool IsFlippedVertically
        {
            get => this.byte2.GetBit(7);
            set => this.byte2.SetBit(7, value);
        }
    }
}

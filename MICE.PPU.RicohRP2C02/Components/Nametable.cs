using MICE.Components.Memory;
using System;

namespace MICE.PPU.RicohRP2C02.Components
{
    public class Nametable : VRAM
    {
        private static class Constants
        {
            public const int NumberOfRows = 30;
            public const int NumberOfColumns = 32;
        }

        public Nametable(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
            this.AttributeTable = new AttributeTable(new ArraySegment<byte>(this.Data, this.Data.Length - 64, this.Data.Length - (this.Data.Length - 64)));
        }

        public AttributeTable AttributeTable { get; private set; }

        public byte GetPatternTableOffset(int x, int y) => this.Data[(x % Constants.NumberOfRows) + (y % Constants.NumberOfColumns)];

        public void GetPalette(int x, int y)
        {
            var attribute = this.AttributeTable.GetAttribute(x, y);
        }
    }
}

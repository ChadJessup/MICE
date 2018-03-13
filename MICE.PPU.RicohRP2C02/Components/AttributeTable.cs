using System;

namespace MICE.PPU.RicohRP2C02.Components
{
    public struct AttributeTable
    {
        private readonly ArraySegment<byte> data;
        public AttributeTable(ArraySegment<byte> bytes)
        {
            this.data = bytes;
        }

        // TODO: this is broke - I know, not sure how I want to do this right now.
        public byte GetAttribute(int x, int y) => this.data.Array[this.data.Offset + x + y];
    }
}

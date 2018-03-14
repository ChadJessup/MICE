using System;

namespace MICE.PPU.RicohRP2C02.Components
{
    public struct AttributeTable
    {
        private static class Constants
        {
            public const int AttributeTableWrap = 8;
        }

        public AttributeTable(ArraySegment<byte> bytes)
        {
            this.Data = bytes;
        }

        public ArraySegment<byte> Data { get; private set; }
        public NametableAttribute GetAttribute(int x, int y)
        {
            var attributeX = x / 2;
            var attributeY = y / 2;
            return new NametableAttribute(this.Data.Array, (short)(this.Data.Offset + (attributeX * attributeY - 2)));
        }
    }
}

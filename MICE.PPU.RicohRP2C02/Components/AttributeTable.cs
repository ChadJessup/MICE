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
            var attributeX = x / 4;
            var attributeY = y / 4;

            var offset = (attributeY * 8) + attributeX;
            return new NametableAttribute(this.Data.Array, (short)(this.Data.Offset + offset));
        }
    }
}

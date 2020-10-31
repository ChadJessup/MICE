using System;

namespace MICE.PPU.RicohRP2C02.Components
{
    public struct AttributeTable
    {
        private static class Constants
        {
            public const int AttributeTableWrap = 8;
        }

        public AttributeTable(ArraySegment<byte> bytes) => this.Data = bytes;

        public ArraySegment<byte> Data { get; private set; }
        public NametableAttribute GetAttribute(Tile tile, ushort v)
        {
            var offset = (short)((v & 0x0C00) | ((v >> 4) & 0x38) | ((v >> 2) & 0x07));
            return new NametableAttribute(tile.AttributeByte, (ushort)(this.Data.Offset + offset));
        }
    }
}

using MICE.Components.Memory;
using MICE.PPU.RicohRP2C02.Handlers;
using System;

namespace MICE.PPU.RicohRP2C02.Components
{
    public class Nametable : External
    {
        private static class Constants
        {
            public const int NumberOfRows = 30;
            public const int NumberOfColumns = 32;
        }

        public Nametable(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
            //this.AttributeTable = new AttributeTable(new ArraySegment<byte>(this.Data, this.Data.Length - 64, this.Data.Length - (this.Data.Length - 64)));
        }

        public byte[] Data { get; set; } = new byte[0x2000];

        public AttributeTable AttributeTable { get; private set; }

        public Tile GetTileFromPixel(int x, int y, ScrollHandler scrollHandler, Tile workingTile, int bgOffset, PPUInternalRegisters registers)
        {
            var scrolledX = scrollHandler.vCoarseXScroll - 1;
            var scrolledY = scrollHandler.vCoarseYScroll;

            var tile = workingTile;

            tile.PPUAddress = (ushort)(0x2000 + x + (y * Constants.NumberOfColumns));
//            tile.Nametable = this.WhichNametableAmI();
            tile.Location = (x, y);
//            tile.TileIndex = tileByte; //this.Data[tileX + (tileY * Constants.NumberOfColumns)];
//            tile.TileAddress = (ushort)(registers.v & 0x0fff);

           // var attrib = this.AttributeTable.GetAttribute(tile, registers.v);
          //  tile.AttributeData = attrib.RawByte;
          //  tile.AttributeAddress = attrib.Address;

            var colorIndex = this.GetColorIndex(tile, scrollHandler);

            byte paletteId = 0;
            if (colorIndex != 0)
            {
                paletteId = (byte)((tile.AttributeData >> 0) & 3);
            }

            tile.PaletteAddress = (ushort)(0x3f00 + 4 * paletteId + colorIndex);

            return tile;
        }

        private int WhichNametableAmI()
        {
            switch (this.Range.Min)
            {
                case 0x2000:
                    return 0;
                case 0x2400:
                    return 1;
                case 0x2800:
                    return 2;
                case 0x2C00:
                    return 3;
                default:
                    throw new InvalidOperationException($"Unknown nametable starting address: {this.Range.Min}");
            }
        }

        private byte GetColorIndex(Tile tile, ScrollHandler scrollHandler)
        {
            // To get a color index, we slice down the data even further to grab specific pixel details
            // and pull the color details from the ROMs pattern table(s).

            byte lowBits = tile.lowTileByte;
            byte highBits = tile.highTileByte;

            byte lowBit = (byte)((lowBits >> (7 - scrollHandler.FineXScroll)) & 1);
            byte highBit = (byte)((highBits >> (7 - scrollHandler.FineXScroll)) & 1);

            return (byte)(lowBit + highBit * 2);
        }

        private byte GetPatternTableOffset(int x, int y)
        {
            // Nametables are a byte per 8 pixels, so convert the pixel's coordinates to its byte index
            var byteIndexX = x / 8;
            var byteIndexY = y / 8;

            // Index into nametable memory, multiplying y by columns so it wraps down.
            return this.ReadByte(byteIndexX + (byteIndexY * Constants.NumberOfColumns));
        }

        //private NametableAttribute GetPalette(int x, int y) => this.AttributeTable.GetAttribute(x, y);
    }
}

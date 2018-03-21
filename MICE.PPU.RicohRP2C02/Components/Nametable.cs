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

        public Tile GetTileFromPixel(ScrollHandler scrollHandler, int bgOffset, byte[] chrBank, PPUInternalRegisters registers)
        {
            var scrolledX = scrollHandler.vCoarseXScroll - 1;
            var scrolledY = scrollHandler.vCoarseYScroll;

            var tileX = scrolledX / 8;
            var tileY = scrolledY / 8;
            var sliverY = scrolledX % 8;
            var sliverX = scrolledY % 8;

            var tile = new Tile();

            tile.PPUAddress = (short)(0x2000 + tileX + (tileY * Constants.NumberOfColumns));
            tile.Nametable = this.WhichNametableAmI();
            tile.Location = (tileX, tileY);
            tile.TileIndex = this.Data[tileX + (tileY * Constants.NumberOfColumns)];
            tile.TileAddress = (short)(registers.v & 0x0fff);

            var attrib = this.AttributeTable.GetAttribute(registers.v);
            tile.AttributeData = attrib.RawByte;
            tile.AttributeAddress = attrib.Address;

            var colorIndex = this.GetColorIndex(scrolledX, scrolledY, tile, chrBank);

            byte paletteId = 0;
            if (colorIndex != 0)
            {
                paletteId = (byte)((tile.AttributeData >> 0) & 3);
            }

            tile.PaletteAddress = (short)(0x3f00 + 4 * paletteId + colorIndex);

            return tile;
        }

        private int WhichNametableAmI()
        {
            switch (this.LowerIndex)
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
                    throw new InvalidOperationException($"Unknown nametable starting address: {this.LowerIndex}");
            }
        }

        private byte GetColorIndex(int x, int y, Tile tile, byte[] patterns)
        {
            // To get a color index, we slice down the data even further to grab specific pixel details
            // and pull the color details from the ROMs pattern table(s).
            // var testAddress = patterns[tile.TileAddress];
            //return testAddress;
            //var testAddress2 = patterns[tile.TileAddress + 8];

            var tiledX = x % 8;
            var tiledY = y % 8;

            var lowBitsOffset = (ushort)(tile.TileAddress + tiledY);
            var highBitsOffset = (ushort)(lowBitsOffset + 8);

            byte lowBits = patterns[lowBitsOffset];
            byte highBits = patterns[highBitsOffset];

            byte lowBit = (byte)((lowBits >> (7 - tiledX)) & 1);
            byte highBit = (byte)((highBits >> (7 - tiledX)) & 1);

            return (byte)(lowBit + highBit * 2);
        }

        private byte GetPatternTableOffset(int x, int y)
        {
            // Nametables are a byte per 8 pixels, so convert the pixel's coordinates to its byte index
            var byteIndexX = x / 8;
            var byteIndexY = y / 8;

            // Index into nametable memory, multiplying y by columns so it wraps down.
            return this.Data[byteIndexX + (byteIndexY * Constants.NumberOfColumns)];
        }

        //private NametableAttribute GetPalette(int x, int y) => this.AttributeTable.GetAttribute(x, y);
    }
}

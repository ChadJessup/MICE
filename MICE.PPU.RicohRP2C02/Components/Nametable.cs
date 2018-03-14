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

        public Tile GetTileFromPixel(int screenX, int screenY, int bgOffset, byte[] chrBank)
        { 
            var tileX = screenX / 8;
            var tileY = screenY / 8;
            var sliverY = screenX % 8;
            var sliverX = screenY % 8;

            var tile = new Tile();

            tile.PPUAddress = (short)(0x2000 + tileX + (tileY * Constants.NumberOfColumns));
            tile.Nametable = this.WhichNametableAmI();
            tile.Location = (tileX, tileY);
            tile.TileIndex = this.Data[tileX + (tileY * Constants.NumberOfColumns)];
            tile.TileAddress = (short)(bgOffset + tile.TileIndex * 16);

            var attrib = this.AttributeTable.GetAttribute(tileX, tileY);
            tile.AttributeData = attrib.RawByte;
            tile.AttributeAddress = (short)(this.LowerIndex + attrib.Address);

            var colorIndex = this.GetColorIndex(tile, chrBank);

            // (0) = palette number
            tile.PaletteAddress = (short)(0x3f00);// + 4 * (0) + colorIndex);

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

        private byte GetColorIndex(Tile tile, byte[] patterns)
        {
            // To get a color index, we slice down the data even further to grab specific pixel details
            // and pull the color details from the ROMs pattern table(s).
            return patterns[tile.TileAddress];

            var tiledY = tile.Location.y % 8;
            var tiledX = tile.Location.x % 8;

            // This borrowed from DotNES project...
            byte lowBits = patterns[(ushort)(tile.TileAddress + tile.TileIndex* 16 + tiledY)];
            byte highBits = patterns[(ushort)(tile.TileAddress + tile.TileIndex * 16 + tiledY + 8)];

            byte lowBit = (byte)((lowBits >> (7 - tiledX)) & 1);
            byte highBit = (byte)((highBits >> (7 - tiledX)) & 1);

            //return (byte)(lowBit + highBit * 2);
        }

        private byte GetPatternTableOffset(int x, int y)
        {
            // Nametables are a byte per 8 pixels, so convert the pixel's coordinates to its byte index
            var byteIndexX = x / 8;
            var byteIndexY = y / 8;

            // Index into nametable memory, multiplying y by columns so it wraps down.
            return this.Data[byteIndexX + (byteIndexY * Constants.NumberOfColumns)];
        }

        private NametableAttribute GetPalette(int x, int y) => this.AttributeTable.GetAttribute(x, y);
    }
}

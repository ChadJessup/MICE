using System;
using MICE.Common.Helpers;
using MICE.Common.Interfaces;

namespace MICE.PPU.RicohRP2C02.Components
{
    // https://wiki.nesdev.com/w/index.php/PPU_OAM
    public class Sprite : IEquatable<Sprite>
    {
        private byte yPosition;
        private byte attributes;
        private byte xPosition;

        /// <summary>
        /// Initializes a new instance of the Sprite class.
        /// </summary>
        /// <param name="spriteIndex">The OAM index of this sprite.</param>
        /// <param name="isSmallSprite">Whether or not this sprite is small (8x8) or large (8x16).</param>
        /// <param name="isSpritePattern1000">Offset of the sprite in memory.</param>
        /// <param name="yPosition">The screen based Y coordinate byte.</param>
        /// <param name="tileIndex">The tile index byte, which maps into character/tile memory.</param>
        /// <param name="attribute">The attribute byte of this sprite.</param>
        /// <param name="xPosition">The screen based X coordinate byte.</param>
        public Sprite(int spriteIndex, bool isSmallSprite, bool isSpritePattern1000, byte yPosition, byte tileIndex, byte attribute, byte xPosition)
        {
            this.SpriteIndex = spriteIndex;
            this.yPosition = yPosition;
            this.TileIndex = tileIndex;
            this.attributes = attribute;
            this.xPosition = xPosition;

            this.IsSmallSprite = isSmallSprite;
            this.TileAddress = (ushort)(isSpritePattern1000 ? 0x1000 : 0x0000 + this.TileIndex * 16);
        }

        /// <summary>
        /// Gets the OAM sprite index of this sprite.
        /// Note: This might change each frame.
        /// </summary>
        public int SpriteIndex { get; private set; }

        /// <summary>
        /// Gets the index into OAM memory.
        /// Note: This should be static across frames.
        /// </summary>
        public int TileIndex { get; }

        /// <summary>
        /// Gets the full address of this Tile in OAM memory.
        /// Note: This should be static across frames.
        /// </summary>
        public ushort TileAddress { get; }

        /// <summary>
        /// Gets the screen position of this Sprite;
        /// Note: This might change each frame.
        /// </summary>
        public (int X, int Y) Position => (X: this.xPosition, Y: this.yPosition);

        /// <summary>
        /// Gets the palette address of this sprite.
        /// Note: This might change each frame.
        /// </summary>
        public ushort PaletteAddress { get; set; }

        /// <summary>
        /// Gets the palette number of this sprite.
        /// Note: This might change each frame.
        /// </summary>
        public byte PaletteNumber => (byte)(this.attributes & 3);

        /// <summary>
        /// Gets a value indicating if this sprite should be behind the background.
        /// Note: This might change each frame.
        /// </summary>
        public bool IsBehindBackground => this.attributes.GetBit(5);

        /// <summary>
        /// Gets a value indicating if this sprite should be flipped horizontally.
        /// Note: This might change each frame.
        /// </summary>
        public bool IsFlippedHorizontally => this.attributes.GetBit(6);

        /// <summary>
        /// Gets a value indicating if this sprite should be flipped vertically.
        /// Note: This might change each frame.
        /// </summary>
        public bool IsFlippedVertically => this.attributes.GetBit(7);

        /// <summary>
        /// Gets whether or not this sprite is considered 'sprite zero'.
        /// Note: This might change each frame.
        /// </summary>
        public bool IsSpriteZero => this.SpriteIndex == 0;

        public bool IsSmallSprite { get; private set; }

        public bool IsOnScanline(int scanline, int offset) => scanline >= this.Position.Y && scanline < this.Position.Y + offset;

        public int Width { get; } = 8;
        public int Height => this.IsSmallSprite ? 8 : 16;

        /// <summary>
        /// Gets a value indicating whether or not this Sprite is visible for a particular X,Y on the screen.
        /// </summary>
        /// <param name="x">The Screen-based X coordinate.</param>
        /// <param name="y">The Screen-based Y coordinate.</param>
        /// <returns>True if visible for a particular Screen based X,Y coordinate.</returns>
        public bool IsVisible(int x, int y)
        {
            if (this.Position.Y == 0 || this.Position.Y >= 240)
            {
                return false;
            }

            if (x - this.Position.X >= 8 || x < this.Position.X)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the final pixel for a particular X,Y coordinate on the screen.
        /// </summary>
        /// <param name="x">The screen-based X coordinate.</param>
        /// <param name="y">The screen-based Y coordinate.</param>
        /// <param name="characterBank">The character bank that would contain pixel details.</param>
        /// <param name="ppuMemoryMap">The PPU memory bank that would contain pixel details.</param>
        /// <returns>The final colored pixel to the muxed to the screen.</returns>
        public byte GetFinalPixel(int x, int y, byte[] characterBank, IMemoryMap ppuMemoryMap)
        {
            int tx = x - this.Position.X;
            int ty = y - this.Position.Y;

            int screenX = this.IsFlippedHorizontally ? 7 - tx : tx;
            int screenY = this.IsFlippedVertically ? 7 - ty : ty;

            byte colorIndex = this.GetColorIndex(screenX, (ushort)(this.TileAddress + screenY), characterBank);

            if (colorIndex == 0)
            {
                return 0;
            }

            this.PaletteAddress = (ushort)(0x3f10 + 4 * this.PaletteNumber + colorIndex);

            return ppuMemoryMap.ReadByte(this.PaletteAddress);
        }

        private byte GetColorIndex(int i, ushort address, byte[] patterns)
        {
            var lowBitsOffset = address;
            var highBitsOffset = (ushort)(lowBitsOffset + 8);

            byte lowBits = patterns[lowBitsOffset];
            byte highBits = patterns[highBitsOffset];

            byte lowBit = (byte)((lowBits >> (7 - i)) & 1);
            byte highBit = (byte)((highBits >> (7 - i)) & 1);

            return (byte)(lowBit + highBit * 2);
        }

        // Certain sprite details are changed per frame, so we don't want the mutable data as part of the comparison.
        // The TileIndex and TileAddress are the only values that are not mutable.
        public static bool operator ==(Sprite a, Sprite b) => a.TileIndex == b.TileIndex && a.TileAddress == b.TileAddress;
        public static bool operator !=(Sprite a, Sprite b) => !(a.TileIndex == b.TileIndex && a.TileAddress == b.TileAddress);
        public override bool Equals(object obj) => this.TileIndex == (obj as Sprite)?.TileIndex && this.TileAddress == (obj as Sprite)?.TileAddress;
        public bool Equals(Sprite other) => this.TileIndex == other?.TileIndex && this.TileAddress == other?.TileAddress;
        public override int GetHashCode() => this.TileIndex.GetHashCode() + this.TileAddress.GetHashCode();
    }
}

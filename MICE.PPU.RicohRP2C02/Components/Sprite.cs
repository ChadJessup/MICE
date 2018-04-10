using MICE.Common.Helpers;
using MICE.Common.Interfaces;
using System;

namespace MICE.PPU.RicohRP2C02.Components
{
    // https://wiki.nesdev.com/w/index.php/PPU_OAM
    public class Sprite : IEquatable<Sprite>
    {
        private byte spriteY;
        private byte spriteX;
        private byte attributes;

        private byte lowBits;
        private byte highBits;

        /// <summary>
        /// Initializes a new instance of the Sprite class.
        /// </summary>
        /// <param name="spriteIndex">The OAM index of this sprite.</param>
        /// <param name="isSmallSprite">Whether or not this sprite is small (8x8) or large (8x16).</param>
        /// <param name="isSpritePattern1000">Offset of the sprite in memory.</param>
        /// <param name="oam">The OAM Memory.</param>
        public Sprite(byte spriteIndex, bool isSmallSprite, bool isSpritePattern1000, OAM oam)
        {
            var indexMultiple = spriteIndex * 4;

            this.spriteY = oam[indexMultiple + 0];
            this.TileIndex = oam[indexMultiple + 1];
            this.attributes = oam[indexMultiple + 2];
            this.spriteX = oam[indexMultiple + 3];

            this.IsSmallSprite = isSmallSprite;

            this.SpriteIndex = spriteIndex;

            this.TileAddress = (ushort)((isSpritePattern1000 ? 0x1000 : 0x0000) + this.TileIndex * 16);
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
        public (int X, int Y) Position => (X: this.spriteX, Y: this.spriteY);

        /// <summary>
        /// Gets the palette address of this sprite.
        /// Note: This might change each frame.
        /// </summary>
        public ushort PaletteAddress { get; set; }

        /// <summary>
        /// Gets the palette number of this sprite.
        /// Note: This might change each frame.
        /// </summary>
        public byte PaletteNumber => (byte)(this.attributes & 0b00000011);

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

        public bool IsTransparentPixel => this.ColorIndex % 4 == 0;

        public bool IsOnScanline(int scanline, int offset) => scanline >= this.Position.Y && scanline < this.Position.Y + offset;

        public int Width { get; } = 8;
        public int Height => this.IsSmallSprite ? 8 : 16;

        public byte ColorIndex { get; private set; }

        public void GatherPattern(int spriteRow, IMemoryMap memoryMap)
        {
            spriteRow = this.IsFlippedVertically
                ? 7 - spriteRow
                : spriteRow;

            this.lowBits = memoryMap.ReadByte(this.TileAddress + spriteRow);
            this.highBits = memoryMap.ReadByte(this.TileAddress + spriteRow + 8);
        }

        /// <summary>
        /// Gets a value indicating whether or not this Sprite is visible for a particular X cycle on a scanline.
        /// Note: Y coordinate (scanline number) was already verified during sprite evaluation stage.
        /// </summary>
        /// <param name="x">The Screen-based X coordinate.</param>
        /// <returns>True if visible for a particular Screen based X coordinate.</returns>
        public bool IsVisible(int x) => !(x - this.Position.X >= 8 || x < this.Position.X);

        /// <summary>
        /// Gets the pixel details for a particular X,Y coordinate on the screen.
        /// </summary>
        /// <param name="ppuMemoryMap">The PPU memory bank that would contain pixel details.</param>
        /// <param name="x">The screen-based X coordinate.</param>
        /// <param name="y">The screen-based Y coordinate.</param>
        /// <returns>The pixel details that contain the final palette details that need to be muxed to the screen.</returns>
        public void SetFinalPixel(IMemoryMap ppuMemoryMap, int x, int y)
        {
            int offsetX = x - this.Position.X;
            int screenX = this.IsFlippedHorizontally ? 7 - offsetX : offsetX;

            this.ColorIndex = this.GetColorIndex(screenX);
        }

        private byte GetColorIndex(int x)
        {
            var offset = 7 - x;
            byte lowBit = (byte)((this.lowBits >> offset) & 0b00000001);
            byte highBit = (byte)((this.highBits >> offset) & 0b00000001);

            return (byte)(lowBit + highBit * 2);
        }

        // Certain sprite details are changed per frame, so we don't want the mutable data as part of the comparison.
        // The TileIndex and TileAddress are the only values that are not mutable.
        public static bool operator ==(Sprite a, Sprite b) => a?.TileIndex == b?.TileIndex && a?.TileAddress == b?.TileAddress;
        public static bool operator !=(Sprite a, Sprite b) => !(a?.TileIndex == b?.TileIndex && a?.TileAddress == b?.TileAddress);
        public override bool Equals(object obj) => this.TileIndex == (obj as Sprite)?.TileIndex && this.TileAddress == (obj as Sprite)?.TileAddress;
        public bool Equals(Sprite other) => this.TileIndex == other?.TileIndex && this.TileAddress == other?.TileAddress;
        public override int GetHashCode() => this.TileIndex.GetHashCode() + this.TileAddress.GetHashCode();
    }
}

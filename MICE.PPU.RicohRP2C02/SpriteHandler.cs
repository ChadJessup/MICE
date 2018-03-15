using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;
using System.Collections.Generic;

namespace MICE.PPU.RicohRP2C02
{
    public class SpriteHandler
    {
        private readonly IMemoryMap ppuMemoryMap;
        private readonly IMemoryMap cpuMemoryMap;
        private readonly PPURegisters registers;
        private byte[] spriteIndices = new byte[8];
        private readonly IList<byte[]> chrBanks;

        public SpriteHandler(IMemoryMap ppuMemoryMap, PPURegisters registers, IMemoryMap cpuMemoryMap, IList<byte[]> chrBanks)
        {
            this.chrBanks = chrBanks;
            this.ppuMemoryMap = ppuMemoryMap;
            this.registers = registers;
            this.cpuMemoryMap = cpuMemoryMap;
        }

        public int CurrentScanlineSpriteCount { get; set; }

        public bool IsSmallSprites
        {
            get => !this.registers.PPUCTRL.GetBit(5);
            set => this.registers.PPUCTRL.SetBit(5, !value);
        }

        public bool DrawLeft8SpritePixels
        {
            get => this.registers.PPUMASK.GetBit(2);
            set => this.registers.PPUMASK.SetBit(2, value);
        }

        public bool ShowSprites
        {
            get => this.registers.PPUMASK.GetBit(4);
            set => this.registers.PPUMASK.SetBit(4, value);
        }

        public bool IsSpritePatternTableAddress1000
        {
            get => this.registers.PPUCTRL.GetBit(3);
            set => this.registers.PPUCTRL.SetBit(3, value);
        }

        /// <summary>
        /// Gets or sets a value indicating if there was sprite overflow (more than 8 sprites on scanline).
        /// Note, there was a hardware bug on this, so behavior might be difficult to follow once fully implemented.
        /// </summary>
        public bool WasSpriteOverflow
        {
            get => this.registers.PPUSTATUS.GetBit(5);
            set => this.registers.PPUSTATUS.SetBit(5, value);
        }

        /// <summary>
        /// Gets or sets a value when Sprite 0 was hit.
        /// </summary>
        public bool WasSprite0Hit
        {
            get
            {
                var value = this.registers.PPUSTATUS.GetBit(6);

                if (value)
                {

                }

                return value;
            }
            set => this.registers.PPUSTATUS.SetBit(6, value);
        }

        public void EvaluateSpritesOnScanline(OAM oam, int scanline)
        {
            this.CurrentScanlineSpriteCount = 0;

            int offset = this.IsSmallSprites ? 8 : 16;

            for (byte index = 0; index < 64 && this.CurrentScanlineSpriteCount < 8; index++)
            {
                byte spriteY = (byte)(oam.Data[index * 4 + 0] + 1);

                if (scanline >= spriteY && (scanline < spriteY + offset))
                {
                    this.spriteIndices[this.CurrentScanlineSpriteCount] = (byte)(index * 4);
                    this.CurrentScanlineSpriteCount++;
                }
            }
        }

        public (byte, Sprite) DrawSpritePixel(int x, int y, OAM primaryOAM)
        {
            if (x <= 8 && !this.DrawLeft8SpritePixels)
            {
                return (0, null);
            }

            for (int index = 0; index < this.CurrentScanlineSpriteCount; ++index)
            {
                var sprite = new Sprite();
                sprite.SpriteIndex = index;
                var oamIndex = this.spriteIndices[index];

                sprite.IsSpriteZero = index == 0;

                sprite.Position =
                (
                    X: primaryOAM[oamIndex + 3],
                    Y: primaryOAM[oamIndex + 0]
                );

                sprite.TileIndex = primaryOAM[oamIndex + 1];
                byte attributes = (byte)(primaryOAM[oamIndex + 2] & 0xE3);

                byte paletteNumber = (byte)(attributes & 3);
                sprite.IsBehindBackground = (attributes & 0x20) == 0;
                sprite.IsFlippedHorizontally = (attributes & 0x40) != 0;
                sprite.IsFlippedVertically = (attributes & 0x80) != 0;

                if (sprite.Position.Y == 0 || sprite.Position.Y >= 240)
                    continue;

                if (x - sprite.Position.X >= 8 || x < sprite.Position.X)
                    continue;

                int ti = x - sprite.Position.X;
                int tj = y - sprite.Position.Y;

                int i = sprite.IsFlippedHorizontally ? 7 - ti : ti;
                int j = sprite.IsFlippedVertically ? 7 - tj : tj;

                sprite.TileAddress = (ushort)(this.IsSpritePatternTableAddress1000 ? 0x1000 : 0x0000 + sprite.TileIndex * 16);

                byte colorIndex = this.GetColorIndex(i, (ushort)(sprite.TileAddress + j), this.chrBanks[0]);

                if (colorIndex == 0)
                {
                    return (0, sprite);
                }

                sprite.PaletteAddress = (ushort)(0x3f10 + 4 * paletteNumber + colorIndex);
                var finalPixel = this.ppuMemoryMap.ReadByte(sprite.PaletteAddress);
                return (finalPixel, sprite);
            }

            return (0, null);
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
    }
}
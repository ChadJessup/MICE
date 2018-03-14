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
                    this.spriteIndices[this.CurrentScanlineSpriteCount] = index;
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

            var sprite = new Sprite();

            for (int spriteIndex = 0; spriteIndex < this.CurrentScanlineSpriteCount; ++spriteIndex)
            {
                int oam_index = this.spriteIndices[spriteIndex];
                if(oam_index == 0)
                {
                    sprite.IsSpriteZero = true;
                }

                byte sprite_y = (byte)(primaryOAM[oam_index * 4 + 0] + 1);
                byte pt_index = primaryOAM[oam_index * 4 + 1];
                byte attributes = (byte)(primaryOAM[oam_index * 4 + 2] & 0xE3);
                byte sprite_x = primaryOAM[oam_index * 4 + 3];

                byte palette_number = (byte)(attributes & 3);
                bool inFrontOfBG = (attributes & 0x20) == 0;
                bool flipHorizontal = (attributes & 0x40) != 0;
                bool flipVertical = (attributes & 0x80) != 0;

                if (sprite_y == 0 || sprite_y >= 240)
                    continue;

                if (x - sprite_x >= 8 || x < sprite_x)
                    continue;

                int ti = x - sprite_x;
                int tj = y - sprite_y;

                int i = flipHorizontal ? 7 - ti : ti;
                int j = flipVertical ? 7 - tj : tj;

                ushort sprite_pattern_table_base = (ushort)(this.IsSpritePatternTableAddress1000 ? 0x1000 : 0x0000);

                byte color_index = this.GetColorIndex(i, (ushort)(sprite_pattern_table_base + pt_index * 16 + j), this.chrBanks[0]);

                if (color_index == 0)
                {
                    return (0, null);
                }

                var palette = this.ppuMemoryMap.ReadByte((short)(0x3f10 + 4 * palette_number + color_index));
                return (palette, sprite);
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
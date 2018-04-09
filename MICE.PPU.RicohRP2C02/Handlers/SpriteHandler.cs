using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;
using System.Collections.Generic;

namespace MICE.PPU.RicohRP2C02.Handlers
{
    public class SpriteHandler
    {
        private static class Constants
        {
            public const int MaxSprites = 64;
            public const int MaxSpritesOnLine = 8;
        }

        private readonly PPURegisters registers;
        private readonly IMemoryMap ppuMemoryMap;
        private readonly IMemoryMap cpuMemoryMap;
        private readonly PaletteHandler paletteHandler;
        private readonly List<Sprite> currentSprites = new List<Sprite>(Constants.MaxSprites);
        private readonly List<Sprite> currentScanlineSprites = new List<Sprite>(Constants.MaxSprites);

        public SpriteHandler(IMemoryMap ppuMemoryMap, PPURegisters registers, PaletteHandler paletteHandler, IMemoryMap cpuMemoryMap)
        {
            this.registers = registers;
            this.cpuMemoryMap = cpuMemoryMap;
            this.ppuMemoryMap = ppuMemoryMap;
            this.paletteHandler = paletteHandler;
        }

        public int CurrentScanlineSpriteCount => this.currentScanlineSprites.Count;

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
            get => this.registers.PPUSTATUS.GetBit(6);
            set => this.registers.PPUSTATUS.SetBit(6, value);
        }

        public void EvaluateSpritesOnScanline(OAM oam, int scanline, int cycle)
        {
            this.currentScanlineSprites.Clear();

            var spriteHeight = this.IsSmallSprites ? 8 : 16;

            for (byte index = 0; index < Constants.MaxSprites && this.CurrentScanlineSpriteCount <= 8; index++)
            {
                var spriteY = oam[index * 4];
                var spriteRow = scanline - spriteY;

                if (spriteRow < 0 || spriteRow >= spriteHeight)
                {
                    continue;
                }

                var newSprite = new Sprite(index, this.IsSmallSprites, this.IsSpritePatternTableAddress1000, oam);
                newSprite.GatherPattern(spriteRow, this.ppuMemoryMap);

                this.currentScanlineSprites.Add(newSprite);
            }

            if (this.CurrentScanlineSpriteCount >= 9)
            {
                this.WasSpriteOverflow = true;
            }
        }

        public (byte, Sprite) GetSpritePixel(int x, int y, OAM primaryOAM)
        {
            if (x <= 7 && !this.DrawLeft8SpritePixels)
            {
                return (0, null);
            }

            for (int index = 0; index < this.CurrentScanlineSpriteCount; ++index)
            {
                var sprite = this.currentScanlineSprites[index];

                if (!sprite.IsVisible(x))
                {
                    continue;
                }

                sprite.SetFinalPixel(this.ppuMemoryMap, x, y);

                var finalPixel = this.paletteHandler.GetSpriteColor(sprite);
                return (finalPixel, sprite);
            }

            return (0, null);
        }

        public (byte, Sprite) GetSpritePixel2(int x, int y, OAM primaryOAM)
        {
            if (x <= 8 && !this.DrawLeft8SpritePixels)
            {
                return (0, null);
            }

            for (int index = 0; index < this.CurrentScanlineSpriteCount; ++index)
            {
                var sprite = this.currentScanlineSprites[index];

                if (!sprite.IsVisible(x))
                {
                    continue;
                }

                sprite.SetFinalPixel(this.ppuMemoryMap, x, y);

                var finalPixel = this.paletteHandler.GetSpriteColor(sprite);
                return (finalPixel, sprite);
            }

            return (0, null);
        }

        public void ClearSprites()
        {
            this.currentScanlineSprites.Clear();
            this.currentSprites.Clear();
        }

        public void EvaluateSpritesOnScanline2(OAM primaryOAM, OAM secondaryOAM, int scanLine)
        {
            // Following steps and naming at: http://wiki.nesdev.com/w/index.php/PPU_sprite_evaluation
            // Most OAM reads are OAM[4 * n + m]
            // n = Sprite (0-63)
            // m = Byte of sprite (0-3)
            var n = 0;
            var m = 0;

            var secondaryOAMIndex = 0;
            var foundSprites = 0;

            var spriteHeight = this.IsSmallSprites ? 8 : 16;

            for (; n < Constants.MaxSprites && foundSprites < 8; n++, m = 0)
            {
                byte spriteY = primaryOAM[4 * n + m++];

                if (foundSprites <= 8)
                {
                    // I think NESDev is wrong here?...would cause issues. 0 in-range sprites would take up
                    // entire secondary OAM twice-over, methinks?
                   // secondaryOAM[secondaryOAMIndex++] = spriteY;
                }

                int spriteRow = scanLine - spriteY;

                if (spriteRow < 0 || spriteRow >= spriteHeight)
                {
                    continue;
                }

                foundSprites++;
                if (foundSprites <= 8)
                {
                    secondaryOAM[secondaryOAMIndex++] = spriteY;
                    secondaryOAM[secondaryOAMIndex++] = primaryOAM[4 * n + m++];
                    secondaryOAM[secondaryOAMIndex++] = primaryOAM[4 * n + m++];
                    secondaryOAM[secondaryOAMIndex++] = primaryOAM[4 * n + m++];
                }
            }
        }
    }
}
using MICE.Common.Interfaces;
using MICE.PPU.RicohRP2C02.Components;
using System.Collections.Generic;

namespace MICE.PPU.RicohRP2C02
{
    public class SpriteHandler
    {
        private static class Constants
        {
            public const int MaxSprites = 64;
            public const int MaxSpritesOnScreen = 8;
        }

        private readonly IMemoryMap ppuMemoryMap;
        private readonly IMemoryMap cpuMemoryMap;
        private readonly PPURegisters registers;
        private readonly IList<byte[]> chrBanks;
        private readonly List<Sprite> currentSprites = new List<Sprite>(Constants.MaxSprites);
        private readonly List<Sprite> currentScanlineSprites = new List<Sprite>(Constants.MaxSprites);

        private int lastScannedScanline = 0;

        public SpriteHandler(IMemoryMap ppuMemoryMap, PPURegisters registers, IMemoryMap cpuMemoryMap, IList<byte[]> chrBanks)
        {
            this.chrBanks = chrBanks;
            this.registers = registers;
            this.cpuMemoryMap = cpuMemoryMap;
            this.ppuMemoryMap = ppuMemoryMap;
        }

        public int CurrentScanlineSpriteCount { get; private set; }

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

        public void EvaluateSpritesOnScanline(OAM oam, int scanline)
        {
            // Don't need to scan the same scanline more than once with current algorithm.
            if (this.lastScannedScanline == scanline)
            {
                return;
            }

            this.lastScannedScanline = scanline;
            this.currentScanlineSprites.Clear();

            this.CurrentScanlineSpriteCount = 0;

            int offset = this.IsSmallSprites ? 8 : 16;

            for (byte index = 0; index < Constants.MaxSprites; index++)
            {
                var indexMultiple = index * 4;

                Sprite newSprite = new Sprite
                    (
                        index,
                        this.IsSmallSprites,
                        this.IsSpritePatternTableAddress1000,
                        oam[indexMultiple + 0], // y position
                        oam[indexMultiple + 1], // tile index
                        oam[indexMultiple + 2], // attributes
                        oam[indexMultiple + 3]  // x position
                    );

                if (newSprite.IsOnScanline(scanline, offset))
                {
                    this.CurrentScanlineSpriteCount++;
                    this.currentScanlineSprites.Add(newSprite);
                }

                if (!this.currentSprites.Contains(newSprite))
                {
                    this.currentSprites.Add(newSprite);
                }
            }
        }

        public (byte, Sprite) GetSpritePixel(int x, int y, OAM primaryOAM)
        {
            if (x <= 8 && !this.DrawLeft8SpritePixels)
            {
                return (0, null);
            }

            for (int index = 0; index < this.CurrentScanlineSpriteCount; ++index)
            {
                var sprite = this.currentScanlineSprites[index];

                if (!sprite.IsVisible(x, y))
                {
                    continue;
                }

                var finalPixel = sprite.GetFinalPixel(x, y, this.chrBanks[0], this.ppuMemoryMap);
                return (finalPixel, sprite);
            }

            return (0, null);
        }

        public void ClearSprites()
        {
            this.currentScanlineSprites.Clear();
            this.currentSprites.Clear();
        }
    }
}
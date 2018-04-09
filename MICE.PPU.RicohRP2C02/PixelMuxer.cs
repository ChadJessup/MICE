﻿using MICE.PPU.RicohRP2C02.Components;

namespace MICE.PPU.RicohRP2C02
{
    public class PixelMuxer
    {
        private readonly PPURegisters registers;

        public PixelMuxer(PPURegisters registers) => this.registers = registers;

        /// <summary>
        /// Gets or sets a value indicating whether or not to output gray scale.
        /// </summary>
        public bool IsGrayScale
        {
            get => this.registers.PPUMASK.GetBit(0);
            set => this.registers.PPUMASK.SetBit(0, value);
        }

        public bool EmphasizeRed
        {
            get => this.registers.PPUMASK.GetBit(5);
            set => this.registers.PPUMASK.SetBit(5, value);
        }

        public bool EmphasizeGreen
        {
            get => this.registers.PPUMASK.GetBit(6);
            set => this.registers.PPUMASK.SetBit(6, value);
        }

        public bool EmphasizeBlue
        {
            get => this.registers.PPUMASK.GetBit(7);
            set => this.registers.PPUMASK.SetBit(7, value);
        }

        public byte MuxPixel((byte pixel, Sprite sprite) drawnSprite, (byte pixel, Tile tile) drawnBackground)
        {
            byte outputPixel = drawnBackground.pixel;

            if (drawnSprite.sprite == null)
            {
                return drawnBackground.pixel;
            }

            switch (outputPixel)
            {
                case var _ when drawnSprite.sprite.IsTransparentPixel && drawnBackground.tile.IsTransparentPixel:
                    break;
                case var _ when drawnBackground.tile.IsTransparentPixel:
                    outputPixel = drawnSprite.pixel;
                    break;
                case var _ when drawnSprite.sprite.IsTransparentPixel:
                    outputPixel = drawnBackground.pixel;
                    break;
                case var _ when drawnSprite.sprite.IsBehindBackground:
                    outputPixel = drawnBackground.pixel;
                    break;
                case var _ when !drawnSprite.sprite.IsTransparentPixel && !drawnSprite.sprite.IsBehindBackground:
                    outputPixel = drawnSprite.pixel;
                    break;
                default:
                    outputPixel = drawnBackground.pixel;
                    break;
            }

            return outputPixel;
        }
    }
}
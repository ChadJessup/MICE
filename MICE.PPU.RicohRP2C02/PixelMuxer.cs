using MICE.PPU.RicohRP2C02.Components;
using System.Runtime.CompilerServices;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte MuxPixel((byte pixel, Sprite sprite) drawnSprite, byte backgroundPixel, Tile backgroundTile)
        {
            return backgroundPixel switch
            {
                var _ when drawnSprite.sprite is null => backgroundPixel,
                var _ when drawnSprite.sprite.IsTransparentPixel && backgroundTile.IsTransparentPixel => backgroundPixel,
                var _ when backgroundTile.IsTransparentPixel => drawnSprite.pixel,
                var _ when drawnSprite.sprite.IsTransparentPixel => backgroundPixel,
                var _ when drawnSprite.sprite.IsBehindBackground => backgroundPixel,
                var _ when !drawnSprite.sprite.IsTransparentPixel && !drawnSprite.sprite.IsBehindBackground => drawnSprite.pixel,
                _ => backgroundPixel,
            };
        }
    }
}
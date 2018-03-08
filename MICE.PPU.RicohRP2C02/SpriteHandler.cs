namespace MICE.PPU.RicohRP2C02
{
    public class SpriteHandler
    {
        private readonly Registers registers;

        public SpriteHandler(Registers registers)
        {
            this.registers = registers;
        }

        public int CurrentScanlineSpriteCount { get; set; }

        public bool IsSmallSprites
        {
            get => this.registers.PPUCTRL.GetBit(5);
            set => this.registers.PPUCTRL.SetBit(5, value);
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

        public void DrawSpritePixel(int x, int y)
        {

        }
    }
}
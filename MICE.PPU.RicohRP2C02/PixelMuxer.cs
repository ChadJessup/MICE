using System;

namespace MICE.PPU.RicohRP2C02
{
    public class PixelMuxer
    {
        private readonly PPURegisters registers;

        public PixelMuxer(PPURegisters registers)
        {
            this.registers = registers;
        }

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

        public byte MuxPixel(byte spritePixel, byte backgroundPixel)
        {
            if(spritePixel != 0)
            {
                return spritePixel;
            }

            return backgroundPixel;
        }
    }
}
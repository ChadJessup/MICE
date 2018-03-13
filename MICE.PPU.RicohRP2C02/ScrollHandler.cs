namespace MICE.PPU.RicohRP2C02
{
    public class ScrollHandler
    {
        private readonly PPURegisters registers;
        public ScrollHandler(PPURegisters registers)
        {
            this.registers = registers;
        }

        public (int scrollX, int scrollY) GetScrollValues()
        {
            int scrollX = (this.registers.PPUSCROLL >> 8) & 0xFF;
            int scrollY = this.registers.PPUSCROLL & 0xFF;

            scrollX += ((this.registers.PPUCTRL >> 0) & 1) * 256;
            scrollY += ((this.registers.PPUCTRL >> 1) & 1) * 240;

            return (scrollX, scrollY);
        }
    }
}

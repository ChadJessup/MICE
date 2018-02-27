namespace MICE.CPU.MOS6502
{
    public enum InterruptType
    {
        None = 0,
        IRQ,
        BRK,
        NMI,
        Reset,
    }
}

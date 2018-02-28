namespace MICE.CPU.MOS6502
{
    // http://www.thealmightyguru.com/Games/Hacking/Wiki/index.php/6502_Opcodes
    public enum AddressingMode
    {
        None = 0,

        Implied,
        Relative,
        Accumulator,
        Immediate,

        ZeroPage,
        ZeroPageX,
        ZeroPageY,

        Absolute,
        AbsoluteX,
        AbsoluteY,

        Indirect,
        IndirectX,
        IndirectY,
    }
}
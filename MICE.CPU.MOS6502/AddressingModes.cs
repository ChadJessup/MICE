namespace MICE.CPU.MOS6502
{
    // http://www.thealmightyguru.com/Games/Hacking/Wiki/index.php/6502_Opcodes
    public enum AddressingModes
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
        AbsoluteXWrite,
        AbsoluteY,

        Indirect,
        IndirectX,
        IndirectY,
    }
}
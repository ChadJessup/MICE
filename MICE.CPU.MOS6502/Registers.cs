using System;
using MICE.Components.CPU;

namespace MICE.CPU.MOS6502
{
    public class Registers
    {
        public Registers()
        {
            this.A.Write(0);
            this.X.Write(0);
            this.Y.Write(0);
        }

        // The MOS 6502 has six registers, 3 for general processing, and 3 for program use:

        // Main registers:
        // Stores intermediate results for arithmetic and logic.
        public Register8Bit A = new Register8Bit("Accumulator");

        // Index registers:
        // General Purpose (Can also set/get the SP register)
        public Register8Bit X = new Register8Bit("X");

        // General Purpose register
        public Register8Bit Y = new Register8Bit("Y");

        // Points to location on stack
        public Register8Bit SP = new Register8Bit("Stack Pointer");

        // Indicates where program is in its program sequence.
        public Register16Bit PC = new Register16Bit("Program Counter");

        // Flags representing state of processor
        public Register8Bit P = new Register8Bit("Processor Status");

        public static object DestructureForLog(Registers registers)
        {
            return null;
        }
    }
}

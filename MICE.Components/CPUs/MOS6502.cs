using MICE.Common;
using MICE.Common.Helpers;
using MICE.Common.Interfaces;

namespace MICE.Components.CPUs
{
    public class MOS6502 : ICPU
    {
        public Endianness Endianness { get; } = Endianness.LittleEndian;

        // The MOS 6502 has six registers, 3 for general processing, and 3 for program use:

        // Main registers:
        // Accumulator - A - 8bit - Stores intermediate results for arithmetic and logic.
        public Register8Bit A = new Register8Bit("Accumulator");

        // Index registers:
        // X - X - 8bit - General Purpose (Can also set/get the SP register)
        public Register8Bit X = new Register8Bit("X");

        // Y - Y - 8bit - General Purpose
        public Register8Bit Y = new Register8Bit("Y");

        // Stack Pointer - SP - 8bit - Points to location on stack
        public Register8Bit SP = new Register8Bit("Stack Pointer");

        // Program Counter - PC - 16bit - Indicates where program is in its program sequence.
        public Register16Bit PC = new Register16Bit("Program Counter");

        // Status Register or Processor Flags - P - 8bit - Flags representing state of processor
        public Register8Bit P = new Register8Bit("Processor Status");

        // Gets a value indicating if the result of the last calculation needs to be carried over to allow for larger calculations.
        public bool WasCarry => this.P.Value.GetBit(0);

        // Get a value indicating if the last instruction resulted in 0.
        public bool WasZero => this.P.Value.GetBit(1);

        //   2 - I - Interrupt disable - Set to disable maskable interrupts
        public bool AreInterruptDisabled => this.P.Value.GetBit(2);

        //   3 - D - Decimal mode - Set when in BCD mode.
        public bool IsDecimalMode => this.P.Value.GetBit(3);

        //   4 - B - Breakpoint
        public bool WillBreak => this.P.Value.GetBit(4);

        //   5 - - - Unused
        public bool Unused => this.P.Value.GetBit(5);

        //   6 - V - Overflow flag - Set if arithmetic overflow has occurred
        public bool WasOverflowed => this.P.Value.GetBit(6);

        //   7 - N - Negative flag - set if number is negative
        public bool WasNegative => this.P.Value.GetBit(7);
    }
}

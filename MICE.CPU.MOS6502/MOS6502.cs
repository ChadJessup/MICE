using MICE.Common;
using MICE.Common.Helpers;
using MICE.Common.Interfaces;
using MICE.Components.CPU;
using System.Threading.Tasks;

namespace MICE.CPU.MOS6502
{
    public class MOS6502 : ICPU
    {
        private Opcodes Opcodes = new Opcodes();

        public Endianness Endianness { get; } = Endianness.LittleEndian;

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

        // Gets a value indicating if the result of the last calculation needs to be carried over to allow for larger calculations.
        public bool WasCarry => this.P.Value.GetBit(0);

        // Below flags reflect the bits in the P register...

        // Get a value indicating if the last instruction resulted in 0.
        public bool WasZero => this.P.Value.GetBit(1);

        // Interrupt disable - Set to disable maskable interrupts
        public bool AreInterruptsDisabled => this.P.Value.GetBit(2);

        // Decimal mode - Set when in BCD mode.
        public bool IsDecimalMode => this.P.Value.GetBit(3);

        // Breakpoint
        public bool WillBreak => this.P.Value.GetBit(4);

        // Unused
        public bool Unused => this.P.Value.GetBit(5);

        // Overflow flag - Set if arithmetic overflow has occurred
        public bool WasOverflowed => this.P.Value.GetBit(6);

        // Negative flag - set if number is negative
        public bool WasNegative => this.P.Value.GetBit(7);

        public async Task PowerOn()
        {
            
            await Task.CompletedTask;
        }
    }
}

using MICE.Common;
using MICE.Common.Helpers;
using MICE.Common.Interfaces;
using MICE.Components.CPU;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MICE.CPU.MOS6502
{
    public class MOS6502 : ICPU
    {
        private static class Constants
        {
            public const int StackPointerStart = 0xFD;
        }

        private Opcodes Opcodes = new Opcodes();
        private readonly IMemoryMap memoryMap;

        public MOS6502(IMemoryMap memoryMap)
        {
            this.memoryMap = memoryMap;
        }

        public IReadOnlyDictionary<InterruptType, int> InterruptOffsets = new Dictionary<InterruptType, int>()
        {
            { InterruptType.BRK, 0xFFFE },
            { InterruptType.IRQ, 0XFFFE },
            { InterruptType.NMI, 0xFFFA },
            { InterruptType.Reset, 0xFFFC},
        };

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
        public bool WasCarry => this.P.Read().GetBit(0);

        // Below flags reflect the bits in the P register...

        // Get a value indicating if the last instruction resulted in 0.
        public bool WasZero => this.P.Read().GetBit(1);

        // Interrupt disable - Set to disable maskable interrupts
        public bool AreInterruptsDisabled => this.P.Read().GetBit(2);

        // Decimal mode - Set when in BCD mode.
        public bool IsDecimalMode => this.P.Read().GetBit(3);

        // Breakpoint
        public bool WillBreak => this.P.Read().GetBit(4);

        // Unused
        public bool Unused => this.P.Read().GetBit(5);

        // Overflow flag - Set if arithmetic overflow has occurred
        public bool WasOverflowed => this.P.Read().GetBit(6);

        // Negative flag - set if number is negative
        public bool WasNegative => this.P.Read().GetBit(7);

        public void PowerOn()
        {
            this.Reset();
        }

        public void Reset()
        {
            this.A.Write(0);
            this.X.Write(0);
            this.Y.Write(0);
            this.SP.Write(Constants.StackPointerStart);
            this.PC.Write(this.memoryMap.Read<ushort>(this.InterruptOffsets[InterruptType.Reset]));
        }
    }
}

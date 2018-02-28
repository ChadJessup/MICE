using MICE.Common;
using MICE.Common.Helpers;
using MICE.Common.Interfaces;
using MICE.Components.CPU;
using System.Collections.Generic;
using System.Threading;

namespace MICE.CPU.MOS6502
{
    public class MOS6502 : ICPU
    {
        private static class Constants
        {
            public const int StackPointerStart = 0xFD;
        }

        private Opcodes Opcodes;
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
        public bool WasZero
        {
            get => this.P.Read().GetBit(1);
            set => this.P.SetBit(1, value);
        }

        // Interrupt disable - Set to disable maskable interrupts
        public bool AreInterruptsDisabled
        {
            get => this.P.Read().GetBit(2);
            set => this.P.SetBit(2, value);
        }

        // Decimal mode - Set when in BCD mode.
        public bool IsDecimalMode
        {
            get => this.P.Read().GetBit(3);
            set => this.P.SetBit(3, value);
        }

        // Breakpoint
        public bool WillBreak => this.P.Read().GetBit(4);

        // Unused
        public bool Unused => this.P.Read().GetBit(5);

        // Overflow flag - Set if arithmetic overflow has occurred
        public bool WasOverflowed => this.P.Read().GetBit(6);

        // Negative flag - set if number is negative
        public bool WasNegative
        {
            get => this.P.Read().GetBit(7);
            set => this.P.SetBit(7, value);
        }

        public void PowerOn(CancellationToken cancellationToken)
        {
            this.Reset(cancellationToken);
        }

        public void Reset(CancellationToken cancellationToken)
        {
            this.Opcodes = new Opcodes(this);
            this.A.Write(1);
            this.X.Write(0);
            this.Y.Write(0);
            this.SP.Write(Constants.StackPointerStart);
            this.PC.Write(this.memoryMap.ReadShort(this.InterruptOffsets[InterruptType.Reset]));
        }

        /// <summary>
        /// Steps the CPU and returns the amount of Cycles that would have occurred if the CPU were real.
        /// The cycles can be used by other components as a timing mechanism.
        /// </summary>
        /// <returns></returns>
        public int Step()
        {
            // Grab an Opcode from the PC register:
            var code = this.ReadNextByte();

            // Grab our version of the opcode...
            var opCode = this.Opcodes[code];

            opCode.Instruction(opCode);

            return opCode.Cycles;
        }

        public void WriteByte(ushort address, byte value, bool incrementPC = true)
        {
            ushort pc = this.PC;
            this.memoryMap.Write(address, value);

            if (incrementPC)
            {
                this.PC.Write(++pc);
            }
        }

        public byte ReadNextByte(bool incrementPC = true)
        {
            ushort pc = this.PC;
            var value = this.memoryMap.ReadByte(pc);

            if (incrementPC)
            {
                this.PC.Write(++pc);
            }

            return value;
        }

        public ushort ReadNextShort(bool incrementPC = true)
        {
            ushort pc = this.PC.Read();
            var value = this.memoryMap.ReadShort(pc);

            if(incrementPC)
            {
                this.PC.Write(++pc);
            }

            return value;
        }
    }
}

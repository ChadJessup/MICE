using MICE.Common;
using MICE.Common.Interfaces;
using MICE.Components.CPU;
using System;
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
        private long ranOpcodeCount = 0;

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

        /// <summary>
        /// Gets or sets the current cycle (or tick) or the CPU.  This increments a specific amount for each instruction that occurs.
        /// </summary>
        public long CurrentCycle = 0;

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

        /// <summary>
        /// Gets a value indicating if the result of the last calculation needs to be carried over to allow for larger calculations.
        /// </summary>
        public bool IsCarry
        {
            get => this.P.GetBit(0);
            set => this.P.SetBit(0, value);
        }

        /// <summary>
        /// Gets or sets a value indicating if the last instruction resulted in 0.
        /// </summary>
        public bool IsZero
        {
            get => this.P.GetBit(1);
            set => this.P.SetBit(1, value);
        }

        /// <summary>
        /// Interrupt disable - Set to disable maskable interrupts
        /// </summary>
        public bool AreInterruptsDisabled
        {
            get => this.P.GetBit(2);
            set => this.P.SetBit(2, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the CPU is in Decimal mode (BCD).
        /// </summary>
        public bool IsDecimalMode
        {
            get => this.P.GetBit(3);
            set => this.P.SetBit(3, value);
        }

        /// <summary>
        /// Gets or sets a value that will cause the CPU to Break.
        /// </summary>
        public bool WillBreak
        {
            get => this.P.GetBit(4);
            set => this.P.SetBit(4, value);
        }

        /// <summary>
        /// Unused bit.
        /// </summary>
        public bool Unused
        {
            get => this.P.GetBit(5);
            set => this.P.SetBit(5, value);
        }

        /// <summary>
        /// Overflow flag - Set if arithmetic overflow has occurred
        /// </summary>
        public bool IsOverflowed
        {
            get => this.P.GetBit(6);
            set => this.P.SetBit(6, value);
        }

        /// <summary>
        /// Negative flag - set if number is negative
        /// </summary>
        public bool IsNegative
        {
            get => this.P.GetBit(7);
            set => this.P.SetBit(7, value);
        }

        public void PowerOn(CancellationToken cancellationToken)
        {
            this.Reset(cancellationToken);
        }

        public void Reset(CancellationToken cancellationToken)
        {
            this.Opcodes = new Opcodes(this);
            this.A.Write(0);
            this.X.Write(0);
            this.Y.Write(0);
            this.SP.Write(Constants.StackPointerStart);
            this.PC.Write(this.memoryMap.ReadShort(this.InterruptOffsets[InterruptType.Reset]));

            this.Unused = true;
            this.AreInterruptsDisabled = true;

            this.IsCarry = false;
            this.IsDecimalMode = false;
            this.IsNegative = false;
            this.IsOverflowed = false;
            this.IsZero = false;
            this.WillBreak = true;

            // TODO: Move the below to NES specific reset logic...APU specifically...
            // Frame IRQ Enabled - APU
            // this.memoryMap.Write(0x4017, 00);

            // All channels disabled.
            // this.memoryMap.Write(0x4015, 00);

            // for (ushort i = 0x4000; i <= 0x400F; i++)
            // {
               //  this.memoryMap.Write(i, 00);
            //}
        }

        /// <summary>
        /// Steps the CPU and returns the amount of Cycles that would have occurred if the CPU were real.
        /// The cycles can be used by other components as a timing mechanism.
        /// </summary>
        /// <returns></returns>
        public int Step()
        {
            // TODO: Put behind debug flag...
            ushort oldPC = this.PC;
            if (this.PC == 0x8eed)
            { }
            // Grab an Opcode from the PC register:
            var code = this.ReadNextByte();

            // Grab our version of the opcode...
            var opCode = this.Opcodes[code];

            opCode.Instruction(opCode);

            if (opCode.ShouldVerifyResults && (oldPC + opCode.PCDelta != this.PC))
            {
                throw new InvalidOperationException($"Program Counter was not what was expected after executing instruction: {opCode.Name} (0x{opCode.Code:X}).{Environment.NewLine}Was: 0x{oldPC:X}{Environment.NewLine}Is: 0x{this.PC.Read():X}{Environment.NewLine}Expected: 0x{oldPC + opCode.PCDelta:X}");
            }

            this.ranOpcodeCount++;
            return opCode.Cycles;
        }

        public ushort StackGetPointer(int steps)
        {
            var stackPointer = this.SP.Read() + 0x0100;
            return (ushort)(stackPointer + steps);
        }

        public void StackMove(int steps)
        {
            var stackPointer = this.SP.Read();
            this.SP.Write((byte)(stackPointer + steps));
        }

        public void WriteByteAt(ushort address, byte value, bool incrementPC = true)
        {
            ushort pc = this.PC;
            this.memoryMap.Write(address, value);

            if (incrementPC)
            {
                this.PC.Write(++pc);
            }
        }

        public void WriteShortAt(ushort address, ushort value, bool incrementPC = true)
        {
            ushort pc = this.PC;
            var bytes = BitConverter.GetBytes(value);
            this.memoryMap.Write(address, bytes[0]);
            this.memoryMap.Write(address + 1, bytes[1]);

            if (incrementPC)
            {
                this.PC.Write(++pc);
            }
        }

        public void IncrementPC(ushort count = 1) => this.PC.Write((ushort)(this.PC + count));
        public void SetPCTo(ushort address) => this.PC.Write(address);
        public byte ReadNextByte(bool incrementPC = true) => this.ReadByteAt(this.PC, incrementPC);
        public byte ReadByteAt(ushort address, bool incrementPC = true)
        {
            ushort pc = this.PC;
            var value = this.memoryMap.ReadByte(address);

            if (incrementPC)
            {
                this.PC.Write(++pc);
            }

            return value;
        }
        public ushort ReadNextShort(bool incrementPC = true) => this.ReadShortAt(this.PC, incrementPC);
        public ushort ReadShortAt(ushort address, bool incrementPC = true)
        {
            ushort pc = this.PC;
            var value = this.memoryMap.ReadShort(address);

            if (incrementPC)
            {
                this.PC.Write(++pc);
            }

            return value;
        }
    }
}

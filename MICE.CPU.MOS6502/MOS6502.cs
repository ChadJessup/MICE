using MICE.Common;
using MICE.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MICE.CPU.MOS6502
{
    public class MOS6502 : ICPU
    {
        public StreamWriter fs;

        private readonly IMemoryMap memoryMap;
        private long ranOpcodeCount = 0;
        private long stepCount = 1;
        private Opcodes Opcodes;

        public MOS6502(IMemoryMap memoryMap, StreamWriter sw)
        {
            this.fs = sw;
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
        /// Gets or sets a value requesting a non-maskable interrrupt.
        /// </summary>
        public bool WasNMIRequested { get; set; }

        /// <summary>
        /// Gets or sets the current cycle (or tick) or the CPU.  This increments a specific amount for each instruction that occurs.
        /// </summary>
        public long CurrentCycle = 0;

        /// <summary>
        /// Gets the Stack for the MOS6502.
        /// This is actually a memory segment that is wrapped in stack-like helpers.
        /// </summary>
        public Stack Stack { get; private set; }

        /// <summary>
        /// Gets the MOS6502 Registers.
        /// </summary>
        public Registers Registers { get; private set; } = new Registers();

        /// <summary>
        /// Gets a value indicating if the result of the last calculation needs to be carried over to allow for larger calculations.
        /// </summary>
        public bool IsCarry
        {
            get => this.Registers.P.GetBit(0);
            set => this.Registers.P.SetBit(0, value);
        }

        /// <summary>
        /// Gets or sets a value indicating if the last instruction resulted in 0.
        /// </summary>
        public bool IsZero
        {
            get => this.Registers.P.GetBit(1);
            set => this.Registers.P.SetBit(1, value);
        }

        /// <summary>
        /// Interrupt disable - Set to disable maskable interrupts
        /// </summary>
        public bool AreInterruptsDisabled
        {
            get => this.Registers.P.GetBit(2);
            set => this.Registers.P.SetBit(2, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the CPU is in Decimal mode (BCD).
        /// </summary>
        public bool IsDecimalMode
        {
            get => this.Registers.P.GetBit(3);
            set => this.Registers.P.SetBit(3, value);
        }

        /// <summary>
        /// Gets or sets a value that will cause the CPU to Break.
        /// </summary>
        public bool WillBreak
        {
            get => this.Registers.P.GetBit(4);
            set => this.Registers.P.SetBit(4, value);
        }

        /// <summary>
        /// Unused bit.
        /// </summary>
        public bool Unused
        {
            get => this.Registers.P.GetBit(5);
            set => this.Registers.P.SetBit(5, value);
        }

        /// <summary>
        /// Overflow flag - Set if arithmetic overflow has occurred
        /// </summary>
        public bool IsOverflowed
        {
            get => this.Registers.P.GetBit(6);
            set => this.Registers.P.SetBit(6, value);
        }

        /// <summary>
        /// Negative flag - set if number is negative
        /// </summary>
        public bool IsNegative
        {
            get => this.Registers.P.GetBit(7);
            set => this.Registers.P.SetBit(7, value);
        }

        public bool IsPowered { get; private set; }

        public void PowerOn()
        {
            this.IsPowered = true;
            this.Reset();
        }

        public void PowerOff() => this.IsPowered = false;

        public void Reset()
        {
            this.Opcodes = new Opcodes(this, this.fs);
            this.Registers = new Registers();
            this.Registers.PC.Write(this.memoryMap.ReadShort(this.InterruptOffsets[InterruptType.Reset]));

            this.Stack = this.memoryMap.GetMemorySegment<Stack>("Stack");
            this.Stack.SetStackPointer(this.Registers.SP);

            this.Unused = true;
            this.AreInterruptsDisabled = true;

            this.IsCarry = false;
            this.IsDecimalMode = false;
            this.IsNegative = false;
            this.IsOverflowed = false;
            this.IsZero = false;
            this.WillBreak = false;

            this.Registers.P.Write(0x34);

            // TODO: Move the below to NES specific reset logic...APU specifically...
            // Frame IRQ Enabled - APU
            this.memoryMap.Write(0x4017, 0x00);

            // All channels disabled.
            this.memoryMap.Write(0x4015, 0x00);
        }

        private long nmiCycleStart = 0;

        /// <summary>
        /// Steps the CPU and returns the amount of Cycles that would have occurred if the CPU were real.
        /// The cycles can be used by other components as a timing mechanism.
        /// </summary>
        /// <returns>The amount of cycles that have passed in this step.</returns>
        public int Step()
        {
            ushort oldPC = this.Registers.PC;

            if (this.WasNMIRequested)
            {
                if (this.nmiCycleStart == 0)
                {
                    this.nmiCycleStart = this.CurrentCycle;
                }
                else if (this.CurrentCycle - this.nmiCycleStart >= 14)
                {
                    this.HandleNMIRequest();
                    this.AreInterruptsDisabled = true;
                    this.WasNMIRequested = false;
                    this.nmiCycleStart = 0;

                    return 1;
                }
            }

            OpcodeContainer opCode = null;
            byte code = 0;
            try
            {
                code = this.ReadNextByte();
                opCode = this.Opcodes[code];

                opCode.Instruction(opCode);
            }
            catch (Exception e)
            {
  //              this.fs.WriteLine($"{this.stepCount:D4}:0x{code:X}:0x{this.Registers.PC.Read():X}:{opCode?.Name}:{opCode?.Cycles + opCode?.AddedCycles}-PC:{Registers.PC.Read()}:A:{Registers.A.Read()}:X:{Registers.X.Read()}:Y:{Registers.Y.Read()}:SP:{Registers.SP.Read()}:P:{Convert.ToString(Registers.P.Read(), 2).PadLeft(8, '0')}");
 //               this.fs.WriteLine($"Exception thrown from opcode attempt: {opCode}{Environment.NewLine}{e.Message}");
  //              this.fs.Flush();
            }

            int logStart = 0;
            int logFor = 0x4d22;
            int logCap = logStart + logFor;

            if (this.stepCount > logStart && this.stepCount < logCap)
            {
//                this.fs.WriteLine($"{this.stepCount:D4}:0x{code:X}:0x{this.Registers.PC.Read():X}:{opCode?.Name}:{opCode?.Cycles + opCode?.AddedCycles}-PC:{Registers.PC.Read()}:A:{Registers.A.Read()}:X:{Registers.X.Read()}:Y:{Registers.Y.Read()}:SP:{Registers.SP.Read()}:P:{Convert.ToString(Registers.P.Read(), 2).PadLeft(8, '0')}");
  //              this.fs.Flush();
            }

            if (this.stepCount >= logCap)
            {
    //            this.fs.Flush();
                //throw new InvalidOperationException("Quitting in release mode.");
            }

            if (opCode.ShouldVerifyResults && (oldPC + opCode.PCDelta != this.Registers.PC))
            {
    //            this.fs.Flush();
                throw new InvalidOperationException($"Program Counter was not what was expected after executing instruction: {opCode.Name} (0x{opCode.Code:X}).{Environment.NewLine}Was: 0x{oldPC:X}{Environment.NewLine}Is: 0x{this.Registers.PC.Read():X}{Environment.NewLine}Expected: 0x{oldPC + opCode.PCDelta:X}");
            }

            this.stepCount++;
            this.ranOpcodeCount++;

            return opCode.Cycles + opCode.AddedCycles;
        }

        public void WriteByteAt(ushort address, byte value, bool incrementPC = true)
        {
            ushort pc = this.Registers.PC;
            this.memoryMap.Write(address, value);

            if (incrementPC)
            {
                this.Registers.PC.Write(++pc);
            }
        }

        public void WriteShortAt(ushort address, ushort value, bool incrementPC = true)
        {
            ushort pc = this.Registers.PC;
            var bytes = BitConverter.GetBytes(value);
            this.memoryMap.Write(address, bytes[0]);
            this.memoryMap.Write(address + 1, bytes[1]);

            if (incrementPC)
            {
                this.Registers.PC.Write(++pc);
            }
        }

        public void IncrementPC(ushort count = 1) => this.Registers.PC.Write((ushort)(this.Registers.PC + count));
        public void SetPCTo(ushort address) => this.Registers.PC.Write(address);
        public byte ReadNextByte(bool incrementPC = true) => this.ReadByteAt(this.Registers.PC, incrementPC);
        public byte ReadByteAt(ushort address, bool incrementPC = true)
        {
            ushort pc = this.Registers.PC;
            var value = this.memoryMap.ReadByte(address);

            if (incrementPC)
            {
                this.Registers.PC.Write(++pc);
            }

            return value;
        }

        public ushort ReadNextShort(bool incrementPC = true) => this.ReadShortAt(this.Registers.PC, incrementPC);
        public ushort ReadShortAt(ushort address, bool incrementPC = true)
        {
            ushort pc = this.Registers.PC;
            var value = this.memoryMap.ReadShort(address);

            if (incrementPC)
            {
                this.Registers.PC.Write(++pc);
            }

            return value;
        }

        private void HandleNMIRequest()
        {
            // Push PC to stack...
            this.Stack.Push(this.Registers.PC);

            // Push P to stack...
            this.Stack.Push(this.Registers.P);

            // Set PC to NMI vector.
            this.Registers.PC.Write(this.memoryMap.ReadShort(this.InterruptOffsets[InterruptType.NMI]));
        }
    }
}

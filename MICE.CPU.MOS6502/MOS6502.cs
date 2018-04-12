using MICE.Common;
using MICE.Common.Interfaces;
using Ninject;
using System;
using System.Collections.Generic;

namespace MICE.CPU.MOS6502
{
    public class MOS6502 : ICPU
    {
        private static class Constants
        {
            public const int ExtraNMIHandledCycles = 7;
        }

        private Opcodes Opcodes;

        public MOS6502([Named("CPU")] IMemoryMap memoryMap) => this.MemoryMap = memoryMap;

        public static long FrequencyHz = 1789773;

        public IReadOnlyDictionary<InterruptType, int> InterruptOffsets = new Dictionary<InterruptType, int>()
        {
            { InterruptType.BRK, 0xFFFE },
            { InterruptType.IRQ, 0XFFFE },
            { InterruptType.NMI, 0xFFFA },
            { InterruptType.Reset, 0xFFFC},
        };

        public Endianness Endianness { get; } = Endianness.LittleEndian;

        public IMemoryMap MemoryMap { get; }
        public ushort LastPC { get; set; }
        public byte NextOpcode { get; private set; }

        private string lastAccessedAddress;
        public string LastAccessedAddress
        {
            get => this.lastAccessedAddress;
            set => this.lastAccessedAddress = value;
        }

        /// <summary>
        /// Gets or sets a value requesting a non-maskable interrrupt.
        /// </summary>
        public bool WasNMIRequested { get; set; }

        /// <summary>
        /// Gets or sets the current cycle (or tick) or the CPU.  This increments a specific amount for each instruction that occurs.
        /// </summary>
        public long CurrentCycle { get; set; }

        /// <summary>
        /// Gets the Stack for the MOS6502.
        /// This is actually a memory segment that is wrapped in stack-like helpers.
        /// </summary>
        public Stack Stack { get; private set; }

        /// <summary>
        /// Gets the MOS6502 Registers.
        /// </summary>
        public Registers Registers { get; private set; }

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
        /// Note: Sometimes marked as 'Unused'.
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
        /// Reserved bit.
        /// Note: Almost always true?
        /// </summary>
        public bool Reserved
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

        public EventHandler CycleComplete { get; set; }

        public OpcodeContainer CurrentOpcode { get; private set; }

        public void PowerOn()
        {
            this.IsPowered = true;
            this.Reset();
        }

        public void PowerOff() => this.IsPowered = false;

        public void Reset()
        {
            this.Opcodes = new Opcodes(this);
            this.Registers = new Registers();
            this.Registers.PC.Write(this.MemoryMap.ReadShort(this.InterruptOffsets[InterruptType.Reset]));

            this.Stack = this.MemoryMap.GetMemorySegment<Stack>("Stack");
            this.Stack.SetInitialStackPointer(this.Registers.SP);
            this.Stack.Push(0x00);
            this.Stack.Push(0x00);

            this.Reserved = true;
            this.AreInterruptsDisabled = true;

            this.IsCarry = false;
            this.IsDecimalMode = false;
            this.IsNegative = false;
            this.IsOverflowed = false;
            this.IsZero = false;
            this.WillBreak = false;

            this.Registers.P.Write(0x24);

            // TODO: Move the below to NES specific reset logic...APU specifically...
            // Frame IRQ Enabled - APU
            // this.memoryMap.Write(0x4017, 0x00);

            // All channels disabled.
            this.MemoryMap.Write(0x4015, 0x00);
        }

        private bool shouldHandleNMI = false;
        /// <summary>
        /// Steps the CPU and returns the amount of Cycles that would have occurred if the CPU were real.
        /// The cycles can be used by other components as a timing mechanism.
        /// </summary>
        /// <returns>The amount of cycles that have passed in this step.</returns>
        public int Step()
        {
            var cycles = FetchInstruction();
            cycles += DecodeInstruction();
            return ExecuteInstruction() + cycles;
        }

        public int FetchInstruction()
        {
            this.LastAccessedAddress = "";

            if (this.WasNMIRequested)
            {
                if (!shouldHandleNMI)
                {
                    this.shouldHandleNMI = true;
                }
                else
                {
                    this.HandleInterruptRequest(InterruptType.NMI, this.Registers.PC);
                    this.AreInterruptsDisabled = true;
                    this.WasNMIRequested = false;
                }
            }

            this.LastPC = this.Registers.PC.Read();

            this.NextOpcode = this.ReadNextByte();

            return 1;
        }

        public int DecodeInstruction()
        {
            this.CurrentOpcode = this.Opcodes[this.NextOpcode];

            return 1;
        }

        public int ExecuteInstruction()
        {
            this.CurrentOpcode.Instruction(this.CurrentOpcode);

            if (this.CurrentOpcode.ShouldVerifyResults && (LastPC + this.CurrentOpcode.PCDelta != this.Registers.PC))
            {
                throw new InvalidOperationException($"Program Counter was not what was expected after executing instruction: {this.CurrentOpcode.Name} (0x{this.CurrentOpcode.Code:X}).{Environment.NewLine}Was: 0x{LastPC:X}{Environment.NewLine}Is: 0x{this.Registers.PC.Read():X}{Environment.NewLine}Expected: 0x{LastPC + this.CurrentOpcode.PCDelta:X}");
            }

            if (!this.WasNMIRequested && this.shouldHandleNMI)
            {
                this.shouldHandleNMI = false;

                //trace.Append($" - [NMI - Cycle: {this.CurrentCycle + Constants.ExtraNMIHandledCycles}]");
                this.CurrentOpcode.AddedCycles += Constants.ExtraNMIHandledCycles;
            }

            // Subtract two from total due to fetch/decode steps prior
            var newCycles = (this.CurrentOpcode.Cycles - 2) + this.CurrentOpcode.AddedCycles;
            //this.CurrentCycle += newCycles;

            return newCycles;
        }

        public void WriteByteAt(ushort address, byte value, bool incrementPC = true)
        {
            ushort pc = this.Registers.PC;
            this.CycleComplete?.Invoke(this, null);

            this.MemoryMap.Write(address, value);

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
            this.CycleComplete?.Invoke(this, null);

            var value = this.MemoryMap.ReadByte(address);

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
            var value = this.MemoryMap.ReadShort(address);

            if (incrementPC)
            {
                this.Registers.PC.Write(++pc);
                this.Registers.PC.Write(++pc);
            }

            return value;
        }

        public void HandleInterruptRequest(InterruptType interruptType, ushort returnAddress)
        {
            // Push return address to stack...usually PC.
            this.Stack.Push(returnAddress);

            // Push P to stack...
            this.Stack.Push(this.Registers.P);

            // Set PC to Interrupt vector.
            this.Registers.PC.Write(this.MemoryMap.ReadShort(this.InterruptOffsets[interruptType]));
        }
    }
}

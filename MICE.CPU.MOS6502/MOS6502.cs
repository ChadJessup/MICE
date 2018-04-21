﻿using MICE.Common;
using MICE.Common.Interfaces;
using Ninject;
using Serilog;
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
        private ushort address;

        public MOS6502([Named("CPU")] IMemoryMap memoryMap) => this.MemoryMap = memoryMap;

        public static long FrequencyHz = 1789773;
        public static bool IsDebug { get; set; } = false;

        public long CurrentOpcodeCycle => this.CurrentCycle - this.StartCycle;

        public IReadOnlyDictionary<InterruptType, ushort> InterruptOffsets = new Dictionary<InterruptType, ushort>()
        {
            { InterruptType.BRK, 0xFFFE },
            { InterruptType.IRQ, 0XFFFE },
            { InterruptType.NMI, 0xFFFA },
            { InterruptType.Reset, 0xFFFC},
        };

        public Endianness Endianness { get; } = Endianness.LittleEndian;
        public string LastAccessedAddress { get; set; }
        public byte NextOpcode { get; private set; }
        public IMemoryMap MemoryMap { get; }
        public ushort LastPC { get; set; }

        /// <summary>
        /// Gets or sets a value requesting a non-maskable interrrupt.
        /// </summary>
        public bool PreviousRunIrq { get; set; }

        public bool CurrentRunIrq { get; set; }

        /// <summary>
        /// Gets or sets the current cycle (or tick) or the CPU.  This increments a specific amount for each instruction that occurs.
        /// </summary>
        public long CurrentCycle { get; set; }

        /// <summary>
        /// Gets or sets the cycle at the beginning of the processing of an Opcode.
        /// </summary>
        public long StartCycle { get; set; }

        /// <summary>
        /// Gets or sets the cycle at the end of the processing of an Opcode.
        /// </summary>
        public long EndCycle { get; set; }

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

        private Action cycleComplete;

        public void PowerOn(Action cycleComplete)
        {
            this.IsPowered = true;
            this.cycleComplete = cycleComplete;
            this.Reset();
        }

        public void PowerOff() => this.IsPowered = false;

        public void Reset()
        {
            this.CurrentCycle = -1;
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

        public bool HandleIfIRQ()
        {
            if (this.PreviousRunIrq)
            {
                this.HandleInterruptRequest(InterruptType.NMI, this.Registers.PC);

                Log.Verbose(" - [NMI - Cycle: {currentCycle}]", this.CurrentCycle);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Steps the CPU and returns the amount of Cycles that would have occurred if the CPU were real.
        /// The cycles can be used by other components as a timing mechanism.
        /// </summary>
        /// <returns>The amount of cycles that have passed in this step.</returns>
        public int Step()
        {
            var cycles = this.FetchInstruction();
            cycles += this.DecodeInstruction();

            cycles += this.ExecuteInstruction();

            return cycles;
        }

        public int FetchInstruction()
        {
            this.LastAccessedAddress = "";
            this.StartCycle = this.CurrentCycle + 1;

            if (this.StartCycle == 116722)
            {
            }

            this.LastPC = this.Registers.PC.Read();

            this.CurrentOpcode = this.Opcodes[this.ReadNextByte()];

            this.IncrementPC();

            return 1;
        }

        public void CycleFinished()
        {
            this.CurrentCycle++;

            this.cycleComplete();
        }

        public int DecodeInstruction()
        {
            this.address = AddressingMode.GetAddressedOperand(this, this.CurrentOpcode);

            return 1;
        }

        public int ExecuteInstruction()
        {
            this.CurrentOpcode.Instruction(this.CurrentOpcode, address);

            //if (MOS6502.IsDebug && !this.shouldHandleNMI && this.CurrentOpcode.ShouldVerifyResults && (this.LastPC + this.CurrentOpcode.PCDelta != this.Registers.PC))
            //{
            //    throw new InvalidOperationException($"Program Counter was not what was expected after executing instruction: {this.CurrentOpcode.Name} (0x{this.CurrentOpcode.Code:X}).{Environment.NewLine}Was: 0x{LastPC:X}{Environment.NewLine}Is: 0x{this.Registers.PC.Read():X}{Environment.NewLine}Expected: 0x{LastPC + this.CurrentOpcode.PCDelta:X}");
            //}

            this.EndCycle = this.CurrentCycle;

            //if (MOS6502.IsDebug && this.CurrentOpcode.ShouldVerifyResults && this.EndCycle - this.StartCycle != this.CurrentOpcode.Cycles)
            //{
            //    Log.Warning("Cycles don't line up: Cycle: {startCycle} Expected to consume: {expectedCycles} Consumed: {consumedCycles} {opCode}",
            //        this.StartCycle, this.CurrentOpcode.Cycles, this.EndCycle - this.StartCycle, this.CurrentOpcode);
            //}

            return (int)(this.EndCycle - this.StartCycle);
        }

        public void SetPCTo(ushort address) => this.Registers.PC.Write(address);
        public void WriteByteAt(ushort address, byte value)
        {
            this.CycleFinished();
            this.MemoryMap.Write(address, value);
        }

        public void IncrementPC(int count = 1) => this.Registers.PC.Write((ushort)(this.Registers.PC + count));

        public ushort ReadShortAt(ushort address) => (ushort)(this.ReadByteAt((ushort)(address + 1)) << 8 | this.ReadByteAt(address));
        public ushort ReadNextShort() => this.ReadShortAt(this.Registers.PC);

        public byte ReadNextByte(bool isCycle = true) => this.ReadByteAt(this.Registers.PC, isCycle);
        public byte ReadByteAt(ushort address, bool isCycle = true)
        {
            if (isCycle)
            {
                this.CycleFinished();
            }

            return this.MemoryMap.ReadByte(address);
        }

        public void HandleInterruptRequest(InterruptType interruptType, ushort returnAddress)
        {
            // Interrupts take up 7 cycles...

            // Initial fetch of vector...
            this.CycleFinished();
            this.CycleFinished();

            // Push return address to stack...usually PC.
            this.Stack.Push(returnAddress);

            // Above...Stack push high
            this.CycleFinished();
            // Stack push low
            this.CycleFinished();

            // Push P to stack...
            this.Stack.Push(this.Registers.P);
            this.CycleFinished();

            // Two more cycles in the ReadShort fetch (1 per byte).
            // Set PC to Interrupt vector.
            this.Registers.PC.Write(this.ReadShortAt(this.InterruptOffsets[interruptType]));
        }
    }
}

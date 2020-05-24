using MICE.Common.Interfaces;
using MICE.Components.CPU;
using MICE.Components.Memory;
using System;
using Range = MICE.Common.Misc.Range;

namespace MICE.CPU.MOS6502
{
    /// <summary>
    /// The stack for the 6502 is a decrementing stack, mapped into memory space.  The SP register points to current element that will be operated on.
    /// </summary>
    public class Stack : BinaryMemorySegment, IStack
    {
        public static class Constants
        {
            public const int StackPointerStart = 0xFF;
            public static Range MemoryRange = new Range(0x0100, 0x01FF);
        }

        private Memory<byte> memory;

        public Stack(Range range, string name)
            : base(range, name)
        {
        }

        public Stack(int lowerIndex, int upperIndex, string name)
            : this(new Range(lowerIndex, upperIndex), name)
        {
        }

        public Stack(Memory<byte> memory, string name)
            : base(new Range(0x0100, 0x01FF), "Stack")
        {
            this.memory = memory;
        }

        public Register8Bit Pointer { get; private set; }

        public void SetInitialStackPointer(Register8Bit register)
        {
            this.Pointer = register;
            this.Pointer.Write(Constants.StackPointerStart);
        }

        public void Push(byte value)
        {
            this.memory.Span[this.GetAdjustedStackPointer()] = value;

            this.DecrementStackPointer();
        }

        public void Push(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);

            this.memory.Span[this.GetAdjustedStackPointer() - 1] = (byte)(bytes[0]);
            this.memory.Span[this.GetAdjustedStackPointer()] = (byte)(bytes[1]);

            this.DecrementStackPointer();
            this.DecrementStackPointer();
        }

        public byte PopByte()
        {
            this.IncrementStackPointer();

            return this.memory.Span[this.GetAdjustedStackPointer()];
        }

        public ushort PopShort()
        {
            this.IncrementStackPointer();
            this.IncrementStackPointer();
            var low =  this.memory.Span[this.GetAdjustedStackPointer() - 1];
            var high = this.memory.Span[this.GetAdjustedStackPointer()];

            return (ushort)(high << 8 | low & 0xff);
        }

        private void IncrementStackPointer() => this.Pointer.Write((byte)(this.Pointer.Read() + 1));
        private void DecrementStackPointer() => this.Pointer.Write((byte)(this.Pointer.Read() - 1));
        private byte GetAdjustedStackPointer() => (byte)(0x100 + this.Pointer);
    }
}

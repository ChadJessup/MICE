using MICE.Common.Interfaces;
using MICE.Common.Misc;
using MICE.Components.CPU;
using MICE.Components.Memory;
using System;

namespace MICE.CPU.MOS6502
{
    /// <summary>
    /// The stack for the 6502 is a decrementing stack, mapped into memory space.  The SP register points to current element that will be operated on.
    /// </summary>
    public class Stack : BinaryMemorySegment, IStack
    {
        private static class Constants
        {
            public const int StackPointerStart = 0xFD;
        }

        public Stack(int lowerIndex, int upperIndex, string name)
            : base(new Range<int>(lowerIndex, upperIndex), name)
        {
        }

        public Register8Bit Pointer { get; private set; }

        public void SetStackPointer(Register8Bit register)
        {
            this.Pointer = register;
            this.Pointer.Write(Constants.StackPointerStart);
        }

        public void Push(byte value)
        {
            this.Data[this.GetAdjustedStackPointer()] = value;

            this.DecrementStackPointer();
        }

        public void Push(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);

            this.Data[this.GetAdjustedStackPointer() - 1] = (byte)(bytes[0]);
            this.Data[this.GetAdjustedStackPointer()] = (byte)(bytes[1]);

            this.DecrementStackPointer();
            this.DecrementStackPointer();
        }

        public byte PopByte()
        {
            this.IncrementStackPointer();

            return this.Data[this.GetAdjustedStackPointer()];
        }

        public ushort PopShort()
        {
            this.IncrementStackPointer();
            this.IncrementStackPointer();
            var low = this.Data[this.GetAdjustedStackPointer() - 1];
            var high = this.Data[this.GetAdjustedStackPointer()];

            return (ushort)(high << 8 | low & 0xff);
        }

        private void IncrementStackPointer() => this.Pointer.Write((byte)(this.Pointer.Read() + 1));
        private void DecrementStackPointer() => this.Pointer.Write((byte)(this.Pointer.Read() - 1));
        private byte GetAdjustedStackPointer() => (byte)(0x100 + this.Pointer);
    }
}

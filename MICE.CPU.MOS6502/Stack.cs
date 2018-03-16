using MICE.Common.Interfaces;
using MICE.Components.CPU;
using MICE.Components.Memory;
using System;
using System.Collections.Generic;
using System.IO;

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

        private StreamWriter fs;
        public Stack(int lowerIndex, int upperIndex, string name, StreamWriter fs)
            : base(lowerIndex, upperIndex, name)
        {
            this.fs = fs;
            // Set the capactiy to the size of the memory segment as setup in the CPU Memory Map.
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

            this.fs.WriteLine($"Write Stack Byte: 0x1{this.Pointer.Read():X}-0x{value:X}");
            this.DecrementStackPointer();
        }

        public void Push(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);

            this.Data[this.GetAdjustedStackPointer() - 1] = (byte)(bytes[0]);
            this.Data[this.GetAdjustedStackPointer()] = (byte)(bytes[1]);

            this.DecrementStackPointer();
            this.DecrementStackPointer();

            this.fs.WriteLine($"Write Stack Short: 0x1{this.Pointer.Read() + 1:X}-0x{value:X}");
        }

        public byte PopByte()
        {
            this.IncrementStackPointer();

            byte outputValue = this.Data[this.GetAdjustedStackPointer()];
            this.fs.WriteLine($"Read Stack Byte: 0x1{this.Pointer.Read():X}-0x{outputValue:X}");

            return outputValue;
        }

        public ushort PopShort()
        {
            this.IncrementStackPointer();
            this.IncrementStackPointer();
            var low = this.Data[this.GetAdjustedStackPointer() - 1];
            var high = this.Data[this.GetAdjustedStackPointer()];

            var value = (ushort)(high << 8 | low & 0xff);

            this.fs.WriteLine($"Read Stack Short: 0x1{this.Pointer.Read() - 1:X}-0x{value:X}");

            return value;
        }

        private void IncrementStackPointer() => this.Pointer.Write((byte)(this.Pointer.Read() + 1));
        private void DecrementStackPointer() => this.Pointer.Write((byte)(this.Pointer.Read() - 1));
        private byte GetAdjustedStackPointer() => (byte)(0x100 + this.Pointer);
    }
}

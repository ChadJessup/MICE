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
    /// We use a standard .net <seealso cref="Stack<byte>"/> to handle normal stack operations.
    /// Some programs use the dedicated stack space to store arbitrary values, so we'll also have a byte array available for direct memory manipulation. This might be a bad idea?
    /// </summary>
    public class Stack : BinaryMemorySegment, IStack
    {
        private static class Constants
        {
            public const int StackPointerStart = 0xFD;
        }

        private Stack<byte> stack;
        private StreamWriter fs;
        public Stack(int lowerIndex, int upperIndex, string name, StreamWriter fs)
            : base(lowerIndex, upperIndex, name)
        {
            this.fs = fs;
            // Set the capactiy to the size of the memory segment as setup in the CPU Memory Map.
            this.stack = new Stack<byte>(upperIndex - lowerIndex);
        }

        public Register8Bit Pointer { get; private set; }

        public void SetStackPointer(Register8Bit register)
        {
            this.Pointer = register;
            this.Pointer.Write(Constants.StackPointerStart);
        }

        public void Push(byte value)
        {
            this.stack.Push(value);
            this.Pointer.Write((byte)(this.Pointer.Read() - 1));
           this.fs.WriteLine($"Write: 0x1{this.Pointer.Read():X}-0x{value:X}");
        }

        public void Push(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);

            this.Push(bytes[0]);
            this.Push(bytes[1]);
        }

        public byte PopByte()
        {
            this.Pointer.Write((byte)(this.Pointer.Read() + 1));
            var value = this.stack.Pop();

            this.fs.WriteLine($"Read: 0x1{this.Pointer.Read():X}-0x{value:X}");
            return value;
        }

        public ushort PopShort()
        {
            var high = this.PopByte();
            var low = this.PopByte();

            return (ushort)(high << 8 | low);
        }
    }
}

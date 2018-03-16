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
            this.Pointer.Write((byte)(this.Pointer.Read() - 1));
            this.stack.Push(value);
            this.fs.WriteLine($"Write Stack Byte: 0x1{this.Pointer.Read():X}-0x{value:X}");
        }

        public void Push(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);

            this.Push(bytes[0]);
            this.Push(bytes[1]);

            this.fs.WriteLine($"Write Stack Short: 0x1{this.Pointer.Read() + 1:X}-0x{value:X}");
        }

        public byte PopByte(bool hopBack2 = true)
        {
            this.Pointer.Write((byte)(this.Pointer.Read() + 1));
            byte outputValue;
            if (hopBack2 && this.stack.Count >= 2)
            {
                var value1 = this.stack.Pop();
                var value2 = this.stack.Pop();

                this.stack.Push(value1);
                outputValue = value2;
            }
            else
            {
                outputValue = this.stack.Pop();
            }

            this.fs.WriteLine($"Read Stack Byte: 0x1{this.Pointer.Read():X}-0x{outputValue:X}");
            return outputValue;
        }

        public ushort PopShort()
        {
            // Since things are pushed with shorts or bytes, popping a byte is non-intuitive...at least for the .NET Stack<T> class.
            var high = this.PopByte(hopBack2: false);
            var low = this.PopByte(hopBack2: false);

            var value = (ushort)(high << 8 | low);
            this.fs.WriteLine($"Read Stack Short: 0x1{this.Pointer.Read() - 1:X}-0x{value:X}");

            return value; ;
        }
    }
}

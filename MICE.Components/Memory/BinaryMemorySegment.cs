﻿using System;

namespace MICE.Components.Memory
{
    public abstract class BinaryMemorySegment : MemorySegment
    {
        public BinaryMemorySegment(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
            var length = Math.Max(1, upperIndex - lowerIndex);


            this.Data = new byte[length];
        }

        public byte[] Data { get; set; }

        public override byte ReadByte(int index) => this.Data[this.GetOffsetInSegment(index - 1)];
        public override ushort ReadShort(int index) => BitConverter.ToUInt16(this.Data, this.GetOffsetInSegment(index - 1));

        public override void Write(int index, byte value) => this.Data[this.GetOffsetInSegment(index - 1)] = value;
        public override void Write(int index, ushort value) => throw new NotImplementedException();
        public override byte[] ReadBytes(ushort startAddress, int size)
        {
            var bytes = new byte[size];
            Array.Copy(this.Data, this.GetOffsetInSegment(startAddress), bytes, 0, size);

            //for (int i = 0; i < size; i++)
            //{
              //  var index = this.GetOffsetInSegment(startAddress);
              //  bytes[i] = this.Data[index + i];
            //}

            return bytes;
        }
    }
}

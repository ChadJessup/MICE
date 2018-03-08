﻿using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    public class ROM : MemorySegment, IROM
    {
        public ROM(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }

        public override byte ReadByte(int index)
        {
            throw new System.NotImplementedException();
        }

        public override byte[] ReadBytes(ushort startAddress, int size)
        {
            throw new System.NotImplementedException();
        }

        public override ushort ReadShort(int index)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(int index, byte value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(int index, ushort value)
        {
            throw new System.NotImplementedException();
        }
    }
}

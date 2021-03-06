﻿using MICE.Common.Interfaces;
using System;
using Range = MICE.Common.Misc.Range;

namespace MICE.Components.Memory
{
    public class ROM : MemorySegment, IROM
    {
        public ROM(int lowerIndex, int upperIndex, string name)
            : base(new Range(lowerIndex, upperIndex), name)
        {
        }

        public override byte[] GetBytes() => throw new NotImplementedException();
        public override byte ReadByte(int index) => throw new NotImplementedException();
        public override ushort ReadShort(int index) => throw new NotImplementedException();
        public override void Write(int index, byte value) => throw new NotImplementedException();
        public override void Write(int index, ushort value) => throw new NotImplementedException();
        public override void CopyBytes(ushort startAddress, Span<byte> destination, int destinationIndex, int length) => throw new NotImplementedException();
    }
}

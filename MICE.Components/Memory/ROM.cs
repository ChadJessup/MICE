using MICE.Common.Interfaces;
using MICE.Common.Misc;
using System;

namespace MICE.Components.Memory
{
    public class ROM : MemorySegment, IROM
    {
        public ROM(int lowerIndex, int upperIndex, string name)
            : base(new Range<int>(lowerIndex, upperIndex), name)
        {
        }

        public override byte[] GetBytes() => throw new NotImplementedException();
        public override byte ReadByte(int index) => throw new NotImplementedException();
        public override ushort ReadShort(int index) => throw new NotImplementedException();
        public override void Write(int index, byte value) => throw new NotImplementedException();
        public override void Write(int index, ushort value) => throw new NotImplementedException();
        public override void CopyBytes(ushort startAddress, Array destination, int destinationIndex, int length) => throw new NotImplementedException();
    }
}

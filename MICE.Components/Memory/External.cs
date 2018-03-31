using System;
using MICE.Common.Interfaces;
using MICE.Common.Misc;

namespace MICE.Components.Memory
{
    public class External : MemorySegment
    {
        private IExternalHandler mapper;

        public External(int lowerIndex, int upperIndex, string name)
            : base(new Range<int>(lowerIndex, upperIndex), name)
        {
        }

        public void AttachHandler(IExternalHandler handler)
        {
            this.mapper = handler;
            this.mapper.AddMemorySegment(this);
        }

        public override byte ReadByte(int index) => this.mapper.ReadByte(index);
        public override ushort ReadShort(int index) => this.mapper.ReadShort(index);
        public override void Write(int index, byte value) => this.mapper.Write(index, value);
        public override void Write(int index, ushort value) => this.mapper.Write(index, value);

        public override byte[] GetBytes() => throw new NotImplementedException();
        public override void CopyBytes(ushort startAddress, Array destination, int destinationIndex, int length) => throw new NotImplementedException();
    }
}

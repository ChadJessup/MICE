using System;

namespace MICE.Components.Memory
{
    public abstract class BinaryMemorySegment : MemorySegment
    {
        public BinaryMemorySegment(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
            this.Data = new byte[upperIndex - lowerIndex];
        }

        public byte[] Data { get; set; }

        public override byte ReadByte(int index) => this.Data[this.GetOffsetInSegment(index - 1)];
        public override ushort ReadShort(int index) => BitConverter.ToUInt16(this.Data, this.GetOffsetInSegment(index - 1));

        public override void Write(int index, byte value) => this.Data[this.GetOffsetInSegment(index - 1)] = value;
        public override void Write(int index, ushort value) => throw new NotImplementedException();
    }
}

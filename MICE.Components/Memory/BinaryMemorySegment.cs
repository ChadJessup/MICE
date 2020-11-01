using System;
using Range = MICE.Common.Misc.Range;

namespace MICE.Components.Memory
{
    public abstract class BinaryMemorySegment : MemorySegment
    {
        public BinaryMemorySegment(Range range, string name, Action<int, byte>? afterWriteAction = null, Action<int, byte>? afterReadAction = null)
            : base(range, name)
        {
            var length = this.Range.Max - this.Range.Min;

            this.AfterReadAction = afterReadAction;
            this.AfterWriteAction = afterWriteAction;

            // TODO: fix this in getoffsetinsegment by overriding it...
            this.Data = new byte[length + 1];
        }

        public byte[] Data { get; set; }

        public override byte[] GetBytes() => this.Data;

        public override byte ReadByte(int index)
        {
            var value = this.Data[this.GetOffsetInSegment(index)];
            this.AfterReadAction?.Invoke(index, value);

            return value;
        }

        public override ushort ReadShort(int index) => BitConverter.ToUInt16(this.Data, this.GetOffsetInSegment(index));
        public override void Write(int index, byte value)
        {
            this.Data[this.GetOffsetInSegment(index)] = value;
            this.AfterWriteAction?.Invoke(index, value);
        }

        public override void Write(int index, ushort value) => throw new NotImplementedException();
        public override void CopyBytes(ushort startAddress, Span<byte> destinationArray, int destinationIndex, int length)
        {
            byte sourceIndex = (byte)this.GetOffsetInSegment(startAddress);

            byte dstIndex = (byte)destinationIndex;

            for (int i = 0; i < length; i++)
            {
                destinationArray[dstIndex++] = this.Data[sourceIndex++];
            }
        }
    }
}

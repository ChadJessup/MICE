using MICE.Common.Misc;
using System;

namespace MICE.Components.Memory
{
    public abstract class BinaryMemorySegment : MemorySegment
    {
        public BinaryMemorySegment(Range range, string name, Memory<byte> memory, Action<int, byte> afterWriteAction = null, Action<int, byte> afterReadAction = null)
            : base(range, memory, name)
        {
            var length = this.Range.Max - this.Range.Min;

            this.AfterReadAction = afterReadAction;
            this.AfterWriteAction = afterWriteAction;
        }

        // TODO: Converting things over to Memory/Span, so some of the code around 'Data' is bad atm.

        public override byte[] GetBytes() => this.Memory.ToArray();

        public override byte ReadByte(int index)
        {
            var value = this.Memory.Span[this.GetOffsetInSegment(index)];
            this.AfterReadAction?.Invoke(index, value);

            return value;
        }

        public override ushort ReadShort(int index) => BitConverter.ToUInt16(this.Memory.ToArray(), this.GetOffsetInSegment(index));
        public override void Write(int index, byte value)
        {
            this.Memory.Span[this.GetOffsetInSegment(index)] = value;
            this.AfterWriteAction?.Invoke(index, value);
        }

        public override void Write(int index, ushort value) => throw new NotImplementedException();
        public override void CopyBytes(ushort startAddress, Span<byte> destinationArray, int destinationIndex, int length)
        {
            byte sourceIndex = (byte)this.GetOffsetInSegment(startAddress);

            byte dstIndex = (byte)destinationIndex;

            for (int i = 0; i < length; i++)
            {
                destinationArray[dstIndex++] = this.Memory.Span[sourceIndex++];
            }
        }
    }
}

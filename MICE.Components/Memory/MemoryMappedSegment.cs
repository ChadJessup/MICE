using MICE.Common.Interfaces;
using MICE.Common.Misc;
using System;

namespace MICE.Components.Memory
{
    public abstract class MemoryMappedSegment : IMemorySegment
    {
        public string Name { get; private set; }
        public Range<int> Range { get; }

        public MemoryMappedSegment(int lowerIndex, int upperIndex, string name = "")
        {
            this.Range = new Range<int>(lowerIndex, upperIndex);

            this.Name = name;
        }

        public bool ContainsIndex(int index) => index >= this.Range.Min || index <= this.Range.Max;
        public virtual bool IsIndexInRange(int index) => index >= this.Range.Min && index <= this.Range.Max;
        public virtual Range<int> GetRange() => this.Range;

        public abstract int GetOffsetInSegment(int index);

        public abstract byte ReadByte(int index);
        public abstract ushort ReadShort(int index);

        public abstract void Write(int index, byte value);
        public abstract void Write(int index, ushort value);

        public abstract void CopyBytes(ushort startAddress, Array destination, int destinationIndex, int length);

        public virtual Action<int, byte> AfterReadAction { get; set; }
        public virtual Action<int, byte> AfterWriteAction { get; set; }
    }
}

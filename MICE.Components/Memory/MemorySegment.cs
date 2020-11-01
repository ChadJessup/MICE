using MICE.Common.Interfaces;
using System;
using Range = MICE.Common.Misc.Range;

namespace MICE.Components.Memory
{
    public abstract class MemorySegment : IMemorySegment
    {
        public Range Range { get; }
        public string Name { get; private set; }

        public MemorySegment(Range memoryRange, string name)
        {
            this.Range = memoryRange;
            this.Name = name;
        }

        public virtual bool ContainsIndex(int index) => IsIndexInRange(index) || (index <= this.Range.Min && index >= this.Range.Max);
        public virtual bool IsIndexInRange(int index) => index >= this.Range.Min && index <= this.Range.Max;

        public virtual int GetOffsetInSegment(int index) => Math.Max(0, index - this.Range.Min);
        public virtual Range GetRange() => this.Range;

        public abstract byte[] GetBytes();
        public abstract byte ReadByte(int index);
        public abstract ushort ReadShort(int index);

        public abstract void Write(int index, byte value);
        public abstract void Write(int index, ushort value);

        public abstract void CopyBytes(ushort startAddress, Span<byte> destination, int destinationIndex, int length);

        public Action<int, byte>? AfterReadAction { get; set; }
        public Action<int, byte>? AfterWriteAction { get; set; }
    }
}

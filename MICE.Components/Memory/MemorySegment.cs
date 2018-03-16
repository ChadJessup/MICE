using MICE.Common.Interfaces;
using System;

namespace MICE.Components.Memory
{
    public abstract class MemorySegment : IMemorySegment
    {
        public int LowerIndex { get; private set; }
        public int UpperIndex { get; private set; }
        public string Name { get; private set; }

        public MemorySegment(int lowerIndex, int upperIndex, string name)
        {
            if (lowerIndex > upperIndex)
            {
                throw new InvalidOperationException($"{Name} The upper index (0x{upperIndex:X}) must be greater than or equal to the lower index (0x{lowerIndex:X})");
            }

            this.LowerIndex = lowerIndex;
            this.UpperIndex = upperIndex;
            this.Name = name;
        }

        public virtual bool ContainsIndex(int index) => IsIndexInRange(index) || (index <= this.LowerIndex && index >= this.UpperIndex);
        public virtual bool IsIndexInRange(int index) => index >= this.LowerIndex && index <= this.UpperIndex;
        public virtual (int min, int max) GetRange() => (min: this.LowerIndex, max: this.UpperIndex);

        public virtual int GetOffsetInSegment(int index) => Math.Max(0, index - this.LowerIndex);

        public abstract byte ReadByte(int index);
        public abstract ushort ReadShort(int index);

        public abstract void Write(int index, byte value);
        public abstract void Write(int index, ushort value);

        public abstract void CopyBytes(ushort startAddress, Array destination, int destinationIndex, int length);

        public Action<int, byte> AfterReadAction { get; set; }
        public Action<int, byte> AfterWriteAction { get; set; }
    }
}

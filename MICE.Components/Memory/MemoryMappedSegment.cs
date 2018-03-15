using MICE.Common.Interfaces;
using System;

namespace MICE.Components.Memory
{
    public abstract class MemoryMappedSegment : IMemorySegment
    {
        public int LowerIndex { get; private set; }
        public int UpperIndex { get; private set; }
        public string Name { get; private set; }

        public MemoryMappedSegment(int lowerIndex, int upperIndex, string name = "")
        {
            if (lowerIndex > upperIndex)
            {
                throw new InvalidOperationException($"{Name} The upper index ({upperIndex}) must be greater than the lower index ({lowerIndex})");
            }

            this.LowerIndex = lowerIndex;
            this.UpperIndex = upperIndex;
            this.Name = name;
        }

        public bool ContainsIndex(int index) => index >= this.LowerIndex || index <= this.UpperIndex;
        public virtual bool IsIndexInRange(int index) => index >= this.LowerIndex && index <= this.UpperIndex;
        public virtual (int min, int max) GetRange() => (min: this.LowerIndex, max: this.UpperIndex);

        public abstract int GetOffsetInSegment(int index);

        public abstract byte ReadByte(int index);
        public abstract ushort ReadShort(int index);

        public abstract void Write(int index, byte value);
        public abstract void Write(int index, ushort value);

        public abstract void CopyBytes(ushort startAddress, Array destination, int destinationIndex, int length);
    }
}

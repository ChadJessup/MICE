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
            if (lowerIndex >= upperIndex || lowerIndex == upperIndex)
            {
                throw new InvalidOperationException($"{Name} The upper index ({upperIndex}) must be greater than the lower index ({lowerIndex})");
            }

            this.LowerIndex = lowerIndex;
            this.UpperIndex = upperIndex;
            this.Name = name;
        }

        public virtual bool IsIndexInRange(int index) => index >= this.LowerIndex && index <= this.UpperIndex;
        public virtual (int min, int max) GetRange() => (min: this.LowerIndex, max: this.UpperIndex);

        public virtual byte Read(int index) => throw new InvalidOperationException("Cannot read from this memory segment.");
        public virtual void Write(int index, byte value) => throw new InvalidOperationException("Cannot write to this memory segment.");
    }
}

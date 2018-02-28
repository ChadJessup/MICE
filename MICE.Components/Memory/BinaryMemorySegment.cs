using System;

namespace MICE.Components.Memory
{
    public abstract class BinaryMemorySegment : MemorySegment
    {
        public BinaryMemorySegment(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }

        public byte[] Data { get; set; }
    }
}

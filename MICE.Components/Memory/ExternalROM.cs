using MICE.Common.Interfaces;
using MICE.Common.Misc;
using System;

namespace MICE.Components.Memory
{
    public class ExternalROM : BinaryMemorySegment, IROM
    {
        public ExternalROM(int lowerIndex, int upperIndex, string name, Memory<byte> memory = default)
            : base(new Range(lowerIndex, upperIndex), name, memory)
        {
        }
    }
}

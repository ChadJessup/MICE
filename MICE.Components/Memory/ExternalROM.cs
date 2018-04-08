using MICE.Common.Interfaces;
using MICE.Common.Misc;

namespace MICE.Components.Memory
{
    public class ExternalROM : BinaryMemorySegment, IROM
    {
        public ExternalROM(int lowerIndex, int upperIndex, string name)
            : base(new Range(lowerIndex, upperIndex), name)
        {
        }
    }
}

using MICE.Common.Interfaces;
using MICE.Common.Misc;

namespace MICE.Components.Memory
{
    public class SRAM : BinaryMemorySegment, ISRAM
    {
        public SRAM(int lowerIndex, int upperIndex, string name)
            : base(new Range<int>(lowerIndex, upperIndex), name)
        {
        }
    }
}

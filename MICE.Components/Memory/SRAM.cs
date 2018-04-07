using MICE.Common.Interfaces;
using MICE.Common.Misc;

namespace MICE.Components.Memory
{
    public class SRAM : External, ISRAM
    {
        public SRAM(Range<int> range, string name)
            : base(range.Min, range.Max, name)
        {
        }

        public SRAM(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }
    }
}

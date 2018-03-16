using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    public class SRAM : BinaryMemorySegment, ISRAM
    {
        public SRAM(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }
    }
}

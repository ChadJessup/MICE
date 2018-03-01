using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    public class RAM : BinaryMemorySegment, IRAM
    {
        public RAM(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }
    }
}

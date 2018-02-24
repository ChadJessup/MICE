using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    public class ROM : MemorySegment, IROM
    {
        public ROM(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }

        public override byte Read(int index)
        {
            return 0;
        }
    }
}

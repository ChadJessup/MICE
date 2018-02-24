using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    public class SRAM : MemorySegment, ISRAM
    {
        public SRAM(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }

        public override byte Read(int index)
        {
            return 0;
        }

        public override void Write(int index, byte value)
        {
        }
    }
}

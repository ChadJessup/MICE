using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    public class ExternalROM : MemorySegment, IROM
    {
        public ExternalROM(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }

        public override byte Read(int index)
        {
            return 0;
        }
    }
}

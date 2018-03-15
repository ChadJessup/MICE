using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    public class ExternalROM : BinaryMemorySegment, IROM
    {
        public ExternalROM(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }
    }
}

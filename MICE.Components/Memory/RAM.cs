using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    public class RAM : BinaryMemorySegment, IRAM
    {
        public RAM(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }
        public override byte ReadByte(int index)
        {
            // TODO: clear this up when input/audio come in for NES code.
            if (index == 0x4017)
            {
                return 0x0;
            }

            return base.ReadByte(index);
        }
    }
}

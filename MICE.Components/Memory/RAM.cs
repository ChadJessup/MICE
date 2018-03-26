using MICE.Common.Interfaces;
using MICE.Common.Misc;
using System;

namespace MICE.Components.Memory
{
    public class RAM : BinaryMemorySegment, IRAM
    {
        public RAM(int lowerIndex, int upperIndex, string name, Action<int, byte> afterWriteAction = null, Action<int, byte> afterReadAction = null)
            : base(new Range<int>(lowerIndex, upperIndex), name, afterWriteAction, afterReadAction)
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

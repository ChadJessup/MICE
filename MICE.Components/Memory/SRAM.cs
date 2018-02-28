using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    public class SRAM : BinaryMemorySegment, ISRAM
    {
        public SRAM(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }

        public override byte ReadByte(int index)
        {
            throw new System.NotImplementedException();
        }

        public override ushort ReadShort(int index)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(int index, byte value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(int index, ushort value)
        {
            throw new System.NotImplementedException();
        }
    }
}

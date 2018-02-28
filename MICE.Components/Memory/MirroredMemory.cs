namespace MICE.Components.Memory
{
    public class MirroredMemory : MemorySegment
    {
        private int mirroredLowerIndex = 0;
        private int mirroredUpperIndex = 0;

        public MirroredMemory(int lowerIndex, int upperIndex, int mirroredLowerIndex, int mirroredUpperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
            this.mirroredLowerIndex = mirroredLowerIndex;
            this.mirroredUpperIndex = mirroredUpperIndex;
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

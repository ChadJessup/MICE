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

        public override byte Read(int index)
        {
            return 0;
        }

        public override void Write(int index, byte value)
        {
        }
    }
}

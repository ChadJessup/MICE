using MICE.Common.Interfaces;
using MICE.Common.Misc;

namespace MICE.Components.Memory
{
    public class MirroredMemory : MemorySegment
    {
        private readonly int mirroredLowerIndex;
        private readonly int mirroredUpperIndex;
        private readonly MemoryMapper memoryMapper;
        private readonly int moduloValue;

        public MirroredMemory(int lowerIndex, int upperIndex, int mirroredLowerIndex, int mirroredUpperIndex, MemoryMapper memoryMapper, string name)
            : base(lowerIndex, upperIndex, name)
        {
            this.mirroredLowerIndex = mirroredLowerIndex;
            this.mirroredUpperIndex = mirroredUpperIndex;

            this.moduloValue = this.mirroredUpperIndex - this.mirroredLowerIndex;

            this.memoryMapper = memoryMapper;
        }

        public override byte ReadByte(int index)
        {
            var newIndex = index % this.moduloValue;
            return this.memoryMapper.ReadByte(newIndex);
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

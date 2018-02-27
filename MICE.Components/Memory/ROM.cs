using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    public class ROM : MemorySegment, IROM
    {
        public ROM(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }

        public override T Read<T>(int index)
        {
            return default(T);
        }

        public override void Write<T>(int index, T value)
        {
        }
    }
}

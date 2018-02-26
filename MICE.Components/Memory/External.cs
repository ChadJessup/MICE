using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    public class External : MemorySegment
    {
        public IExternalHandler Handler { get; set; }

        public External(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }

        public override byte Read(int index)
        {
            return this.Handler.Read(index);
        }

        public override void Write(int index, byte value)
        {
            this.Handler.Write(index, value);
        }
    }
}

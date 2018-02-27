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

        public override T Read<T>(int index) => this.Handler.Read<T>(index);
        public override void Write<T>(int index, T value) => this.Handler.Write(index, value);
    }
}

using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    public class External : MemorySegment
    {
        private IExternalHandler mapper;

        public External(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }

        public void AttachHandler(IExternalHandler handler)
        {
            this.mapper = handler;
            this.mapper.AddMemorySegment(this);
        }

        public override byte ReadByte(int index) => this.mapper.ReadByte(index);
        public override ushort ReadShort(int index) => this.mapper.ReadShort(index);

        public override void Write(int index, byte value) => this.mapper.Write(index, value);
        public override void Write(int index, ushort value) => this.mapper.Write(index, value);
    }
}

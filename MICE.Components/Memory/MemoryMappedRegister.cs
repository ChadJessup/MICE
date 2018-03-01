using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    /// <summary>
    /// Memory segment that is actually mapped to a Register of a component.
    /// Reading/writing to this MemoryMappedSegment will directly reference the Register that is being mapped.
    /// </summary>
    public class MemoryMappedRegister<T> : MemoryMappedSegment, IRegister<T> where T : struct
    {
        private IRegister<T> mappedRegister;

        public MemoryMappedRegister(int lowerIndex, int upperIndex, IRegister<T> mappedRegister, string name)
            : base(lowerIndex, upperIndex, name)
        {
            this.mappedRegister = mappedRegister;
        }

        public override int GetOffsetInSegment(int index) => throw new System.NotImplementedException();

        public T Read() => this.mappedRegister.Read();
        public override byte ReadByte(int index) => throw new System.NotImplementedException();
        public override ushort ReadShort(int index) => throw new System.NotImplementedException();

        // TODO: blech...rethink generics here...
        public override void Write(int index, byte value) => this.Write((T)(object)value);
        public override void Write(int index, ushort value) => this.Read();
        public void Write(T value) => this.mappedRegister.Write(value);
    }
}

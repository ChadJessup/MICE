using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    /// <summary>
    /// Memory segment that is actually mapped to a Register of a component.
    /// Reading/writing to this MemoryMappedSegment will directly reference the Register that is being mapped.
    /// </summary>
    public class MemoryMappedRegister<T> : MemoryMappedSegment, IRegister<T>
    {
        private IRegister<T> mappedRegister;

        public MemoryMappedRegister(int lowerIndex, int upperIndex, IRegister<T> mappedRegister, string name)
            : base(lowerIndex, upperIndex, name)
        {
            this.mappedRegister = mappedRegister;
        }

        public T Read()
        {
            throw new System.NotImplementedException();
        }

        public void Write(T value)
        {
            throw new System.NotImplementedException();
        }
    }
}

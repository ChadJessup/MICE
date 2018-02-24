using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    /// <summary>
    /// Memory segment that is actually mapped to a Register of a component.
    /// Reading/writing to this MemoryMappedSegment will directly reference the Register that is being mapped.
    /// </summary>
    public class MemoryMappedRegister : MemoryMappedSegment, IRegister
    {
        private IRegister mappedRegister;

        public MemoryMappedRegister(int lowerIndex, int upperIndex, IRegister mappedRegister, string name)
            : base(lowerIndex, upperIndex, name)
        {
            this.mappedRegister = mappedRegister;
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

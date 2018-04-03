using MICE.Common.Interfaces;
using MICE.Nintendo.Components;

namespace MICE.Nintendo.Interfaces
{
    public interface INESInput : IMemorySegment, IInput
    {
        NESInputs Inputs { get; }
        void InputsChanged(NESInputs inputs);
    }
}

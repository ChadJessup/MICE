using MICE.Common.Interfaces;
using MICE.Nintendo.Components;

namespace MICE.Nintendo.Interfaces
{
    public interface INESInput : IInput
    {
        NESInputs Inputs { get; }
    }
}

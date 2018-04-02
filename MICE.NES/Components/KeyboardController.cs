using MICE.Common.Interfaces;
using MICE.Nintendo.Interfaces;

namespace MICE.Nintendo.Components
{
    public class KeyboardController : INESInput
    {
        public NESInputs Inputs { get; }
    }
}

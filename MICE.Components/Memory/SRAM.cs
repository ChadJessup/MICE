using MICE.Common.Interfaces;

namespace MICE.Components.Memory
{
    public class SRAM : External, ISRAM
    {
        public SRAM(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }
    }
}

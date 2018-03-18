using MICE.Components.Memory;

namespace MICE.PPU.RicohRP2C02.Components
{
    public class PatternTable : External
    {
        public PatternTable(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }
    }
}

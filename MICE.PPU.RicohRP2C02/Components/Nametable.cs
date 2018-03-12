using MICE.Components.Memory;

namespace MICE.PPU.RicohRP2C02.Components
{
    public class Nametable : VRAM
    {
        public Nametable(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }
    }
}

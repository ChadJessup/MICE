using MICE.Components.Memory;

namespace MICE.PPU.RicohRP2C02.Components
{
    // https://wiki.nesdev.com/w/index.php/PPU_palettes
    public class Palette : VRAM
    {
        public Palette(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }
    }
}

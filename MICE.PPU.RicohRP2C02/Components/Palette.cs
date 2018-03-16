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

        public override void Write(int index, byte value)
        {
            if (index == 0x3f10 || index == 0x3f00)
            {

            }

            base.Write(index, value);
        }

        public override byte ReadByte(int index)
        {
            if (index == 0x3f00)
            {

            }

            return base.ReadByte(index);
        }
    }
}

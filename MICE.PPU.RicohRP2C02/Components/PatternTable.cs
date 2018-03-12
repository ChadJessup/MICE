using MICE.Components.Memory;
using System.Windows.Forms;

namespace MICE.PPU.RicohRP2C02.Components
{
    public class PatternTable : VRAM
    {
        public PatternTable(int lowerIndex, int upperIndex, string name)
            : base(lowerIndex, upperIndex, name)
        {
        }

        public override void Write(int index, byte value)
        {
            base.Write(index, value);
            if (index == 0x1247)
            {
                MessageBox.Show($"{this.Name} written to: 0x{index:X4} - 0x{value:X4}");
            }
        }

        public override byte ReadByte(int index)
        {
            var value = base.ReadByte(index);
          //  MessageBox.Show($"{this.Name} read from: 0x{index:X4} - 0x{value:X4}");
            return value;
        }
    }
}

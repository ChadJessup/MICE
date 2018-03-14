using System.Drawing;

namespace MICE.PPU.RicohRP2C02.Components
{
    public class Tile
    {
        public Tile()
        {
        }

        //public Tile(byte data, NametableAttribute attribute)
        //{
        //    this.PatternTableIndex = data;
        //    this.Attribute = attribute;
        //}

        public short PPUAddress { get; set; }
        public int Nametable { get; set; }
        public (int x, int y) Location { get; set; }
        public int TileIndex { get; set; }
        public short TileAddress { get; set; }
        public byte AttributeData { get; set; }
        public short AttributeAddress { get; set; }
        public short PaletteAddress { get; set; }

        //public NametableAttribute Attribute { get; private set; }
    }
}

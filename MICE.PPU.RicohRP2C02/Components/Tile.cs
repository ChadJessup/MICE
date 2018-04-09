namespace MICE.PPU.RicohRP2C02.Components
{
    public class Tile
    {
        public ushort PPUAddress { get; set; }
        public int Nametable { get; set; }
        public (int x, int y) Location { get; set; }
        public int TileIndex { get; set; }
        public ushort TileAddress { get; set; }
        public byte AttributeData { get; set; }
        public ushort AttributeAddress { get; set; }
        public ushort PaletteAddress { get; set; }

        public NametableAttribute Attribute { get; private set; }
        public byte TileByte { get; set; }

        public byte ColorIndex { get; set; }

        public bool IsTransparentPixel => this.ColorIndex % 4 == 0;

        public byte nameTableByte = 0;
        public byte attributeByte = 0;
        public byte highTileByte = 0;
        public byte lowTileByte = 0;
    }
}

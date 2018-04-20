namespace MICE.PPU.RicohRP2C02.Components
{
    public struct NametableAttribute
    {
        public NametableAttribute(byte attributeByte, ushort address)
        {
            this.Address = address;
            this.RawByte = attributeByte;
        }

        public ushort Address { get; set; }
        public byte RawByte { get; set; }

        public int Shifted => ((this.Address >> 4) & 0b00000100) | (this.Address & 0b00000010);
        public byte AttributeTableByte => (byte)(((this.RawByte >> this.Shifted) & 0b00000011) << 2);
        public int PaletteOffset => (((this.RawByte >> this.Shifted) & 0b00000011) << 2) & 0b00000011;

        public int TopLeft => (this.RawByte & 0b00000011) >> 0;
        public int TopRight => (this.RawByte & 0b00001100) >> 2;
        public int BottomLeft => (this.RawByte & 0b00110000) >> 4;
        public int BottomRight => (this.RawByte & 0b11000000) >> 6;

        public int GetPaletteOffset() => this.PaletteOffset;
    }
}
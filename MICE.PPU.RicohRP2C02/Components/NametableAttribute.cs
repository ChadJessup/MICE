namespace MICE.PPU.RicohRP2C02.Components
{
    public struct NametableAttribute
    {
        public NametableAttribute(byte attributeByte, ushort address)
        {
            this.Address = address;
            this.RawByte = attributeByte;
        }

        public ushort Address { get; private set; }
        public byte RawByte { get; private set; }
        public int TopLeft => (this.RawByte & 0b00000011) >> 0;
        public int TopRight => (this.RawByte & 0b00001100) >> 2;
        public int BottomLeft => (this.RawByte & 0b00110000) >> 4;
        public int BottomRight => (this.RawByte & 0b11000000) >> 6;
    }
}
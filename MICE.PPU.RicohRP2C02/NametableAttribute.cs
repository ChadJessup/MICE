namespace MICE.PPU.RicohRP2C02.Components
{
    public struct NametableAttribute
    {
        public NametableAttribute(byte attributeByte, ushort offset)
        {
            var rawAttribute = offset;

            this.Address = (ushort)(0x2000 + offset);
            this.RawByte = attributeByte;
            this.TopLeft = rawAttribute & 0b10000000 | rawAttribute & 0b0100000;
            this.TopRight = rawAttribute & 0b00100000 | rawAttribute & 0b0001000;

            this.BottomLeft = rawAttribute & 0b00001000 | rawAttribute & 0b0000100;
            this.BottomRight = rawAttribute & 0b00000010 | rawAttribute & 0b0000001;
        }

        public ushort Address { get; private set; }
        public byte RawByte { get; private set; }
        public int TopLeft { get; private set; }
        public int TopRight { get; private set; }
        public int BottomLeft { get; private set; }
        public int BottomRight { get; private set; }
    }
}
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

        public int Shifted => ((this.Address >> 4) & 0b00000100) | (this.Address & 0b00000010);
        public byte AttributeTableByte => (byte)(((this.RawByte >> this.Shifted) & 0b00000011) << 2);
        public int PaletteOffset => (((this.RawByte >> this.Shifted) & 0b00000011) << 2) & 0b00000011;

        public byte RawByte { get; private set; }
        public int TopLeft => (this.RawByte & 0b00000011) >> 0;
        public int TopRight => (this.RawByte & 0b00001100) >> 2;
        public int BottomLeft => (this.RawByte & 0b00110000) >> 4;
        public int BottomRight => (this.RawByte & 0b11000000) >> 6;

        public int GetPaletteOffset()
        {
            return this.PaletteOffset;

            //var quadrant = 0;
            //if ((this.Address & 64) != 64)
            //{
            //    // 1 or 2
            //    if ((this.Address & 2) == 2)
            //    { //QUADRANT 1
            //        quadrant = this.RawByte & 0x03;
            //    }
            //    else
            //    {// QUADRANT 2
            //        quadrant = (this.RawByte & 0x0C) >> 2;
            //    }
            //}
            //else
            //{
            //    // 3 or 4
            //    if ((this.Address & 2) == 2)
            //    {
            //        quadrant = (this.RawByte & 0x30) >> 4;      // QUADRANT 3
            //    }
            //    else
            //    {
            //        quadrant = (this.RawByte & 0xC0) >> 6;      // QUADRANT 4
            //    }
            //}

            ////return quadrant;

            //if (indexedX == 0 && indexedY == 0)
            //{
            //    return this.TopLeft;
            //}

            //// http://web.mit.edu/6.111/www/f2004/projects/dkm_report.pdf
            //if (indexedX == 0 && indexedY == 1)
            //{
            //    return this.BottomLeft;
            //}

            //if (indexedX == 1 && indexedY == 0)
            //{
            //    return this.TopRight;
            //}

            //if (indexedX == 1 && indexedY == 1)
            //{
            //    return this.BottomRight;
            //}

            //throw new InvalidOperationException();
        }
    }
}
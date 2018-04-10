namespace MICE.PPU.RicohRP2C02.Handlers
{
    public class ScrollHandler
    {
        private readonly PPURegisters registers;
        private readonly PPUInternalRegisters internalRegisters;

        public ScrollHandler(PPURegisters registers, PPUInternalRegisters internalRegisters)
        {
            this.registers = registers;
            this.internalRegisters = internalRegisters;
        }

        public int vCoarseXScroll
        {
            get => (this.internalRegisters.v & 0b00000000_00011111);
            set => this.internalRegisters.v = (ushort)((this.internalRegisters.v & ~0b00000000_00011111) | (0b00000000_00011111 & (value << 0)));
        }

        public int vCoarseYScroll
        {
            get => (this.internalRegisters.v & 0b0000011_11100000) >> 5;
            set => this.internalRegisters.v = (ushort)((this.internalRegisters.v & ~0b0000011_11100000) | (0b0000011_11100000 & (value << 5)));
        }

        public int tCoarseXScroll
        {
            get => (this.internalRegisters.t & 0b00000000_00011111);
            set => this.internalRegisters.t = (ushort)((this.internalRegisters.t & ~0b00000000_00011111) | (0b00000000_00011111 & (value << 0)));
        }

        public int tCoarseYScroll
        {
            get => (this.internalRegisters.t & 0b0000011_11100000) >> 5;
            set => this.internalRegisters.t = (ushort)((this.internalRegisters.t & ~0b0000011_11100000) | (0b0000011_11100000 & (value << 5)));
        }

        public int vNametable
        {
            get => (this.internalRegisters.v & 0b0001100_00000000) >> 10;
            set => this.internalRegisters.v = (ushort)((this.internalRegisters.v & ~0b0001100_00000000) | (0b0001100_00000000 & (value << 10)));
        }

        public int tNametable
        {
            get => (this.internalRegisters.t & 0b0001100_00000000) >> 10;
            set => this.internalRegisters.t = (ushort)((this.internalRegisters.t & ~0b0001100_00000000) | (0b0001100_00000000 & (value << 10)));
        }

        public int vFineYScroll
        {
            get => (this.internalRegisters.v & 0b11100000_00000000) >> 13;
            set => this.internalRegisters.v = (ushort)((this.internalRegisters.v & ~0b11100000_00000000) | (0b11100000_00000000 & (value << 13)));
        }

        public int tFineYScroll
        {
            get => (this.internalRegisters.t & 0b11100000_00000000) >> 13;
            set => this.internalRegisters.t = (ushort)((this.internalRegisters.t & ~0b11100000_00000000) | (0b11100000_00000000 & (value << 13)));
        }

        public int FineXScroll { get; set; }

        public void IncrementCoarseX()
        {
            if (this.vCoarseXScroll == 31)
            {
                this.vCoarseXScroll = 0;
                this.internalRegisters.v ^= 0x0400;
            }
            else
            {
                this.vCoarseXScroll++;
            }
        }

        public void IncrementCoarseY2()
        {
            // increment vert(v)
            // if fine Y < 7
            if ((this.internalRegisters.v & 0x7000) != 0x7000)
            {
                // increment fine Y
                this.internalRegisters.v += 0x1000;
            }
            else
            {
                // fine Y = 0
                this.internalRegisters.v &= 0x8FFF;
                // let y = coarse Y
                int y = (this.internalRegisters.v & 0x03E0) >> 5;

                if (y == 29)
                {
                    // coarse Y = 0
                    y = 0;
                    // switch vertical nametable
                    this.internalRegisters.v ^= 0x0800;
                }
                else if (y == 31)
                {
                    // coarse Y = 0, nametable not switched
                    y = 0;
                }
                else
                {
                    // increment coarse Y
                    ++y;
                }

                // put coarse Y back into v
                this.internalRegisters.v = (ushort)((this.internalRegisters.v & 0xFC1F) | (y << 5));
            }

        }
        public void IncrementCoarseY()
        {
            if (this.vFineYScroll < 7)
            {
                this.vFineYScroll++;
            }
            else
            {
                this.vFineYScroll = 0;
                if (this.vCoarseYScroll == 29)
                {
                    this.vCoarseYScroll = 0;
                    this.internalRegisters.v ^= 0x0800;
                }
                else if (this.vCoarseYScroll == 31)
                {
                    this.vCoarseYScroll = 0;
                }
                else
                {
                    this.vCoarseYScroll++;
                }
            }
        }

        public (int scrollX, int scrollY) GetScrollValues()
        {
            int scrollX = (this.registers.PPUSCROLL >> 8) & 0xFF;
            int scrollY = this.registers.PPUSCROLL & 0xFF;

            scrollX += ((this.registers.PPUCTRL >> 0) & 1) * 256;
            scrollY += ((this.registers.PPUCTRL >> 1) & 1) * 240;

            return (scrollX, scrollY);
        }

        public void CopyHorizontalBits()
        {
            // v: ....F.. ...EDCBA = t: ....F.. ...EDCBA
            this.internalRegisters.v = (ushort)((this.internalRegisters.v & 0xFBE0) | (this.internalRegisters.t & 0x041F));
        }

        public void CopyVerticalBits()
        {
            // v: IHGF.ED CBA..... = t: IHGF.ED CBA.....
            this.internalRegisters.v = (ushort)((this.internalRegisters.v & 0x841F) | (this.internalRegisters.t & 0x7be0));
        }

        // $2005/PPUSCROLL in some docs
        public void PPUScrollWrittenTo(int? address, ushort value)
        {
            if (this.internalRegisters.w)
            {
                this.internalRegisters.t = (ushort)((this.internalRegisters.t & 0xFFE0) | (value >> 3));
                this.FineXScroll = value & 0x07;
            }
            else
            {
                this.internalRegisters.t = (ushort)((this.internalRegisters.t & 0x8FFF) | ((value & 0x07) << 12));
                this.internalRegisters.t = (ushort)((this.internalRegisters.t & 0xFC1F) | ((value & 0xF8) << 2));
            }

            this.internalRegisters.w = !this.internalRegisters.w;
        }

        public override string ToString()
            => $"CourseX: {this.vCoarseXScroll} FineX: {this.vFineYScroll} CourseY: {this.vCoarseYScroll} FineY: {this.vFineYScroll}";
    }
}

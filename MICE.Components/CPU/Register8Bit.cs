using System;

namespace MICE.Components.CPU
{
    public class Register8Bit : Register<byte>
    {
        public Register8Bit(string name, Action<int?, byte>? afterRead = null, Action<int?, byte>? afterWrite = null)
            : base(name, afterRead, afterWrite)
        {
        }

        public bool Bit0 { get; set; }
        public bool Bit1 { get; set; }
        public bool Bit2 { get; set; }
        public bool Bit3 { get; set; }
        public bool Bit4 { get; set; }
        public bool Bit5 { get; set; }
        public bool Bit6 { get; set; }
        public bool Bit7 { get; set; }

        public void SetBit(int index, bool bit)
        {
            switch (index)
            {
                case 0: this.Bit0 = bit; return;
                case 1: this.Bit1 = bit; return;
                case 2: this.Bit2 = bit; return;
                case 3: this.Bit3 = bit; return;
                case 4: this.Bit4 = bit; return;
                case 5: this.Bit5 = bit; return;
                case 6: this.Bit6 = bit; return;
                case 7: this.Bit7 = bit; return;
                default: throw new NotImplementedException();
            }
        }

        public bool GetBit(int index)
        {
            var bit = index switch
            {
                0 => this.Bit0,
                1 => this.Bit1,
                2 => this.Bit2,
                3 => this.Bit3,
                4 => this.Bit4,
                5 => this.Bit5,
                6 => this.Bit6,
                7 => this.Bit7,
                _ => throw new NotImplementedException(),
            };

            return bit;
        }

        public override void Write(byte value)
        {
            this.Bit0 = (value & (1 << 0)) != 0;
            this.Bit1 = (value & (1 << 1)) != 0;
            this.Bit2 = (value & (1 << 2)) != 0;
            this.Bit3 = (value & (1 << 3)) != 0;
            this.Bit4 = (value & (1 << 4)) != 0;
            this.Bit5 = (value & (1 << 5)) != 0;
            this.Bit6 = (value & (1 << 6)) != 0;
            this.Bit7 = (value & (1 << 7)) != 0;

            this.WriteInternal(value);

            this.AfterWriteAction?.Invoke(null, this.Value);
        }

        public override byte Read()
        {
            var fillByte = (byte)((this.Bit0 ? 1 : 0) << 0);
            fillByte |= (byte)((this.Bit1 ? 1 : 0) << 1);
            fillByte |= (byte)((this.Bit2 ? 1 : 0) << 2);
            fillByte |= (byte)((this.Bit3 ? 1 : 0) << 3);
            fillByte |= (byte)((this.Bit4 ? 1 : 0) << 4);
            fillByte |= (byte)((this.Bit5 ? 1 : 0) << 5);
            fillByte |= (byte)((this.Bit6 ? 1 : 0) << 6);
            fillByte |= (byte)((this.Bit7 ? 1 : 0) << 7);

            var toReturn = this.ReadByteInsteadAction?.Invoke(null, fillByte) ?? fillByte;
            this.AfterReadAction?.Invoke(null, toReturn);

            return toReturn;
        }

        public override string ToString()
            => $"0x{this.Value:X4} - 0b{Convert.ToString(this.Value, 2).PadLeft(8, '0')} - {this.Name}";

        public byte ReadInternal() => this.Value;

        public void WriteInternal(byte value) => this.Value = value;
    }
}

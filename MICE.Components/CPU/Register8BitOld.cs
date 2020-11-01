using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace MICE.Components.CPU
{
    public class Register8BitOld : Register<byte>
    {
        private readonly byte[] tempArray = new byte[1];

        public Register8BitOld(string name, Action<int?, byte>? afterRead = null, Action<int?, byte>? afterWrite = null)
            : base(name, afterRead, afterWrite)
        {
        }

        public BitArray Bits { get; } = new BitArray(8, defaultValue: false);

        public void SetBit(int index, bool bit)
            => this.Bits[index] = bit;

        public bool GetBit(int index)
            => this.Bits[index];

        public override void Write(byte value)
        {
            this.Bits[0] = (value & (1 << 0)) != 0;
            this.Bits[1] = (value & (1 << 1)) != 0;
            this.Bits[2] = (value & (1 << 2)) != 0;
            this.Bits[3] = (value & (1 << 3)) != 0;
            this.Bits[4] = (value & (1 << 4)) != 0;
            this.Bits[5] = (value & (1 << 5)) != 0;
            this.Bits[6] = (value & (1 << 6)) != 0;
            this.Bits[7] = (value & (1 << 7)) != 0;

            this.WriteInternal(value);

            this.AfterWriteAction?.Invoke(null, this.Value);
        }

        public override byte Read()
        {
            this.Bits.CopyTo(this.tempArray, 0);

            var toReturn = this.ReadByteInsteadAction?.Invoke(null, this.tempArray[0]) ?? this.tempArray[0];
            this.AfterReadAction?.Invoke(null, toReturn);

            //toReturn = this.Value;

            return toReturn;
        }

        public override string ToString()
            => $"0x{this.Value:X4} - 0b{Convert.ToString(this.Value, 2).PadLeft(8, '0')} - {this.Name}";

        public byte ReadInternal() => this.Value;

        public void WriteInternal(byte value) => this.Value = value;
    }
}

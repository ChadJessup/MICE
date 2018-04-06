using System;

namespace MICE.Components.CPU
{
    public class Register8Bit : Register<byte>
    {
        public Register8Bit(string name, Action<int?, byte> afterRead = null, Action<int?, byte> afterWrite = null)
            : base(name, afterRead, afterWrite)
        {
        }

        public void SetBit(int index, bool value)
        {
            this.Value = value
            ? (byte)(this.Value | (1 << index))
            : (byte)(this.Value & ~(1 << index));
        }

        public bool GetBit(int bitNumber) => (this.Value & (1 << bitNumber)) != 0;

        public override void Write(byte value)
        {
            this.Value = value;
            this.AfterWriteAction?.Invoke(null, this.Value);
        }

        public override byte Read()
        {
            var value = this.ReadByteInsteadAction?.Invoke(null, this.Value) ?? this.Value;
            this.AfterReadAction?.Invoke(null, value);

            return value;
        }

        public override string ToString() => $"0x{this.Value:X4} - 0b{Convert.ToString(this.Value, 2).PadLeft(8, '0')} - {this.Name}";

        public byte ReadInternal() => this.Value;
        public void WriteInternal(byte value) => this.Value = value;
    }
}
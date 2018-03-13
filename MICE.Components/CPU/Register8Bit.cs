using System;

namespace MICE.Components.CPU
{
    public class Register8Bit : Register<byte>
    {
        public Register8Bit(string name, Action afterRead = null, Action<byte> afterWrite = null)
            : base(name, afterRead, afterWrite)
        {
        }

        public void SetBit(int index, bool value)
        {
            this.Value = value
            ? (byte)(this.Value | (1 << index))
            : (byte)(this.Value & ~(1 << index));

            this.AfterWriteAction?.Invoke(this.Value);
        }

        public bool GetBit(int bitNumber)
        {
            var value = (this.Value & (1 << bitNumber)) != 0;
            this.AfterReadAction?.Invoke();

            return value;
        }

        public override void Write(byte value)
        {
            this.Value = value;
            this.AfterWriteAction?.Invoke(this.Value);
        }

        public override byte Read()
        {
            var value = this.Value;
            this.AfterReadAction?.Invoke();

            return value;
        }

        public override string ToString() => $"0x{this.Value:X4} - 0b{Convert.ToString(this.Value, 2).PadLeft(8, '0')} - {this.Name}";
    }
}
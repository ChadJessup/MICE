using System;

namespace MICE.Components.CPU
{
    public class Register8Bit : Register<byte>
    {
        public Register8Bit(string name, Action afterRead = null)
            : base(name, afterRead)
        {
        }

        public void SetBit(int index, bool value) => this.Value = value
            ? (byte)(this.Value | (1 << index))
            : (byte)(this.Value & ~(1 << index));

        public bool GetBit(int bitNumber)
        {
            var value = (this.Value & (1 << bitNumber)) != 0;
            this.AfterReadAction?.Invoke();

            return value;
        }

        public override void Write(byte value) => this.Value = value;
        public override byte Read()
        {
            var value = this.Value;
            this.AfterReadAction?.Invoke();

            return value;
        }

        public override string ToString() => $"0x{this.Value:X} - {this.Name}";
    }
}
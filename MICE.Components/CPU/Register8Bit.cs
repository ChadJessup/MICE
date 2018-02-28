namespace MICE.Components.CPU
{
    public class Register8Bit : Register<byte>
    {
        public Register8Bit(string name)
            : base(name)
        {
        }

        public void SetBit(int index, bool value)
        {
            this.Value = value
                ? (byte)(this.Value | (1 << index))
                : (byte)(this.Value & ~(1 << index));
        }

        public override void Write(byte value) => this.Value = value;
        public override byte Read() => this.Value;

        public override string ToString() => $"{this.Value} - {this.Name}";
    }
}
namespace MICE.Components.CPU
{
    public class ShiftRegister8Bit : Register8Bit
    {
        public ShiftRegister8Bit(string name)
            : base(name)
        {
        }

        public override void Write(byte value)
        {
            this.Value >>= 1;
            this.Value |= (byte)((value & 1) << 4);
        }

        public override byte Read() => this.Value;

        public override string ToString() => $"0x{this.Value:X} - {this.Name}";
    }
}
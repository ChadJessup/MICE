namespace MICE.Components.CPU
{
    public class ShiftRegister16Bit : Register16Bit
    {
        public ShiftRegister16Bit(string name)
            : base(name)
        {
        }

        public override void Write(ushort value) => this.Value = (ushort)(this.Value << 8 | value);
        public override ushort Read() => this.Value;

        public override string ToString() => $"0x{this.Value:X} - {this.Name}";
    }
}
namespace MICE.Components.CPU
{
    public class ShiftRegister16Bit : Register16Bit
    {
        public ShiftRegister16Bit(string name)
            : base(name)
        {
        }

        public override void Write(ushort value)
        {
            this.Value = (ushort)(this.Value << 8 | value);
            this.AfterWriteAction?.Invoke(null, value);
        }

        public override ushort Read()
        {
            var value = this.Value;
            this.AfterReadAction?.Invoke(null, value);

            return value;
        }

        public override string ToString() => $"0x{this.Value:X} - {this.Name}";
    }
}
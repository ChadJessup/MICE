namespace MICE.Components.CPU
{
    public class Register8Bit : Register<byte>
    {
        private byte value;

        public Register8Bit(string name)
            : base(name)
        {
        }

        public override void Write(byte value) => this.value = value;
        public override byte Read() => this.value;
    }
}
namespace MICE.Components.CPU
{
    public class Register16Bit : Register<ushort>
    {
        private ushort value;

        public Register16Bit(string name)
            : base(name)
        {
        }

        public override void Write(ushort value) => this.value = value;
        public override ushort Read() => this.value;
    }
}
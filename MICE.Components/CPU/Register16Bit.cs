namespace MICE.Components.CPU
{
    public class Register16Bit : Register
    {
        public ushort Value { get; set; }

        public Register16Bit(string name)
            : base(name)
        {
        }
    }
}
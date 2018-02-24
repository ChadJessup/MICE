namespace MICE.Components.CPUs
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
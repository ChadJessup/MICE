namespace MICE.Components.CPU
{
    public class Register8Bit : Register
    {
        public byte Value { get; set; }

        public Register8Bit(string name)
            : base(name)
        {
        }
    }
}
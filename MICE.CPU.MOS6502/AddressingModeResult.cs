namespace MICE.CPU.MOS6502
{
    public struct AddressingModeResult
    {
        public AddressingModeResult(byte value, ushort address, bool? samePage)
            : this(value, null, address, samePage)
        {
        }

        public AddressingModeResult(byte value, ushort? intermediateAddress, ushort address, bool? samePage)
        {
            this.Value = value;
            this.IntermediateAddress = intermediateAddress;
            this.Address = address;
            this.IsSamePage = samePage;
        }

        public byte Value;
        public ushort? IntermediateAddress;
        public ushort Address;
        public bool? IsSamePage;
    }
}

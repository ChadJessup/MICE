namespace MICE.CPU.MOS6502
{
    public struct AddressingModeResult
    {
        public AddressingModeResult(byte operand, ushort address, bool? samePage)
            : this(operand, null, address, samePage)
        {
        }

        public AddressingModeResult(byte operandValue, ushort? intermediateAddress, ushort address, bool? samePage)
        {
            this.OperandValue = operandValue;
            this.IntermediateAddress = intermediateAddress;
            this.Address = address;
            this.IsSamePage = samePage;
        }

        public byte OperandValue;
        public ushort? IntermediateAddress;
        public ushort Address;
        public bool? IsSamePage;
    }
}

using MICE.Common.Attributes;
using System;

namespace MICE.CPU.MOS6502
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Delegate | AttributeTargets.Method | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public class MOS6502OpcodeAttribute : OpcodeAttribute
    {
        public AddressingMode AddressingMode { get; private set; } = AddressingMode.None;
        public int Cycles { get; private set; } = 1;

        public MOS6502OpcodeAttribute(int code, int cycles, string name, AddressingMode addressingMode = AddressingMode.None, string description = "")
            : base(code, name, description)
        {
            this.AddressingMode = addressingMode;
            this.Cycles = cycles;
        }
    }
}

using System;
using System.Reflection;

namespace MICE.CPU.MOS6502
{
    public class OpcodeContainer
    {
        public AddressingMode AddressingMode { get; private set; } = AddressingMode.None;
        public int Cycles { get; private set; }
        public int Code { get; private set; }
        public int PCDelta { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public Action<OpcodeContainer> Instruction { get; private set; }

        public OpcodeContainer(MOS6502OpcodeAttribute details, Opcodes opcodes, MethodInfo methodInfo)
        {
            this.AddressingMode = details.AddressingMode;
            this.Cycles = details.Cycles;
            this.Code = details.Code;
            this.Name = details.Name;
            this.Description = details.Description;
            this.PCDelta = details.PCDelta;

            this.Instruction = (Action<OpcodeContainer>)Delegate.CreateDelegate(typeof(Action<OpcodeContainer>), opcodes, methodInfo, throwOnBindFailure: true);
        }

        public override string ToString() => $"{this.Name} (0x{this.Code:X})";
    }
}

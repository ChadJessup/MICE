using System;
using System.Reflection;

namespace MICE.CPU.MOS6502
{
    public class OpcodeContainer
    {
        public int Cycles { get; private set; }
        public int PCDelta { get; set; }
        public int AddedCycles { get; set; }

        public AddressingModes AddressingMode { get; private set; } = AddressingModes.None;
        public int Code { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool ShouldVerifyResults { get; private set; }

        private Action<OpcodeContainer> instruction;
        public Action<OpcodeContainer> Instruction
        {
            get
            {
                this.AddedCycles = 0;
                return this.instruction;
            }

            private set => this.instruction = value;
        }

        public OpcodeContainer(MOS6502OpcodeAttribute details, Opcodes opcodes, MethodInfo methodInfo)
        {
            this.AddressingMode = details.AddressingMode;
            this.Cycles = details.Cycles;
            this.Code = details.Code;
            this.Name = details.Name;
            this.Description = details.Description;
            this.PCDelta = details.PCDelta;
            this.ShouldVerifyResults = details.ShouldVerify;

            this.Instruction = (Action<OpcodeContainer>)Delegate.CreateDelegate(typeof(Action<OpcodeContainer>), opcodes, methodInfo, throwOnBindFailure: true);
        }

        public override string ToString() => $"{this.Name} (0x{this.Code:X})";
    }
}

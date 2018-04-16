using System;
using System.Reflection;

namespace MICE.CPU.MOS6502
{
    public class OpcodeContainer
    {
        public int Cycles { get; private set; }
        public int PCDelta { get; set; }
        public bool Processed { get; set; }

        public AddressingModes AddressingMode { get; private set; } = AddressingModes.None;
        public int Code { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool ShouldVerifyResults { get; private set; }
        public bool IsUnofficial { get; private set; }

        private Action<OpcodeContainer, ushort> instruction;
        public Action<OpcodeContainer, ushort> Instruction
        {
            get
            {
                this.Processed = false;
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
            this.IsUnofficial = details.Unofficial;

            this.Instruction = (Action<OpcodeContainer, ushort>)Delegate.CreateDelegate(typeof(Action<OpcodeContainer, ushort>), opcodes, methodInfo, throwOnBindFailure: true);
        }

        public override string ToString() => $"{this.Name} (0x{this.Code:X}) - {this.AddressingMode}";
    }
}

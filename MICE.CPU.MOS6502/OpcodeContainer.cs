using System;
using System.Reflection;

namespace MICE.CPU.MOS6502
{
    public class OpcodeContainer
    {
        public AddressingMode AddressingMode { get; private set; } = AddressingMode.None;
        public int Cycles { get; private set; } = 1;
        public int Code { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public Action Instruction { get; private set; }

        public OpcodeContainer(MOS6502OpcodeAttribute details, MethodInfo methodInfo)
        {
            this.AddressingMode = details.AddressingMode;
            this.Cycles = details.Cycles;
            this.Name = details.Name;
            this.Description = details.Description;

            this.Instruction = (Action)Delegate.CreateDelegate(typeof(Action), methodInfo);
        }
    }
}

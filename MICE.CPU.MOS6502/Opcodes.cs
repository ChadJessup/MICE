using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MICE.CPU.MOS6502
{
    public class Opcodes
    {
        public Opcodes(MOS6502 cpu)
        {
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            foreach (var methodInfo in typeof(Opcodes).GetMethods(bindingFlags).Where(m => m.CustomAttributes.Any(ca => ca.AttributeType == typeof(MOS6502OpcodeAttribute))))
            {
                foreach (var attrib in methodInfo.GetCustomAttributes<MOS6502OpcodeAttribute>())
                {
                    this.OpCodeMap.Add(attrib.Code, new OpcodeContainer(attrib, cpu, methodInfo));
                }
            }
        }

        public Dictionary<int, OpcodeContainer> OpCodeMap = new Dictionary<int, OpcodeContainer>();

        public OpcodeContainer this[int code]
        {
            get => this.OpCodeMap[code];
        }

        // Most of the following is from: http://www.6502.org/tutorials/6502opcodes.html

        //[MOS6502Opcode(0x24, 3, "BIT", AddressingMode.ZeroPage)]
        //[MOS6502Opcode(0x2C, 4, "BIT", AddressingMode.Absolute)]
        public void BIT(OpcodeContainer container)
        {
        }

        [MOS6502Opcode(0x78, 2, "SEI", AddressingMode.Implied)]
        public void SEI(OpcodeContainer container) => container.CPU.AreInterruptsDisabled = true;

        [MOS6502Opcode(0xD8, 2, "CLD", AddressingMode.Implied)]
        public void CLD(OpcodeContainer container) => container.CPU.IsDecimalMode = false;

        [MOS6502Opcode(0xA9, 2, "LDA", AddressingMode.Immediate)]
        public void LDA(OpcodeContainer container)
        {
            var nextByte = container.CPU.ReadNextByte();
            container.CPU.A.Write(nextByte);

            this.HandleZero(container.CPU, nextByte);
            this.HandleNegative(container.CPU, nextByte);
        }

        [MOS6502Opcode(0x8D, 3, "STA", AddressingMode.Absolute)]
        public void STA(OpcodeContainer container)
        {
            switch (container.AddressingMode)
            {
                case AddressingMode.Absolute:
                    var address = container.CPU.ReadNextShort();
                    container.CPU.WriteByte(address, container.CPU.A);
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected AddressingMode in STA: {container.AddressingMode}");
            }
        }

        private void HandleNegative(MOS6502 cpu, byte nextByte)
        {
            // TODO: hmmm...seems too easy, we'll see.
            cpu.WasNegative = nextByte >= 0x80;
        }

        private void HandleZero(MOS6502 cpu, byte operand)
        {
            cpu.WasZero = operand == 0;
        }
    }
}

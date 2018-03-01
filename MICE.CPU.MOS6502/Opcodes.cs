using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MICE.Components.CPU;

namespace MICE.CPU.MOS6502
{
    public class Opcodes
    {
        private Dictionary<int, int> opCodeCount = new Dictionary<int, int>();
        private MOS6502 CPU;
        public Opcodes(MOS6502 cpu)
        {
            this.CPU = cpu;

            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            foreach (var methodInfo in typeof(Opcodes).GetMethods(bindingFlags).Where(m => m.CustomAttributes.Any(ca => ca.AttributeType == typeof(MOS6502OpcodeAttribute))))
            {
                foreach (var attrib in methodInfo.GetCustomAttributes<MOS6502OpcodeAttribute>())
                {
                    this.OpCodeMap.Add(attrib.Code, new OpcodeContainer(attrib, methodInfo));
                }
            }
        }

        public Dictionary<int, OpcodeContainer> OpCodeMap = new Dictionary<int, OpcodeContainer>();

        public OpcodeContainer this[int code]
        {
            get
            {
                if (this.opCodeCount.ContainsKey(code))
                {
                    this.opCodeCount[code]++;
                }
                else
                {
                    this.opCodeCount.Add(code, 1);
                }

                return this.OpCodeMap[code];
            }
        }

        // Most of the following is from: http://www.6502.org/tutorials/6502opcodes.html

        //[MOS6502Opcode(0x24, 3, "BIT", AddressingMode.ZeroPage)]
        //[MOS6502Opcode(0x2C, 4, "BIT", AddressingMode.Absolute)]
        public void BIT(OpcodeContainer container)
        {
        }

        [MOS6502Opcode(0xA2, 2, "LDX", AddressingMode.Immediate)]
        public void LDX(OpcodeContainer container) => this.WriteNextByteToRegister(CPU.X, S: true, Z: true);

        [MOS6502Opcode(0x78, 2, "SEI", AddressingMode.Implied)]
        public void SEI(OpcodeContainer container) => CPU.AreInterruptsDisabled = true;

        [MOS6502Opcode(0xD8, 2, "CLD", AddressingMode.Implied)]
        public void CLD(OpcodeContainer container) => CPU.IsDecimalMode = false;

        [MOS6502Opcode(0xA9, 2, "LDA", AddressingMode.Immediate)]
        public void LDA(OpcodeContainer container) => this.WriteNextByteToRegister(CPU.A, S: true, Z: true);

        [MOS6502Opcode(0x8D, 3, "STA", AddressingMode.Absolute)]
        public void STA(OpcodeContainer container)
        {
            switch (container.AddressingMode)
            {
                case AddressingMode.Absolute:
                    var address = CPU.ReadNextShort();
                    CPU.WriteByte(address, CPU.A);
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected AddressingMode in STA: {container.AddressingMode}");
            }
        }

        private void HandleNegative(byte nextByte)
        {
            // TODO: hmmm...seems too easy, we'll see. 0x80 = 128, max of signed byte.
            CPU.WasNegative = nextByte >= 0x80;
        }

        private void HandleZero(byte operand)
        {
            CPU.WasZero = operand == 0;
        }

        /// <summary>
        /// Has the CPU read the next byte, and write it to an 8bit register.
        /// Optionally, it can set CPU status flags based on read byte:
        /// Z: Zero Flag
        /// S: Sign Flag
        /// 
        /// </summary>
        /// <param name="register">The register to write to.</param>
        /// <param name="S">Whether or not to set the Sign flag on the CPU's Status Register.</param>
        /// <param name="Z">Whether or not to set the Zero flag on the CPU's Status Register.</param>
        private void WriteNextByteToRegister(Register8Bit register, bool S, bool Z)
        {
            var nextByte = CPU.ReadNextByte();
            register.Write(nextByte);

            if (S)
            {
                this.HandleNegative(nextByte);
            }

            if (Z)
            {
                this.HandleZero(nextByte);
            }
        }
    }
}

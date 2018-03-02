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
                    this.OpCodeMap.Add(attrib.Code, new OpcodeContainer(attrib, this, methodInfo));
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
        // Also: http://www.obelisk.me.uk/6502/reference.html

        //[MOS6502Opcode(0x24, 3, "BIT", AddressingMode.ZeroPage)]
        //[MOS6502Opcode(0x2C, 4, "BIT", AddressingMode.Absolute)]
        public void BIT(OpcodeContainer container)
        {
        }

        [MOS6502Opcode(0xA2, "LDX", AddressingMode.Immediate, cycles: 2, pcDelta: 2)]
        public void LDX(OpcodeContainer container) => this.WriteNextByteToRegister(CPU.X, S: true, Z: true);

        [MOS6502Opcode(0x9A, "TXS", AddressingMode.Immediate, cycles: 2, pcDelta: 1)]
        public void TXS(OpcodeContainer container) => CPU.SP = CPU.X;

        [MOS6502Opcode(0xA9, "LDA", AddressingMode.Immediate, cycles: 2, pcDelta: 2)]
        [MOS6502Opcode(0xAD, "LDA", AddressingMode.Absolute, cycles: 4, pcDelta: 3)]
        public void LDA(OpcodeContainer container)
        {
            switch(container.AddressingMode)
            {
                case AddressingMode.Immediate:
                    this.WriteNextByteToRegister(CPU.A, S: true, Z: true);
                    break;
                case AddressingMode.Absolute:
                    ushort address = CPU.ReadNextShort();
                    this.WriteByteAtToRegister(CPU.A, address, S: true, Z: true);
                    break;
                default:
                    throw this.ExceptionForUnhandledAddressingMode(container);
            }
        }

        [MOS6502Opcode(0x8D, "STA", AddressingMode.Absolute, cycles: 4, pcDelta: 3)]
        public void STA(OpcodeContainer container)
        {
            switch (container.AddressingMode)
            {
                case AddressingMode.Absolute:
                    CPU.WriteByteAt(CPU.ReadNextShort(), CPU.A);
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected AddressingMode in STA: {container.AddressingMode}");
            }
        }

        #region Branches

        [MOS6502Opcode(0x10, "BPL", AddressingMode.Relative, cycles: 2, pcDelta: 2)]
        public void BPL(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(!CPU.IsNegative);

            container.Cycles = cycles;
            container.PCDelta = pcDelta;
        }

        #endregion

        #region Processor Status Instructions

        [MOS6502Opcode(0x78, "SEI", AddressingMode.Implied, cycles: 2, pcDelta: 1)]
        public void SEI(OpcodeContainer container) => CPU.AreInterruptsDisabled = true;

        [MOS6502Opcode(0x58, "CLI", AddressingMode.Implied, cycles: 2, pcDelta: 1)]
        public void CLI(OpcodeContainer container) => CPU.AreInterruptsDisabled = false;

        [MOS6502Opcode(0xF8, "SED", AddressingMode.Implied, cycles: 2, pcDelta: 1)]
        public void SED(OpcodeContainer container) => CPU.IsDecimalMode = true;

        [MOS6502Opcode(0xD8, "CLD", AddressingMode.Implied, cycles: 2, pcDelta: 1)]
        public void CLD(OpcodeContainer container) => CPU.IsDecimalMode = false;

        [MOS6502Opcode(0x38, "SEC", AddressingMode.Implied, cycles: 2, pcDelta: 1)]
        public void SEC(OpcodeContainer container) => CPU.IsCarry = true;

        [MOS6502Opcode(0x18, "CLC", AddressingMode.Implied, cycles: 2, pcDelta: 1)]
        public void CLC(OpcodeContainer container) => CPU.IsCarry = false;

        [MOS6502Opcode(0xB8, "CLV", AddressingMode.Implied, cycles: 2, pcDelta: 1)]
        public void CLV(OpcodeContainer container) => CPU.IsOverflowed = false;

        #endregion

        // TODO: hmmm...seems too easy, we'll see. 0x80 = 128, max of signed byte.
        private void HandleNegative(byte nextByte) => CPU.IsNegative = nextByte >= 0x80;
        private void HandleZero(byte operand) => CPU.IsZero = operand == 0;

        private (int cycles, int pcDelta) Branch(bool condition)
        {
            // TODO: handle page boundaries
            var originalPC = CPU.PC.Read();
            var cycles = 2;

            if (condition)
            {
                var offset = (sbyte)CPU.ReadNextByte();
                ushort newPC = (ushort)(CPU.PC + offset);
                CPU.SetPCTo(newPC);

                cycles++;
            }

            // Adding 1 to PC delta to compensate for the original PC++ to get here.
            return (cycles, 1 + CPU.PC - originalPC);
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

        /// <summary>
        /// Has the CPU read the byte at a particular address, and write it to an 8bit register.
        /// Optionally, it can set CPU status flags based on read byte:
        /// Z: Zero Flag
        /// S: Sign Flag
        ///
        /// </summary>
        /// <param name="register">The register to write to.</param>
        /// <param name="address">The address to read the byte from.</param>
        /// <param name="S">Whether or not to set the Sign flag on the CPU's Status Register.</param>
        /// <param name="Z">Whether or not to set the Zero flag on the CPU's Status Register.</param>
        private void WriteByteAtToRegister(Register8Bit register, ushort address, bool S, bool Z)
        {
            var nextByte = CPU.ReadByteAt(address);
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

        private InvalidOperationException ExceptionForUnhandledAddressingMode(OpcodeContainer container) => new InvalidOperationException($"Unhandled AddressMode ({container.AddressingMode}) for Opcode: ({container.Name})");
    }
}

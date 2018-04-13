using MICE.Components.CPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MICE.CPU.MOS6502
{
    public class oldOpcodes
    {
        private Dictionary<int, int> opCodeCount = new Dictionary<int, int>();
        private MOS6502 CPU;

        public oldOpcodes(MOS6502 cpu)
        {
            this.CPU = cpu;

            //var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            //foreach (var methodInfo in typeof(Opcodes).GetMethods(bindingFlags).Where(m => m.CustomAttributes.Any(ca => ca.AttributeType == typeof(MOS6502OpcodeAttribute))))
            //{
            //    foreach (var attrib in methodInfo.GetCustomAttributes<MOS6502OpcodeAttribute>())
            //    {
            //        this.OpCodeMap.Add(attrib.Code, new OpcodeContainer(attrib, this, methodInfo));
            //    }
            //}
        }

        public Dictionary<int, OpcodeContainer> OpCodeMap = new Dictionary<int, OpcodeContainer>();

        public OpcodeContainer this[int code]
        {
            get
            {
                try
                {
                    return this.OpCodeMap[code];
                }
                catch
                {
                    throw new InvalidOperationException($"Opcode requested that isn't implemented: 0x{code:X}");
                }
            }
        }

        // Most of the following is from: http://www.6502.org/tutorials/6502opcodes.html
        // Also: http://www.obelisk.me.uk/6502/reference.html
        // And: http://www.thealmightyguru.com/Games/Hacking/Wiki/index.php?title=6502_Opcodes
        // Unofficial Opcodes: http://www.oxyron.de/html/opcodes02.html

        #region Bit Operations

        [MOS6502Opcode(0x24, "BIT", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0x2C, "BIT", AddressingModes.Absolute, timing: 4, length: 3)]
        public void BIT(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            CPU.IsOverflowed = (value & 0x40) != 0;
            CPU.IsNegative = (value & 0x80) != 0;
            CPU.IsZero = (value & CPU.Registers.A) == 0;
        }

        [MOS6502Opcode(0x09, "ORA", AddressingModes.Immediate, timing: 2, length: 2)]
        [MOS6502Opcode(0x05, "ORA", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0x15, "ORA", AddressingModes.ZeroPageX, timing: 4, length: 2)]
        [MOS6502Opcode(0x0D, "ORA", AddressingModes.Absolute, timing: 4, length: 3)]
        [MOS6502Opcode(0x1D, "ORA", AddressingModes.AbsoluteX, timing: 4, length: 3)]
        [MOS6502Opcode(0x19, "ORA", AddressingModes.AbsoluteY, timing: 4, length: 3)]
        [MOS6502Opcode(0x01, "ORA", AddressingModes.IndirectX, timing: 6, length: 2)]
        [MOS6502Opcode(0x11, "ORA", AddressingModes.IndirectY, timing: 5, length: 2)]
        public void ORA(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            CPU.Registers.A.Write((byte)(CPU.Registers.A | value));

            this.HandleNegative(CPU.Registers.A);
            this.HandleZero(CPU.Registers.A);

            //this.HandlePageBoundaryCrossed(container, result.IsSamePage);
        }

        [MOS6502Opcode(0x29, "AND", AddressingModes.Immediate, timing: 2, length: 2)]
        [MOS6502Opcode(0x25, "AND", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0x35, "AND", AddressingModes.ZeroPageX, timing: 4, length: 2)]
        [MOS6502Opcode(0x2D, "AND", AddressingModes.Absolute, timing: 4, length: 3)]
        [MOS6502Opcode(0x3D, "AND", AddressingModes.AbsoluteX, timing: 4, length: 3)]
        [MOS6502Opcode(0x39, "AND", AddressingModes.AbsoluteY, timing: 4, length: 3)]
        [MOS6502Opcode(0x21, "AND", AddressingModes.IndirectX, timing: 6, length: 2)]
        [MOS6502Opcode(0x31, "AND", AddressingModes.IndirectY, timing: 5, length: 2)]
        public void AND(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            var newValue = CPU.Registers.A & value;
            this.WriteByteToRegister(CPU.Registers.A, (byte)newValue, S: true, Z: true);

            //this.HandlePageBoundaryCrossed(container, result.IsSamePage);
        }

        [MOS6502Opcode(0x4A, "LSR", AddressingModes.Accumulator, timing: 2, length: 1)]
        [MOS6502Opcode(0x46, "LSR", AddressingModes.ZeroPage, timing: 5, length: 2)]
        [MOS6502Opcode(0x56, "LSR", AddressingModes.ZeroPageX, timing: 6, length: 2)]
        [MOS6502Opcode(0x4E, "LSR", AddressingModes.Absolute, timing: 6, length: 3)]
        [MOS6502Opcode(0x5E, "LSR", AddressingModes.AbsoluteX, timing: 7, length: 3)]
        public void LSR(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            switch (container.AddressingMode)
            {
                case AddressingModes.Accumulator:
                    CPU.IsCarry = (CPU.Registers.A & 1) == 1;
                    CPU.Registers.A.Write((byte)(CPU.Registers.A >> 1));

                    CPU.IsNegative = false;
                    this.HandleZero(CPU.Registers.A);
                    break;
                default:
                    CPU.IsCarry = (value & 1) == 1;
                    byte shiftedValue = (byte)(value >> 1);
                    CPU.WriteByteAt(address, shiftedValue);

                    CPU.IsNegative = false;
                    this.HandleZero(shiftedValue);
                    break;
            }
        }

        [MOS6502Opcode(0x6A, "ROR", AddressingModes.Accumulator, timing: 2, length: 1)]
        [MOS6502Opcode(0x66, "ROR", AddressingModes.ZeroPage, timing: 5, length: 2)]
        [MOS6502Opcode(0x76, "ROR", AddressingModes.ZeroPageX, timing: 6, length: 2)]
        [MOS6502Opcode(0x6E, "ROR", AddressingModes.Absolute, timing: 6, length: 3)]
        [MOS6502Opcode(0x7E, "ROR", AddressingModes.AbsoluteX, timing: 7, length: 3)]
        public void ROR(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            var originalCarry = CPU.IsCarry ? 1 : 0;
            var newCarry = (byte)(value & 1);

            var newValue = (byte)(originalCarry << 7 | value >> 1);
            CPU.IsCarry = newCarry == 1;

            switch (container.AddressingMode)
            {
                case AddressingModes.Accumulator:
                    this.WriteByteToRegister(CPU.Registers.A, newValue, S: false, Z: false);
                    break;
                default:
                    CPU.WriteByteAt(address, newValue);
                    break;
            }

            this.HandleNegative(newValue);
            this.HandleZero(newValue);
        }

        [MOS6502Opcode(0x2A, "ROL", AddressingModes.Accumulator, timing: 2, length: 1)]
        [MOS6502Opcode(0x26, "ROL", AddressingModes.ZeroPage, timing: 5, length: 2)]
        [MOS6502Opcode(0x36, "ROL", AddressingModes.ZeroPageX, timing: 6, length: 2)]
        [MOS6502Opcode(0x2E, "ROL", AddressingModes.Absolute, timing: 6, length: 3)]
        [MOS6502Opcode(0x3E, "ROL", AddressingModes.AbsoluteX, timing: 7, length: 3)]
        public void ROL(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            byte originalCarry = (byte)(CPU.IsCarry ? 1 : 0);
            CPU.IsCarry = (value & 0b10000000) == 0x80;

            value = (byte)(value << 1 | originalCarry);

            switch (container.AddressingMode)
            {
                case AddressingModes.Accumulator:
                    this.WriteByteToRegister(CPU.Registers.A, value, S: false, Z: false);
                    break;
                default:
                    CPU.WriteByteAt(address, value);
                    break;
            }

            this.HandleNegative(value);
            this.HandleZero(value);
        }

        [MOS6502Opcode(0x0A, "ASL", AddressingModes.Accumulator, timing: 2, length: 1)]
        [MOS6502Opcode(0x06, "ASL", AddressingModes.ZeroPage, timing: 5, length: 2)]
        [MOS6502Opcode(0x16, "ASL", AddressingModes.ZeroPageX, timing: 6, length: 2)]
        [MOS6502Opcode(0x0E, "ASL", AddressingModes.Absolute, timing: 6, length: 3)]
        [MOS6502Opcode(0x1E, "ASL", AddressingModes.AbsoluteX, timing: 7, length: 3)]
        public void ASL(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            CPU.IsCarry = (value & 0b10000000) == 0b10000000;
            value = (byte)(value << 1);

            if (container.AddressingMode == AddressingModes.Accumulator)
            {
                this.WriteByteToRegister(CPU.Registers.A, value, S: true, Z: true);
            }
            else
            {
                CPU.WriteByteAt(address, value);
                this.HandleNegative(value);
                this.HandleZero(value);
            }
        }

        [MOS6502Opcode(0xEE, "INC", AddressingModes.Absolute, timing: 6, length: 3)]
        [MOS6502Opcode(0xFE, "INC", AddressingModes.AbsoluteX, timing: 7, length: 3)]
        [MOS6502Opcode(0xE6, "INC", AddressingModes.ZeroPage, timing: 5, length: 2)]
        [MOS6502Opcode(0xF6, "INC", AddressingModes.ZeroPageX, timing: 6, length: 2)]
        public void INC(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            CPU.WriteByteAt(address, ++value);

            this.HandleNegative(value);
            this.HandleZero(value);
        }

        [MOS6502Opcode(0xC6, "DEC", AddressingModes.ZeroPage, timing: 5, length: 2)]
        [MOS6502Opcode(0xD6, "DEC", AddressingModes.ZeroPageX, timing: 6, length: 2)]
        [MOS6502Opcode(0xCE, "DEC", AddressingModes.Absolute, timing: 6, length: 3)]
        [MOS6502Opcode(0xDE, "DEC", AddressingModes.AbsoluteX, timing: 7, length: 3)]
        public void DEC(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            value--;
            CPU.WriteByteAt(address, value);

            this.HandleNegative(value);
            this.HandleZero(value);
        }

        [MOS6502Opcode(0x49, "EOR", AddressingModes.Immediate, timing: 2, length: 2)]
        [MOS6502Opcode(0x45, "EOR", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0x55, "EOR", AddressingModes.ZeroPageX, timing: 4, length: 2)]
        [MOS6502Opcode(0x4D, "EOR", AddressingModes.Absolute, timing: 4, length: 3)]
        [MOS6502Opcode(0x5D, "EOR", AddressingModes.AbsoluteX, timing: 4, length: 3)]
        [MOS6502Opcode(0x59, "EOR", AddressingModes.AbsoluteY, timing: 4, length: 3)]
        [MOS6502Opcode(0x41, "EOR", AddressingModes.IndirectX, timing: 6, length: 2)]
        [MOS6502Opcode(0x51, "EOR", AddressingModes.IndirectY, timing: 5, length: 2)]
        public void EOR(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            var newValue = CPU.Registers.A ^ value;

            this.WriteByteToRegister(CPU.Registers.A, (byte)newValue, S: true, Z: true);
        }

        #endregion

        #region Compare Operations

        [MOS6502Opcode(0xC0, "CPY", AddressingModes.Immediate, timing: 2, length: 2)]
        [MOS6502Opcode(0xC4, "CPY", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0xCC, "CPY", AddressingModes.Absolute, timing: 4, length: 3)]
        public void CPY(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            this.CompareValues(CPU.Registers.Y, value);
        }

        [MOS6502Opcode(0xE0, "CPX", AddressingModes.Immediate, timing: 2, length: 2)]
        [MOS6502Opcode(0xE4, "CPX", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0xEC, "CPX", AddressingModes.Absolute, timing: 4, length: 3)]
        public void CPX(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            this.CompareValues(CPU.Registers.X, value);
        }

        [MOS6502Opcode(0xC9, "CMP", AddressingModes.Immediate, timing: 2, length: 2)]
        [MOS6502Opcode(0xC5, "CMP", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0xD5, "CMP", AddressingModes.ZeroPageX, timing: 4, length: 2)]
        [MOS6502Opcode(0xCD, "CMP", AddressingModes.Absolute, timing: 4, length: 3)]
        [MOS6502Opcode(0xDD, "CMP", AddressingModes.AbsoluteX, timing: 4, length: 3)]
        [MOS6502Opcode(0xD9, "CMP", AddressingModes.AbsoluteY, timing: 4, length: 3)]
        [MOS6502Opcode(0xC1, "CMP", AddressingModes.IndirectX, timing: 6, length: 2)]
        [MOS6502Opcode(0xD1, "CMP", AddressingModes.IndirectY, timing: 5, length: 2)]
        public void CMP(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            this.CompareValues(CPU.Registers.A, value, S: true, Z: true, C: true);

            //this.HandlePageBoundaryCrossed(container, result.IsSamePage);
        }

        #endregion

        #region Load Operations

        [MOS6502Opcode(0xA2, "LDX", AddressingModes.Immediate, timing: 2, length: 2)]
        [MOS6502Opcode(0xA6, "LDX", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0xB6, "LDX", AddressingModes.ZeroPageY, timing: 4, length: 2)]
        [MOS6502Opcode(0xBE, "LDX", AddressingModes.AbsoluteY, timing: 4, length: 3)]
        [MOS6502Opcode(0xAE, "LDX", AddressingModes.Absolute, timing: 4, length: 3)]
        public void LDX(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            this.WriteByteToRegister(CPU.Registers.X, value, S: true, Z: true);
        }

        [MOS6502Opcode(0xA4, "LDY", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0xB4, "LDY", AddressingModes.ZeroPageX, timing: 4, length: 2)]
        [MOS6502Opcode(0xBC, "LDY", AddressingModes.AbsoluteX, timing: 4, length: 3)]
        [MOS6502Opcode(0xAC, "LDY", AddressingModes.Absolute, timing: 4, length: 3)]
        [MOS6502Opcode(0xA0, "LDY", AddressingModes.Immediate, timing: 2, length: 2)]
        public void LDY(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            this.WriteByteToRegister(CPU.Registers.Y, value, S: true, Z: true);

            //this.HandlePageBoundaryCrossed(container, result.IsSamePage);
        }

        [MOS6502Opcode(0xA9, "LDA", AddressingModes.Immediate, timing: 2, length: 2)]
        [MOS6502Opcode(0xA5, "LDA", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0xB5, "LDA", AddressingModes.ZeroPageX, timing: 4, length: 2)]
        [MOS6502Opcode(0xAD, "LDA", AddressingModes.Absolute, timing: 4, length: 3)]
        [MOS6502Opcode(0xBD, "LDA", AddressingModes.AbsoluteX, timing: 4, length: 3)]
        [MOS6502Opcode(0xB9, "LDA", AddressingModes.AbsoluteY, timing: 4, length: 3)]
        [MOS6502Opcode(0xA1, "LDA", AddressingModes.IndirectX, timing: 6, length: 2)]
        [MOS6502Opcode(0xB1, "LDA", AddressingModes.IndirectY, timing: 5, length: 2)]
        public void LDA(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            this.WriteByteToRegister(CPU.Registers.A, value, S: true, Z: true);

            //this.HandlePageBoundaryCrossed(container, result.IsSamePage);
        }

        #endregion

        #region Stack Instructions

        [MOS6502Opcode(0x9A, "TXS", AddressingModes.Immediate, timing: 2, length: 1)]
        public void TXS(OpcodeContainer container) => CPU.Registers.SP.Write(CPU.Registers.X);

        [MOS6502Opcode(0xBA, "TSX", AddressingModes.Immediate, timing: 2, length: 1)]
        public void TSX(OpcodeContainer container) => this.WriteByteToRegister(CPU.Registers.X, CPU.Registers.SP, S: true, Z: true);

        [MOS6502Opcode(0x48, "PHA", AddressingModes.Immediate, timing: 3, length: 1)]
        public void PHA(OpcodeContainer container) => CPU.Stack.Push(CPU.Registers.A);

        [MOS6502Opcode(0x68, "PLA", AddressingModes.Immediate, timing: 4, length: 1)]
        public void PLA(OpcodeContainer container) => this.WriteByteToRegister(CPU.Registers.A, CPU.Stack.PopByte(), S: true, Z: true);

        [MOS6502Opcode(0x08, "PHP", AddressingModes.Immediate, timing: 3, length: 1)]
        public void PHP(OpcodeContainer container) => CPU.Stack.Push((byte)(CPU.Registers.P | 0x10));

        [MOS6502Opcode(0x28, "PLP", AddressingModes.Immediate, timing: 4, length: 1)]
        public void PLP(OpcodeContainer container)
        {
            this.WriteByteToRegister(CPU.Registers.P, CPU.Stack.PopByte(), S: false, Z: false);
            CPU.WillBreak = false;
            CPU.Reserved = true;
        }

        #endregion

        #region STore

        [MOS6502Opcode(0x85, "STA", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0x95, "STA", AddressingModes.ZeroPageX, timing: 4, length: 2)]
        [MOS6502Opcode(0x8D, "STA", AddressingModes.Absolute, timing: 4, length: 3)]
        [MOS6502Opcode(0x9D, "STA", AddressingModes.AbsoluteX, timing: 5, length: 3)]
        [MOS6502Opcode(0x99, "STA", AddressingModes.AbsoluteY, timing: 5, length: 3)]
        [MOS6502Opcode(0x81, "STA", AddressingModes.IndirectX, timing: 6, length: 2)]
        [MOS6502Opcode(0x91, "STA", AddressingModes.IndirectY, timing: 6, length: 2)]
        public void STA(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);

            CPU.WriteByteAt(address, CPU.Registers.A);
        }

        [MOS6502Opcode(0x86, "STX", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0x96, "STX", AddressingModes.ZeroPageY, timing: 4, length: 2)]
        [MOS6502Opcode(0x8E, "STX", AddressingModes.Absolute, timing: 4, length: 3)]
        public void STX(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            CPU.WriteByteAt(address, CPU.Registers.X);
        }

        [MOS6502Opcode(0x84, "STY", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0x94, "STY", AddressingModes.ZeroPageX, timing: 4, length: 2)]
        [MOS6502Opcode(0x8C, "STY", AddressingModes.Absolute, timing: 4, length: 3)]
        public void STY(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);

            CPU.WriteByteAt(address, CPU.Registers.Y);
        }

        #endregion

        #region Jumps

        [MOS6502Opcode(0x00, "BRK", AddressingModes.Implied, timing: 7, length: 1, verify: false)]
        public void BRK(OpcodeContainer container)
        {
            CPU.WillBreak = true;
            CPU.Reserved = true;

            CPU.HandleInterruptRequest(InterruptType.IRQ, (ushort)(CPU.Registers.PC + 1));
            CPU.AreInterruptsDisabled = true;
        }

        [MOS6502Opcode(0x04, "NOP", AddressingModes.ZeroPage, timing: 3, length: 2, unofficial: true)]
        [MOS6502Opcode(0x44, "NOP", AddressingModes.ZeroPage, timing: 3, length: 2, unofficial: true)]
        [MOS6502Opcode(0x64, "NOP", AddressingModes.ZeroPage, timing: 3, length: 2, unofficial: true)]
        [MOS6502Opcode(0x14, "NOP", AddressingModes.ZeroPageX, timing: 4, length: 2, unofficial: true)]
        [MOS6502Opcode(0x34, "NOP", AddressingModes.ZeroPageX, timing: 4, length: 2, unofficial: true)]
        [MOS6502Opcode(0x54, "NOP", AddressingModes.ZeroPageX, timing: 4, length: 2, unofficial: true)]
        [MOS6502Opcode(0x74, "NOP", AddressingModes.ZeroPageX, timing: 4, length: 2, unofficial: true)]
        [MOS6502Opcode(0xD4, "NOP", AddressingModes.ZeroPageX, timing: 4, length: 2, unofficial: true)]
        [MOS6502Opcode(0xF4, "NOP", AddressingModes.ZeroPageX, timing: 4, length: 2, unofficial: true)]
        [MOS6502Opcode(0x0C, "NOP", AddressingModes.Absolute, timing: 4, length: 3, unofficial: true)]
        [MOS6502Opcode(0x1C, "NOP", AddressingModes.AbsoluteX, timing: 4, length: 3, unofficial: true)]
        [MOS6502Opcode(0x3C, "NOP", AddressingModes.AbsoluteX, timing: 4, length: 3, unofficial: true)]
        [MOS6502Opcode(0x5C, "NOP", AddressingModes.AbsoluteX, timing: 4, length: 3, unofficial: true)]
        [MOS6502Opcode(0x7C, "NOP", AddressingModes.AbsoluteX, timing: 4, length: 3, unofficial: true)]
        [MOS6502Opcode(0xDC, "NOP", AddressingModes.AbsoluteX, timing: 4, length: 3, unofficial: true)]
        [MOS6502Opcode(0xFC, "NOP", AddressingModes.AbsoluteX, timing: 4, length: 3, unofficial: true)]
        [MOS6502Opcode(0x1A, "NOP", AddressingModes.Implied, timing: 2, length: 1, unofficial: true)]
        [MOS6502Opcode(0x3A, "NOP", AddressingModes.Implied, timing: 2, length: 1, unofficial: true)]
        [MOS6502Opcode(0x5A, "NOP", AddressingModes.Implied, timing: 2, length: 1, unofficial: true)]
        [MOS6502Opcode(0x7A, "NOP", AddressingModes.Implied, timing: 2, length: 1, unofficial: true)]
        [MOS6502Opcode(0xDA, "NOP", AddressingModes.Implied, timing: 2, length: 1)]
        [MOS6502Opcode(0xEA, "NOP", AddressingModes.Implied, timing: 2, length: 1)]
        [MOS6502Opcode(0xFA, "NOP", AddressingModes.Implied, timing: 2, length: 1, unofficial: true)]
        [MOS6502Opcode(0x80, "NOP", AddressingModes.Immediate, timing: 2, length: 2, unofficial: true)]
        [MOS6502Opcode(0x82, "NOP", AddressingModes.Immediate, timing: 2, length: 2, unofficial: true)]
        [MOS6502Opcode(0xC2, "NOP", AddressingModes.Immediate, timing: 2, length: 2, unofficial: true)]
        [MOS6502Opcode(0xE2, "NOP", AddressingModes.Immediate, timing: 2, length: 2, unofficial: true)]
        [MOS6502Opcode(0x89, "NOP", AddressingModes.Immediate, timing: 2, length: 2, unofficial: true)]
        public void NOP(OpcodeContainer container) => AddressingMode.GetAddressedOperand(CPU, container);

        [MOS6502Opcode(0x6C, "JMP", AddressingModes.Indirect, timing: 5, length: 3, verify: false)]
        [MOS6502Opcode(0x4C, "JMP", AddressingModes.Absolute, timing: 3, length: 3, verify: false)]
        public void JMP(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);

            // 6502 bug in indirect JMP...
            var nextShort = CPU.ReadNextShort();
            if (container.AddressingMode == AddressingModes.Indirect && (nextShort & 0x00FF) == 0xFF)
            {
                byte lowByte = CPU.ReadByteAt(nextShort);
                byte highByte = CPU.ReadByteAt((ushort)(nextShort - 0x00FF));

                address = (ushort)(highByte << 8 | lowByte);
            }

            CPU.SetPCTo(address);
        }

        [MOS6502Opcode(0x20, "JSR", AddressingModes.Absolute, timing: 6, length: 3, verify: false)]
        public void JSR(OpcodeContainer container)
        {
            CPU.Stack.Push((ushort)(CPU.Registers.PC + 1));
            var nextPC = CPU.ReadNextShort();
            CPU.SetPCTo(nextPC);

            CPU.LastAccessedAddress = $"${nextPC:X4}";
        }

        [MOS6502Opcode(0x60, "RTS", AddressingModes.Absolute, timing: 6, length: 3, verify: false)]
        public void RTS(OpcodeContainer container)
        {
            var address = CPU.Stack.PopShort();
            CPU.SetPCTo((ushort)(address + 1));
        }

        [MOS6502Opcode(0x40, "RTI", AddressingModes.Implied, timing: 6, length: 1, verify: false)]
        public void RTI(OpcodeContainer container)
        {
            this.WriteByteToRegister(CPU.Registers.P, CPU.Stack.PopByte(), S: false, Z: false);
            CPU.SetPCTo(CPU.Stack.PopShort());
        }

        #endregion

        #region Branches

        [MOS6502Opcode(0x10, "BPL", AddressingModes.Relative, timing: 2, length: 2)]
        public void BPL(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(!CPU.IsNegative);

            container.AddedCycles = cycles;
            container.PCDelta = pcDelta;
        }

        [MOS6502Opcode(0x30, "BMI", AddressingModes.Relative, timing: 2, length: 2)]
        public void BMI(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(CPU.IsNegative);

            container.AddedCycles = cycles;
            container.PCDelta = pcDelta;
        }

        [MOS6502Opcode(0x50, "BVC", AddressingModes.Relative, timing: 2, length: 2)]
        public void BVC(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(!CPU.IsOverflowed);

            container.AddedCycles = cycles;
            container.PCDelta = pcDelta;
        }

        [MOS6502Opcode(0x70, "BVS", AddressingModes.Relative, timing: 2, length: 2)]
        public void BVS(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(CPU.IsOverflowed);

            container.AddedCycles = cycles;
            container.PCDelta = pcDelta;
        }

        [MOS6502Opcode(0x90, "BCC", AddressingModes.Relative, timing: 2, length: 2)]
        public void BCC(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(!CPU.IsCarry);

            container.AddedCycles = cycles;
            container.PCDelta = pcDelta;
        }

        [MOS6502Opcode(0xB0, "BCS", AddressingModes.Relative, timing: 2, length: 2)]
        public void BCS(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(CPU.IsCarry);

            container.AddedCycles = cycles;
            container.PCDelta = pcDelta;
        }

        [MOS6502Opcode(0xD0, "BNE", AddressingModes.Relative, timing: 2, length: 2)]
        public void BNE(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(CPU.IsZero == false);

            container.AddedCycles = cycles;
            container.PCDelta = pcDelta;
        }

        [MOS6502Opcode(0xF0, "BEQ", AddressingModes.Relative, timing: 2, length: 2)]
        public void BEQ(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(CPU.IsZero);

            container.AddedCycles = cycles;
            container.PCDelta = pcDelta;
        }

        #endregion

        #region Processor Status Instructions

        [MOS6502Opcode(0x78, "SEI", AddressingModes.Implied, timing: 2, length: 1)]
        public void SEI(OpcodeContainer container) => CPU.AreInterruptsDisabled = true;

        [MOS6502Opcode(0x58, "CLI", AddressingModes.Implied, timing: 2, length: 1)]
        public void CLI(OpcodeContainer container) => CPU.AreInterruptsDisabled = false;

        [MOS6502Opcode(0xF8, "SED", AddressingModes.Implied, timing: 2, length: 1)]
        public void SED(OpcodeContainer container) => CPU.IsDecimalMode = true;

        [MOS6502Opcode(0xD8, "CLD", AddressingModes.Implied, timing: 2, length: 1)]
        public void CLD(OpcodeContainer container) => CPU.IsDecimalMode = false;

        [MOS6502Opcode(0x38, "SEC", AddressingModes.Implied, timing: 2, length: 1)]
        public void SEC(OpcodeContainer container) => CPU.IsCarry = true;

        [MOS6502Opcode(0x18, "CLC", AddressingModes.Implied, timing: 2, length: 1)]
        public void CLC(OpcodeContainer container) => CPU.IsCarry = false;

        [MOS6502Opcode(0xB8, "CLV", AddressingModes.Implied, timing: 2, length: 1)]
        public void CLV(OpcodeContainer container) => CPU.IsOverflowed = false;

        #endregion

        #region Register Instructions

        [MOS6502Opcode(0xCA, "DEX", AddressingModes.Implied, timing: 2, length: 1)]
        public void DEX(OpcodeContainer container)
        {
            byte x = CPU.Registers.X;
            CPU.Registers.X.Write(--x);

            this.HandleNegative(CPU.Registers.X);
            this.HandleZero(CPU.Registers.X);
        }

        [MOS6502Opcode(0x88, "DEY", AddressingModes.Implied, timing: 2, length: 1)]
        public void DEY(OpcodeContainer container)
        {
            byte y = CPU.Registers.Y;
            CPU.Registers.Y.Write(--y);

            this.HandleNegative(CPU.Registers.Y);
            this.HandleZero(CPU.Registers.Y);
        }

        [MOS6502Opcode(0xC8, "INY", AddressingModes.Implied, timing: 2, length: 1)]
        public void INY(OpcodeContainer container)
        {
            byte y = CPU.Registers.Y;

            CPU.Registers.Y.Write(++y);

            this.HandleNegative(CPU.Registers.Y);
            this.HandleZero(CPU.Registers.Y);
        }

        [MOS6502Opcode(0xE8, "INX", AddressingModes.Implied, timing: 2, length: 1)]
        public void INX(OpcodeContainer container)
        {
            byte x = CPU.Registers.X;

            this.WriteByteToRegister(CPU.Registers.X, ++x, S: true, Z: true);
        }

        [MOS6502Opcode(0x8A, "TXA", AddressingModes.Immediate, timing: 2, length: 1)]
        public void TXA(OpcodeContainer container) => this.WriteByteToRegister(CPU.Registers.A, CPU.Registers.X, S: true, Z: true);

        [MOS6502Opcode(0xAA, "TAX", AddressingModes.Immediate, timing: 2, length: 1)]
        public void TAX(OpcodeContainer container) => this.WriteByteToRegister(CPU.Registers.X, CPU.Registers.A, S: true, Z: true);

        [MOS6502Opcode(0xA8, "TAY", AddressingModes.Immediate, timing: 2, length: 1)]
        public void TAY(OpcodeContainer container) => this.WriteByteToRegister(CPU.Registers.Y, CPU.Registers.A, S: true, Z: true);

        [MOS6502Opcode(0x98, "TYA", AddressingModes.Immediate, timing: 2, length: 1)]
        public void TYA(OpcodeContainer container) => this.WriteByteToRegister(CPU.Registers.A, CPU.Registers.Y, S: true, Z: true);

        #endregion

        #region Math Instructions

        // https://stackoverflow.com/a/29224684/1865301
        [MOS6502Opcode(0x69, "ADC", AddressingModes.Immediate, timing: 2, length: 2)]
        [MOS6502Opcode(0x65, "ADC", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0x75, "ADC", AddressingModes.ZeroPageX, timing: 4, length: 2)]
        [MOS6502Opcode(0x6D, "ADC", AddressingModes.Absolute, timing: 4, length: 3)]
        [MOS6502Opcode(0x7D, "ADC", AddressingModes.AbsoluteX, timing: 4, length: 3)]
        [MOS6502Opcode(0x79, "ADC", AddressingModes.AbsoluteY, timing: 4, length: 3)]
        [MOS6502Opcode(0x61, "ADC", AddressingModes.IndirectX, timing: 6, length: 2)]
        [MOS6502Opcode(0x71, "ADC", AddressingModes.IndirectY, timing: 5, length: 2)]
        public void ADC(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            this.ADD(container, value);
            //this.HandlePageBoundaryCrossed(container, result.IsSamePage);
        }

        [MOS6502Opcode(0xE9, "SBC", AddressingModes.Immediate, timing: 2, length: 2)]
        [MOS6502Opcode(0xEB, "SBC", AddressingModes.Immediate, timing: 2, length: 2, unofficial: true)]
        [MOS6502Opcode(0xE5, "SBC", AddressingModes.ZeroPage, timing: 3, length: 2)]
        [MOS6502Opcode(0xF5, "SBC", AddressingModes.ZeroPageX, timing: 4, length: 2)]
        [MOS6502Opcode(0xED, "SBC", AddressingModes.Absolute, timing: 4, length: 3)]
        [MOS6502Opcode(0xFD, "SBC", AddressingModes.AbsoluteX, timing: 4, length: 3)]
        [MOS6502Opcode(0xF9, "SBC", AddressingModes.AbsoluteY, timing: 4, length: 3)]
        [MOS6502Opcode(0xE1, "SBC", AddressingModes.IndirectX, timing: 6, length: 2)]
        [MOS6502Opcode(0xF1, "SBC", AddressingModes.IndirectY, timing: 5, length: 2)]
        public void SBC(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            this.ADD(container, (byte)(~value));
        }

        #endregion

        #region Unofficial

        [MOS6502Opcode(0x2B, "ANC", AddressingModes.Immediate, timing: 2, length: 2, verify: false, unofficial: true)]
        [MOS6502Opcode(0x0B, "ANC", AddressingModes.Immediate, timing: 2, length: 2, verify: false, unofficial: true)]
        public void ANC(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            this.WriteByteToRegister(CPU.Registers.A, (byte)(CPU.Registers.A & value), S: true, Z: true);
            CPU.IsCarry = (CPU.Registers.A & 0x80) != 0;
        }

        [MOS6502Opcode(0x4B, "ALR", AddressingModes.Immediate, timing: 2, length: 2, unofficial: true)]
        public void ALR(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            var andedValue = (CPU.Registers.A & value);
            CPU.IsCarry = (andedValue & 0x01) != 0;

            andedValue >>= 1;
            this.WriteByteToRegister(CPU.Registers.A, (byte)andedValue, S: true, Z: true);
        }

        [MOS6502Opcode(0x6B, "ARR", AddressingModes.Immediate, timing: 2, length: 2, unofficial: true)]
        public void ARR(OpcodeContainer container)
        {
            // Thank you open source community - Mesen's version. No idea how this was determined from the limited docs...
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            var newA = (((CPU.Registers.A & value) >> 1) | (CPU.IsCarry ? 0x80 : 0x00));

            this.WriteByteToRegister(CPU.Registers.A, (byte)newA, S: true, Z: true);

            CPU.IsCarry = (CPU.Registers.A & 0x40) != 0;
            CPU.IsOverflowed = ((CPU.IsCarry ? 0x01 : 0x00) ^ ((CPU.Registers.A >> 5) & 0x01)) != 0;
        }

        [MOS6502Opcode(0xAB, "LAX", AddressingModes.Immediate, timing: 2, length: 2, unofficial: true)]
        [MOS6502Opcode(0xA7, "LAX", AddressingModes.ZeroPage, timing: 3, length: 2, unofficial: true)]
        [MOS6502Opcode(0xB7, "LAX", AddressingModes.ZeroPageY, timing: 4, length: 2, unofficial: true)]
        [MOS6502Opcode(0xAF, "LAX", AddressingModes.Absolute, timing: 4, length: 3, unofficial: true)]
        [MOS6502Opcode(0xBF, "LAX", AddressingModes.AbsoluteY, timing: 4, length: 3, unofficial: true)]
        [MOS6502Opcode(0xA3, "LAX", AddressingModes.IndirectX, timing: 6, length: 2, unofficial: true)]
        [MOS6502Opcode(0xB3, "LAX", AddressingModes.IndirectY, timing: 5, length: 2, unofficial: true)]
        public void LAX(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            this.WriteByteToRegister(CPU.Registers.A, value, S: true, Z: true);
            this.WriteByteToRegister(CPU.Registers.X, value, S: true, Z: true);
        }

        [MOS6502Opcode(0xCB, "AXS", AddressingModes.Immediate, timing: 2, length: 2, unofficial: true)]
        public void AXS(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            byte newValue = (byte)(CPU.Registers.A & CPU.Registers.X);
            newValue -= value;

            CPU.IsCarry = (CPU.Registers.A & CPU.Registers.X) >= value;

            this.WriteByteToRegister(CPU.Registers.X, newValue, S: true, Z: true);
        }

        [MOS6502Opcode(0xE7, "ISC", AddressingModes.ZeroPage, timing: 5, length: 2, unofficial: true)]
        [MOS6502Opcode(0xF7, "ISC", AddressingModes.ZeroPageX, timing: 6, length: 2, unofficial: true)]
        [MOS6502Opcode(0xEF, "ISC", AddressingModes.Absolute, timing: 6, length: 3, unofficial: true)]
        [MOS6502Opcode(0xFF, "ISC", AddressingModes.AbsoluteX, timing: 7, length: 3, unofficial: true)]
        [MOS6502Opcode(0xFB, "ISC", AddressingModes.AbsoluteY, timing: 7, length: 3, unofficial: true)]
        [MOS6502Opcode(0xE3, "ISC", AddressingModes.IndirectX, timing: 8, length: 2, unofficial: true)]
        [MOS6502Opcode(0xF3, "ISC", AddressingModes.IndirectY, timing: 8, length: 2, unofficial: true)]
        public void ISC(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            value++;
            this.ADD(container, (byte)(value ^ 0xFF));
            CPU.WriteByteAt(address, value);
        }

        [MOS6502Opcode(0x07, "SLO", AddressingModes.ZeroPage, timing: 5, length: 2, unofficial: true)]
        [MOS6502Opcode(0x17, "SLO", AddressingModes.ZeroPageX, timing: 6, length: 2, unofficial: true)]
        [MOS6502Opcode(0x0F, "SLO", AddressingModes.Absolute, timing: 6, length: 3, unofficial: true)]
        [MOS6502Opcode(0x1F, "SLO", AddressingModes.AbsoluteX, timing: 7, length: 3, unofficial: true)]
        [MOS6502Opcode(0x1B, "SLO", AddressingModes.AbsoluteY, timing: 7, length: 3, unofficial: true)]
        [MOS6502Opcode(0x03, "SLO", AddressingModes.IndirectX, timing: 8, length: 2, unofficial: true)]
        [MOS6502Opcode(0x13, "SLO", AddressingModes.IndirectY, timing: 8, length: 2, unofficial: true)]
        public void SLO(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            CPU.IsCarry = (value & 0x80) != 0;

            byte shifted = (byte)(value << 1);
            this.HandleNegative(shifted);
            this.HandleZero(shifted);

            this.WriteByteToRegister(CPU.Registers.A, (byte)(CPU.Registers.A | shifted), S: true, Z: true);
            CPU.WriteByteAt(address, (byte)shifted);
        }

        [MOS6502Opcode(0x27, "RLA", AddressingModes.ZeroPage, timing: 5, length: 2, unofficial: true)]
        [MOS6502Opcode(0x37, "RLA", AddressingModes.ZeroPageX, timing: 6, length: 2, unofficial: true)]
        [MOS6502Opcode(0x2F, "RLA", AddressingModes.Absolute, timing: 6, length: 3, unofficial: true)]
        [MOS6502Opcode(0x3F, "RLA", AddressingModes.AbsoluteX, timing: 7, length: 3, unofficial: true)]
        [MOS6502Opcode(0x3B, "RLA", AddressingModes.AbsoluteY, timing: 7, length: 3, unofficial: true)]
        [MOS6502Opcode(0x23, "RLA", AddressingModes.IndirectX, timing: 8, length: 2, unofficial: true)]
        [MOS6502Opcode(0x33, "RLA", AddressingModes.IndirectY, timing: 8, length: 2, unofficial: true)]
        public void RLA(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            byte oldCarry = (byte)(CPU.IsCarry ? 0x0b00000001 : 0x0);
            CPU.IsCarry = (value & 0b10000000) != 0;

            byte shifted = (byte)(value << 1 | oldCarry);
            this.HandleNegative(shifted);
            this.HandleZero(shifted);

            this.WriteByteToRegister(CPU.Registers.A, (byte)(CPU.Registers.A & shifted), S: true, Z: true);

            CPU.WriteByteAt(address, shifted);
        }

        [MOS6502Opcode(0x47, "SRE", AddressingModes.ZeroPage, timing: 5, length: 2, unofficial: true)]
        [MOS6502Opcode(0x57, "SRE", AddressingModes.ZeroPageX, timing: 6, length: 2, unofficial: true)]
        [MOS6502Opcode(0x4F, "SRE", AddressingModes.Absolute, timing: 6, length: 3, unofficial: true)]
        [MOS6502Opcode(0x5F, "SRE", AddressingModes.AbsoluteX, timing: 7, length: 3, unofficial: true)]
        [MOS6502Opcode(0x5B, "SRE", AddressingModes.AbsoluteY, timing: 7, length: 3, unofficial: true)]
        [MOS6502Opcode(0x43, "SRE", AddressingModes.IndirectX, timing: 8, length: 2, unofficial: true)]
        [MOS6502Opcode(0x53, "SRE", AddressingModes.IndirectY, timing: 8, length: 2, unofficial: true)]
        public void SRE(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            CPU.IsCarry = (value & 0b00000001) != 0;

            var shifted = (byte)(value >> 1);
            this.HandleNegative(shifted);
            this.HandleZero(shifted);

            this.WriteByteToRegister(CPU.Registers.A, (byte)(CPU.Registers.A ^ shifted), S: true, Z: true);
            CPU.WriteByteAt(address, shifted);
        }

        [MOS6502Opcode(0x67, "RRA", AddressingModes.ZeroPage, timing: 5, length: 2, unofficial: true)]
        [MOS6502Opcode(0x77, "RRA", AddressingModes.ZeroPageX, timing: 6, length: 2, unofficial: true)]
        [MOS6502Opcode(0x6F, "RRA", AddressingModes.Absolute, timing: 6, length: 3, unofficial: true)]
        [MOS6502Opcode(0x7F, "RRA", AddressingModes.AbsoluteX, timing: 7, length: 3, unofficial: true)]
        [MOS6502Opcode(0x7B, "RRA", AddressingModes.AbsoluteY, timing: 7, length: 3, unofficial: true)]
        [MOS6502Opcode(0x63, "RRA", AddressingModes.IndirectX, timing: 8, length: 2, unofficial: true)]
        [MOS6502Opcode(0x73, "RRA", AddressingModes.IndirectY, timing: 8, length: 2, unofficial: true)]
        public void RRA(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            var shiftedValue = this.ROR(value);
            this.ADD(container, shiftedValue);
            CPU.WriteByteAt(address, shiftedValue);
        }

        [MOS6502Opcode(0x87, "SAX", AddressingModes.ZeroPage, timing: 5, length: 2, unofficial: true)]
        [MOS6502Opcode(0x97, "SAX", AddressingModes.ZeroPageY, timing: 4, length: 2, unofficial: true)]
        [MOS6502Opcode(0x8F, "SAX", AddressingModes.Absolute, timing: 4, length: 3, unofficial: true)]
        [MOS6502Opcode(0x83, "SAX", AddressingModes.IndirectX, timing: 6, length: 2, unofficial: true)]
        public void SAX(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);

            CPU.WriteByteAt(address, (byte)(CPU.Registers.A & CPU.Registers.X));
        }

        [MOS6502Opcode(0xC7, "DCP", AddressingModes.ZeroPage, timing: 5, length: 2, unofficial: true)]
        [MOS6502Opcode(0xD7, "DCP", AddressingModes.ZeroPageX, timing: 6, length: 2, unofficial: true)]
        [MOS6502Opcode(0xCF, "DCP", AddressingModes.Absolute, timing: 6, length: 3, unofficial: true)]
        [MOS6502Opcode(0xDF, "DCP", AddressingModes.AbsoluteX, timing: 7, length: 3, unofficial: true)]
        [MOS6502Opcode(0xDB, "DCP", AddressingModes.AbsoluteY, timing: 7, length: 3, unofficial: true)]
        [MOS6502Opcode(0xC3, "DCP", AddressingModes.IndirectX, timing: 7, length: 2, unofficial: true)]
        [MOS6502Opcode(0xD3, "DCP", AddressingModes.IndirectY, timing: 7, length: 2, unofficial: true)]
        public void DCP(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);
            var value = CPU.ReadByteAt(address);

            value--;
            this.CompareValues(CPU.Registers.A, value, S: true, Z: true, C: true);
            CPU.WriteByteAt(address, value);
        }

        //SYA - this seems to be a very poopy opcode to implement: http://forums.nesdev.com/viewtopic.php?f=3&t=3831&start=30
        // Implementation below is how Mesen implemented it.
        [MOS6502Opcode(0x9C, "SHY", AddressingModes.AbsoluteX, timing: 5, length: 3, unofficial: true)]
        public void SHY(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);

            byte addrHigh = (byte)(address >> 8);
            byte addrLow = (byte)(address & 0xFF);
            byte value = (byte)(CPU.Registers.Y & (addrHigh + 1));

            ushort newAddress = (ushort)(((CPU.Registers.Y & (addrHigh + 1)) << 8) | addrLow);
            CPU.WriteByteAt(newAddress, value);
        }

        // aka, SXA - this seems to be a very poopy opcode to implement: http://forums.nesdev.com/viewtopic.php?f=3&t=3831&start=30
        // Implementation below is how Mesen implemented it.
        [MOS6502Opcode(0x9E, "SHX", AddressingModes.AbsoluteY, timing: 5, length: 3, unofficial: true)]
        public void SHX(OpcodeContainer container)
        {
            var address = AddressingMode.GetAddressedOperand(CPU, container);

            byte addrHigh = (byte)(address >> 8);
            byte addrLow = (byte)(address & 0xFF);
            byte value = (byte)(CPU.Registers.X & (addrHigh + 1));

            ushort newAddress = (ushort)(((CPU.Registers.X & (addrHigh + 1)) << 8) | addrLow);
            CPU.WriteByteAt(newAddress, value);
        }

        #endregion

        private void ADD(OpcodeContainer container, byte operand)
        {
            var originalValue = CPU.Registers.A.Read();
            var sum = originalValue + operand + (CPU.IsCarry ? 1 : 0);

            CPU.IsCarry = sum >> 8 != 0;

            this.WriteByteToRegister(CPU.Registers.A, (byte)sum, S: true, Z: true);
            this.HandleOverflow(originalValue, operand, (byte)sum);
        }

        private void HandleNegative(byte operand) => CPU.IsNegative = operand >= 0x80;

        private void HandleZero(byte operand) => CPU.IsZero = operand == 0;

        // https://stackoverflow.com/a/29224684/1865301
        private void HandleOverflow(byte original, byte operand, byte value)
        {
            CPU.IsOverflowed = (~(original ^ operand) & 0x80) != 0 && ((original ^ value) & 0x80) != 0;
        }

        private byte ROR(byte value)
        {
            bool carryFlag = CPU.IsCarry;
            CPU.IsCarry = false;
            CPU.IsNegative = false;
            CPU.IsZero = false;

            CPU.IsCarry = (value & 0x01) != 0;

            byte result = (byte)(value >> 1 | (carryFlag ? 0x80 : 0x00));
            this.HandleNegative(result);
            this.HandleZero(result);

            return result;
        }

        private void CompareValues(byte value1, byte value2, bool S = true, bool Z = true, bool C = true)
        {
            if (C)
            {
                CPU.IsCarry = (value1 >= value2);
            }

            if (Z)
            {
                this.HandleNegative((byte)(value1 - value2));
            }

            if (S)
            {
                this.HandleZero((byte)(value1 - value2));
            }
        }

        private (int cycles, int pcDelta) Branch(bool condition)
        {
            var originalPC = CPU.Registers.PC.Read();
            var offset = (sbyte)CPU.ReadNextByte();
            ushort newPC = (ushort)(CPU.Registers.PC + offset);

            var cycles = 0;

            if (condition)
            {
                if (!this.AreSamePage(originalPC, newPC))
                {
                    cycles++;
                }

                CPU.SetPCTo(newPC);

                cycles++;
            }

            CPU.LastAccessedAddress = $"${newPC:X4}";

            // Adding 1 to PC delta to compensate for the original PC++ to get here.
            return (cycles, 1 + CPU.Registers.PC - originalPC);
        }

        /// <summary>
        /// Has the CPU read the next byte, and write it to an 8bit register.
        /// Optionally, it can set CPU status flags based on read byte:
        /// Z: Zero Flag
        /// S: Sign Flag
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

        /// <summary>
        /// Write byte to an 8bit register.
        /// Optionally, it can set CPU status flags based on read byte:
        /// Z: Zero Flag
        /// S: Sign Flag
        /// </summary>
        /// <param name="register">The register to write to.</param>
        /// <param name="byte">The byte to write..</param>
        /// <param name="S">Whether or not to set the Sign flag on the CPU's Status Register.</param>
        /// <param name="Z">Whether or not to set the Zero flag on the CPU's Status Register.</param>
        private void WriteByteToRegister(Register8Bit register, byte value, bool S, bool Z)
        {
            register.Write(value);

            if (S)
            {
                this.HandleNegative(value);
            }

            if (Z)
            {
                this.HandleZero(value);
            }
        }

        private bool AreSamePage(ushort a1, ushort a2) => ((a1 ^ a2) & 0xFF00) == 0;
        private void HandlePageBoundaryCrossed(OpcodeContainer container, bool? samePage)
        {
            if (samePage == null)
            {
                return;
            }

            switch (container.AddressingMode)
            {
                case AddressingModes.Relative:
                case AddressingModes.AbsoluteX:
                case AddressingModes.AbsoluteY:
                case AddressingModes.IndirectY:
                    if (!samePage.Value)
                    {
                        container.AddedCycles++;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

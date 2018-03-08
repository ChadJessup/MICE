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
        // And: http://www.thealmightyguru.com/Games/Hacking/Wiki/index.php?title=6502_Opcodes

        #region Bit Operations

        //[MOS6502Opcode(0x24, 3, "BIT", AddressingMode.ZeroPage)]
        [MOS6502Opcode(0x2C, "BIT", AddressingModes.Absolute, cycles: 4, pcDelta: 3)]
        public void BIT(OpcodeContainer container)
        {
            var value = CPU.ReadNextShort();
            var accumulator = CPU.Registers.A.Read();

            byte result = (byte)(value & accumulator);

            CPU.IncrementPC();

            this.HandleNegative(result);
            this.HandleZero(result);
            this.HandleOverflow(result);
        }

        [MOS6502Opcode(0x09, "ORA", AddressingModes.Immediate, cycles: 2, pcDelta: 2)]
        [MOS6502Opcode(0x05, "ORA", AddressingModes.ZeroPage, cycles: 3, pcDelta: 2)]
        public void ORA(OpcodeContainer container)
        {
            switch (container.AddressingMode)
            {
                case AddressingModes.Immediate:
                    CPU.Registers.A.Write((byte)(CPU.Registers.A | CPU.ReadNextByte()));
                    break;
                case AddressingModes.ZeroPage:
                    var zeroPageAddress = CPU.ReadNextByte();
                    CPU.Registers.A.Write((byte)(CPU.Registers.A | CPU.ReadByteAt(zeroPageAddress, incrementPC: false)));
                    break;
                default:
                    throw this.ExceptionForUnhandledAddressingMode(container);
            }

            this.HandleNegative(CPU.Registers.A);
            this.HandleZero(CPU.Registers.A);
        }

        [MOS6502Opcode(0x35, "AND", AddressingModes.ZeroPageX, cycles: 4, pcDelta: 2)]
        [MOS6502Opcode(0x25, "AND", AddressingModes.ZeroPage, cycles: 3, pcDelta: 2)]
        [MOS6502Opcode(0x3D, "AND", AddressingModes.AbsoluteX, cycles: 4, pcDelta: 3)]
        [MOS6502Opcode(0x29, "AND", AddressingModes.Immediate, cycles: 2, pcDelta: 2)]
        public void AND(OpcodeContainer container)
        {
            switch (container.AddressingMode)
            {
                case AddressingModes.ZeroPageX:
                    CPU.Registers.A.Write((byte)(CPU.Registers.A & AddressingMode.GetZeroPageX(CPU).value));
                    break;
                case AddressingModes.ZeroPage:
                    var zeroPageAddress = CPU.ReadNextByte(incrementPC: false);
                    CPU.Registers.A.Write((byte)(CPU.Registers.A & CPU.ReadByteAt(zeroPageAddress)));
                    break;
                case AddressingModes.AbsoluteX:
                    var abXAddress = (ushort)(CPU.ReadNextShort() + CPU.Registers.X);
                    CPU.Registers.A.Write((byte)(CPU.Registers.A & CPU.ReadByteAt(abXAddress)));
                    break;
                case AddressingModes.Immediate:
                    CPU.Registers.A.Write((byte)(CPU.Registers.A & CPU.ReadNextByte()));
                    break;
                default:
                    throw this.ExceptionForUnhandledAddressingMode(container);
            }

            this.HandleNegative(CPU.Registers.A);
            this.HandleZero(CPU.Registers.A);
        }

        [MOS6502Opcode(0x46, "LSR", AddressingModes.ZeroPage, cycles: 5, pcDelta: 2)]
        [MOS6502Opcode(0x4A, "LSR", AddressingModes.Accumulator, cycles: 2, pcDelta: 1)]
        public void LSR(OpcodeContainer container)
        {
            var (value, address, isSamePage) = AddressingMode.GetAddressedValue(CPU, container);

            switch (container.AddressingMode)
            {
                case AddressingModes.ZeroPage:
                    CPU.IsCarry = (value & 1) == 1;
                    CPU.WriteByteAt(address, (byte)(value >> 1), incrementPC: false);

                    CPU.IsNegative = false;
                    this.HandleNegative(value);
                    break;
                case AddressingModes.Accumulator:
                    CPU.IsCarry = (CPU.Registers.A & 1) == 1;
                    CPU.Registers.A.Write((byte)(CPU.Registers.A >> 1));

                    CPU.IsNegative = false;
                    this.HandleZero(CPU.Registers.A);
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected AddressingMode in LSR: {container.AddressingMode}");
            }
        }

        [MOS6502Opcode(0x7E, "ROR", AddressingModes.AbsoluteX, cycles: 7, pcDelta: 3)]
        public void ROR(OpcodeContainer container)
        {
            switch (container.AddressingMode)
            {
                case AddressingModes.AbsoluteX:
                    var absoluteXAddress = (ushort)(CPU.ReadNextShort() + CPU.Registers.X);
                    var absoluteXValue = CPU.ReadByteAt(absoluteXAddress);

                    var originalCarry = CPU.IsCarry ? 1 : 0;
                    var newCarry = (byte)(absoluteXValue & 1);

                    absoluteXValue = (byte)(originalCarry << 8 | absoluteXValue >> 1);
                    CPU.IsCarry = newCarry == 1;

                    this.HandleNegative(absoluteXValue);
                    this.HandleZero(absoluteXValue);
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected AddressingMode in LSR: {container.AddressingMode}");
            }
        }

        [MOS6502Opcode(0x06, "ASL", AddressingModes.ZeroPage, cycles: 5, pcDelta: 2)]
        [MOS6502Opcode(0x0A, "ASL", AddressingModes.Accumulator, cycles: 2, pcDelta: 1)]
        public void ASL(OpcodeContainer container)
        {
            byte value;
            switch (container.AddressingMode)
            {
                case AddressingModes.ZeroPage:
                    value = CPU.ReadNextByte();
                    break;
                case AddressingModes.Accumulator:
                    value = CPU.Registers.A.Read();
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected AddressingMode in LSR: {container.AddressingMode}");
            }

            CPU.IsCarry = (value & 1) == 1;
            value = (byte)(value << 1);

            CPU.Registers.A.Write(value);
            this.HandleNegative(value);
            this.HandleZero(value);
        }

        [MOS6502Opcode(0x2A, "ROL", AddressingModes.Accumulator, cycles: 2, pcDelta: 1)]
        public void ROL(OpcodeContainer container)
        {
            switch (container.AddressingMode)
            {
                case AddressingModes.Accumulator:
                    byte originalCarry = (byte)(CPU.IsCarry ? 1 : 0);
                    CPU.IsCarry = (CPU.Registers.A & 8) == 1;

                    CPU.Registers.A.Write((byte)(CPU.Registers.A << 1 | originalCarry));

                    CPU.IsNegative = false;
                    this.HandleZero(CPU.Registers.A);
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected AddressingMode in LSR: {container.AddressingMode}");
            }
        }

        [MOS6502Opcode(0xEE, "INC", AddressingModes.Absolute, cycles: 6, pcDelta: 3)]
        [MOS6502Opcode(0xE6, "INC", AddressingModes.ZeroPage, cycles: 5, pcDelta: 2)]
        public void INC(OpcodeContainer container)
        {
            byte value;

            switch (container.AddressingMode)
            {
                case AddressingModes.Absolute:
                    var address = CPU.ReadNextShort();
                    value = CPU.ReadByteAt(address);
                    value++;
                    CPU.WriteByteAt(address, value, incrementPC: false);
                    break;
                case AddressingModes.ZeroPage:
                    var zeroPageMemory = CPU.ReadNextByte();
                    value = CPU.ReadByteAt(zeroPageMemory, incrementPC: false);
                    value++;
                    CPU.WriteByteAt(zeroPageMemory, value, incrementPC: false);
                    break;
                default:
                    throw this.ExceptionForUnhandledAddressingMode(container);
            }

            this.HandleNegative(value);
            this.HandleZero(value);
        }

        [MOS6502Opcode(0xD6, "DEC", AddressingModes.ZeroPageX, cycles: 6, pcDelta: 2)]
        [MOS6502Opcode(0xC6, "DEC", AddressingModes.ZeroPage, cycles: 5, pcDelta: 2)]
        [MOS6502Opcode(0xCE, "DEC", AddressingModes.Absolute, cycles: 6, pcDelta: 3)]
        public void DEC(OpcodeContainer container)
        {
            var (value, address, isSamePage) = AddressingMode.GetAddressedValue(CPU, container);
            value--;
            CPU.WriteByteAt(address, value, incrementPC: false);

            this.HandleNegative(value);
            this.HandleZero(value);

            /*
            switch (container.AddressingMode)
            {
                case AddressingModes.ZeroPageX:

                    break;
                case AddressingModes.ZeroPage:
                    var zeroPageAddress = CPU.ReadNextByte(incrementPC: false);
                    value = CPU.ReadByteAt(zeroPageAddress);
                    value--;
                    CPU.WriteByteAt(zeroPageAddress, value, incrementPC: false);
                    break;
                case AddressingModes.Absolute:
                    var absoluteAddress = CPU.ReadNextShort();
                    value = CPU.ReadByteAt(absoluteAddress);
                    value--;
                    CPU.WriteByteAt(absoluteAddress, value, incrementPC: false);
                    break;
                default:
                    throw this.ExceptionForUnhandledAddressingMode(container);
            }

            this.HandleNegative(value);
            this.HandleZero(value);
            */
        }

        [MOS6502Opcode(0x49, "EOR", AddressingModes.Immediate, cycles: 2, pcDelta: 2)]
        [MOS6502Opcode(0x45, "EOR", AddressingModes.ZeroPage, cycles: 3, pcDelta: 2)]
        public void EOR(OpcodeContainer container)
        {
            switch (container.AddressingMode)
            {
                case AddressingModes.Immediate:
                    var immediateValue = CPU.ReadNextByte();
                    CPU.Registers.A.Write((byte)(CPU.Registers.A ^ immediateValue));
                    break;
                case AddressingModes.ZeroPage:
                    var zeroPageAddress = CPU.ReadNextByte(incrementPC: false);
                    var zeroPageValue = CPU.ReadByteAt(zeroPageAddress);
                    CPU.Registers.A.Write((byte)(CPU.Registers.A ^ zeroPageValue));
                    break;
                default:
                    throw this.ExceptionForUnhandledAddressingMode(container);
            }

            this.HandleNegative(CPU.Registers.A);
            this.HandleZero(CPU.Registers.A);
        }

        #endregion

        #region Compare Operations

        [MOS6502Opcode(0xC0, "CPY", AddressingModes.Immediate, cycles: 2, pcDelta: 2)]
        public void CPY(OpcodeContainer container) => this.CompareValues(CPU.Registers.Y, CPU.ReadNextByte());

        [MOS6502Opcode(0xE0, "CPX", AddressingModes.Immediate, cycles: 2, pcDelta: 2)]
        public void CPX(OpcodeContainer container) => this.CompareValues(CPU.Registers.X, CPU.ReadNextByte());

        [MOS6502Opcode(0xC9, "CMP", AddressingModes.Immediate, cycles: 2, pcDelta: 2)]
        public void CMP(OpcodeContainer container) => this.CompareValues(CPU.Registers.A, CPU.ReadNextByte());

        #endregion

        #region Load Operations

        [MOS6502Opcode(0xA6, "LDX", AddressingModes.ZeroPage, cycles: 3, pcDelta: 2)]
        [MOS6502Opcode(0xBE, "LDX", AddressingModes.AbsoluteY, cycles: 4, pcDelta: 3)]
        [MOS6502Opcode(0xAE, "LDX", AddressingModes.Absolute, cycles: 4, pcDelta: 3)]
        [MOS6502Opcode(0xA2, "LDX", AddressingModes.Immediate, cycles: 2, pcDelta: 2)]
        public void LDX(OpcodeContainer container)
        {
            switch (container.AddressingMode)
            {
                case AddressingModes.ZeroPage:
                    var zeroPageAddress = CPU.ReadNextByte(incrementPC: false);
                    this.WriteByteAtToRegister(CPU.Registers.X, zeroPageAddress, S: true, Z: true);
                    break;
                case AddressingModes.Absolute:
                    ushort address = CPU.ReadNextShort();
                    this.WriteByteAtToRegister(CPU.Registers.X, address, S: true, Z: true);
                    break;
                case AddressingModes.AbsoluteY:
                    ushort addressWithY = (ushort)(CPU.ReadNextShort() + CPU.Registers.Y);
                    this.WriteByteAtToRegister(CPU.Registers.X, addressWithY, S: true, Z: true);
                    break;
                case AddressingModes.Immediate:
                    this.WriteNextByteToRegister(CPU.Registers.X, S: true, Z: true);
                    break;
                default:
                    throw this.ExceptionForUnhandledAddressingMode(container);
            }
        }

        [MOS6502Opcode(0xA4, "LDY", AddressingModes.ZeroPage, cycles: 3, pcDelta: 2)]
        [MOS6502Opcode(0xB4, "LDY", AddressingModes.ZeroPageX, cycles: 4, pcDelta: 2)]
        [MOS6502Opcode(0xAC, "LDY", AddressingModes.Absolute, cycles: 4, pcDelta: 3)]
        [MOS6502Opcode(0xA0, "LDY", AddressingModes.Immediate, cycles: 2, pcDelta: 2)]
        public void LDY(OpcodeContainer container)
        {
            var (value, address, isSamePage) = AddressingMode.GetAddressedValue(CPU, container);
            CPU.Registers.Y.Write(value);

            /*
            switch (container.AddressingMode)
            {
                case AddressingModes.ZeroPage:
                    CPU.Registers.Y.Write(value);
                    break;
                case AddressingModes.ZeroPageX:
                    var zeroPageXAddress = (byte)(CPU.ReadNextByte(incrementPC: false) + CPU.Registers.X);
                    this.WriteByteAtToRegister(CPU.Registers.Y, zeroPageXAddress, S: true, Z: true);
                    break;
                case AddressingModes.Absolute:
                    ushort absoluteAddress = CPU.ReadNextShort();
                    this.WriteByteAtToRegister(CPU.Registers.Y, absoluteAddress, S: true, Z: true);
                    break;
                case AddressingModes.Immediate:
                    this.WriteNextByteToRegister(CPU.Registers.Y, S: true, Z: true);
                    break;
                default:
                    throw this.ExceptionForUnhandledAddressingMode(container);
            }
            */
        }

        [MOS6502Opcode(0xB9, "LDA", AddressingModes.AbsoluteY, cycles: 4, pcDelta: 3)]
        [MOS6502Opcode(0xB5, "LDA", AddressingModes.ZeroPageX, cycles: 4, pcDelta: 2)]
        [MOS6502Opcode(0xA5, "LDA", AddressingModes.ZeroPage, cycles: 3, pcDelta: 2)]
        [MOS6502Opcode(0xA9, "LDA", AddressingModes.Immediate, cycles: 2, pcDelta: 2)]
        [MOS6502Opcode(0xAD, "LDA", AddressingModes.Absolute, cycles: 4, pcDelta: 3)]
        [MOS6502Opcode(0xBD, "LDA", AddressingModes.AbsoluteX, cycles: 4, pcDelta: 3)]
        [MOS6502Opcode(0xB1, "LDA", AddressingModes.IndirectY, cycles: 5, pcDelta: 2)]
        public void LDA(OpcodeContainer container)
        {
            var (value, address, samePage) = AddressingMode.GetAddressedValue(CPU, container);
            this.WriteByteToRegister(CPU.Registers.A, value, S: true, Z: true);

            this.HandlePageBoundaryCrossed(container, samePage);
        }

        #endregion

        #region Stack Instructions

        [MOS6502Opcode(0x9A, "TXS", AddressingModes.Immediate, cycles: 2, pcDelta: 1)]
        public void TXS(OpcodeContainer container) => CPU.Registers.SP.Write(CPU.Registers.X);

        [MOS6502Opcode(0x48, "PHA", AddressingModes.Immediate, cycles: 2, pcDelta: 1)]
        public void PHA(OpcodeContainer container) => CPU.Stack.Push(CPU.Registers.A);

        [MOS6502Opcode(0x68, "PLA", AddressingModes.Immediate, cycles: 4, pcDelta: 1)]
        public void PLA(OpcodeContainer container) => CPU.Registers.A.Write(CPU.Stack.PopByte());

        #endregion

        #region STore

        [MOS6502Opcode(0x95, "STA", AddressingModes.ZeroPageX, cycles: 4, pcDelta: 2)]
        [MOS6502Opcode(0x91, "STA", AddressingModes.IndirectY, cycles: 6, pcDelta: 2)]
        [MOS6502Opcode(0x85, "STA", AddressingModes.ZeroPage, cycles: 3, pcDelta: 2)]
        [MOS6502Opcode(0x8D, "STA", AddressingModes.Absolute, cycles: 4, pcDelta: 3)]
        [MOS6502Opcode(0x99, "STA", AddressingModes.AbsoluteY, cycles: 5, pcDelta: 3)]
        [MOS6502Opcode(0x9D, "STA", AddressingModes.AbsoluteX, cycles: 5, pcDelta: 3)]
        public void STA(OpcodeContainer container)
        {
            switch (container.AddressingMode)
            {
                case AddressingModes.ZeroPageX:
                    var zeroPageXAddress = (byte)(CPU.ReadNextByte(incrementPC: false) + CPU.Registers.X);
                    CPU.WriteByteAt(zeroPageXAddress, CPU.Registers.A);
                    break;
                case AddressingModes.ZeroPage:
                    var value = CPU.ReadNextByte(incrementPC: false);
                    CPU.WriteByteAt(value, CPU.Registers.A);
                    break;
                case AddressingModes.Absolute:
                    var address = CPU.ReadNextShort();
                    CPU.WriteByteAt(address, CPU.Registers.A);
                    break;
                case AddressingModes.AbsoluteY:
                    var absoluteYAddress = CPU.ReadNextShort();
                    absoluteYAddress += CPU.Registers.Y;

                    CPU.WriteByteAt(absoluteYAddress, CPU.Registers.A);
                    break;
                case AddressingModes.AbsoluteX:
                    var absoluteXAddress = CPU.ReadNextShort();
                    absoluteXAddress += CPU.Registers.X;

                    CPU.WriteByteAt(absoluteXAddress, CPU.Registers.A);
                    break;
                case AddressingModes.IndirectY:
                    var indirectYAddress = (ushort)CPU.ReadNextByte(incrementPC: false);
                    ushort completeYAddress = (ushort)(CPU.ReadShortAt(indirectYAddress) + CPU.Registers.Y);
                    CPU.WriteByteAt(completeYAddress, CPU.Registers.A, incrementPC: false);
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected AddressingMode in STA: {container.AddressingMode}");
            }
        }

        [MOS6502Opcode(0x86, "STX", AddressingModes.ZeroPage, cycles: 3, pcDelta: 2)]
        [MOS6502Opcode(0x96, "STX", AddressingModes.ZeroPageY, cycles:4, pcDelta: 2)]
        [MOS6502Opcode(0x8E, "STX", AddressingModes.Absolute, cycles: 4, pcDelta: 3)]
        public void STX(OpcodeContainer container)
        {
            var (value, address, isSamePage) = AddressingMode.GetAddressedValue(CPU, container, getValue: false);
            CPU.WriteByteAt(address, CPU.Registers.X);
        }

        [MOS6502Opcode(0x84, "STY", AddressingModes.ZeroPage, cycles: 3, pcDelta: 2)]
        [MOS6502Opcode(0x94, "STY", AddressingModes.ZeroPageX, cycles: 4, pcDelta: 2)]
        [MOS6502Opcode(0x8C, "STY", AddressingModes.Absolute, cycles: 4, pcDelta: 3)]
        public void STY(OpcodeContainer container)
        {
            var (value, address, isSamePage) = AddressingMode.GetAddressedValue(CPU, container, getValue: false);

            CPU.WriteByteAt(address, CPU.Registers.Y);
        }

        #endregion

        #region Jumps

        [MOS6502Opcode(0x6C, "JMP", AddressingModes.Indirect, cycles: 5, pcDelta: 3, verify: false)]
        [MOS6502Opcode(0x4C, "JMP", AddressingModes.Absolute, cycles: 3, pcDelta: 3, verify: false)]
        public void JMP(OpcodeContainer container)
        {
            switch (container.AddressingMode)
            {
                case AddressingModes.Indirect:
                    var indirectAddress = CPU.ReadNextShort();
                    var newPCAddress = CPU.ReadShortAt(indirectAddress);
                    CPU.SetPCTo(newPCAddress);
                    break;
                case AddressingModes.Absolute:
                    CPU.SetPCTo(CPU.ReadNextShort());
                    break;
                default:
                    throw this.ExceptionForUnhandledAddressingMode(container);
            }
        }

        [MOS6502Opcode(0x20, "JSR", AddressingModes.Absolute, cycles: 6, pcDelta: 3, verify: false)]
        public void JSR(OpcodeContainer container)
        {
            CPU.Stack.Push((ushort)(CPU.Registers.PC + 1));
            CPU.SetPCTo(CPU.ReadNextShort());
        }

        [MOS6502Opcode(0x60, "RTS", AddressingModes.Absolute, cycles: 6, pcDelta: 1, verify: false)]
        public void RTS(OpcodeContainer container)
        {
            var address = CPU.Stack.PopShort();
            CPU.SetPCTo((ushort)(address + 1));
            //CPU.fs.WriteLine($"RTS-M: PC: 0x{CPU.Registers.PC.Read():X} SP: 0x{CPU.Registers.SP.Read():X4}");
        }

        #endregion

        #region Branches

        [MOS6502Opcode(0x30, "BMI", AddressingModes.Relative, cycles: 2, pcDelta: 2)]
        public void BMI(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(CPU.IsNegative);

            container.Cycles = cycles;
            container.PCDelta = pcDelta;
        }

        [MOS6502Opcode(0x10, "BPL", AddressingModes.Relative, cycles: 2, pcDelta: 2)]
        public void BPL(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(!CPU.IsNegative);

            container.Cycles = cycles;
            container.PCDelta = pcDelta;
        }

        [MOS6502Opcode(0x90, "BCC", AddressingModes.Relative, cycles: 2, pcDelta: 2)]
        public void BCC(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(!CPU.IsCarry);

            container.Cycles = cycles;
            container.PCDelta = pcDelta;
        }

        [MOS6502Opcode(0xB0, "BCS", AddressingModes.Relative, cycles: 2, pcDelta: 2)]
        public void BCS(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(CPU.IsCarry);

            container.Cycles = cycles;
            container.PCDelta = pcDelta;
        }

        [MOS6502Opcode(0xD0, "BNE", AddressingModes.Relative, cycles: 2, pcDelta: 2)]
        public void BNE(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(CPU.IsZero == false);

            container.Cycles = cycles;
            container.PCDelta = pcDelta;
        }

        [MOS6502Opcode(0xF0, "BEQ", AddressingModes.Relative, cycles: 2, pcDelta: 2)]
        public void BEQ(OpcodeContainer container)
        {
            var (cycles, pcDelta) = this.Branch(CPU.IsZero);

            container.Cycles = cycles;
            container.PCDelta = pcDelta;
        }

        #endregion

        #region Processor Status Instructions

        [MOS6502Opcode(0x78, "SEI", AddressingModes.Implied, cycles: 2, pcDelta: 1)]
        public void SEI(OpcodeContainer container) => CPU.AreInterruptsDisabled = true;

        [MOS6502Opcode(0x58, "CLI", AddressingModes.Implied, cycles: 2, pcDelta: 1)]
        public void CLI(OpcodeContainer container) => CPU.AreInterruptsDisabled = false;

        [MOS6502Opcode(0xF8, "SED", AddressingModes.Implied, cycles: 2, pcDelta: 1)]
        public void SED(OpcodeContainer container) => CPU.IsDecimalMode = true;

        [MOS6502Opcode(0xD8, "CLD", AddressingModes.Implied, cycles: 2, pcDelta: 1)]
        public void CLD(OpcodeContainer container) => CPU.IsDecimalMode = false;

        [MOS6502Opcode(0x38, "SEC", AddressingModes.Implied, cycles: 2, pcDelta: 1)]
        public void SEC(OpcodeContainer container) => CPU.IsCarry = true;

        [MOS6502Opcode(0x18, "CLC", AddressingModes.Implied, cycles: 2, pcDelta: 1)]
        public void CLC(OpcodeContainer container) => CPU.IsCarry = false;

        [MOS6502Opcode(0xB8, "CLV", AddressingModes.Implied, cycles: 2, pcDelta: 1)]
        public void CLV(OpcodeContainer container) => CPU.IsOverflowed = false;

        #endregion

        #region Register Instructions

        [MOS6502Opcode(0xCA, "DEX", AddressingModes.Implied, cycles: 2, pcDelta: 1)]
        public void DEX(OpcodeContainer container)
        {
            byte x = CPU.Registers.X;
            CPU.Registers.X.Write(--x);

            this.HandleNegative(CPU.Registers.X);
            this.HandleZero(CPU.Registers.X);
        }

        [MOS6502Opcode(0x88, "DEY", AddressingModes.Implied, cycles: 2, pcDelta: 1)]
        public void DEY(OpcodeContainer container)
        {
            byte y = CPU.Registers.Y;
            CPU.Registers.Y.Write(--y);

            this.HandleNegative(CPU.Registers.Y);
            this.HandleZero(CPU.Registers.Y);
        }

        [MOS6502Opcode(0xC8, "INY", AddressingModes.Implied, cycles: 2, pcDelta: 1)]
        public void INY(OpcodeContainer container)
        {
            byte y = CPU.Registers.Y;
            CPU.Registers.Y.Write(++y);

            this.HandleNegative(CPU.Registers.Y);
            this.HandleZero(CPU.Registers.Y);
        }

        [MOS6502Opcode(0xE8, "INX", AddressingModes.Implied, cycles: 2, pcDelta: 1)]
        public void INX(OpcodeContainer container)
        {
            byte x = CPU.Registers.X;
            CPU.Registers.X.Write(++x);

            this.HandleNegative(CPU.Registers.X);
            this.HandleZero(CPU.Registers.X);
        }

        [MOS6502Opcode(0x8A, "TXA", AddressingModes.Immediate, cycles: 2, pcDelta: 1)]
        public void TXA(OpcodeContainer container) => CPU.Registers.A.Write(CPU.Registers.X);

        [MOS6502Opcode(0xAA, "TAX", AddressingModes.Immediate, cycles: 2, pcDelta: 1)]
        public void TAX(OpcodeContainer container) => CPU.Registers.X.Write(CPU.Registers.A);

        [MOS6502Opcode(0xA8, "TAY", AddressingModes.Immediate, cycles: 2, pcDelta: 1)]
        public void TAY(OpcodeContainer container) => CPU.Registers.Y.Write(CPU.Registers.A);

        [MOS6502Opcode(0x98, "TYA", AddressingModes.Immediate, cycles: 2, pcDelta: 1)]
        public void TYA(OpcodeContainer container) => CPU.Registers.A.Write(CPU.Registers.Y);

        #endregion

        #region Math Instructions

        [MOS6502Opcode(0x6D, "ADC", AddressingModes.Absolute, cycles: 4, pcDelta: 3)]
        [MOS6502Opcode(0x69, "ADC", AddressingModes.Immediate, cycles: 2, pcDelta: 2)]
        [MOS6502Opcode(0x65, "ADC", AddressingModes.ZeroPage, cycles: 3, pcDelta: 2)]
        public void ADC(OpcodeContainer container)
        {
            var (value, address, isSamePage) = AddressingMode.GetAddressedValue(CPU, container);

            var result = (byte)(CPU.Registers.A + value);
            CPU.IsCarry = result >= 0x80;

            this.HandlePageBoundaryCrossed(container, isSamePage);

            this.HandleNegative(CPU.Registers.A);
            this.HandleZero(CPU.Registers.A);
            this.HandleOverflow(CPU.Registers.A);
        }

        [MOS6502Opcode(0xE5, "SBC", AddressingModes.ZeroPage, cycles: 3, pcDelta: 2)]
        [MOS6502Opcode(0xF9, "SBC", AddressingModes.AbsoluteY, cycles: 4, pcDelta: 3)]
        public void SBC(OpcodeContainer container)
        {
            byte carry = (byte)(CPU.IsCarry ? 1 : 0);
            var (value, address, isSamePage) = AddressingMode.GetAddressedValue(CPU, container);
            this.WriteByteToRegister(CPU.Registers.A, (byte)(CPU.Registers.A - value), S: true, Z: true);

            this.HandlePageBoundaryCrossed(container, isSamePage);
            this.HandleOverflow(CPU.Registers.A);
        }

        #endregion

        // TODO: hmmm...seems too easy, we'll see. 0x80 = 128, max of signed byte.
        private void HandleNegative(byte operand) => CPU.IsNegative = operand >= 0x80;

        private void HandleZero(byte operand) => CPU.IsZero = operand == 0;

        private void HandleOverflow(byte result) => CPU.IsOverflowed = result > 127;

        private void CompareValues(byte value1, byte value2, bool S = true, bool Z = true, bool C = true)
        {
            sbyte result = (sbyte)(value1 - value2);

            CPU.IsCarry = C && (value1 >= value2);

            this.HandleNegative((byte)result);
            this.HandleZero((byte)result);
        }

        private (int cycles, int pcDelta) Branch(bool condition)
        {
            // TODO: handle page boundaries
            var originalPC = CPU.Registers.PC.Read();
            var cycles = 2;

            if (condition)
            {
                var offset = (sbyte)CPU.ReadNextByte();
                ushort newPC = (ushort)(CPU.Registers.PC + offset);
                CPU.SetPCTo(newPC);

                cycles++;
            }
            else
            {
                CPU.IncrementPC(1);
            }

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
                case AddressingModes.AbsoluteX:
                case AddressingModes.AbsoluteY:
                case AddressingModes.IndirectY:
                    if (!samePage.Value)
                    {
                        container.Cycles++;
                    }
                    break;
                default:
                    break;
            }
        }

        private InvalidOperationException ExceptionForUnhandledAddressingMode(OpcodeContainer container) => new InvalidOperationException($"Unhandled AddressMode ({container.AddressingMode}) for Opcode: ({container.Name})");
    }
}

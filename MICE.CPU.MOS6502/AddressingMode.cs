using System;

namespace MICE.CPU.MOS6502
{
    public static class AddressingMode
    {
        private static ushort intermediateAddress;
        private static byte operandValue;
        private static ushort address;

        public static ushort GetZeroPageX(MOS6502 CPU)
        {
            intermediateAddress = CPU.ReadNextByte();
            var address = (ushort)(intermediateAddress + CPU.Registers.X);
            address &= 0xFF;

            CPU.CycleFinished();
            CPU.IncrementPC();

            return address;
        }

        public static ushort GetZeroPageY(MOS6502 CPU)
        {
            intermediateAddress = CPU.ReadNextByte();
            var address = (ushort)(intermediateAddress + CPU.Registers.Y);
            address &= 0xFF;

            CPU.CycleFinished();
            CPU.IncrementPC();

            return address;
        }

        public static ushort GetIndirect(MOS6502 CPU)
        {
            intermediateAddress = CPU.ReadNextShort();

            // 6502 bug in indirect JMP...
            if ((intermediateAddress & 0x00FF) == 0xFF)
            {
                byte lowByte = CPU.ReadByteAt(intermediateAddress);
                byte highByte = CPU.ReadByteAt((ushort)(intermediateAddress - 0x00FF));

                intermediateAddress = (ushort)(highByte << 8 | lowByte);
            }

            return CPU.ReadShortAt(intermediateAddress);
        }

        public static ushort GetZeroPage(MOS6502 CPU)
        {
            var pc = CPU.ReadNextByte();
            CPU.IncrementPC();

            return pc;
        }

        public static ushort GetImmediate(MOS6502 CPU)
        {
            var pc = CPU.Registers.PC.Read();
            operandValue = CPU.ReadNextByte(isCycle: false);

            CPU.IncrementPC();

            return pc;
        }

        public static ushort GetAbsolute(MOS6502 CPU)
        {
            var nextShort = CPU.ReadNextShort();
            CPU.IncrementPC(2);

            return nextShort;
        }

        public static ushort GetAbsoluteX(MOS6502 CPU)
        {
            intermediateAddress = CPU.ReadNextShort();
            var nextShort = (ushort)(intermediateAddress + CPU.Registers.X);

            CPU.IncrementPC(2);

            return nextShort;
        }

        public static ushort GetAbsoluteXWrite(MOS6502 CPU)
        {
            intermediateAddress = CPU.ReadNextShort();
            var nextShort = (ushort)(intermediateAddress + CPU.Registers.X);

            CPU.IncrementPC(2);
            CPU.CycleFinished();

            return nextShort;
        }

        public static ushort GetAbsoluteY(MOS6502 CPU)
        {
            intermediateAddress = CPU.ReadNextShort();
            var nextShort = (ushort)(intermediateAddress + CPU.Registers.Y);

            CPU.IncrementPC(2);

            return nextShort;
        }

        public static ushort GetIndirectX(MOS6502 CPU)
        {
            var incompleteXAddress = CPU.ReadNextByte();
            CPU.IncrementPC();

            var intermediateAddress = (byte)(incompleteXAddress + CPU.Registers.X);

            return intermediateAddress == 0xFF
                ? (ushort)(CPU.ReadByteAt(0xFF) | CPU.ReadByteAt(0x00) << 8)
                : CPU.ReadShortAt(intermediateAddress);
        }

        public static ushort GetIndirectY(MOS6502 CPU)
        {
            intermediateAddress = CPU.ReadNextByte();
            CPU.IncrementPC();

            var baseAddress = intermediateAddress == 0xFF
                ? (ushort)(CPU.ReadByteAt(0xFF) | CPU.ReadByteAt(0x00) << 8)
                : CPU.ReadShortAt((ushort)intermediateAddress);

            var finalAddress = (ushort)(baseAddress + CPU.Registers.Y);

            if (AddressingMode.NotSamePage(baseAddress, CPU.Registers.Y))
            {
                CPU.CycleFinished();
            }

            return finalAddress;
        }

        public static ushort GetIndirectYWrite(MOS6502 CPU)
        {
            intermediateAddress = CPU.ReadNextByte();
            CPU.IncrementPC();

            var baseAddress = intermediateAddress == 0xFF
                ? (ushort)(CPU.ReadByteAt(0xFF) | CPU.ReadByteAt(0x00) << 8)
                : CPU.ReadShortAt((ushort)intermediateAddress);

            CPU.CycleFinished();

            return (ushort)(baseAddress + CPU.Registers.Y);
        }

        private static ushort GetRelative(MOS6502 CPU) => 0x0000;

        public static ushort GetAddressedOperand(MOS6502 CPU, OpcodeContainer container)
        {
            operandValue = 0;
            intermediateAddress = 0;
            address = 0;

            switch (container.AddressingMode)
            {
                case AddressingModes.Implied:
                    address = 0x0000;
                    CPU.CycleFinished();
                    break;
                case AddressingModes.Accumulator:
                    address = 0x0000;
                    CPU.CycleFinished();
                    break;
                case AddressingModes.Immediate:
                    address = AddressingMode.GetImmediate(CPU);
                    break;
                case AddressingModes.Absolute:
                    address = AddressingMode.GetAbsolute(CPU);
                    break;
                case AddressingModes.AbsoluteX:
                    address = AddressingMode.GetAbsoluteX(CPU);
                    break;
                case AddressingModes.AbsoluteXWrite:
                    address = AddressingMode.GetAbsoluteXWrite(CPU);
                    break;
                case AddressingModes.AbsoluteY:
                    address = AddressingMode.GetAbsoluteY(CPU);
                    break;
                case AddressingModes.Indirect:
                    address = AddressingMode.GetIndirect(CPU);
                    break;
                case AddressingModes.IndirectX:
                    address = AddressingMode.GetIndirectX(CPU);
                    break;
                case AddressingModes.IndirectY:
                    address = AddressingMode.GetIndirectY(CPU);
                    break;
                case AddressingModes.IndirectYWrite:
                    address = AddressingMode.GetIndirectYWrite(CPU);
                    break;
                case AddressingModes.ZeroPage:
                    address = AddressingMode.GetZeroPage(CPU);
                    break;
                case AddressingModes.ZeroPageX:
                    address = AddressingMode.GetZeroPageX(CPU);
                    break;
                case AddressingModes.ZeroPageY:
                    address = AddressingMode.GetZeroPageY(CPU);
                    break;
                case AddressingModes.Relative:
                    address = AddressingMode.GetRelative(CPU);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            //if (MOS6502.IsDebug)
            //{
                //CPU.LastAccessedAddress = AddressingMode.GetAddressAsString(CPU, container);
            //}

            return address;
        }

        public static string GetAddressAsString(MOS6502 CPU, OpcodeContainer container)
        {
            if (container.Name == "RTS")
            {
                return "";
            }

            switch (container.AddressingMode)
            {
                case AddressingModes.Implied:
                    return "";
                case AddressingModes.Accumulator:
                    return "A";
                case AddressingModes.Indirect:
                    return $"(${intermediateAddress:X4}) @ ${address:X4}";
                case AddressingModes.IndirectYWrite:
                case AddressingModes.IndirectY:
                    return $"(${intermediateAddress:X2}),Y @ ${address:X4}";
                case AddressingModes.IndirectX:
                    return $"(${intermediateAddress:X2},X) @ ${address:X4}";
                case AddressingModes.ZeroPage:
                    return $"${address:X2}";
                case AddressingModes.ZeroPageX:
                    return $"${intermediateAddress:X2},X @ ${address:X2}";
                case AddressingModes.ZeroPageY:
                    return $"${intermediateAddress:X2},Y @ ${address:X2}";
                case AddressingModes.AbsoluteY:
                    return $"${intermediateAddress:X4},Y @ ${address:X4}";
                case AddressingModes.AbsoluteX:
                    return $"${intermediateAddress:X4},X @ ${address:X4}";
                case AddressingModes.Absolute:
                    return $"${address:X4}";
                case AddressingModes.Immediate:
                    return $"#${operandValue:X2}";
                default:
                    return $"0x{address:X4}";
            }
        }

        public static bool NotSamePage(ushort a1, byte a2) => !AddressingMode.WasSamePage(a1, (sbyte)a2);
        public static bool WasSamePage(ushort a1, ushort a2) => AddressingMode.WasSamePage(a1, (byte)a2);
        public static bool WasSamePage(ushort a1, byte a2) => AddressingMode.WasSamePage(a1, (sbyte)a2);
        public static bool WasSamePage(ushort a1, sbyte a2) => ((a1 + a2) & 0xFF00) == (a1 & 0xFF00);
    }
}

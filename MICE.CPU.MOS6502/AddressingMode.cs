using System;

namespace MICE.CPU.MOS6502
{
    public static class AddressingMode
    {
        public static AddressingModeResult GetZeroPage(MOS6502 CPU, bool getValue = true)
        {
            var zeroPageAddress = CPU.ReadNextByte();

            if (getValue)
            {
                return new AddressingModeResult(CPU.ReadByteAt(zeroPageAddress), zeroPageAddress, null);
            }

            return new AddressingModeResult(0x00, zeroPageAddress, null);
        }

        public static AddressingModeResult GetZeroPageY(MOS6502 CPU, bool getValue = true)
        {
            var intermediateAddress = CPU.ReadNextByte();
            var zeroPageYAddress = (byte)(intermediateAddress + CPU.Registers.Y);

            if (getValue)
            {
                return new AddressingModeResult(CPU.ReadByteAt(zeroPageYAddress), intermediateAddress, zeroPageYAddress, null);
            }

            return new AddressingModeResult(0x00, intermediateAddress, zeroPageYAddress, null);
        }

        public static AddressingModeResult GetZeroPageX(MOS6502 CPU, bool getValue = true)
        {
            var intermediateAddress = CPU.ReadNextByte();
            var zeroPageXAddress = (byte)(intermediateAddress + CPU.Registers.X);

            if (getValue)
            {
                return new AddressingModeResult(CPU.ReadByteAt(zeroPageXAddress), intermediateAddress, zeroPageXAddress, null);
            }

            return new AddressingModeResult(0x00, intermediateAddress, zeroPageXAddress, null);
        }

        public static AddressingModeResult GetIndirect(MOS6502 CPU, bool getValue = true)
        {
            var indirectAddress = CPU.ReadNextShort();
            var baseAddress = CPU.ReadShortAt(indirectAddress);

            return getValue
                ? new AddressingModeResult(CPU.ReadByteAt(baseAddress), indirectAddress, baseAddress, AreSamePage(indirectAddress, baseAddress))
                : new AddressingModeResult(0x00, indirectAddress, baseAddress, AreSamePage(indirectAddress, baseAddress));
        }

        public static AddressingModeResult GetIndirectY(MOS6502 CPU, bool getValue = true)
        {
            var incompleteYAddress = CPU.ReadNextByte();

            ushort addressWithoutY = 0x0000;
            if (incompleteYAddress == 0xFF)
            {
                addressWithoutY = (ushort)(CPU.ReadByteAt(0xFF) | CPU.ReadByteAt(0x00) << 8);
            }
            else
            {
                addressWithoutY = CPU.ReadShortAt(incompleteYAddress);
            }

            var addressWithY = (ushort)(addressWithoutY + CPU.Registers.Y);

            return getValue
                ? new AddressingModeResult(CPU.ReadByteAt(addressWithY), incompleteYAddress, addressWithY, AreSamePage(addressWithoutY, CPU.Registers.Y))
                : new AddressingModeResult(0x00, incompleteYAddress, addressWithY, AreSamePage(addressWithoutY, CPU.Registers.Y));
        }

        public static AddressingModeResult GetIndirectX(MOS6502 CPU, bool getValue = true)
        {
            // val = PEEK(PEEK((arg + X) % 256) + PEEK((arg + X + 1) % 256) * 256)
            var incompleteXAddress = CPU.ReadNextByte();
            var baseAddress = (byte)(incompleteXAddress + CPU.Registers.X);

            ushort addressWithX = 0x0000;
            if (baseAddress == 0xFF)
            {
                addressWithX = (ushort)(CPU.ReadByteAt(0xFF) | CPU.ReadByteAt(0x00) << 8);
            }
            else
            {
                addressWithX = CPU.ReadShortAt(baseAddress);
            }

            return getValue
                ? new AddressingModeResult(CPU.ReadByteAt(addressWithX), incompleteXAddress, addressWithX, AreSamePage(addressWithX, baseAddress))
                : new AddressingModeResult(0x00, incompleteXAddress, addressWithX, AreSamePage(addressWithX, baseAddress));
        }

        public static AddressingModeResult GetAbsolute(MOS6502 CPU, bool getValue = true)
        {
            var absoluteAddress = CPU.ReadNextShort();

            if (getValue)
            {
                return new AddressingModeResult(CPU.ReadByteAt(absoluteAddress), absoluteAddress, null);
            }

            return new AddressingModeResult(0x00, absoluteAddress, null);
        }

        public static AddressingModeResult GetAbsoluteX(MOS6502 CPU, bool getValue = true)
        {
            var nextAddress = CPU.ReadNextShort();
            var nextAddressWithX = (ushort)(nextAddress + CPU.Registers.X);

            return getValue
                ? new AddressingModeResult(CPU.ReadByteAt(nextAddressWithX), nextAddress, nextAddressWithX, AreSamePage(nextAddress, nextAddressWithX))
                : new AddressingModeResult(0x00, nextAddress, nextAddressWithX, AreSamePage(nextAddress, nextAddressWithX));
        }

        public static AddressingModeResult GetAbsoluteY(MOS6502 CPU, bool getValue = true)
        {
            var nextAddress = CPU.ReadNextShort();
            var nextAddressWithY = (ushort)(nextAddress + CPU.Registers.Y);

            return getValue
                ? new AddressingModeResult(CPU.ReadByteAt(nextAddressWithY), nextAddress, nextAddressWithY, AreSamePage(nextAddress, nextAddressWithY))
                : new AddressingModeResult(0x00, nextAddress, nextAddressWithY, AreSamePage(nextAddress, nextAddressWithY));
        }

        public static AddressingModeResult GetImmediate(MOS6502 CPU, bool getValue = true)
        {
            if (getValue)
            {
                return new AddressingModeResult(CPU.ReadNextByte(), CPU.Registers.PC, null);
            }

            return new AddressingModeResult(0x00, CPU.Registers.PC, null);
        }

        public static ushort GetZeroPageX(MOS6502 CPU)
        {
            intermediateAddress = CPU.ReadNextByte();
            var address = (ushort)(intermediateAddress + CPU.Registers.X);
            address &= 0xFF;

            CPU.IncrementPC();

            return address;
        }

        public static ushort GetZeroPageY(MOS6502 CPU)
        {
            intermediateAddress = CPU.ReadNextByte();
            var address = (ushort)(intermediateAddress + CPU.Registers.Y);
            address &= 0xFF;

            CPU.IncrementPC();

            return address;
        }

        public static ushort GetIndirect(MOS6502 CPU)
        {
            intermediateAddress = CPU.ReadNextShort();
            //return CPU.ReadShortAt(intermediateAddress.Value);

            return intermediateAddress.Value;
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
            operandValue = CPU.ReadNextByte();

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


            return (ushort)(baseAddress + CPU.Registers.Y);
        }

        private static ushort GetRelative(MOS6502 CPU) => 0x0000;

        private static byte operandValue;
        private static ushort? intermediateAddress;
        private static ushort address;

        public static ushort GetAddressedOperand(MOS6502 CPU, OpcodeContainer container)
        {
            operandValue = 0;
            intermediateAddress = 0;
            address = 0;

            switch (container.AddressingMode)
            {
                case AddressingModes.Implied:
                    address = 0x0000;
                    break;
                case AddressingModes.Accumulator:
                    address = 0x0000;
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

            if (MOS6502.IsDebug)
            {
                CPU.LastAccessedAddress = AddressingMode.GetAddressAsString(CPU, container);
            }

            return address;
        }

        public static AddressingModeResult GetAddressedOperand(MOS6502 CPU, OpcodeContainer container, bool getValue = true)
        {
            AddressingModeResult result;

            switch (container.AddressingMode)
            {
                case AddressingModes.Implied:
                    result = new AddressingModeResult(0x0, 0x0000, 0x0000, null);
                    break;
                case AddressingModes.Accumulator:
                    result = new AddressingModeResult(CPU.Registers.A, 0x0000, 0x0000, null);
                    break;
                case AddressingModes.Indirect:
                    result = AddressingMode.GetIndirect(CPU, getValue);
                    break;
                case AddressingModes.IndirectY:
                    result = AddressingMode.GetIndirectY(CPU, getValue);
                    break;
                case AddressingModes.IndirectX:
                    result = AddressingMode.GetIndirectX(CPU, getValue);
                    break;
                case AddressingModes.Immediate:
                    result = AddressingMode.GetImmediate(CPU, getValue);
                    break;
                case AddressingModes.ZeroPageY:
                    result = AddressingMode.GetZeroPageY(CPU, getValue);
                    break;
                case AddressingModes.ZeroPageX:
                    result = AddressingMode.GetZeroPageX(CPU, getValue);
                    break;
                case AddressingModes.ZeroPage:
                    result = AddressingMode.GetZeroPage(CPU, getValue);
                    break;
                case AddressingModes.Absolute:
                    result = AddressingMode.GetAbsolute(CPU, getValue);
                    break;
                case AddressingModes.AbsoluteY:
                    result = AddressingMode.GetAbsoluteY(CPU, getValue);
                    break;
                case AddressingModes.AbsoluteX:
                    result = AddressingMode.GetAbsoluteX(CPU, getValue);
                    break;
                default:
                    throw new InvalidOperationException($"Unhandled AddressMode ({container.AddressingMode}) for Opcode: ({container.Name})");
            }

           // CPU.LastAccessedAddress = AddressingMode.GetAddressAsString(CPU, container, result);

            return result;
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

        private static bool AreSamePage(ushort a1, ushort a2) => ((a1 ^ a2) & 0xFF00) == 0;
    }
}

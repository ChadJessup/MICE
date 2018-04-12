using System;

namespace MICE.CPU.MOS6502
{
    public static class AddressingMode
    {
        public static AddressingModeResult GetZeroPageY(MOS6502 CPU, bool getValue = true)
        {
            var intermediateAddress = CPU.ReadNextByte();
            var zeroPageYAddress = (byte)(intermediateAddress + CPU.Registers.Y);

            if (getValue)
            {
                return new AddressingModeResult(CPU.ReadByteAt(zeroPageYAddress, incrementPC: false), intermediateAddress, zeroPageYAddress, null);
            }

            return new AddressingModeResult(0x00, intermediateAddress, zeroPageYAddress, null);
        }

        public static AddressingModeResult GetZeroPageX(MOS6502 CPU, bool getValue = true)
        {
            var intermediateAddress = CPU.ReadNextByte(incrementPC: true);
            var zeroPageXAddress = (byte)(intermediateAddress + CPU.Registers.X);

            if (getValue)
            {
                return new AddressingModeResult(CPU.ReadByteAt(zeroPageXAddress, incrementPC: false), intermediateAddress, zeroPageXAddress, null);
            }

            return new AddressingModeResult(0x00, intermediateAddress, zeroPageXAddress, null);
        }

        public static AddressingModeResult GetZeroPage(MOS6502 CPU, bool getValue = true)
        {
            var zeroPageAddress = CPU.ReadNextByte();

            if (getValue)
            {
                return new AddressingModeResult(CPU.ReadByteAt(zeroPageAddress, incrementPC: false), zeroPageAddress, null);
            }

            return new AddressingModeResult(0x00, zeroPageAddress, null);
        }

        public static AddressingModeResult GetAbsolute(MOS6502 CPU, bool getValue = true)
        {
            var absoluteAddress = CPU.ReadNextShort();

            if (getValue)
            {
                return new AddressingModeResult(CPU.ReadByteAt(absoluteAddress, incrementPC: false), absoluteAddress, null);
            }

            return new AddressingModeResult(0x00, absoluteAddress, null);
        }

        public static AddressingModeResult GetIndirect(MOS6502 CPU, bool getValue = true)
        {
            var indirectAddress = CPU.ReadNextShort(incrementPC: false);
            var baseAddress = CPU.ReadShortAt(indirectAddress, incrementPC: false);

            return getValue
                ? new AddressingModeResult(CPU.ReadByteAt(baseAddress), indirectAddress, baseAddress, AreSamePage(indirectAddress, baseAddress))
                : new AddressingModeResult(0x00, indirectAddress, baseAddress, AreSamePage(indirectAddress, baseAddress));
        }

        public static AddressingModeResult GetIndirectY(MOS6502 CPU, bool getValue = true)
        {
            var incompleteYAddress = CPU.ReadNextByte(incrementPC: true);

            ushort addressWithoutY = 0x0000;
            if (incompleteYAddress == 0xFF)
            {
                addressWithoutY = (ushort)(CPU.ReadByteAt(0xFF, incrementPC: false) | CPU.ReadByteAt(0x00, incrementPC: false) << 8);
            }
            else
            {
                addressWithoutY = CPU.ReadShortAt(incompleteYAddress, incrementPC: false);
            }

            var addressWithY = (ushort)(addressWithoutY + CPU.Registers.Y);

            return getValue
                ? new AddressingModeResult(CPU.ReadByteAt(addressWithY, incrementPC: false), incompleteYAddress, addressWithY, AreSamePage(addressWithoutY, CPU.Registers.Y))
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
                addressWithX = (ushort)(CPU.ReadByteAt(0xFF, incrementPC: false) | CPU.ReadByteAt(0x00, incrementPC: false) << 8);
            }
            else
            {
                addressWithX = CPU.ReadShortAt(baseAddress, incrementPC: false);
            }

            return getValue
                ? new AddressingModeResult(CPU.ReadByteAt(addressWithX, incrementPC: false), incompleteXAddress, addressWithX, AreSamePage(addressWithX, baseAddress))
                : new AddressingModeResult(0x00, incompleteXAddress, addressWithX, AreSamePage(addressWithX, baseAddress));
        }

        public static AddressingModeResult GetAbsoluteX(MOS6502 CPU, bool getValue = true)
        {
            var nextAddress = CPU.ReadNextShort();
            var nextAddressWithX = (ushort)(nextAddress + CPU.Registers.X);

            return getValue
                ? new AddressingModeResult(CPU.ReadByteAt(nextAddressWithX, incrementPC: false), nextAddress, nextAddressWithX, AreSamePage(nextAddress, nextAddressWithX))
                : new AddressingModeResult(0x00, nextAddress, nextAddressWithX, AreSamePage(nextAddress, nextAddressWithX));
        }

        public static AddressingModeResult GetAbsoluteY(MOS6502 CPU, bool getValue = true)
        {
            var nextAddress = CPU.ReadNextShort();
            var nextAddressWithY = (ushort)(nextAddress + CPU.Registers.Y);

            return getValue
                ? new AddressingModeResult(CPU.ReadByteAt(nextAddressWithY, incrementPC: false), nextAddress, nextAddressWithY, AreSamePage(nextAddress, nextAddressWithY))
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

        public static AddressingModeResult GetAddressedOperand(MOS6502 CPU, OpcodeContainer container, bool getValue = true)
        {
            AddressingModeResult result;

            switch (container.AddressingMode)
            {
                case AddressingModes.Implied:
                    result = new AddressingModeResult(0x0, 0x0000, 0x0000, null);
                    break;
                case AddressingModes.Accumulator:
                    // Keep an eye on this one, could cause an issue if caller thinks accumulator has an address like this...
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

        public static string GetAddressAsString(MOS6502 CPU, OpcodeContainer container, AddressingModeResult result)
        {
            switch (container.AddressingMode)
            {
                case AddressingModes.Implied:
                    return "";
                case AddressingModes.Accumulator:
                    return "A";
                case AddressingModes.Indirect:
                    return $"(${result.IntermediateAddress:X4}) @ ${result.Address:X4}";
                case AddressingModes.IndirectY:
                    return $"(${result.IntermediateAddress:X2}),Y @ ${result.Address:X4}";
                case AddressingModes.IndirectX:
                    return $"(${result.IntermediateAddress:X2},X) @ ${result.Address:X4}";
                case AddressingModes.ZeroPage:
                    return $"${result.Address:X2}";
                case AddressingModes.ZeroPageX:
                    return $"${result.IntermediateAddress:X2},X @ ${result.Address:X2}";
                case AddressingModes.ZeroPageY:
                    return $"${result.IntermediateAddress:X2},Y @ ${result.Address:X2}";
                case AddressingModes.AbsoluteY:
                    return $"${result.IntermediateAddress:X4},Y @ ${result.Address:X4}";
                case AddressingModes.AbsoluteX:
                    return $"${result.IntermediateAddress:X4},X @ ${result.Address:X4}";
                case AddressingModes.Absolute:
                    return $"${result.Address:X4}";
                case AddressingModes.Immediate:
                    return $"#${result.OperandValue:X2}";
                default:
                    return $"0x{result.Address:X4}";
            }
        }

        private static bool AreSamePage(ushort a1, ushort a2) => ((a1 ^ a2) & 0xFF00) == 0;
    }
}

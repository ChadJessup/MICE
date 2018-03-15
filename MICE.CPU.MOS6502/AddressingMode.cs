using System;

namespace MICE.CPU.MOS6502
{
    public static class AddressingMode
    {
        public static (byte value, ushort address, bool? samePage) GetZeroPageX(MOS6502 CPU, bool getValue = true)
        {
            var zeroPageXAddress = (byte)(CPU.ReadNextByte(incrementPC: false) + CPU.Registers.X);

            if (getValue)
            {
                return (CPU.ReadByteAt(zeroPageXAddress), zeroPageXAddress, null);
            }

            return (0x00, zeroPageXAddress, null);
        }

        public static (byte value, ushort address, bool? samePage) GetZeroPage(MOS6502 CPU, bool getValue = true)
        {
            var zeroPageAddress = CPU.ReadNextByte(incrementPC: false);

            if (getValue)
            {
                return (CPU.ReadByteAt(zeroPageAddress), zeroPageAddress, null);
            }

            return (0x00, zeroPageAddress, null);
        }

        public static (byte value, ushort address, bool? samePage) GetAbsolute(MOS6502 CPU, bool getValue = true)
        {
            var absoluteAddress = CPU.ReadNextShort();

            if (getValue)
            {
                return (CPU.ReadByteAt(absoluteAddress), absoluteAddress, null);
            }

            return (0x00, absoluteAddress, null);
        }

        public static (byte value, ushort address, bool? samePage) GetIndirectY(MOS6502 CPU, bool getValue = true)
        {
            var incompleteYAddress = CPU.ReadNextByte(incrementPC: false);
            var baseAddress = CPU.ReadShortAt(incompleteYAddress, incrementPC: false);
            var addressWithY = (ushort)(baseAddress + CPU.Registers.Y);

            return getValue
                ? (CPU.ReadByteAt(addressWithY), addressWithY, AreSamePage(addressWithY, baseAddress))
                : ((byte)0x00, addressWithY, AreSamePage(addressWithY, baseAddress));
        }


        public static (byte value, ushort address, bool? samePage) GetAbsoluteX(MOS6502 CPU, bool getValue = true)
        {
            var nextAddress = CPU.ReadNextShort();
            var nextAddressWithX = (ushort)(nextAddress + CPU.Registers.X);

            return getValue
                ? (CPU.ReadByteAt(nextAddressWithX), nextAddressWithX, AreSamePage(nextAddress, nextAddressWithX))
                : ((byte)0x00, nextAddressWithX, AreSamePage(nextAddress, nextAddressWithX));
        }

        public static (byte value, ushort address, bool? samePage) GetAbsoluteY(MOS6502 CPU, bool getValue = true)
        {
            var nextAddress = CPU.ReadNextShort();
            var nextAddressWithY = (ushort)(nextAddress + CPU.Registers.Y);

            return getValue
                ? (CPU.ReadByteAt(nextAddressWithY), nextAddressWithY, AreSamePage(nextAddress, nextAddressWithY))
                : ((byte)0x00, nextAddressWithY, AreSamePage(nextAddress, nextAddressWithY));
        }

        public static (byte value, ushort address, bool? samePage) GetImmediate(MOS6502 CPU, bool getValue = true)
        {
            if (getValue)
            {
                return (CPU.ReadNextByte(), CPU.Registers.PC, null);
            }

            return (0x00, CPU.Registers.PC, null);
        }

        public static (byte value, ushort address, bool? samePage) GetAddressedValue(MOS6502 CPU, OpcodeContainer container, bool getValue = true)
        {
            switch (container.AddressingMode)
            {
                case AddressingModes.Accumulator:
                    // Keep an eye on this one, could cause an issue if caller thinks accumulator has an address like this...
                    return (CPU.Registers.A, 0x0000, null);
                case AddressingModes.IndirectY:
                    return AddressingMode.GetIndirectY(CPU, getValue);
                case AddressingModes.Immediate:
                    return AddressingMode.GetImmediate(CPU, getValue);
                case AddressingModes.ZeroPageX:
                    return AddressingMode.GetZeroPageX(CPU, getValue);
                case AddressingModes.ZeroPage:
                    return AddressingMode.GetZeroPage(CPU, getValue);
                case AddressingModes.Absolute:
                    return AddressingMode.GetAbsolute(CPU, getValue);
                case AddressingModes.AbsoluteY:
                    return AddressingMode.GetAbsoluteY(CPU, getValue);
                case AddressingModes.AbsoluteX:
                    return AddressingMode.GetAbsoluteX(CPU, getValue);
                default:
                    throw new InvalidOperationException($"Unhandled AddressMode ({container.AddressingMode}) for Opcode: ({container.Name})");
            }
        }

        private static bool AreSamePage(ushort a1, ushort a2) => ((a1 ^ a2) & 0xFF00) == 0;
    }
}

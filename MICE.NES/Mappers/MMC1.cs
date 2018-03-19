﻿using MICE.Common.Helpers;
using MICE.Common.Interfaces;
using MICE.Components.CPU;
using MICE.Nintendo.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MICE.Nintendo.Mappers
{
    [Mapper(MemoryMapperIds.MMC1)]
    public class MMC1 : BaseMapper
    {
        private List<(IMemorySegment segment, byte[] bytes)> bankLinkage = new List<(IMemorySegment segment, byte[] bytes)>();

        private byte[] currentProgramRAMBank;

        // PRG bank(internal, $E000-$FFFF)
        private byte[] currentProgramROMBank8000;

        // CHR bank 1 (internal, $C000-$DFFF)
        private byte[] currentProgramROMBankC000;

        // CHR bank 0 (internal, $A000-$BFFF)
        private byte[] currentCharacterROMBank0000;

        // CHR bank 1 (internal, $C000-$DFFF)
        private byte[] currentCharacterROMBank1000;

        /// <summary>
        /// Control ($8000-$9FFF)
        ///PRG bank (internal, $E000-$FFFF)
        /// </summary>
        public byte Control { get; private set; }

        public MMC1(NESCartridge cartridge)
            : base(MemoryMapperIds.MMC1.ToString(), cartridge)
        {
        }

        /// <summary>
        /// Load register ($8000-$FFFF)
        /// </summary>
        public ShiftRegister8Bit LoadRegister { get; } = new ShiftRegister8Bit("Load Register");

        public override void AddMemorySegment(IMemorySegment memorySegment)
        {
            switch (memorySegment)
            {
                case var ms when ms.LowerIndex == 0x6000:
                    break;
                case var ms when ms.LowerIndex == 0x8000:
                    this.bankLinkage.Add((memorySegment, this.cartridge.ProgramROMBanks[0]));
                    this.currentProgramROMBank8000 = this.cartridge.ProgramROMBanks[0];
                    break;
                case var ms when ms.LowerIndex == 0xC000:
                    var whichBank = this.cartridge.ProgramROMBanks.Count == 1
                        ? this.cartridge.ProgramROMBanks[0]
                        : this.cartridge.ProgramROMBanks[1];

                    this.bankLinkage.Add((memorySegment, whichBank));
                    this.currentProgramROMBankC000 = whichBank;
                    break;
                case var ms when ms.LowerIndex == 0x0000:
                    this.bankLinkage.Add((memorySegment, this.cartridge.CharacterRomBanks[0]));
                    this.currentCharacterROMBank0000 = this.cartridge.CharacterRomBanks[0];
                    break;
                case var ms when ms.LowerIndex == 0x1000:
                    this.bankLinkage.Add((memorySegment, this.cartridge.CharacterRomBanks.Last()));
                    this.currentCharacterROMBank1000 = this.cartridge.CharacterRomBanks.Last();
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected memory segment in MMC1 when mapping memory segments (0x{memorySegment.LowerIndex:X4}).");
            }
        }

        /// <summary>
        /// Reads a ushort from a Cartridge's addressable memory space.
        /// The NROM has mirrored PRG-ROM data if only one PRG-ROM bank.
        /// </summary>
        /// <param name="index">The index to read from.</param>
        /// <returns>The data that was read.</returns>
        public override ushort ReadShort(int index)
        {
            foreach (var (segment, bytes) in this.bankLinkage.Where(linkage => linkage.segment.IsIndexInRange(index)))
            {
                var arrayOffset = segment.GetOffsetInSegment(index);
                return BitConverter.ToUInt16(bytes, arrayOffset);
            }

            throw new InvalidOperationException($"Invalid memory range and/or size (ushort) was requested to be read from in NROM Mapper: 0x{index:X4}");
        }

        /// <summary>
        /// Reads a byte from a Cartridge's addressable memory space.
        /// The NROM has mirrored PRG-ROM data if only one PRG-ROM bank.
        /// </summary>
        /// <param name="index">The index to read from.</param>
        /// <returns>The data that was read.</returns>
        public override byte ReadByte(int address)
        {
            var offset = this.GetOffsetInSegment(address);

            switch (address)
            {
                case var _ when address >= 0x8000 && address <= 0x9FFF:
                    return this.currentProgramROMBank8000[address & 0x3FFF];
                case var _ when address >= 0xA000 && address <= 0xBFFF:
                    return this.currentCharacterROMBank0000[offset];
                case var _ when address >= 0xC000 && address <= 0xDFFF:
                    return this.currentCharacterROMBank1000[offset];
                case var _ when address >= 0xE000 && address <= 0xFFFF:
                    // TODO: not sure if this is the right bank to map to for this range atm...
                    return this.currentProgramROMBank8000[address & 0x3FFF];
                default:
                    throw new InvalidOperationException($"Unexpected address (0x{address:X4}) requested in MMC1 Mapper");
            }

            /*
            foreach (var (segment, bytes) in this.bankLinkage.Where(linkage => linkage.segment.IsIndexInRange(index)))
            {
                var arrayOffset = segment.GetOffsetInSegment(index);

                var value = bytes[arrayOffset];
                return value;
            }

            throw new InvalidOperationException($"Invalid memory range and/or size (byte) was requested to be read from in NROM Mapper: 0x{index:X4}");
            */
        }

        public override void Write(int address, byte value)
        {
            // TODO: Convert to memory map maayyyyyybe?
            switch (address)
            {
                case var _ when address >= 0x8000 && address <= 0x9FFF:
                    this.HandleLoadRegister(address, value);
                    break;
                case var _ when address >= 0xA000 && address <= 0xBFFF:
                    break;
                case var _ when address >= 0xC000 && address <= 0xDFFF:
                    break;
                case var _ when address >= 0xE000 && address <= 0xFFFF:
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected address (0x{address:X4}) requested in MMC1 Mapper");
            }
        }

        public override void Write(int index, ushort value) => throw new NotImplementedException();

        public override void CopyBytes(ushort startAddress, Array destination, int destinationIndex, int length) => throw new NotImplementedException();

        private int loadRegisterWriteCount = 0;
        private void HandleLoadRegister(int address, byte value)
        {
            if (value.GetBit(7))
            {
                this.ResetLoadRegister();
                return;
            }

            this.LoadRegister.Write(value);
            this.loadRegisterWriteCount++;

            if (this.loadRegisterWriteCount == 5)
            {
                this.ParseLoadRegister(address);
                this.ResetLoadRegister();
            }

            var incomingValue = Convert.ToString(value, 2).PadLeft(8, '0');
            var existingValue = Convert.ToString(this.LoadRegister.Read(), 2).PadLeft(8, '0');
            Console.WriteLine($"Incoming: {incomingValue} Register: {existingValue}");
        }

        private void ParseLoadRegister(int address)
        {
            switch (address)
            {
                case var _ when address >= 0x8000 && address <= 0x9FFF:
                    this.Control = this.LoadRegister.Read();
                    this.SetNametableMirroring();
                    break;
                case var _ when address >= 0xA000 && address <= 0xBFFF:
                    break;
                case var _ when address >= 0xC000 && address <= 0xDFFF:
                    break;
                case var _ when address >= 0xE000 && address <= 0xFFFF:
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected address (0x{address:X4}) requested in MMC1 Mapper");
            }
        }

        private void SetNametableMirroring()
        {
            switch (this.Control & 0x3)
            {
                case 0:
                    this.cartridge.MirroringMode = MirroringMode.SingleScreen;
                    break;
                case 1:
                    this.cartridge.MirroringMode = MirroringMode.FourScreen;
                    break;
                case 2:
                    this.cartridge.MirroringMode = MirroringMode.Vertical;
                    break;
                case 3:
                    this.cartridge.MirroringMode = MirroringMode.Horizontal;
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected Nametable Mirror value: {this.Control & 0x3}");
            }
        }

        private void ResetLoadRegister()
        {
            this.LoadRegister.Write(0);
            this.loadRegisterWriteCount = 0;
        }
    }
}
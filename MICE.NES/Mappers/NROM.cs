using MICE.Common.Interfaces;
using MICE.Nintendo.Loaders;
using MICE.PPU.RicohRP2C02.Components;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MICE.Nintendo.Mappers
{
    [Mapper(MemoryMapperIds.NROM)]
    public class NROM : BaseMapper
    {
        protected Nametable nametable2000;
        protected Nametable nametable2400;
        protected Nametable nametable2800;
        protected Nametable nametable2C00;

        protected Memory<byte> programROMFirstBank;
        protected Memory<byte> programROMLastBank;

        protected Memory<byte> SRAM;

        protected Memory<byte> CharacterROM0Bank;
        protected Memory<byte> CharacterROM1Bank;

        public NROM(NESCartridge cartridge, MemoryMapperIds id = MemoryMapperIds.NROM)
            : base(id.ToString(), cartridge)
        {
        }

        public override void AddMemorySegment(IMemorySegment memorySegment)
        {
            if (!this.cartridge.CharacterRomBanks.Any())
            {
                this.cartridge.CharacterRomBanks.Add(new byte[0x2000]);
            }

            if (memorySegment.Range.IsInRange(MemoryRanges.SRAM))
            {
                this.SRAM = this.cartridge.SRAM;
            }
            else if (memorySegment.Range.IsInRange(MemoryRanges.ProgramROMFirstBank))
            {
                this.programROMFirstBank = this.cartridge.ProgramROMBanks[0];
            }
            else if (memorySegment.Range.IsInRange(MemoryRanges.ProgramROMLastBank))
            {
                this.programROMLastBank = this.cartridge.ProgramROMBanks.Last();
            }
            else if (memorySegment.Range.IsInRange(MemoryRanges.CharacterROM0))
            {
                this.CharacterROM0Bank = this.cartridge.CharacterRomBanks[0];
            }
            else if (memorySegment.Range.IsInRange(MemoryRanges.CharacterROM1))
            {
                this.CharacterROM1Bank = this.cartridge.CharacterRomBanks.Count == 1
                    ? this.cartridge.CharacterRomBanks[0]
                    : this.cartridge.CharacterRomBanks[1];
            }

            if (MemoryRanges.Nametables.IsInRange(memorySegment.Range))
            {
                this.MapNametable(memorySegment);
            }
        }

        private void MapNametable(IMemorySegment memorySegment)
        {
            var nametable = (memorySegment as Nametable);
            if (nametable == null)
            {
                throw new InvalidOperationException("NROM was given a non-Nametable memory segment in the range of a Nametable to map: " + memorySegment);
            }

            if (this.cartridge.MirroringMode == MirroringMode.SingleScreen)
            {
                this.nametable2000 = nametable;
                this.nametable2400 = nametable;
                this.nametable2800 = nametable;
                this.nametable2C00 = nametable;
            }
            else if (this.cartridge.MirroringMode == MirroringMode.Horizontal)
            {
                switch (nametable.Range.Min)
                {
                    case 0x2000:
                        this.nametable2000 = nametable;
                        break;
                    case 0x2400:
                        this.nametable2400 = this.nametable2000;
                        break;
                    case 0x2800:
                        this.nametable2800 = nametable;
                        break;
                    case 0x2C00:
                        this.nametable2C00 = this.nametable2800;
                        break;
                }
            }
            else if (this.cartridge.MirroringMode == MirroringMode.Vertical)
            {
                switch (nametable.Range.Min)
                {
                    case 0x2000:
                        this.nametable2000 = nametable;
                        break;
                    case 0x2800:
                        this.nametable2800 = this.nametable2000;
                        break;
                    case 0x2400:
                        this.nametable2400 = nametable;
                        break;
                    case 0x2C00:
                        this.nametable2C00 = this.nametable2400;
                        break;
                }
            }
        }

        /// <summary>
        /// Reads a ushort from a Cartridge's addressable memory space.
        /// The NROM has mirrored PRG-ROM data if only one PRG-ROM bank.
        /// </summary>
        /// <param name="index">The index to read from.</param>
        /// <returns>The data that was read.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ushort ReadShort(int index)
        {
            switch (index)
            {
                case var _ when MemoryRanges.ProgramROMFirstBank.TryGetOffset(index, out int offset):
                    return (ushort)(this.programROMFirstBank.Span[offset + 1] << 8 | this.programROMFirstBank.Span[offset]);
                case var _ when MemoryRanges.ProgramROMLastBank.TryGetOffset(index, out int offset):
                    return (ushort)(this.programROMLastBank.Span[offset + 1] << 8 | this.programROMLastBank.Span[offset]);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Reads a byte from a Cartridge's addressable memory space.
        /// The NROM has mirrored PRG-ROM data if only one PRG-ROM bank.
        /// </summary>
        /// <param name="index">The index to read from.</param>
        /// <returns>The data that was read.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte ReadByte(int index)
        {
            switch (index)
            {
                case var _ when MemoryRanges.ProgramROMFirstBank.TryGetOffset(index, out int offset):
                    return this.programROMFirstBank.Span[offset];
                case var _ when MemoryRanges.ProgramROMLastBank.TryGetOffset(index, out int offset):
                    return this.programROMLastBank.Span[offset];
                case var _ when MemoryRanges.CharacterROM1.TryGetOffset(index, out int offset):
                    return this.CharacterROM1Bank.Span[index];
                case var _ when MemoryRanges.Nametable0.TryGetOffset(index, out int offset):
                    return this.nametable2000.Data[offset];
                case var _ when MemoryRanges.Nametable1.TryGetOffset(index, out int offset):
                    return this.nametable2400.Data[offset];
                case var _ when MemoryRanges.Nametable2.TryGetOffset(index, out int offset):
                    return this.nametable2800.Data[offset];
                case var _ when MemoryRanges.Nametable3.TryGetOffset(index, out int offset):
                    return this.nametable2C00.Data[offset];
                case var _ when MemoryRanges.CharacterROM0.TryGetOffset(index, out int offset):
                    return this.CharacterROM0Bank.Span[offset];
                case var _ when MemoryRanges.SRAM.TryGetOffset(index, out int offset):
                    return this.SRAM.Span[offset];
                default:
                    throw new NotImplementedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(int index, byte value)
        {
            switch (index)
            {
                case var _ when MemoryRanges.ProgramROMFirstBank.TryGetOffset(index, out int offset):
                    this.programROMFirstBank.Span[offset] = value;
                    break;
                case var _ when MemoryRanges.ProgramROMLastBank.TryGetOffset(index, out int offset):
                    this.programROMLastBank.Span[offset] = value;
                    break;
                case var _ when MemoryRanges.Nametable0.TryGetOffset(index, out int offset):
                    this.nametable2000.Data[offset] = value;
                    break;
                case var _ when MemoryRanges.Nametable1.TryGetOffset(index, out int offset):
                    this.nametable2400.Data[offset] = value;
                    break;
                case var _ when MemoryRanges.Nametable2.TryGetOffset(index, out int offset):
                    this.nametable2800.Data[offset] = value;
                    break;
                case var _ when MemoryRanges.Nametable3.TryGetOffset(index, out int offset):
                    this.nametable2C00.Data[offset] = value;
                    break;
                case var _ when MemoryRanges.SRAM.TryGetOffset(index, out int offset):
                    this.SRAM.Span[offset] = value;
                    break;
                case var _ when MemoryRanges.CharacterROM0.TryGetOffset(index, out int offset):
                    this.CharacterROM0Bank.Span[offset] = value;
                    break;
                case var _ when MemoryRanges.CharacterROM1.TryGetOffset(index, out int offset):
                    this.CharacterROM1Bank.Span[offset] = value;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void CopyBytes(ushort startAddress, Span<byte> destination, int destinationIndex, int length) => throw new NotImplementedException();
        public override void Write(int index, ushort value) => throw new NotImplementedException();
    }
}

using MICE.Common.Interfaces;
using MICE.Nintendo.Loaders;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MICE.Nintendo.Mappers
{
    [Mapper(MemoryMapperIds.NROM2)]
    public class NROM2 : BaseMapper
    {
        private byte[] memory = new byte[0x10000];
        public Memory<byte> AllMemory { get; }

        private readonly MemoryHandle memoryHandle;

        public Memory<byte> ExpansionROM { get; }
        public Memory<byte> SRAM { get; private set; }
        public Memory<byte> ProgramROMLowerBank { get; private set; }
        public Memory<byte> ProgramROMUpperBank { get; private set; }

        public NROM2(NESCartridge cartridge, MemoryMapperIds id = MemoryMapperIds.NROM2)
            : base(id.ToString(), cartridge)
        {
            this.AllMemory = new Memory<byte>(this.memory);
            this.memoryHandle = this.AllMemory.Pin();
            this.SRAM = MemoryRanges.SRAM.SliceRange(this.AllMemory);
            this.ExpansionROM = MemoryRanges.ExpansionROM.SliceRange(this.AllMemory);
            this.ProgramROMLowerBank = MemoryRanges.ProgramROMLowerBank.SliceRange(this.AllMemory);
            this.ProgramROMUpperBank = MemoryRanges.ProgramROMUpperBank.SliceRange(this.AllMemory);

            this.CopyCartridgeToLocal();

            // this.DumpMemoryBanks();
        }

        private void DumpMemoryBanks()
        {
            var path = @"c:\dumps\";
            Directory.CreateDirectory(path);

            File.WriteAllBytes(Path.Combine(path, "allMemory.hex"), this.AllMemory.ToArray());
            File.WriteAllBytes(Path.Combine(path, "sram.hex"), this.SRAM.ToArray());
            File.WriteAllBytes(Path.Combine(path, "expansion.hex"), this.ExpansionROM.ToArray());
            File.WriteAllBytes(Path.Combine(path, "progromlow.hex"), this.ProgramROMLowerBank.ToArray());
            File.WriteAllBytes(Path.Combine(path, "progromup.hex"), this.ProgramROMUpperBank.ToArray());
        }

        private void CopyCartridgeToLocal()
        {
            if (this.cartridge.SRAM != null)
            {
                this.cartridge.SRAM.CopyTo(this.SRAM);
            }

            this.cartridge.ProgramROMBanks.First().CopyTo(this.ProgramROMLowerBank);
            this.cartridge.ProgramROMBanks.Last().CopyTo(this.ProgramROMUpperBank);
        }

        public override void AddMemorySegment(IMemorySegment memorySegment)
        {
        }

        /// <summary>
        /// Reads a ushort from a Cartridge's addressable memory space.
        /// The NROM has mirrored PRG-ROM data if only one PRG-ROM bank.
        /// </summary>
        /// <param name="index">The index to read from.</param>
        /// <returns>The data that was read.</returns>
        public override ushort ReadShort(int index)
        {
            throw new NotImplementedException();
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
            if (index < 0x2000)
            {
                return this.cartridge.CharacterRomBanks[0][index];
            }

            return this.AllMemory.Span[index];

            return index switch
            {
                var _ when MemoryRanges.ProgramROMFirstBank.TryGetOffset(index, out int offset) => this.AllMemory.Span[index],
                var _ when MemoryRanges.ProgramROMLastBank.TryGetOffset(index, out int offset) => this.AllMemory.Span[index],
                var _ when MemoryRanges.CharacterROM1.TryGetOffset(index, out int offset) => this.cartridge.CharacterRomBanks[0][index],
                var _ when MemoryRanges.Nametable0.TryGetOffset(index, out int offset) => this.AllMemory.Span[index],
                var _ when MemoryRanges.Nametable1.TryGetOffset(index, out int offset) => this.AllMemory.Span[index],
                var _ when MemoryRanges.Nametable2.TryGetOffset(index, out int offset) => this.AllMemory.Span[index],
                var _ when MemoryRanges.Nametable3.TryGetOffset(index, out int offset) => this.AllMemory.Span[index],
                var _ when MemoryRanges.CharacterROM0.TryGetOffset(index, out int offset) => this.cartridge.CharacterRomBanks[0][index],
                var _ when MemoryRanges.SRAM.TryGetOffset(index, out int offset) => this.SRAM.Span[offset],
                _ => throw new NotImplementedException(),
            };
            return this.AllMemory.Span[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Write(int index, byte value)
        {
            switch (index)
            {
                case var _ when MemoryRanges.ProgramROMFirstBank.TryGetOffset(index, out int offset):
                    this.AllMemory.Span[index] = value;
                    break;
                case var _ when MemoryRanges.ProgramROMLastBank.TryGetOffset(index, out int offset):
                    this.AllMemory.Span[index] = value;
                    break;
                case var _ when MemoryRanges.Nametable0.TryGetOffset(index, out int offset):
                    this.AllMemory.Span[index] = value;
                    break;
                case var _ when MemoryRanges.Nametable1.TryGetOffset(index, out int offset):
                    this.AllMemory.Span[index] = value;
                    break;
                case var _ when MemoryRanges.Nametable2.TryGetOffset(index, out int offset):
                    this.AllMemory.Span[index] = value;
                    break;
                case var _ when MemoryRanges.Nametable3.TryGetOffset(index, out int offset):
                    this.AllMemory.Span[index] = value;
                    break;
                case var _ when MemoryRanges.SRAM.TryGetOffset(index, out int offset):
                    this.SRAM.Span[offset] = value;
                    break;
                case var _ when MemoryRanges.CharacterROM0.TryGetOffset(index, out int offset):
                    this.AllMemory.Span[index] = value;
                    break;
                case var _ when MemoryRanges.CharacterROM1.TryGetOffset(index, out int offset):
                    this.AllMemory.Span[index] = value;
                    break;
                default:
                    throw new NotImplementedException();
            }
            this.AllMemory.Span[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void CopyBytes(ushort startAddress, Span<byte> destination, int destinationIndex, int length) => throw new NotImplementedException();
        public override void Write(int index, ushort value) => throw new NotImplementedException();
    }
}

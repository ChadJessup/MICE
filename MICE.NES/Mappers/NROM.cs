using MICE.Common.Interfaces;
using MICE.Nintendo.Loaders;
using MICE.PPU.RicohRP2C02.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MICE.Nintendo.Mappers
{
    [Mapper(MemoryMapperIds.NROM)]
    public class NROM : BaseMapper
    {
        private List<(IMemorySegment segment, byte[] bytes)> bankLinkage = new List<(IMemorySegment segment, byte[] bytes)>();

        private List<Nametable> nametables = new List<Nametable>();

        public NROM(NESCartridge cartridge)
            : base(MemoryMapperIds.NROM.ToString(), cartridge)
        {
        }

        public override void AddMemorySegment(IMemorySegment memorySegment)
        {
            if (!this.cartridge.CharacterRomBanks.Any())
            {
                this.cartridge.CharacterRomBanks.Add(new byte[0x2000]);
            }

            if (memorySegment.Range.Min == 0x6000)
            {
                this.bankLinkage.Add((memorySegment, this.cartridge.SRAM));
            }
            else if (memorySegment.Range.Min == 0x8000)
            {
                this.bankLinkage.Add((memorySegment, this.cartridge.ProgramROMBanks[0]));
            }
            else if (memorySegment.Range.Min == 0xC000)
            {
                var whichBank = this.cartridge.ProgramROMBanks.Count == 1
                    ? this.cartridge.ProgramROMBanks[0]
                    : this.cartridge.ProgramROMBanks[1];

                this.bankLinkage.Add((memorySegment, whichBank));
            }
            else if (memorySegment.Range.Min == 0x0000)
            {
                this.bankLinkage.Add((memorySegment, this.cartridge.CharacterRomBanks[0]));
            }
            else if (memorySegment.Range.Min == 0x1000)
            {
                var whichBank = this.cartridge.CharacterRomBanks.Count == 1
                    ? this.cartridge.CharacterRomBanks[0]
                    : this.cartridge.CharacterRomBanks[1];

                this.bankLinkage.Add((memorySegment, whichBank));
            }

            if (memorySegment.Range.Min >= 0x2000 && memorySegment.Range.Min <= 0x2C00)
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

            this.nametables.Add(nametable);

            if (this.cartridge.MirroringMode == MirroringMode.SingleScreen)
            {
                nametable.Data = this.nametables.First().Data;
            }
            else if (this.cartridge.MirroringMode == MirroringMode.Horizontal)
            {
                switch (nametable.Range.Min)
                {
                    case 0x2000:
                        var other2000 = this.nametables.FirstOrDefault(nt => nt.Range.Min == 0x2400) ?? nametable;
                        nametable.Data = other2000.Data;
                        break;
                    case 0x2400:
                        var other2400 = this.nametables.FirstOrDefault(nt => nt.Range.Min == 0x2000) ?? nametable;
                        nametable.Data = other2400.Data;
                        break;
                    case 0x2800:
                        var other2800 = this.nametables.FirstOrDefault(nt => nt.Range.Min == 0x2C00) ?? nametable;
                        nametable.Data = other2800.Data;
                        break;
                    case 0x2C00:
                        var other2C00 = this.nametables.FirstOrDefault(nt => nt.Range.Min == 0x2800) ?? nametable;
                        nametable.Data = other2C00.Data;
                        break;
                }
            }
            else if (this.cartridge.MirroringMode == MirroringMode.Vertical)
            {
                switch (nametable.Range.Min)
                {
                    case 0x2000:
                        var other2000 = this.nametables.FirstOrDefault(nt => nt.Range.Min == 0x2800) ?? nametable;
                        nametable.Data = other2000.Data;
                        break;
                    case 0x2400:
                        var other2400 = this.nametables.FirstOrDefault(nt => nt.Range.Min == 0x2C00) ?? nametable;
                        nametable.Data = other2400.Data;
                        break;
                    case 0x2800:
                        var other2800 = this.nametables.FirstOrDefault(nt => nt.Range.Min == 0x2000) ?? nametable;
                        nametable.Data = other2800.Data;
                        break;
                    case 0x2C00:
                        var other2C00 = this.nametables.FirstOrDefault(nt => nt.Range.Min == 0x2400) ?? nametable;
                        nametable.Data = other2C00.Data;
                        break;
                }
            }

            this.bankLinkage.Add((memorySegment, nametable.Data));
        }

        /// <summary>
        /// Reads a ushort from a Cartridge's addressable memory space.
        /// The NROM has mirrored PRG-ROM data if only one PRG-ROM bank.
        /// </summary>
        /// <param name="index">The index to read from.</param>
        /// <returns>The data that was read.</returns>
        public override ushort ReadShort(int index)
        {
            var (segment, bytes) = this.bankLinkage.First(linkage => linkage.segment.IsIndexInRange(index));
            var arrayOffset = segment.GetOffsetInSegment(index);

            return BitConverter.ToUInt16(bytes, arrayOffset);
        }

        /// <summary>
        /// Reads a byte from a Cartridge's addressable memory space.
        /// The NROM has mirrored PRG-ROM data if only one PRG-ROM bank.
        /// </summary>
        /// <param name="index">The index to read from.</param>
        /// <returns>The data that was read.</returns>
        public override byte ReadByte(int index)
        {
            var (segment, bytes) = this.bankLinkage.First(linkage => linkage.segment.IsIndexInRange(index));

            if (index < 0x2000)
            {
                return bytes[index];
            }

            var arrayOffset = segment.GetOffsetInSegment(index);

            return bytes[arrayOffset];
        }

        public override void Write(int index, byte value)
        {
            var (segment, bytes) = this.bankLinkage.First(linkage => linkage.segment.IsIndexInRange(index));
            var arrayOffset = segment.GetOffsetInSegment(index);

            bytes[arrayOffset] = value;
        }

        public override void CopyBytes(ushort startAddress, Span<byte> destination, int destinationIndex, int length) => throw new NotImplementedException();
        public override void Write(int index, ushort value) => throw new NotImplementedException();
    }
}

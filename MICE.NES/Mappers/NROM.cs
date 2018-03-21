using MICE.Common.Interfaces;
using MICE.Nintendo.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MICE.Nintendo.Mappers
{
    [Mapper(MemoryMapperIds.NROM)]
    public class NROM : BaseMapper
    {
        private List<(IMemorySegment segment, byte[] bytes)> bankLinkage = new List<(IMemorySegment segment, byte[] bytes)>();

        public NROM(NESCartridge cartridge)
            : base(MemoryMapperIds.NROM.ToString(), cartridge)
        {
        }

        public override void AddMemorySegment(IMemorySegment memorySegment)
        {
            if (!this.cartridge.CharacterRomBanks.Any())
            {
                this.cartridge.CharacterRomBanks.Add(new byte[2000]);
            }

            if (memorySegment.LowerIndex == 0x8000)
            {
                this.bankLinkage.Add((memorySegment, this.cartridge.ProgramROMBanks[0]));
            }
            else if (memorySegment.LowerIndex == 0xC000)
            {
                var whichBank = this.cartridge.ProgramROMBanks.Count == 1
                    ? this.cartridge.ProgramROMBanks[0]
                    : this.cartridge.ProgramROMBanks[1];

                this.bankLinkage.Add((memorySegment, whichBank));
            }
            else if (memorySegment.LowerIndex == 0x0000)
            {
                this.bankLinkage.Add((memorySegment, this.cartridge.CharacterRomBanks[0]));
            }
            else if (memorySegment.LowerIndex == 0x1000)
            {
                var whichBank = this.cartridge.CharacterRomBanks.Count == 1
                    ? this.cartridge.CharacterRomBanks[0]
                    : this.cartridge.CharacterRomBanks[1];

                this.bankLinkage.Add((memorySegment, whichBank));
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
        public override byte ReadByte(int index)
        {
            foreach (var (segment, bytes) in this.bankLinkage.Where(linkage => linkage.segment.IsIndexInRange(index)))
            {
                var arrayOffset = segment.GetOffsetInSegment(index);

                return bytes[arrayOffset];
            }

            throw new InvalidOperationException($"Invalid memory range and/or size (byte) was requested to be read from in NROM Mapper: 0x{index:X4}");
        }

        public override void Write(int index, byte value) {}
        public override void Write(int index, ushort value) { }

        public override void CopyBytes(ushort startAddress, Array destination, int destinationIndex, int length) => throw new NotImplementedException();
    }
}

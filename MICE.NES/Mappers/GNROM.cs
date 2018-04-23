using MICE.Common.Interfaces;
using MICE.Components.Memory;
using MICE.Nintendo.Loaders;
using System;

namespace MICE.Nintendo.Mappers
{
    [Mapper(MemoryMapperIds.GNROM)]
    public class GNROM : NROM
    {
        private int programBank = 0;
        private int characterBank = 0;

        public GNROM(NESCartridge cartridge)
            : base(cartridge, MemoryMapperIds.GNROM)
        {
        }

        public override void CopyBytes(ushort startAddress, Span<byte> destination, int destinationIndex, int length)
        {
        }

        public override byte ReadByte(int index)
        {
            return 0x0;
        }

        public override void Write(int index, byte value)
        {
        }

        public override void Write(int index, ushort value)
        {
        }
    }
}
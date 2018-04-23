using MICE.Common.Interfaces;
using MICE.Common.Misc;
using MICE.Components.Memory;
using MICE.Nintendo.Interfaces;
using MICE.Nintendo.Loaders;
using System.Runtime.CompilerServices;

namespace MICE.Nintendo.Mappers
{
    public abstract class BaseMapper : MemorySegment, IMemoryManagementController
    {
        protected static class MemoryRanges
        {
            public static Range CharacterROMBanks = new Range(0x0000, 0x1FFF);
            public static Range CharacterROM0 = new Range(0x0000, 0x0FFF);
            public static Range CharacterROM1 = new Range(0x1000, 0x1FFF);

            public static Range Nametables = new Range(0x2000, 0x2FFF);
            public static Range Nametable0 = new Range(0x2000, 0x23FF);
            public static Range Nametable1 = new Range(0x2400, 0x27FF);
            public static Range Nametable2 = new Range(0x2800, 0x2BFF);
            public static Range Nametable3 = new Range(0x2C00, 0x2FFF);

            public static Range ExpansionROM = new Range(0x4020, 0x05FFF);

            // These names are sometimes interchanged, so let's have both.
            public static Range ProgramRAM = new Range(0x6000, 0x7FFF);
            public static Range SRAM = new Range(0x6000, 0x7FFF);

            public static Range ProgramROM = new Range(0x8000, 0xFFFF);

            public static Range ProgramROMLowerBank = new Range(0x8000, 0xBFFF);
            public static Range ProgramROMFirstBank = new Range(0x8000, 0xBFFF);

            public static Range ProgramROMUpperBank = new Range(0xC000, 0xFFFF);
            public static Range ProgramROMLastBank = new Range(0xC000, 0xFFFF);
        }

        protected readonly NESCartridge cartridge;

        public BaseMapper(string name, NESCartridge cartridge)
            : base(new Range(0x8000, 0xFFFF), $"{name} Mapper")
        {
            this.cartridge = cartridge;
        }

        public override byte[] GetBytes() => new byte[] { 0x0 };
        public abstract void AddMemorySegment(IMemorySegment memorySegment);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetOffset(Range range, int index)
        {
            if (range.Min == 0)
            {
                return index;
            }

            return index - range.Min;
        }
    }
}

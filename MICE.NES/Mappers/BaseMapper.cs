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
            public static Range CharacterROMBankRange = new Range(0x0000, 0x1FFF);
            public static Range CharacterROM0Range = new Range(0x0000, 0x0FFF);
            public static Range CharacterROM1Range = new Range(0x1000, 0x1FFF);

            public static Range NametableRange = new Range(0x2000, 0x2FFF);
            public static Range Nametable0Range = new Range(0x2000, 0x23FF);
            public static Range Nametable1Range = new Range(0x2400, 0x27FF);
            public static Range Nametable2Range = new Range(0x2800, 0x2BFF);
            public static Range Nametable3Range = new Range(0x2C00, 0x2FFF);

            public static Range ProgramRAM = new Range(0x6000, 0x7FFF);

            public static Range ProgramROMRange = new Range(0x8000, 0xFFFF);
            public static Range ProgramROMFirstBank = new Range(0x8000, 0xBFFF);
            public static Range ProgramROMLastBank = new Range(0xC000, 0xFFFF);
        }

        protected readonly NESCartridge cartridge;

        public BaseMapper(string name, NESCartridge cartridge)
            : base(new Range(0x8000, 0xFFFF), null, $"{name} Mapper")
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

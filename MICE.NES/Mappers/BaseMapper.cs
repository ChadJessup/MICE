using MICE.Common.Interfaces;
using MICE.Common.Misc;
using MICE.Components.Memory;
using MICE.Nintendo.Interfaces;
using MICE.Nintendo.Loaders;

namespace MICE.Nintendo.Mappers
{
    public abstract class BaseMapper : MemorySegment, IMemoryManagementController
    {
        protected readonly NESCartridge cartridge;

        public BaseMapper(string name, NESCartridge cartridge)
            : base(new Range(0x8000, 0xFFFF), $"{name} Mapper")
        {
            this.cartridge = cartridge;
        }

        public override byte[] GetBytes() => new byte[] { 0x0 };
        public abstract void AddMemorySegment(IMemorySegment memorySegment);

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

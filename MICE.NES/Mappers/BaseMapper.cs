using MICE.Common.Interfaces;
using MICE.Components.Memory;
using MICE.Nintendo.Interfaces;
using MICE.Nintendo.Loaders;

namespace MICE.Nintendo.Mappers
{
    public abstract class BaseMapper : MemorySegment, IMemoryManagementController
    {
        protected readonly NESCartridge cartridge;

        public BaseMapper(string name, NESCartridge cartridge)
            : base(0x8000, 0xFFFF, $"{name} Mapper")
        {
            this.cartridge = cartridge;
        }

        public abstract void AddMemorySegment(IMemorySegment memorySegment);
    }
}

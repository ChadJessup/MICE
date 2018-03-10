using MICE.Common.Interfaces;
using MICE.Components.Memory;
using MICE.Nintendo.Interfaces;
using MICE.Nintendo.Loaders;
using System.IO;

namespace MICE.Nintendo.Mappers
{
    public abstract class BaseMapper : MemorySegment, IMemoryManagementController
    {
        protected StreamWriter sw;
        protected readonly NESCartridge cartridge;

        public BaseMapper(string name, NESCartridge cartridge, StreamWriter sw)
            : base(0x8000, 0xFFFF, $"{name} Mapper")
        {
            this.sw = sw;
            this.cartridge = cartridge;
        }

        public abstract void AddMemorySegment(IMemorySegment memorySegment);
    }
}

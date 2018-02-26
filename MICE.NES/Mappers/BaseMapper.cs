using MICE.Components.Memory;
using MICE.Nintendo.Interfaces;

namespace MICE.Nintendo.Mappers
{
    public abstract class BaseMapper : MemorySegment, IMemoryManagementController
    {
        public BaseMapper(string name)
            : base(0x8000, 0xFFFF, $"{name} Mapper")
        {
        }
    }
}

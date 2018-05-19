using MICE.Nintendo.Loaders;

namespace MICE.Nintendo.Mappers
{
    [Mapper(MemoryMapperIds.CNROM)]
    public class CNROM : NROM
    {
        public CNROM(NESCartridge cartridge, MemoryMapperIds id = MemoryMapperIds.CNROM)
            : base(cartridge, id)
        {
        }
    }
}
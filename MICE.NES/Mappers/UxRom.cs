using MICE.Nintendo.Loaders;

namespace MICE.Nintendo.Mappers
{
    [Mapper(MemoryMapperIds.UNROM)]
    public class UNROM : NROM2
    {
        public UNROM(NESCartridge cartridge)
            : base(cartridge)
        {
        }
    }
}
namespace MICE.Nintendo.Mappers
{
    [Mapper(MemoryMapperIds.NROM)]
    public class NROM : BaseMapper
    {
        public NROM() : base(MemoryMapperIds.NROM.ToString())
        {
        }
    }
}

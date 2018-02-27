namespace MICE.Nintendo.Mappers
{
    [Mapper(MemoryMapperIds.NROM)]
    public class NROM : BaseMapper
    {
        public NROM() : base(MemoryMapperIds.NROM.ToString())
        {
        }

        public override T Read<T>(int index)
        {
            return default(T);
        }

        public override void Write<T>(int index, T value)
        {
        }
    }
}

namespace MICE.Common.Interfaces
{
    public interface IExternal : IMemorySegment
    {
        void AttachHandler(IExternalHandler handler);
    }
}

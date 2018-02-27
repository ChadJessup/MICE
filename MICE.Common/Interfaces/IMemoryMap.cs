namespace MICE.Common.Interfaces
{
    /// <summary>
    /// Interface that represents memory that is mapped to various memory segments.
    /// </summary>
    public interface IMemoryMap
    {
        T Read<T>(int index);
        void Write<T>(int index, T value);
    }
}

namespace MICE.Common.Interfaces
{
    public interface IExternalHandler
    {
        T Read<T>(int index);
        void Write<T>(int index, T value);
    }
}

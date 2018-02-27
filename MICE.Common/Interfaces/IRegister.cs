namespace MICE.Common.Interfaces
{
    public interface IRegister<T>
    {
        void Write(T value);
        T Read();
    }
}

namespace MICE.Common.Interfaces
{
    public interface IExternalHandler
    {
        byte Read(int index);
        void Write(int index, byte value);
    }
}

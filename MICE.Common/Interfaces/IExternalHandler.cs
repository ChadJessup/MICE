namespace MICE.Common.Interfaces
{
    public interface IExternalHandler
    {
        void AddMemorySegment(IMemorySegment memorySegment);

        byte ReadByte(int index);
        ushort ReadShort(int index);

        void Write(int index, byte value);
        void Write(int index, ushort value);
    }
}

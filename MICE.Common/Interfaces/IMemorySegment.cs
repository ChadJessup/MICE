namespace MICE.Common.Interfaces
{
    /// <summary>
    /// Inteferface that represents a segment of memory.
    /// </summary>
    public interface IMemorySegment
    {
        int LowerIndex { get; }
        int UpperIndex { get; }
        string Name { get; }

        bool IsIndexInRange(int index);

        (int min, int max) GetRange();
        byte Read(int index);
        void Write(int index, byte value);
    }
}

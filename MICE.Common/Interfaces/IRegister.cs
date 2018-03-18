using System;

namespace MICE.Common.Interfaces
{
    public interface IRegister<T> where T : struct
    {
        void Write(T value);
        T Read();
        Action<int?, T> AfterReadAction { get; }
        Action<int?, T> AfterWriteAction { get; }
        Func<int?, T, byte> ReadByteInsteadAction { get; set; }
        Func<int?, T, ushort> ReadShortInsteadAction { get; set; }
    }
}

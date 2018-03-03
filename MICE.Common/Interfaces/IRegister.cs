using System;

namespace MICE.Common.Interfaces
{
    public interface IRegister<T> where T : struct
    {
        void Write(T value);
        T Read();
        Action AfterReadAction { get; }
    }
}

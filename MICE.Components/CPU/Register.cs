using MICE.Common.Interfaces;
using System;

namespace MICE.Components.CPU
{
    public abstract class Register<T> : IRegister<T> where T : struct
    {
        public string Name { get; private set; }
        protected T Value { get; set; }

        public Register(string name, Action<int?, T>? afterRead = null, Action<int?, T>? afterWrite = null)
        {
            this.Name = name;
            this.AfterReadAction = afterRead;
            this.AfterWriteAction = afterWrite;
        }

        public abstract void Write(T value);
        public abstract T Read();
        public Action<int?, T>? AfterReadAction { get; set; }
        public Action<int?, T>? AfterWriteAction { get; set; }
        public virtual Func<int?, T, byte>? ReadByteInsteadAction { get; set; }
        public virtual Func<int?, T, ushort>? ReadShortInsteadAction { get; set; }

        public static implicit operator T(Register<T> register) => register.Value;
    }
}
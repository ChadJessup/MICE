using MICE.Common.Interfaces;
using System;

namespace MICE.Components.CPU
{
    public abstract class Register<T> : IRegister<T> where T : struct
    {
        public string Name { get; private set; }
        protected T Value { get; set; }

        public Register(string name, Action afterRead = null, Action<T> afterWrite = null)
        {
            this.Name = name;
            this.AfterReadAction = afterRead;
            this.AfterWriteAction = afterWrite;
        }

        public abstract void Write(T value);
        public abstract T Read();
        public Action AfterReadAction { get; set; }
        public Action<T> AfterWriteAction { get; set; }

        public static implicit operator T(Register<T> register) => register.Value;
    }
}
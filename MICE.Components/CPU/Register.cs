using MICE.Common.Interfaces;
using System;

namespace MICE.Components.CPU
{
    public abstract class Register<T> : IRegister<T> where T : struct
    {
        public string Name { get; private set; }
        protected T Value { get; set; }

        public Register(string name, Action afterRead = null)
        {
            this.Name = name;
            this.AfterReadAction = afterRead;
        }

        public abstract void Write(T value);
        public abstract T Read();
        public Action AfterReadAction { get; set; }

        public static implicit operator T(Register<T> register) => register.Value;
    }
}
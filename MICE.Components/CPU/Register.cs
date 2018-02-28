﻿using MICE.Common.Interfaces;

namespace MICE.Components.CPU
{
    public abstract class Register<T> : IRegister<T>
    {
        public string Name { get; private set; }
        protected T Value { get; set; }

        public Register(string name)
        {
            this.Name = name;
        }

        public abstract void Write(T value);
        public abstract T Read();

        public static implicit operator T(Register<T> register) => register.Value;
    }
}
using MICE.Common.Interfaces;

namespace MICE.Components.CPUs
{
    public abstract class Register : IRegister
    {
        public string Name { get; private set; }

        public Register(string name)
        {
            this.Name = name;
        }
    }
}
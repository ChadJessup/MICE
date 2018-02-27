using System;

namespace MICE.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Delegate | AttributeTargets.Method | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public abstract class OpcodeAttribute : Attribute
    {
        public int Code { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public OpcodeAttribute(int code, string name, string description = "")
        {
            this.Code = code;
            this.Name = name;
            this.Description = description;
        }
    }
}

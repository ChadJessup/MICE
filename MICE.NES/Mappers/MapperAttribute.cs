using System;

namespace MICE.Nintendo.Mappers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MapperAttribute : Attribute
    {
        public MemoryMapperIds Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public MapperAttribute(MemoryMapperIds id) => this.Id = id;
    }
}
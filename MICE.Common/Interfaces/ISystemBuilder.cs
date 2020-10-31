using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MICE.Common.Interfaces
{
    public interface ISystemBuilder
    {
        public IServiceCollection ServiceCollection { get; }
        public ISystemBuilder WithCPU<TCPU>() where TCPU : class, ICPU;
        public ISystemBuilder WithLoader<TLoader>() where TLoader : class, ILoader;
        public ISystemBuilder WithMemoryMap<TMemoryMap>() where TMemoryMap : class, IMemoryMap;
        public ISystemBuilder WithComponent<TComponent>() where TComponent : IMICEComponent;

        public ISystem Build<TSystem>() where TSystem : ISystem;
    }
}

using MICE.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;

namespace MICE.Components
{
    public class SystemBuilder : ISystemBuilder
    {
        public SystemBuilder(IConfiguration configuration, IServiceCollection serviceCollection)
        {
            this.Configuration = configuration;
            this.ServiceCollection = serviceCollection;
        }

        public SystemBuilder(IConfiguration configuration)
            : this(configuration, new ServiceCollection())
        {
        }

        public SystemBuilder(IServiceCollection serviceCollection)
            : this(new ConfigurationBuilder().Build(), serviceCollection)
        {
        }

        public SystemBuilder()
            : this(new ConfigurationBuilder().Build(), new ServiceCollection())
        {
        }

        public IServiceCollection ServiceCollection { get; }
        public IConfiguration Configuration { get; }

        public ISystemBuilder WithCPU<TCPU>()
            where TCPU : class, ICPU
        {
            this.ServiceCollection.TryAddSingleton<ICPU, TCPU>();

            return this;
        }

        public ISystemBuilder WithLoader<TLoader>()
            where TLoader : class, ILoader
        {
            this.ServiceCollection.TryAddSingleton(typeof(TLoader));

            return this;
        }

        public ISystemBuilder WithComponent<TComponent>()
            where TComponent : IMICEComponent
        {
            this.ServiceCollection.TryAddSingleton(typeof(TComponent));

            return this;
        }

        public ISystemBuilder WithMemoryMap<TMemoryMap>()
            where TMemoryMap : class, IMemoryMap
        {
            this.ServiceCollection.TryAddSingleton<IMemoryMap, TMemoryMap>();

            return this;
        }

        public ISystem Build<TSystem>()
            where TSystem : ISystem
        {
            var system = ActivatorUtilities.GetServiceOrCreateInstance<TSystem>(this.ServiceCollection.BuildServiceProvider());

            return system;
        }
    }
}

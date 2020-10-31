using MICE.Common.Interfaces;
using MICE.CPU.MOS6502;
using MICE.Nintendo;
using MICE.Nintendo.Handlers;
using MICE.PPU.RicohRP2C02;
using MICE.PPU.RicohRP2C02.Handlers;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MICE.Components
{
    public static class SystemBuilderExtensions
    {
        public static ISystemBuilder WithNESComponents(this ISystemBuilder builder)
        {
            var collection = builder.ServiceCollection;

            collection.TryAddSingleton<Registers>();
            collection.TryAddSingleton<NESContext>();
            collection.TryAddSingleton<RicohRP2C02>();
            collection.TryAddSingleton<PPURegisters>();
            collection.TryAddSingleton<IPPUMemoryMap, PPURawMemoryMap>();
            collection.TryAddSingleton<IClock, LockedFramerateClock>();
            collection.TryAddSingleton<PPUInternalRegisters>();
            collection.TryAddSingleton<BackgroundHandler>();
            collection.TryAddSingleton<PaletteHandler>();
            collection.TryAddSingleton<SpriteHandler>();
            collection.TryAddSingleton<ScrollHandler>();
            collection.TryAddSingleton<InputHandler>();
            collection.TryAddSingleton<PixelMuxer>();


            return builder;
        }
    }
}

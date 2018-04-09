using MICE.Common.Interfaces;
using MICE.CPU.MOS6502;
using MICE.Nintendo.Handlers;
using MICE.PPU.RicohRP2C02;
using MICE.PPU.RicohRP2C02.Handlers;
using Ninject.Modules;

namespace MICE.Nintendo
{
    public class NintendoModule : NinjectModule
    {
        public override void Load()
        {
            Bind<NES>().To<NES>().InSingletonScope();
            Bind<NESContext>().To<NESContext>().InSingletonScope();

            Bind<ICPU>().To<Ricoh2A03>().InSingletonScope();
            Bind<IMemoryMap>().To<NESRawMemoryMap>().InSingletonScope().Named("CPU");

            Bind<Registers>().To<Registers>().InSingletonScope();
            Bind<RicohRP2C02>().To<RicohRP2C02>().InSingletonScope();
            Bind<PPURegisters>().To<PPURegisters>().InSingletonScope();
            Bind<IMemoryMap>().To<PPUMemoryMap>().InSingletonScope().Named("PPU");
            Bind<PPUInternalRegisters>().To<PPUInternalRegisters>().InSingletonScope();

            Bind<BackgroundHandler>().To<BackgroundHandler>().InSingletonScope();
            Bind<PaletteHandler>().To<PaletteHandler>().InSingletonScope();
            Bind<ScrollHandler>().To<ScrollHandler>().InSingletonScope();
            Bind<InputHandler>().To<InputHandler>().InSingletonScope();
            Bind<PixelMuxer>().To<PixelMuxer>().InSingletonScope();
        }
    }
}

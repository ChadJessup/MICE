using MICE.Common.Interfaces;
using MICE.CPU.MOS6502;
using MICE.PPU.RicohRP2C02;
using Ninject.Modules;

namespace MICE.Nintendo
{
    public class NintendoModule : NinjectModule
    {
        public override void Load()
        {
            Bind<NESContext>().To<NESContext>().InSingletonScope();
            Bind<NES>().To<NES>().InSingletonScope();

            Bind<ICPU>().To<Ricoh2A03>().InSingletonScope();
            Bind<IMemoryMap>().To<NESMemoryMap>().InSingletonScope().Named("CPU");

            Bind<RicohRP2C02>().To<RicohRP2C02>().InSingletonScope();
            Bind<IMemoryMap>().To<PPUMemoryMap>().InSingletonScope().Named("PPU");
            Bind<PPURegisters>().To<PPURegisters>().InSingletonScope();
            Bind<PPUInternalRegisters>().To<PPUInternalRegisters>().InSingletonScope();
            Bind<Registers>().To<Registers>().InSingletonScope();
        }
    }
}

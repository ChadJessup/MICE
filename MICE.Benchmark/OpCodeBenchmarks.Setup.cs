using BenchmarkDotNet.Attributes;
using MICE.CPU.MOS6502;
using MICE.Nintendo;
using MICE.Nintendo.Loaders;

namespace MICE.Benchmark
{
    public partial class OpCodeBenchmarks
    {
        private NESContext nesContext;
        private Opcodes opcodes;
        private NES nes;

        [GlobalSetup]
        public void Setup()
        {
            var cartridge = new NESLoader().Load<NESCartridge>(@"C:\Emulators\NES\Games\Super Mario Bros.nes");

            this.nes.Load(cartridge);

            this.nes.PowerOn();
            this.opcodes = this.nes.CPU.Opcodes;
        }
    }
}

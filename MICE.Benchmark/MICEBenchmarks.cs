using BenchmarkDotNet.Attributes;
using MICE.Nintendo;
using MICE.Nintendo.Loaders;

namespace MICE.Benchmark
{
    public class MICEBenchmarks
    {
        private NESContext nesContext;
        private NES nes;

        [Params
            (
            @"C:\Emulators\NES\Games\USA\Legend of Zelda, The (U) (PRG 1).nes",
            @"C:\Emulators\NES\Games\World\Donkey Kong (JU).nes",
            @"C:\Emulators\NES\Games\Super Mario Bros.nes",
            @"C:\Emulators\NES\Games\USA\Bionic Commando (U).nes",
            @"C:\Emulators\NES\Games\USA\Mega Man (U).nes",
            @"C:\Emulators\NES\Games\USA\Slalom (U).nes"
            )]
        public string cartridgePath;

        [GlobalSetup]
        public void Setup()
        {
            // this.nesContext = this.kernel.Get<NESContext>();
            // var cartridge = NESLoader.CreateCartridge(this.cartridgePath);
            // 
            // this.nes = this.kernel.Get<NES>();
            // this.nes.LoadCartridge(cartridge);
            // 
            // this.nes.PowerOn();
        }



        /// <summary>
        /// Baseline writing the data to the pipe with out Bedrock; used to compare the cost of adding the protocol reader.
        /// </summary>
        [Benchmark(Baseline = true)]
        public void StepFrame()
        {
            this.nes.StepFrame();
        }
    }
}
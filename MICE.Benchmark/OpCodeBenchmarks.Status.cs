using BenchmarkDotNet.Attributes;
using MICE.CPU.MOS6502;
using MICE.Nintendo;
using MICE.Nintendo.Loaders;

namespace MICE.Benchmark
{
    public partial class OpCodeBenchmarks
    {
        [Benchmark]
        public void SEI() => this.opcodes.SEI(null, 0x1234);
    }
}

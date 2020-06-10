using BenchmarkDotNet.Attributes;

namespace MICE.Benchmark
{
    public partial class OpCodeBenchmarks
    {
        [Benchmark]
        public void STA() => this.opcodes.STA(null, 0x1234);
    }
}

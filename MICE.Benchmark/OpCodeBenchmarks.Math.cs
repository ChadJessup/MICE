using BenchmarkDotNet.Attributes;

namespace MICE.Benchmark
{
    public partial class OpCodeBenchmarks
    {
        [Benchmark] [BenchmarkCategory("math")] public void ADC() => this.opcodes.ADC(null, 0x1234);
        [Benchmark] [BenchmarkCategory("math")] public void SBC() => this.opcodes.SBC(null, 0x1234);
    }
}

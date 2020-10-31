using BenchmarkDotNet.Attributes;

namespace MICE.Benchmark
{
    public partial class OpCodeBenchmarks
    {
        [Benchmark] [BenchmarkCategory("compare")] public void CPY() => this.opcodes.CPY(null, 0x1234);
        [Benchmark] [BenchmarkCategory("compare")] public void CPX() => this.opcodes.CPX(null, 0x1234);
        [Benchmark] [BenchmarkCategory("compare")] public void CMP() => this.opcodes.CMP(null, 0x1234);
    }
}

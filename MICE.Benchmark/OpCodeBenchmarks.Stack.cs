using BenchmarkDotNet.Attributes;

namespace MICE.Benchmark
{
    public partial class OpCodeBenchmarks
    {
        [Benchmark] [BenchmarkCategory("stack")] public void TXS() => this.opcodes.TXS(null, 0x1234);
        [Benchmark] [BenchmarkCategory("stack")] public void TSX() => this.opcodes.TSX(null, 0x1234);
        [Benchmark] [BenchmarkCategory("stack")] public void PHA() => this.opcodes.PHA(null, 0x1234);
        [Benchmark] [BenchmarkCategory("stack")] public void PLA() => this.opcodes.PLA(null, 0x1234);
        [Benchmark] [BenchmarkCategory("stack")] public void PHP() => this.opcodes.PHP(null, 0x1234);
        [Benchmark] [BenchmarkCategory("stack")] public void PLP() => this.opcodes.PLP(null, 0x1234);
    }
}

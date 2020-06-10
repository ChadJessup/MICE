using BenchmarkDotNet.Attributes;

namespace MICE.Benchmark
{
    public partial class OpCodeBenchmarks
    {
        [Benchmark] [BenchmarkCategory("bit")] public void BIT() => this.opcodes.BIT(null, 0x1234);
        [Benchmark] [BenchmarkCategory("bit")] public void ORA() => this.opcodes.ORA(null, 0x1234);
        [Benchmark] [BenchmarkCategory("bit")] public void AND() => this.opcodes.AND(null, 0x1234);
        [Benchmark] [BenchmarkCategory("bit")] public void LSR() => this.opcodes.LSR(null, 0x1234);
        [Benchmark] [BenchmarkCategory("bit")] public void ROR() => this.opcodes.ROR(null, 0x1234);
        [Benchmark] [BenchmarkCategory("bit")] public void ROL() => this.opcodes.ROL(null, 0x1234);
        [Benchmark] [BenchmarkCategory("bit")] public void ASL() => this.opcodes.ASL(null, 0x1234);
        [Benchmark] [BenchmarkCategory("bit")] public void INC() => this.opcodes.INC(null, 0x1234);
        [Benchmark] [BenchmarkCategory("bit")] public void DEC() => this.opcodes.DEC(null, 0x1234);
        [Benchmark] [BenchmarkCategory("bit")] public void EOR() => this.opcodes.EOR(null, 0x1234);
    }
}

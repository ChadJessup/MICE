using MICE.Common.Interfaces;
using System.Text;

namespace MICE.CPU.MOS6502
{
    public class Ricoh2A03 : MOS6502
    {
        public Ricoh2A03(IMemoryMap memoryMap, StringBuilder trace)
            : base(memoryMap, trace)
        {
        }
    }
}

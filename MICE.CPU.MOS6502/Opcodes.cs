using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MICE.CPU.MOS6502
{
    public class Opcodes
    {
        private static class Constants
        {
            static Constants()
            {
                var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                foreach (var methodInfo in typeof(Constants).GetMethods(bindingFlags).Where(m => m.CustomAttributes.Any(ca => ca.AttributeType == typeof(MOS6502OpcodeAttribute))))
                {
                    foreach (var attrib in methodInfo.GetCustomAttributes<MOS6502OpcodeAttribute>())
                    {
                        Constants.OpCodeMap.Add(attrib.Code, new OpcodeContainer(attrib, methodInfo));
                    }
                }
            }

            public static Dictionary<int, OpcodeContainer> OpCodeMap = new Dictionary<int, OpcodeContainer>();
        }

        [MOS6502Opcode(0x24, 3, "BIT", AddressingMode.ZeroPage)]
        [MOS6502Opcode(0x2C, 4, "BIT", AddressingMode.Absolute)]
        private void BIT()
        {
        }
    }
}

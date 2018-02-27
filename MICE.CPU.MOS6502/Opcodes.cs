using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MICE.CPU.MOS6502
{
    public class Opcodes
    {
        private Dictionary<int, OpcodeContainer> opCodeMap = new Dictionary<int, OpcodeContainer>();

        public Opcodes()
        {
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            foreach (var methodInfo in this.GetType().GetMethods(bindingFlags).Where(m => m.CustomAttributes.Any(ca => ca.AttributeType == typeof(MOS6502OpcodeAttribute))))
            {
                foreach (var attrib in methodInfo.GetCustomAttributes<MOS6502OpcodeAttribute>())
                {
                    this.opCodeMap.Add(attrib.Code, new OpcodeContainer(attrib, methodInfo));
                }
            }


        }

        [MOS6502Opcode(0x24, 3, "BIT", AddressingMode.ZeroPage)]
        [MOS6502Opcode(0x2C, 4, "BIT", AddressingMode.Absolute)]
        private void BIT()
        {
        }
    }

    public class OpcodeContainer
    {
        public OpcodeContainer(MOS6502OpcodeAttribute details, MethodInfo obj)
        {

        }
    }
}

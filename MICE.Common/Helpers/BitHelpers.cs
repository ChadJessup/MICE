using System;
using System.Collections.Generic;
using System.Text;

namespace MICE.Common.Helpers
{
    public static class BitHelpers
    {
        public static bool GetBit(this byte b, int bitNumber) => (b & (1 << bitNumber)) != 0;
    }
}

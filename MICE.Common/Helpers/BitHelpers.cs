using System;

namespace MICE.Common.Helpers
{
    public static class BitHelpers
    {
        public static bool GetBit(this byte b, int bitNumber) => (b & (1 << bitNumber)) != 0;
        public static T Get<T>(this ArraySegment<T> segment, int index) => segment.Array[segment.Offset + index];
        public static void SetBit(this byte b, int index, bool value)
        {
            b = value
            ? (byte)(b | (1 << index))
            : (byte)(b & ~(1 << index));
        }
    }
}

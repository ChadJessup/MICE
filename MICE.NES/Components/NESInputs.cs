using System;

namespace MICE.Nintendo.Components
{
    [Flags]
    public enum NESInputs
    {
        Unknown = 1 << 0,
        A       = 1 << 1,
        B       = 1 << 2,
        Up      = 1 << 3,
        Down    = 1 << 4,
        Left    = 1 << 5,
        Right   = 1 << 6,
        Select  = 1 << 7,
        Start   = 1 << 8,
    }
}

using System.Threading;

namespace MICE.Common.Interfaces
{
    /// <summary>
    /// Interface that represents a CPU of a system.
    /// </summary>
    public interface ICPU : IMicroprocessor
    {
        Endianness Endianness { get; }
        void Reset(CancellationToken cancellationToken);
    }
}

using System.Threading;

namespace MICE.Common.Interfaces
{
    public interface IMicroprocessor : IIntegratedCircuit
    {
        void PowerOn(CancellationToken cancellationToken);
        int Step();
    }
}

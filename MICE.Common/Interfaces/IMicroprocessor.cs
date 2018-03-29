using System.Threading;

namespace MICE.Common.Interfaces
{
    public interface IMicroprocessor : IIntegratedCircuit
    {
        bool IsPowered { get; }
        void PowerOff();
        void PowerOn();
        int Step();
    }
}

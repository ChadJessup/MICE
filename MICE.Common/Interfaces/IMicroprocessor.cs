using System;
using System.Threading;

namespace MICE.Common.Interfaces
{
    public interface IMicroprocessor : IIntegratedCircuit
    {
        bool IsPowered { get; }
        void PowerOff();
        void PowerOn(Action cycleComplete);
        int Step();
    }
}

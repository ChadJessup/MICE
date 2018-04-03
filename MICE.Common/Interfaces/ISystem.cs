using System.Threading.Tasks;

namespace MICE.Common.Interfaces
{
    /// <summary>
    /// Interface for a system, or collection, of components that represents a complete system, e.g., NES/SNES, etc.
    /// </summary>
    public interface ISystem
    {
        string Name { get; }

        bool IsPoweredOn { get; }
        bool IsPaused { get; }

        void InputChanged(object inputs);
        void Pause(bool isPaused);
        void PowerOn();
        void PowerOff();
        void Reset();
        Task Run();
        void Step();
    }
}

using System.Threading.Tasks;

namespace MICE.Common.Interfaces
{
    /// <summary>
    /// Interface for a system, or collection, of components that represents a complete system, e.g., NES/SNES, etc.
    /// </summary>
    public interface ISystem
    {
        string Name { get; }

        void PowerOn();
        Task PowerOff();
        Task Reset();
    }
}

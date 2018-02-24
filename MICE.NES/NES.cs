using MICE.Common.Interfaces;
using MICE.Components.Buses;
using MICE.Components.CPUs;
using System.Threading.Tasks;

namespace MICE.Nintendo
{
    public class NES : ISystem
    {
        public string Name { get; } = "Nintendo Entertainment System";

        // Create components...
        public Ricoh2A03 CPU { get; } = new Ricoh2A03();

        public DataBus DataBus { get; } = new DataBus();
        public AddressBus AddressBus { get; } = new AddressBus();
        public ControlBus ControlBus { get; } = new ControlBus();

        public NESMemoryMap MemoryMap { get; } = new NESMemoryMap();

        // Hook them up...

        public async Task PowerOn()
        {
            await Task.CompletedTask;
        }

        public async Task PowerOff()
        {
            await Task.CompletedTask;
        }

        public async Task Reset()
        {
            await Task.CompletedTask;
        }
    }
}

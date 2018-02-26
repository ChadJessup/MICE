using MICE.Common.Interfaces;
using MICE.Components.Bus;
using MICE.Components.Memory;
using MICE.CPU.MOS6502;
using MICE.Nintendo.Loaders;
using System;
using System.Threading.Tasks;
using System.Linq;

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

        public async Task Run()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Loads an <seealso cref="NESCartridge"/> into the NES system.
        /// </summary>
        /// <param name="cartridge">The cartridge to load.</param>
        public void LoadCartridge(NESCartridge cartridge)
        {
            // Various parts of a cartridge are mapped seamlessly into the NES's memory map.
            this.MemoryMap.Get<SRAM>("SRAM").Data = cartridge.SRAM;

            this.MemoryMap.Get<External>("PRG-ROM Lower Bank").Handler = cartridge.Mapper;
            this.MemoryMap.Get<External>("PRG-ROM Upper Bank").Handler = cartridge.Mapper;
        }
    }
}

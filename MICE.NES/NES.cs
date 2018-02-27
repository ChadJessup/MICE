using MICE.Common.Interfaces;
using MICE.Components.Bus;
using MICE.Components.Memory;
using MICE.CPU.MOS6502;
using MICE.Nintendo.Loaders;
using System;
using System.Threading.Tasks;

namespace MICE.Nintendo
{
    public class NES : ISystem
    {
        public string Name { get; } = "Nintendo Entertainment System";

        // Create components...
        public DataBus DataBus { get; } = new DataBus();
        public AddressBus AddressBus { get; } = new AddressBus();
        public ControlBus ControlBus { get; } = new ControlBus();
        public NESMemoryMap MemoryMap { get; } = new NESMemoryMap();

        public NESCartridge Cartridge { get; private set; }

        public Ricoh2A03 CPU { get; private set; }

        // Hook them up...

        public void PowerOn()
        {
            if (this.Cartridge == null)
            {
                throw new InvalidOperationException("Cartridge must be loaded first, unable to power on.");
            }

            this.CPU = new Ricoh2A03(this.MemoryMap);

            this.CPU.PowerOn();
        }

        private void SetupOpcodes()
        {
            throw new NotImplementedException();
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
            this.Cartridge = cartridge;

            // Various parts of a cartridge are mapped into the NES's memory map.
            this.MemoryMap.GetMemorySegment<SRAM>("SRAM").Data = cartridge.SRAM;

            this.MemoryMap.GetMemorySegment<External>("PRG-ROM Lower Bank").Handler = cartridge.Mapper;
            this.MemoryMap.GetMemorySegment<External>("PRG-ROM Upper Bank").Handler = cartridge.Mapper;
        }
    }
}

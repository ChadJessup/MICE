using System.Diagnostics;

namespace MICE.PPU.RicohRP2C02
{
        // This is heavily based upon: https://wiki.nesdev.com/w/index.php/PPU_scrolling
    public class PPUInternalRegisters
    {
        public RicohRP2C02 tempPPU { get; set; }

        /// <summary>
        /// Contains the PPU Address that writes at $2007 will be written to.
        /// Note: Sometimes called 'v'.
        /// </summary>
        private ushort _v = 0;
        public ushort v
        {
            get => this._v;
            set
            {
                this._v = value;
             //   Debug.WriteLine($"SL: {tempPPU.ScanLine} C: {tempPPU.Cycle} : 0x{this._v:X4}");
            }
        }

        /// <summary>
        /// Partial PPU Address, as two writes are needed in order to complete the address.
        /// Note: Sometimes called 't'.
        /// </summary>
        public ushort t { get; set; }

        /// <summary>
        /// Fine X scroll.
        /// Note: Sometimes called 'x'.
        /// </summary>
        public byte x { get; set; }

        /// <summary>
        /// First or second write toggle.
        /// Note: Sometimes called 'w'.
        /// </summary>
        public bool w { get; set; }
    }
}

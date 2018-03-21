namespace MICE.PPU.RicohRP2C02
{
        // This is heavily based upon: https://wiki.nesdev.com/w/index.php/PPU_scrolling
    public class PPUInternalRegisters
    {
        /// <summary>
        /// Contains the PPU Address that writes at $2007 will be written to.
        /// Note: Sometimes called 'v'.
        /// </summary>
        public ushort v { get; set; }

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

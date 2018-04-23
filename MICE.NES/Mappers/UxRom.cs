using MICE.Common.Misc;
using MICE.Nintendo.Loaders;

namespace MICE.Nintendo.Mappers
{
    [Mapper(MemoryMapperIds.UNROM)]
    public class UNROM : NROM
    {
        private Range BankSelectRange = new Range(0x8000, 0xFFFF);

        public UNROM(NESCartridge cartridge, MemoryMapperIds memoryMapperIds)
            : base(cartridge, MemoryMapperIds.UNROM)
        {
        }

        public override void Write(int index, byte value)
        {
            if (this.BankSelectRange.IsInRange(index))
            {
                var newBank = value & 0b00000111;

                this.programROMFirstBank = this.cartridge.ProgramROMBanks[newBank];
                return;
            }

            base.Write(index, value);
        }
    }
}
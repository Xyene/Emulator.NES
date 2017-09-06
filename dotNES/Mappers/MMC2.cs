using System.Runtime.CompilerServices;

namespace dotNES.Mappers
{
    class MMC2 : MMC4
    {
        public MMC2(Emulator emulator) : base(emulator)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint ReadByte(uint addr)
        {
            if (addr < 0x8000) return _prgRAM[addr - 0x6000];
            if (addr < 0xA000) return _prgROM[_prgBankOffset + (addr - 0x8000)];
            return _prgROM[_lastBankOffset - 0x2000 + (addr - 0xA000)];
        }

        protected override void GetLatch(uint addr, out uint latch, out bool? on)
        {
            base.GetLatch(addr, out latch, out on);

            // For MMC2, only 0xFD8 and 0xFE8 trigger the latch,
            // not the whole range like in MMC4
            if (latch == 0 && (addr & 0x3) != 0)
                on = null;
        }

        public override void WriteByte(uint addr, uint _val)
        {
            if (addr < 0x6000) return;

            byte val = (byte)_val;

            if (addr < 0x8000)
                _prgRAM[addr - 0x6000] = val;

            if (addr < 0xA000) return;

            if (addr <= 0xAFFF)
                _prgBankOffset = (val & 0xF) * 0x2000;
            else if (addr < 0xF000)
            {
                var bank = (addr - 0xB000) / 0x2000;
                var latch = ((addr & 0x1FFF) == 0).AsByte();
                _chrBankOffsets[bank, latch] = (val & 0x1F) * 0x1000;
            }
            else
                _emulator.Cartridge.MirroringMode = _mirroringModes[val & 0x1];
        }
    }
}

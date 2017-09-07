using System;
using System.Runtime.CompilerServices;
using static dotNES.Cartridge.VRAMMirroringMode;

namespace dotNES.Mappers
{
    [MapperDef(10)]
    class MMC4 : BaseMapper
    {
        protected readonly Cartridge.VRAMMirroringMode[] _mirroringModes = { Vertical, Horizontal };

        protected int _prgBankOffset;
        protected int[,] _chrBankOffsets = new int[2, 2];
        protected bool[] _latches = new bool[2];

        public MMC4(Emulator emulator) : base(emulator)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint ReadByte(uint addr)
        {
            if (addr < 0x8000) return _prgRAM[addr - 0x6000];
            if (addr < 0xC000) return _prgROM[_prgBankOffset + (addr - 0x8000)];
            return _prgROM[_lastBankOffset + (addr - 0xC000)];
        }

        protected virtual void GetLatch(uint addr, out uint latch, out bool? on)
        {
            latch = (addr >> 12) & 0x1;
            on = null;

            addr = (addr >> 4) & 0xFF;

            if (addr == 0xFE)
                on = true;
            else if (addr == 0xFD)
                on = false;
        }

        public override uint ReadBytePPU(uint addr)
        {
            uint ret;
            if (addr < 0x2000)
            {
                var bank = addr / 0x1000;
                ret = _chrROM[_chrBankOffsets[bank, _latches[bank].AsByte()] + addr % 0x1000];
            }
            else throw new NotImplementedException();

            if ((addr & 0x08) > 0)
            {
                uint latch;
                bool? on;

                GetLatch(addr, out latch, out on);

                if (on != null) _latches[latch] = (bool)on;
            }

            return ret;
        }

        public override void WriteByte(uint addr, uint _val)
        {
            // Fire Emblem Gaiden writes to $4000 range
            if (addr < 0x6000) return;

            byte val = (byte)_val;

            if (addr < 0x8000)
                _prgRAM[addr - 0x6000] = val;

            if (addr < 0xA000) return;

            if (addr <= 0xAFFF)
                _prgBankOffset = (val & 0xF) * 0x4000;
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

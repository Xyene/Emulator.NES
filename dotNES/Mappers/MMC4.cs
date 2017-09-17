using System;
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

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x6000, 0x7FFF, addr => _prgRAM[addr - 0x6000]);
            cpu.MapReadHandler(0x8000, 0xBFFF, addr => _prgROM[_prgBankOffset + (addr - 0x8000)]);
            cpu.MapReadHandler(0xC000, 0xFFFF, addr => _prgROM[_prgROM.Length - 0x4000 + (addr - 0xC000)]);

            cpu.MapWriteHandler(0x6000, 0x7FFF, (addr, val) => _prgRAM[addr - 0x6000] = val);
            cpu.MapWriteHandler(0xA000, 0xAFFF, (addr, val) => _prgBankOffset = (val & 0xF) * 0x4000);
            cpu.MapWriteHandler(0xB000, 0xEFFF, (addr, val) =>
            {
                var bank = (addr - 0xB000) / 0x2000;
                var latch = ((addr & 0x1FFF) == 0).AsByte();
                _chrBankOffsets[bank, latch] = (val & 0x1F) * 0x1000;
            });
            cpu.MapWriteHandler(0xF000, 0xFFFF, (addr, val) => _emulator.Cartridge.MirroringMode = _mirroringModes[val & 0x1]);
        }

        public override void InitializeMemoryMap(PPU ppu)
        {
            ppu.MapReadHandler(0x0000, 0x1FFF, addr =>
            {
                var bank = addr / 0x1000;
                var ret = _chrROM[_chrBankOffsets[bank, _latches[bank].AsByte()] + addr % 0x1000];
                if ((addr & 0x08) > 0)
                {
                    GetLatch(addr, out uint latch, out bool? on);

                    if (on != null) _latches[latch] = (bool)on;
                }
                return ret;
            });
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
    }
}

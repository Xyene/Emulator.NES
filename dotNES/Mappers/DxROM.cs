namespace dotNES.Mappers
{
    [MapperDef(206)]
    class DxROM : MMC3
    {
        public DxROM(Emulator emulator) : base(emulator)
        {
            _prgBankingMode = PRGBankingMode.SwitchFix;
            _chrBankingMode = CHRBankingMode.TwoFour;
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x8000, 0xFFFF, addr => _prgROM[_prgBankOffsets[(addr - 0x8000) / 0x2000] + addr % 0x2000]);

            cpu.MapWriteHandler(0x8000, 0x9FFF, (addr, val) =>
            {
                if ((addr & 0x1) == 0)
                {
                    _currentBank = val & 0x7u;
                }
                else
                {
                    if (_currentBank <= 1) val &= 0x1F;
                    else if (_currentBank <= 5) val &= 0x3F;
                    else val &= 0xF;
                  
                    _banks[_currentBank] = val;
                    UpdateOffsets();
                }
            });
        }

        public override void InitializeMemoryMap(PPU ppu)
        {
            ppu.MapReadHandler(0x0000, 0x1FFF, addr => _chrROM[_chrBankOffsets[addr / 0x400] + addr % 0x400]);
        }
    }
}

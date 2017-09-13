namespace dotNES.Mappers
{
    [MapperDef(2)]
    class UxROM : BaseMapper
    {
        protected int _bankOffset;

        public UxROM(Emulator emulator) : base(emulator)
        {
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x6000, 0x7FFF, addr => _prgRAM[addr - 0x6000]);
            cpu.MapReadHandler(0x8000, 0xBFFF, addr => _prgROM[_bankOffset + (addr - 0x8000)]);
            cpu.MapReadHandler(0xC000, 0xFFFF, addr => _prgROM[_prgROM.Length - 0x4000 + (addr - 0xC000)]);

            cpu.MapWriteHandler(0x6000, 0x7FFF, (addr, val) => _prgRAM[addr - 0x6000] = val);
            cpu.MapWriteHandler(0x8000, 0xFFFF, (addr, val) => _bankOffset = (val & 0xF) * 0x4000);
        }
    }
}

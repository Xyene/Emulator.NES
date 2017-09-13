namespace dotNES.Mappers
{
    [MapperDef(3)]
    class CNROM : BaseMapper
    {
        protected int _bankOffset;

        public CNROM(Emulator emulator) : base(emulator)
        {
        }

        public override void InitializeMemoryMap(PPU ppu)
        {
           ppu.MapReadHandler(0x0000, 0x1FFF, addr => _chrROM[_bankOffset + addr]);
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x8000, 0xFFFF, addr => _prgROM[addr - 0x8000]);
            cpu.MapWriteHandler(0x8000, 0xFFFF, (addr, val) => _bankOffset = (val & 0x3) * 0x2000);
        }
    }
}

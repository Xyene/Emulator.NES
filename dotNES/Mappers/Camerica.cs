namespace dotNES.Mappers
{
    [MapperDef(71)]
    class Camerica : BaseMapper
    {
        protected int _prgBankOffset;

        public Camerica(Emulator emulator) : base(emulator)
        {
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x8000, 0xBFFF, addr => _prgROM[_prgBankOffset + (addr - 0x8000)]);
            cpu.MapReadHandler(0xC000, 0xFFFF, addr => _prgROM[_prgROM.Length - 0x4000 + (addr - 0xC000)]);

            // Actually starts at 0x8000, but use 0x9000 for compatibility w/o submapper
            cpu.MapWriteHandler(0x9000, 0x9FFF, (addr, val) =>
            {
                // TODO: Fire Hawk mirroring
            });
            // The number of bits available vary: 4 for the BF9093, 3 for the BF9097, and 2 for the BF9096. 
            cpu.MapWriteHandler(0xC000, 0xFFFF, (addr, val) => _prgBankOffset = (val & 0xF) * 0x4000 % _prgROM.Length);
        }
    }
}

namespace dotNES.Mappers
{
    [MapperDef(0)]
    class NROM : BaseMapper
    {
        private readonly byte[] _addressSpace = new byte[0x2000 + 0x8000]; // Space for $2000 VRAM + $8000 PRG

        public NROM(Emulator emulator) : base(emulator)
        {
            for (int i = 0; i < 0x8000; i++)
            {
                int offset = _emulator.Cartridge.PRGROMSize == 0x4000 ? i & 0xBFFF : i;
                _addressSpace[0x2000 + i] = _prgROM[offset];
            }
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x6000, 0xFFFF, addr => _addressSpace[addr - 0x6000]);
            cpu.MapWriteHandler(0x6000, 0x7FFF, (addr, val) => _addressSpace[addr - 0x6000] = val);
        }
    }
}

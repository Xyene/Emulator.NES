using static dotNES.Cartridge.VRAMMirroringMode;

namespace dotNES.Mappers
{
    [MapperDef(7)]
    class AxROM : BaseMapper
    {
        protected int _bankOffset;
        private readonly Cartridge.VRAMMirroringMode[] _mirroringModes = { Lower, Upper };

        public AxROM(Emulator emulator) : base(emulator)
        {
            _emulator.Cartridge.MirroringMode = _mirroringModes[0];
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x8000, 0xFFFF, addr => _prgROM[_bankOffset + (addr - 0x8000)]);           
            cpu.MapWriteHandler(0x8000, 0xFFFF, (addr, val) =>
            {
                _bankOffset = (val & 0x7) * 0x8000;
                _emulator.Cartridge.MirroringMode = _mirroringModes[(val >> 4) & 0x1];
            });
        }
    }
}

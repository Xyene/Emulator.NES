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

        public override uint ReadByte(uint addr) => _addressSpace[(addr & 0xFFFF) - 0x6000];

        public override void WriteByte(uint addr, uint _val)
        {
            byte val = (byte) _val;
            switch (addr & 0xF000)
            {
                case 0x6000:
                case 0x7000:
                    _addressSpace[addr - 0x6000] = val;
                    break;
            }
        }
    }
}

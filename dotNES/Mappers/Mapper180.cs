using System.Runtime.CompilerServices;

namespace dotNES.Mappers
{
    // Mapper used strictly for Crazy Climber; logic is slighly different:
    // $8000-$C000 is fixed to *first* bank
    // $C000-$FFFF is switchable, controlled the same as UxROM
    [MapperDef(Id = 180, Description = "Crazy Climber")]
    class Mapper180 : UxROM
    {
        public Mapper180(Emulator emulator) : base(emulator)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint ReadByte(uint addr)
        {
            if (addr < 0x8000) return _prgRAM[addr - 0x6000];
            if (addr < 0xC000) return _prgROM[addr - 0x8000];
            return _prgROM[_bankOffset + (addr - 0xC000)];
        }
    }
}

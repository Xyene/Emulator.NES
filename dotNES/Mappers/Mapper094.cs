using System;

namespace dotNES.Mappers
{
    class Mapper094 : UxROM
    {
        public Mapper094(Emulator emulator) : base(emulator)
        {
        }

        public override void WriteByte(uint addr, uint _val)
        {
            byte val = (byte) _val;

            if (addr < 0x8000)
                _prgRAM[addr - 0x6000] = val;
            else if (addr >= 0x8000)
                _bankOffset = (val & 0x1C) << 12;
            else
                throw new NotImplementedException(addr.ToString("X4") + " = " + val.ToString("X2"));
        }
    }
}

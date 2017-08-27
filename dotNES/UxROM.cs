using System;
using System.Runtime.CompilerServices;

namespace dotNES
{
    class UxROM : Memory
    {
        protected readonly byte[] _RAM = new byte[0x2000];
        protected readonly byte[] _PRGROM;
        protected int _bankOffset;
        protected readonly int _lastBankOffset;

        public UxROM(Emulator emulator) : base(emulator)
        {
            var cart = emulator.Cartridge;
            _PRGROM = cart.PRGROM;
            _lastBankOffset = cart.PRGROM.Length - 0x4000;
            _bankOffset = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte ReadByte(uint addr)
        {
            if (addr < 0x8000) return _RAM[addr - 0x6000];
            if (addr < 0xC000) return _PRGROM[_bankOffset + (addr - 0x8000)];
            return _PRGROM[_lastBankOffset + (addr - 0xC000)];
        }

        public override void WriteByte(uint addr, uint _val)
        {
            byte val = (byte) _val;

            if ((addr & 0x6000) == 0x6000)
                _RAM[addr - 0x6000] = val;
            else if (addr >= 0x8000)
                _bankOffset = (val & 0xF) * 0x4000;
            else
                throw new NotImplementedException(addr.ToString("X4") + " = " + val.ToString("X2"));
        }
    }
}

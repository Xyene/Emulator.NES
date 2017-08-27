using System;
using System.Runtime.CompilerServices;

namespace dotNES
{
    class UxROM : Memory
    {
        private readonly byte[] RAM = new byte[0x2000];
        private readonly byte[] PRGROM;
        private int bankOffset;
        private int lastBankOffset;

        public UxROM(Emulator emulator) : base(emulator)
        {
            var cart = emulator.Cartridge;
            PRGROM = cart.PRGROM;
            lastBankOffset = cart.PRGROM.Length - 0x4000;
            bankOffset = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte ReadByte(uint addr)
        {
            if (addr < 0x8000) return RAM[addr - 0x6000];
            if (addr < 0xC000) return PRGROM[bankOffset + (addr - 0x8000)];
            return PRGROM[lastBankOffset + (addr - 0xC000)];
        }

        public override void WriteByte(uint addr, uint _val)
        {
            byte val = (byte) _val;
            switch (addr & 0xF000)
            {
                case 0x6000:
                case 0x7000:
                    RAM[addr - 0x6000] = val;
                    break;
                default:
                    if (addr >= 0x8000)
                    {
                        bankOffset = (val & 0xF) * 16384;
                        break;
                    }
                    throw new NotImplementedException(addr.ToString("X4") + " = " + val.ToString("X2"));
            }
        }
    }
}

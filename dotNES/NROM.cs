using System;

namespace dotNES
{
    class NROM : Memory
    {
        private readonly byte[] RAM = new byte[8192];

        public NROM(Emulator emulator) : base(emulator)
        {
        }

        public override byte ReadByte(int addr)
        {
            addr &= 0xFFFF;
            switch (addr & 0xF000)
            {
                case 0x6000:
                case 0x7000:
                    return RAM[addr - 0x6000];
                case 0x8000:
                case 0x9000:
                case 0xA000:
                case 0xB000:
                case 0xC000:
                case 0xD000:
                case 0xE000:
                case 0xF000:
                    int offset = Emulator.Cartridge.PRGROMSize == 16384 ? (addr & 0xBFFF) : addr;
                    return Emulator.Cartridge.PRGROM[offset - 0x8000];
                default:
                    throw new NotImplementedException(addr.ToString("X4"));
            }
        }

        public override void WriteByte(int addr, int _val)
        {
            byte val = (byte) _val;
            switch (addr & 0xF000)
            {
                case 0x6000:
                case 0x7000:
                    RAM[addr - 0x6000] = val;
                    break;
                default:
                    throw new NotImplementedException(addr.ToString("X4") + " = " + val.ToString("X2"));
            }
        }
    }
}

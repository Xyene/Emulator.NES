using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    class NROM : Memory
    {
        private byte[] ram = new byte[8192];

        public NROM(Emulator emulator) : base(emulator)
        {
        }

        public override byte ReadAddress(int addr)
        {
            addr &= 0xFFFF;
            switch (addr & 0xF000)
            {
                case 0x6000:
                case 0x7000:
                    return ram[addr - 0x6000];
                case 0x8000:
                case 0x9000:
                case 0xA000:
                case 0xB000:
                case 0xC000:
                case 0xD000:
                case 0xE000:
                case 0xF000:
                    return emulator.Cartridge.raw[emulator.Cartridge.PRGROMOffset + ((addr & 0xBFFF) - 0x8000)];
                default:
                    throw new NotImplementedException(addr.ToString("X4"));
            }
        }

        public override void WriteAddress(int addr, int _val)
        {
            byte val = (byte) _val;
            switch (addr & 0xF000)
            {
                case 0x6000:
                case 0x7000:
                    ram[addr - 0x6000] = val;
                    break;
                default:
                    throw new NotImplementedException(addr.ToString("X4") + " = " + val.ToString("X2"));
            }
        }
    }
}

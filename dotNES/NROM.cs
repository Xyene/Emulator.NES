using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    class NROM : Memory
    {
        public NROM(Emulator emulator) : base(emulator)
        {
        }

        public override byte ReadAddress(ushort addr)
        {
            switch (addr & 0xF000)
            {
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

        public override void WriteAddress(ushort addr, byte val)
        {
            throw new NotImplementedException(addr.ToString("X4") + " = " + val.ToString("X2"));
        }
    }
}

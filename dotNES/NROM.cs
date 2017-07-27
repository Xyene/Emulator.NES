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
                    return emulator.Cartridge.raw[emulator.Cartridge.PRGROMOffset + (addr - 0x8000)];
                case 0x6000:
                case 0x7000:
                default:
                    throw new NotImplementedException();
            }
        }

        public override void WriteAddress(ushort addr, byte val)
        {
            throw new NotImplementedException();
        }
    }
}

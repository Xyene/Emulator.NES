using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    class PPU
    {
        private Emulator emulator;

        public PPU(Emulator emulator)
        {
            this.emulator = emulator;
        }

        public void WriteRegister(int reg)
        {
            throw new NotImplementedException();
        }

        public byte ReadRegister(int reg)
        {
            throw new NotImplementedException();
        }
    }
}

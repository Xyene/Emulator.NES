using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    class Memory : IAddressable
    {
        private Emulator emulator;

        public Memory(Emulator emulator)
        {
            this.emulator = emulator;
        }

        public byte ReadAddress(ushort addr)
        {
            throw new NotImplementedException();
        }

        public void WriteAddress(ushort addr, byte val)
        {
            throw new NotImplementedException();
        }
    }
}

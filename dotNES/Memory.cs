using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    abstract class Memory : IAddressable
    {
        protected Emulator emulator;

        public Memory(Emulator emulator)
        {
            this.emulator = emulator;
        }

        public abstract byte ReadAddress(ushort addr);

        public abstract void WriteAddress(ushort addr, byte val);
    }
}

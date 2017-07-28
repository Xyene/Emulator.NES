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

        public abstract byte ReadAddress(int addr);

        public abstract void WriteAddress(int addr, byte val);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    class Emulator
    {
        public Emulator()
        {
            this.CPU = new CPU(this);
            this.PPU = new PPU(this);
        }

        public CPU CPU { get; }

        public PPU PPU { get;  }
    }
}

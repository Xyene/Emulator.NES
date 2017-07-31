using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class CPU
    {
        public void WriteIORegister(int reg, byte val)
        {
            if (0x0000 <= reg && reg <= 0x0017) return; // APU write
            throw new NotImplementedException($"{reg.ToString("X4")} = {val.ToString("X2")}");
        }

        public byte ReadIORegister(int reg)
        {
            throw new NotImplementedException();
        }
    }
}

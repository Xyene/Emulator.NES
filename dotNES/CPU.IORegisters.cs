using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class CPU
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteIORegister(uint reg, byte val)
        {
            switch (reg)
            {
                case 0x0014: // OAM DMA
                    PerformDMA(val);
                    break;
            }
            if (0x0000 <= reg && reg <= 0x0017) return; // APU write
            throw new NotImplementedException($"{reg.ToString("X4")} = {val.ToString("X2")}");
        }

        public uint ReadIORegister(uint reg)
        {
            return 0x00;
            //throw new NotImplementedException();
        }
    }
}

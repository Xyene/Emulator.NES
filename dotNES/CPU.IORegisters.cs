using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    sealed partial class CPU
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteIORegister(uint reg, byte val)
        {
            switch (reg)
            {
                case 0x4014: // OAM DMA
                    _emulator.PPU.PerformDMA(val);
                    break;
                case 0x4016:
                    _emulator.Controller.Strobe(val == 1);
                    break;
            }
            if (reg <= 0x401F) return; // APU write
            throw new NotImplementedException($"{reg.ToString("X4")} = {val.ToString("X2")}");
        }

        public uint ReadIORegister(uint reg)
        {
            switch (reg)
            {
                case 0x4016:
                    return (uint) _emulator.Controller.ReadState() & 0x1;
            }
            return 0x00;
            //throw new NotImplementedException();
        }
    }
}

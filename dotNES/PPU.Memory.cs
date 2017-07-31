using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class PPU
    {
        public void WriteRegister(int reg, byte val)
        {
            switch (reg)
            {
                case 0x0000:
                    PPUCTRL = val;
                    return;
            }
            throw new NotImplementedException();
        }

        public byte ReadRegister(int reg)
        {
            switch (reg)
            {
                case 0x0000:
                    return PPUCTRL;
            }
            throw new NotImplementedException();
        }

        public byte ReadByte(int addr)
        {
            addr &= 0xFFFF;
            throw new NotImplementedException();
        }

        public void WriteByte(int addr, int val)
        {
            addr &= 0xFFFF;
            throw new NotImplementedException();
        }
    }
}

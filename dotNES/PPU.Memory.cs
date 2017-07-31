using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class PPU
    {
        private int _lastWrittenRegister;

        public void WriteRegister(int reg, byte val)
        {
            reg &= 0xF;
            _lastWrittenRegister = val & 0xFF;
            switch (reg)
            {
                case 0x0000:
                    PPUCTRL = val;
                    return;
                case 0x0001:
                    PPUMASK = val;
                    return;
                case 0x0002:
                    PPUSTATUS = val;
                    return;
            }

            throw new NotImplementedException($"{reg.ToString("X4")} = {val.ToString("X2")}");
        }

        public byte ReadRegister(int reg)
        {
            reg &= 0xF;
            switch (reg)
            {
                case 0x0000:
                    return (byte) PPUCTRL;
                case 0x0001:
                    return (byte) PPUMASK;
                case 0x0002:
                    return (byte) PPUSTATUS;
            }
            throw new NotImplementedException(reg.ToString("X2"));
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

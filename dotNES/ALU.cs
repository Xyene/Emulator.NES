using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class CPU
    {
        private void ADC(byte val)
        {
            int nA = (sbyte)A + (sbyte)val + (sbyte)(flags.Carry ? 1 : 0);
            flags.Overflow = nA < -128 || nA > 127;
            flags.Carry = (A + val + (flags.Carry ? 1 : 0)) > 0xFF;
            flags.Negative = (nA & 0x80) > 0;
            flags.Zero = (nA & 0xFF) == 0;
            A = (byte)(nA & 0xFF);
        }

        private void CMP(byte reg, byte M)
        {
            int d = reg - M;

            flags.Negative = (d & 0x80) > 0 && d != 0;
            flags.Carry = d >= 0;
            flags.Zero = d == 0;
        }

        private void LSR(int addr)
        {
            byte D = ReadAddress(addr);
            flags.Carry = (D & 0x1) > 0;
            D >>= 1;
            flags.Negative = (D & 0x80) > 0;
            flags.Zero = D == 0;
            WriteAddress(addr, D);
        }

        private void ASL(int addr)
        {
            byte D = ReadAddress(addr);
            flags.Carry = (D & 0x80) > 0;
            D <<= 1;
            flags.Negative = (D & 0x80) > 0;
            flags.Zero = D == 0;
            WriteAddress(addr, D);
        }

        private void ROR(int addr)
        {
            byte D = ReadAddress(addr);
            bool c = flags.Carry;
            flags.Carry = (D & 0x1) > 0;
            D >>= 1;
            if (c) D |= 0x80;
            flags.Negative = c;
            flags.Zero = D == 0;
            WriteAddress(addr, D);
        }


        private void ROL(int addr)
        {
            byte D = ReadAddress(addr);
            bool c = flags.Carry;
            flags.Carry = (D & 0x80) > 0;
            D <<= 1;
            if (c) D |= 0x1;
            flags.Negative = (D & 0x80) > 0;
            flags.Zero = D == 0;
            WriteAddress(addr, D);
        }

        private void INC(int addr)
        {
            byte D = (byte) (ReadAddress(addr) + 1);
            flags.Negative = (D & 0x80) > 0;
            flags.Zero = D == 0;
            WriteAddress(addr, D);
        }

        private void DEC(int addr)
        {
            byte D = (byte)(ReadAddress(addr) - 1);
            flags.Negative = (D & 0x80) > 0;
            flags.Zero = D == 0;
            WriteAddress(addr, D);
        }
    }
}

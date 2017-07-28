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

        private void CMP(byte M)
        {
            int d = A - M;

            flags.Negative = (d & 0x80) > 0 && d != 0;
            flags.Carry = d >= 0;
            flags.Zero = d == 0;
        }
    }
}

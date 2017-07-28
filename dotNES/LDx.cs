using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class CPU
    {
        private void LDA(byte val)
        {
            A = val;
            flags.Zero = A == 0;
            flags.Negative = (A & 0x80) > 0;
        }

        private void LDX(byte val)
        {
            X = val;
            flags.Zero = X == 0;
            flags.Negative = (X & 0x80) > 0;
        }

        private void LDY(byte val)
        {
            Y = val;
            flags.Zero = Y == 0;
            flags.Negative = (Y & 0x80) > 0;
        }
    }
}

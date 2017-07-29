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
            F.Zero = A == 0;
            F.Negative = (A & 0x80) > 0;
        }

        private void LDX(byte val)
        {
            X = val;
            F.Zero = X == 0;
            F.Negative = (X & 0x80) > 0;
        }

        private void LDY(byte val)
        {
            Y = val;
            F.Zero = Y == 0;
            F.Negative = (Y & 0x80) > 0;
        }
    }
}

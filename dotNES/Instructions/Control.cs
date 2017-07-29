using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class CPU
    {
        private void ExecuteControl(int instruction)
        {
            switch (instruction)
            {

            }
        }

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

        private void Push(byte what)
        {
            WriteAddress((ushort)(0x100 + SP), what);
            SP--;
        }

        private byte Pop()
        {
            SP++;
            byte val = ReadByte((ushort)(0x100 + SP));
            return val;
        }

        private void PushWord(int what)
        {
            Push((byte)(what >> 8));
            Push((byte)(what & 0xFF));
        }

        private int PopWord()
        {
            return Pop() | (Pop() << 8);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class CPU
    {
        private void Push(byte what)
        {
            WriteAddress((ushort)(0x100 + SP), what);
            SP--;
        }

        private byte Pop()
        {
            SP++;
            byte val = ReadAddress((ushort)(0x100 + SP));
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

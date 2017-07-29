using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class CPU
    {
        private void Push(int what)
        {
            WriteAddress(0x100 + SP, what);
            SP--;
        }

        private byte Pop()
        {
            SP++;
            return ReadAddress(0x100 + SP);
        }

        private void PushWord(int what)
        {
            Push(what >> 8);
            Push(what & 0xFF);
        }

        private int PopWord()
        {
            return Pop() | (Pop() << 8);
        }
    }
}

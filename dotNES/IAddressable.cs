using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    interface IAddressable
    {
        byte ReadAddress(int addr);

        void WriteAddress(int addr, byte val);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    interface IAddressable
    {
        byte ReadAddress(ushort addr);

        void WriteAddress(ushort addr, byte val);
    }
}

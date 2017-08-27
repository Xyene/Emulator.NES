using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    interface IAddressable
    {
        uint ReadByte(uint addr);

        void WriteByte(uint addr, uint val);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    static class Utility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static byte AsByte(this bool to) => *((byte*)&to);
    }
}

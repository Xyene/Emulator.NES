using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static dotNES.Utility;

namespace dotNES
{
    partial class CPU
    {
        private const int CARRY_BIT = 0x1;
        private const int ZERO_BIT = 0x2;
        private const int INTERRUPT_DISABLED_BIT = 0x4;
        private const int DECIMAL_MODE_BIT = 0x8;
        private const int BREAK_SOURCE_BIT = 0x10;
        private const int OVERFLOW_BIT = 0x40;
        private const int NEGATIVE_BIT = 0x80;

        public class CPUFlags
        {
            public bool Negative;
            public bool Overflow;
            public bool BreakSource;
            public bool DecimalMode;
            public bool InterruptsDisabled;
            public bool Zero;
            public bool Carry;
        }

        public readonly CPUFlags F = new CPUFlags();

        public int _A, _X, _Y, _SP;
        public int PC { get; private set; }

        public int A
        {
            get => _A;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set { _A = _F(value & 0xFF); }
        }

        public int X
        {
            get => _X;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set { _X = _F(value & 0xFF); }
        }

        public int Y
        {
            get => _Y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set { _Y = _F(value & 0xFF); }
        }

        public int SP
        {
            get => _SP;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set { _SP = value & 0xFF; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _F(int val)
        {
            F.Zero = val == 0;
            F.Negative = (val & 0x80) > 0;
            return val;
        }

        public int P
        {
            get => (F.Carry.AsByte() << 0) |
                   (F.Zero.AsByte() << 1) |
                   (F.InterruptsDisabled.AsByte() << 2) |
                   (F.DecimalMode.AsByte() << 3) |
                   (F.BreakSource.AsByte() << 4) |
                   (1 << 5) |
                   (F.Overflow.AsByte() << 6) |
                   (F.Negative.AsByte() << 7);
            set
            {
                F.Carry = (value & CARRY_BIT) > 0;
                F.Zero = (value & ZERO_BIT) > 0;
                F.InterruptsDisabled = (value & INTERRUPT_DISABLED_BIT) > 0;
                F.DecimalMode = (value & DECIMAL_MODE_BIT) > 0;
                F.BreakSource = (value & BREAK_SOURCE_BIT) > 0;
                F.Overflow = (value & OVERFLOW_BIT) > 0;
                F.Negative = (value & NEGATIVE_BIT) > 0;
            }
        }
    }
}

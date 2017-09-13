using System.Runtime.CompilerServices;

namespace dotNES
{
    sealed partial class CPU
    {
        private const int CarryBit = 0x1;
        private const int ZeroBit = 0x2;
        private const int InterruptDisabledBit = 0x4;
        private const int DecimalModeBit = 0x8;
        private const int BreakSourceBit = 0x10;
        private const int OverflowBit = 0x40;
        private const int NegativeBit = 0x80;

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

        public uint _A, _X, _Y, _SP;
        public uint PC;

        public uint A
        {
            get => _A;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _A = _F(value & 0xFF);
        }

        public uint X
        {
            get => _X;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _X = _F(value & 0xFF);
        }

        public uint Y
        {
            get => _Y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _Y = _F(value & 0xFF);
        }

        public uint SP
        {
            get => _SP;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => _SP = value & 0xFF;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint _F(uint val)
        {
            F.Zero = (val & 0xFF) == 0;
            F.Negative = (val & 0x80) > 0;
            return val;
        }

        public uint P
        {
            get => (uint) ((F.Carry.AsByte() << 0) |
                           (F.Zero.AsByte() << 1) |
                           (F.InterruptsDisabled.AsByte() << 2) |
                           (F.DecimalMode.AsByte() << 3) |
                           (F.BreakSource.AsByte() << 4) |
                           (1 << 5) |
                           (F.Overflow.AsByte() << 6) |
                           (F.Negative.AsByte() << 7));
            set
            {
                F.Carry = (value & CarryBit) > 0;
                F.Zero = (value & ZeroBit) > 0;
                F.InterruptsDisabled = (value & InterruptDisabledBit) > 0;
                F.DecimalMode = (value & DecimalModeBit) > 0;
                F.BreakSource = (value & BreakSourceBit) > 0;
                F.Overflow = (value & OverflowBit) > 0;
                F.Negative = (value & NegativeBit) > 0;
            }
        }
    }
}

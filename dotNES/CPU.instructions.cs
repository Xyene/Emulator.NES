namespace dotNES
{
    partial class CPU
    {
        private void BIT(int addr)
        {
                                byte val = ReadByte(addr);
                    F.Overflow = (val & 0x40) > 0;
                    F.Zero = (val & A) == 0;
                    F.Negative = (val & 0x80) > 0;
        }

        private void Branch(bool cond)
        {
            int nPC = PC + NextSByte() + 1;
            if (cond)
                PC = nPC;
        }

        private void SBC(byte val) => ADC((byte)~val);

        private void ADC(byte val)
        {
            int nA = (sbyte)A + (sbyte)val + (sbyte)(F.Carry ? 1 : 0);
            F.Overflow = nA < -128 || nA > 127;
            F.Carry = (A + val + (F.Carry ? 1 : 0)) > 0xFF;
            A = (byte)(nA & 0xFF);
        }

        private void CMP(int reg, byte val)
        {
            int d = reg - val;

            F.Negative = (d & 0x80) > 0 && d != 0;
            F.Carry = d >= 0;
            F.Zero = d == 0;
        }

        private void LSR(int addr)
        {
            byte D = ReadByte(addr);
            F.Carry = (D & 0x1) > 0;
            D >>= 1;
            _F(D);
            WriteByte(addr, D);
        }

        private void ASL(int addr)
        {
            byte D = ReadByte(addr);
            F.Carry = (D & 0x80) > 0;
            D <<= 1;
            _F(D);
            WriteByte(addr, D);
        }

        private void ROR(int addr)
        {
            byte D = ReadByte(addr);
            bool c = F.Carry;
            F.Carry = (D & 0x1) > 0;
            D >>= 1;
            if (c) D |= 0x80;
            _F(D);
            WriteByte(addr, D);
        }

        private void ROL(int addr)
        {
            byte D = ReadByte(addr);
            bool c = F.Carry;
            F.Carry = (D & 0x80) > 0;
            D <<= 1;
            if (c) D |= 0x1;
            _F(D);
            WriteByte(addr, D);
        }

        private void INC(int addr)
        {
            byte D = (byte)(ReadByte(addr) + 1);
            _F(D);
            WriteByte(addr, D);
        }

        private void DEC(int addr)
        {
            byte D = (byte)(ReadByte(addr) - 1);
            _F(D);
            WriteByte(addr, D);
        }
    }
}

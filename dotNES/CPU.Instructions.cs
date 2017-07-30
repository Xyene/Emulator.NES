namespace dotNES
{
    partial class CPU
    {
        private void BIT(Addressor addr)
        {
            int val = addr.Read();
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

        private void LDA(Addressor addr) => A = addr.Read();

        private void LDY(Addressor addr) => Y = addr.Read();

        private void LDX(Addressor addr) => X = addr.Read();

        private void ORA(Addressor addr) => A |= addr.Read();

        private void AND(Addressor addr) => A &= addr.Read();

        private void EOR(Addressor addr) => A ^= addr.Read();

        private void SBC(Addressor addr) => ADCImpl((byte)~addr.Read());

        private void ADC(Addressor addr) => ADCImpl(addr.Read());

        private void ADCImpl(int val)
        {
            int nA = (sbyte)A + (sbyte)val + (sbyte)(F.Carry ? 1 : 0);
            F.Overflow = nA < -128 || nA > 127;
            F.Carry = (A + val + (F.Carry ? 1 : 0)) > 0xFF;
            A = (byte)(nA & 0xFF);
        }

        private void CMP(int reg, Addressor addr)
        {
            int d = reg - addr.Read();

            F.Negative = (d & 0x80) > 0 && d != 0;
            F.Carry = d >= 0;
            F.Zero = d == 0;
        }

        private void LSR(Addressor addr)
        {
            int D = addr.Read();
            F.Carry = (D & 0x1) > 0;
            D >>= 1;
            _F(D);
            addr.Write(D);
        }

        private void ASL(Addressor addr)
        {
            int D = addr.Read();
            F.Carry = (D & 0x80) > 0;
            D <<= 1;
            _F(D);
            addr.Write(D);
        }

        private void ROR(Addressor addr)
        {
            int D = addr.Read();
            bool c = F.Carry;
            F.Carry = (D & 0x1) > 0;
            D >>= 1;
            if (c) D |= 0x80;
            _F(D);
            addr.Write(D);
        }

        private void ROL(Addressor addr)
        {
            int D = addr.Read();
            bool c = F.Carry;
            F.Carry = (D & 0x80) > 0;
            D <<= 1;
            if (c) D |= 0x1;
            _F(D);
            addr.Write(D);
        }

        private void INC(Addressor addr)
        {
            byte D = (byte)(addr.Read() + 1);
            _F(D);
            addr.Write(D);
        }

        private void DEC(Addressor addr)
        {
            byte D = (byte)(addr.Read() - 1);
            _F(D);
            addr.Write(D);
        }
    }
}

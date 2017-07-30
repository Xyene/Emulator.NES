namespace dotNES
{
    partial class CPU
    {
        private void BIT()
        {
            int val = AddressRead();
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

        private void BCS() => Branch(F.Carry);

        private void BCC() => Branch(!F.Carry);

        private void BEQ() => Branch(F.Zero);

        private void BNE() => Branch(!F.Zero);

        private void BVS() => Branch(F.Overflow);

        private void BVC() => Branch(!F.Overflow);

        private void BPL() => Branch(!F.Negative);

        private void BMI() => Branch(F.Negative);

        private void STA() => AddressWrite(A);

        private void STX() => AddressWrite(X);

        private void STY() => AddressWrite(Y);

        private void CLC() => F.Carry = false;

        private void SEC() => F.Carry = true;

        private void CLI() => F.InterruptsDisabled = false;

        private void SEI() => F.InterruptsDisabled = true;

        private void CLV() => F.Overflow = false;

        private void CLD() => F.DecimalMode = false;

        private void SED() => F.DecimalMode = true;

        private void NOP() { }

        private void LDA() => A = AddressRead();

        private void LDY() => Y = AddressRead();

        private void LDX() => X = AddressRead();

        private void ORA() => A |= AddressRead();

        private void AND() => A &= AddressRead();

        private void EOR() => A ^= AddressRead();

        private void SBC() => ADCImpl((byte)~AddressRead());

        private void ADC() => ADCImpl(AddressRead());

        private void ADCImpl(int val)
        {
            int nA = (sbyte)A + (sbyte)val + (sbyte)(F.Carry ? 1 : 0);
            F.Overflow = nA < -128 || nA > 127;
            F.Carry = (A + val + (F.Carry ? 1 : 0)) > 0xFF;
            A = (byte)(nA & 0xFF);
        }

        private void CMP(int reg)
        {
            int d = reg - AddressRead();

            F.Negative = (d & 0x80) > 0 && d != 0;
            F.Carry = d >= 0;
            F.Zero = d == 0;
        }

        private void LSR()
        {
            int D = AddressRead();
            F.Carry = (D & 0x1) > 0;
            D >>= 1;
            _F(D);
            AddressWrite(D);
        }

        private void ASL()
        {
            int D = AddressRead();
            F.Carry = (D & 0x80) > 0;
            D <<= 1;
            _F(D);
            AddressWrite(D);
        }

        private void ROR()
        {
            int D = AddressRead();
            bool c = F.Carry;
            F.Carry = (D & 0x1) > 0;
            D >>= 1;
            if (c) D |= 0x80;
            _F(D);
            AddressWrite(D);
        }

        private void ROL()
        {
            int D = AddressRead();
            bool c = F.Carry;
            F.Carry = (D & 0x80) > 0;
            D <<= 1;
            if (c) D |= 0x1;
            _F(D);
            AddressWrite(D);
        }

        private void INC()
        {
            byte D = (byte)(AddressRead() + 1);
            _F(D);
            AddressWrite(D);
        }

        private void DEC()
        {
            byte D = (byte)(AddressRead() - 1);
            _F(D);
            AddressWrite(D);
        }
    }
}

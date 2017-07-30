using System;
using static dotNES.CPU.AddressingMode;

namespace dotNES
{
    partial class CPU
    {
        [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
        public class OpcodeDef : Attribute
        {
            public int Opcode;
            public AddressingMode Mode = None;
        }

        [OpcodeDef(Opcode = 0x20)]
        private void JSR()
        {
            PushWord(PC + 1);
            PC = NextWord();
        }

        [OpcodeDef(Opcode = 0x40)]
        private void RTI()
        {
            P = Pop();
            PC = PopWord();
        }

        [OpcodeDef(Opcode = 0x60)]
        private void RTS() => PC = PopWord() + 1;

        [OpcodeDef(Opcode = 0xC8)]
        private void INY() => Y++;

        [OpcodeDef(Opcode = 0x88)]
        private void DEY() => Y--;

        [OpcodeDef(Opcode = 0xE8)]
        private void INX() => X++;

        [OpcodeDef(Opcode = 0xCA)]
        private void DEX() => X--;

        [OpcodeDef(Opcode = 0xA8)]
        private void TAY() => Y = A;

        [OpcodeDef(Opcode = 0x98)]
        private void TYA() => A = Y;

        [OpcodeDef(Opcode = 0xAA)]
        private void TAX() => X = A;

        [OpcodeDef(Opcode = 0x8A)]
        private void TXA() => A = X;

        [OpcodeDef(Opcode = 0xBA)]
        private void TSX() => X = SP;

        [OpcodeDef(Opcode = 0x9A)]
        private void TXS() => SP = X;

        [OpcodeDef(Opcode = 0x08)]
        private void PHP() => Push(P | BreakSourceBit);

        [OpcodeDef(Opcode = 0x28)]
        private void PLP() => P = Pop() & ~BreakSourceBit;

        [OpcodeDef(Opcode = 0x68)]
        private void PLA() => A = Pop();

        [OpcodeDef(Opcode = 0x48)]
        private void PHA() => Push(A);

        [OpcodeDef(Opcode = 0x24, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0x2C, Mode = Absolute)]
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

        [OpcodeDef(Opcode = 0x4C)]
        [OpcodeDef(Opcode = 0x6C)]
        private void JMP()
        {
            if (currentInstruction == 0x4C)
                PC = NextWord();
            else if (currentInstruction == 0x6C)
            {
                int off = NextWord();
                // AN INDIRECT JUMP MUST NEVER USE A VECTOR BEGINNING ON THE LAST BYTE OF A PAGE
                //
                // If address $3000 contains $40, $30FF contains $80, and $3100 contains $50, 
                // the result of JMP ($30FF) will be a transfer of control to $4080 rather than
                // $5080 as you intended i.e. the 6502 took the low byte of the address from
                // $30FF and the high byte from $3000.
                //
                // http://www.6502.org/tutorials/6502opcodes.html
                int hi = (off & 0xFF) == 0xFF ? off - 0xFF : off + 1;
                PC = ReadByte(off) | (ReadByte(hi) << 8);
            }
            else throw new NotImplementedException();
        }

        [OpcodeDef(Opcode = 0xB0)]
        private void BCS() => Branch(F.Carry);

        [OpcodeDef(Opcode = 0x90)]
        private void BCC() => Branch(!F.Carry);

        [OpcodeDef(Opcode = 0xF0)]
        private void BEQ() => Branch(F.Zero);

        [OpcodeDef(Opcode = 0xD0)]
        private void BNE() => Branch(!F.Zero);

        [OpcodeDef(Opcode = 0x70)]
        private void BVS() => Branch(F.Overflow);

        [OpcodeDef(Opcode = 0x50)]
        private void BVC() => Branch(!F.Overflow);

        [OpcodeDef(Opcode = 0x10)]
        private void BPL() => Branch(!F.Negative);

        [OpcodeDef(Opcode = 0x30)]
        private void BMI() => Branch(F.Negative);

        [OpcodeDef(Opcode = 0x81, Mode = IndirectX)]
        [OpcodeDef(Opcode = 0x91, Mode = IndirectY)]
        [OpcodeDef(Opcode = 0x95, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0x99, Mode = AbsoluteY)]
        [OpcodeDef(Opcode = 0x9D, Mode = AbsoluteX)]
        [OpcodeDef(Opcode = 0x85, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0x8D, Mode = Absolute)]
        private void STA() => AddressWrite(A);

        [OpcodeDef(Opcode = 0x96, Mode = ZeroPageY)]
        [OpcodeDef(Opcode = 0x86, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0x8E, Mode = Absolute)]
        private void STX() => AddressWrite(X);

        [OpcodeDef(Opcode = 0x94, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0x84, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0x8C, Mode = Absolute)]
        private void STY() => AddressWrite(Y);

        [OpcodeDef(Opcode = 0x18)]
        private void CLC() => F.Carry = false;

        [OpcodeDef(Opcode = 0x38)]
        private void SEC() => F.Carry = true;

        [OpcodeDef(Opcode = 0x58)]
        private void CLI() => F.InterruptsDisabled = false;

        [OpcodeDef(Opcode = 0x78)]
        private void SEI() => F.InterruptsDisabled = true;

        [OpcodeDef(Opcode = 0xB8)]
        private void CLV() => F.Overflow = false;

        [OpcodeDef(Opcode = 0xD8)]
        private void CLD() => F.DecimalMode = false;

        [OpcodeDef(Opcode = 0xF8)]
        private void SED() => F.DecimalMode = true;

        [OpcodeDef(Opcode = 0xEA)]
        private void NOP() { }

        [OpcodeDef(Opcode = 0xA1, Mode = IndirectX)]
        [OpcodeDef(Opcode = 0xA5, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0xA9, Mode = Immediate)]
        [OpcodeDef(Opcode = 0xAD, Mode = Absolute)]
        [OpcodeDef(Opcode = 0xB1, Mode = IndirectY)]
        [OpcodeDef(Opcode = 0xB5, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0xB9, Mode = AbsoluteY)]
        [OpcodeDef(Opcode = 0xBD, Mode = AbsoluteX)]
        private void LDA() => A = AddressRead();

        [OpcodeDef(Opcode = 0xA0, Mode = Immediate)]
        [OpcodeDef(Opcode = 0xA4, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0xAC, Mode = Absolute)]
        [OpcodeDef(Opcode = 0xB4, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0xBC, Mode = AbsoluteX)]
        private void LDY() => Y = AddressRead();

        [OpcodeDef(Opcode = 0xA2, Mode = Immediate)]
        [OpcodeDef(Opcode = 0xA6, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0xAE, Mode = Absolute)]
        [OpcodeDef(Opcode = 0xB6, Mode = ZeroPageY)]
        [OpcodeDef(Opcode = 0xBE, Mode = AbsoluteY)]
        private void LDX() => X = AddressRead();

        [OpcodeDef(Opcode = 0x01, Mode = IndirectX)]
        [OpcodeDef(Opcode = 0x05, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0x09, Mode = Immediate)]
        [OpcodeDef(Opcode = 0x0D, Mode = Absolute)]
        [OpcodeDef(Opcode = 0x11, Mode = IndirectY)]
        [OpcodeDef(Opcode = 0x15, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0x19, Mode = AbsoluteY)]
        [OpcodeDef(Opcode = 0x1D, Mode = AbsoluteX)]
        private void ORA() => A |= AddressRead();

        [OpcodeDef(Opcode = 0x21, Mode = IndirectX)]
        [OpcodeDef(Opcode = 0x25, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0x29, Mode = Immediate)]
        [OpcodeDef(Opcode = 0x2D, Mode = Absolute)]
        [OpcodeDef(Opcode = 0x31, Mode = IndirectY)]
        [OpcodeDef(Opcode = 0x35, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0x39, Mode = AbsoluteY)]
        [OpcodeDef(Opcode = 0x3D, Mode = AbsoluteX)]
        private void AND() => A &= AddressRead();

        [OpcodeDef(Opcode = 0x41, Mode = IndirectX)]
        [OpcodeDef(Opcode = 0x45, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0x49, Mode = Immediate)]
        [OpcodeDef(Opcode = 0x4D, Mode = Absolute)]
        [OpcodeDef(Opcode = 0x51, Mode = IndirectY)]
        [OpcodeDef(Opcode = 0x55, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0x59, Mode = AbsoluteY)]
        [OpcodeDef(Opcode = 0x5D, Mode = AbsoluteX)]
        private void EOR() => A ^= AddressRead();

        [OpcodeDef(Opcode = 0xE1, Mode = IndirectX)]
        [OpcodeDef(Opcode = 0xE5, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0xE9, Mode = Immediate)]
        [OpcodeDef(Opcode = 0xED, Mode = Absolute)]
        [OpcodeDef(Opcode = 0xF1, Mode = IndirectY)]
        [OpcodeDef(Opcode = 0xF5, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0xF9, Mode = AbsoluteY)]
        [OpcodeDef(Opcode = 0xFD, Mode = AbsoluteX)]
        private void SBC() => ADCImpl((byte)~AddressRead());

        [OpcodeDef(Opcode = 0x61, Mode = IndirectX)]
        [OpcodeDef(Opcode = 0x65, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0x69, Mode = Immediate)]
        [OpcodeDef(Opcode = 0x6D, Mode = Absolute)]
        [OpcodeDef(Opcode = 0x71, Mode = IndirectY)]
        [OpcodeDef(Opcode = 0x75, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0x79, Mode = AbsoluteY)]
        [OpcodeDef(Opcode = 0x7D, Mode = AbsoluteX)]
        private void ADC() => ADCImpl(AddressRead());

        private void ADCImpl(int val)
        {
            int nA = (sbyte)A + (sbyte)val + (sbyte)(F.Carry ? 1 : 0);
            F.Overflow = nA < -128 || nA > 127;
            F.Carry = (A + val + (F.Carry ? 1 : 0)) > 0xFF;
            A = (byte)(nA & 0xFF);
        }

        [OpcodeDef(Opcode = 0x00)]
        private void BRK() => throw new NotImplementedException();

        [OpcodeDef(Opcode = 0xC1, Mode = IndirectX)]
        [OpcodeDef(Opcode = 0xC5, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0xC9, Mode = Immediate)]
        [OpcodeDef(Opcode = 0xCD, Mode = Absolute)]
        [OpcodeDef(Opcode = 0xD1, Mode = IndirectY)]
        [OpcodeDef(Opcode = 0xD5, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0xD9, Mode = AbsoluteY)]
        [OpcodeDef(Opcode = 0xDD, Mode = AbsoluteX)]
        private void CMP() => CMPImpl(A);

        [OpcodeDef(Opcode = 0xE0, Mode = Immediate)]
        [OpcodeDef(Opcode = 0xE4, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0xEC, Mode = Absolute)]
        private void CPX() => CMPImpl(X);

        [OpcodeDef(Opcode = 0xC0, Mode = Immediate)]
        [OpcodeDef(Opcode = 0xC4, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0xCC, Mode = Absolute)]
        private void CPY() => CMPImpl(Y);

        private void CMPImpl(int reg)
        {
            int d = reg - AddressRead();

            F.Negative = (d & 0x80) > 0 && d != 0;
            F.Carry = d >= 0;
            F.Zero = d == 0;
        }

        [OpcodeDef(Opcode = 0x46, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0x4E, Mode = Absolute)]
        [OpcodeDef(Opcode = 0x56, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0x5E, Mode = AbsoluteX)]
        [OpcodeDef(Opcode = 0x4A, Mode = Direct)]
        private void LSR()
        {
            int D = AddressRead();
            F.Carry = (D & 0x1) > 0;
            D >>= 1;
            _F(D);
            AddressWrite(D);
        }

        [OpcodeDef(Opcode = 0x06, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0x0E, Mode = Absolute)]
        [OpcodeDef(Opcode = 0x16, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0x1E, Mode = AbsoluteX)]
        [OpcodeDef(Opcode = 0x0A, Mode = Direct)]
        private void ASL()
        {
            int D = AddressRead();
            F.Carry = (D & 0x80) > 0;
            D <<= 1;
            _F(D);
            AddressWrite(D);
        }

        [OpcodeDef(Opcode = 0x66, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0x6E, Mode = Absolute)]
        [OpcodeDef(Opcode = 0x76, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0x7E, Mode = AbsoluteX)]
        [OpcodeDef(Opcode = 0x6A, Mode = Direct)]
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

        [OpcodeDef(Opcode = 0x26, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0x2E, Mode = Absolute)]
        [OpcodeDef(Opcode = 0x36, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0x3E, Mode = AbsoluteX)]
        [OpcodeDef(Opcode = 0x2A, Mode = Direct)]
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

        [OpcodeDef(Opcode = 0xE6, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0xEE, Mode = Absolute)]
        [OpcodeDef(Opcode = 0xF6, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0xFE, Mode = AbsoluteX)]
        private void INC()
        {
            byte D = (byte)(AddressRead() + 1);
            _F(D);
            AddressWrite(D);
        }

        [OpcodeDef(Opcode = 0xC6, Mode = ZeroPage)]
        [OpcodeDef(Opcode = 0xCE, Mode = Absolute)]
        [OpcodeDef(Opcode = 0xD6, Mode = ZeroPageX)]
        [OpcodeDef(Opcode = 0xDE, Mode = AbsoluteX)]
        private void DEC()
        {
            byte D = (byte)(AddressRead() - 1);
            _F(D);
            AddressWrite(D);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class CPU : IAddressable
    {
        public class CPUFlags
        {
            public bool Negative;
            public bool Overflow;
            public bool IRQ;
            public bool DecimalMode;
            public bool InterruptsDisabled;
            public bool Zero;
            public bool Carry;
        }

        private Emulator emulator;
        private byte[] ram = new byte[0x800];
        public byte A { get; private set; }
        public byte X { get; private set; }
        public byte Y { get; private set; }
        public byte SP { get; private set; }
        public int PC { get; private set; }
        public long cycle { get; private set; }

        public readonly CPUFlags flags = new CPUFlags();

        /**
         * 7  bit  0
         * ---- ----
         * NVss DIZC
         * |||| ||||
         * |||| |||+- Carry: 1 if last addition or shift resulted in a carry, or if
         * |||| |||     last subtraction resulted in no borrow
         * |||| ||+-- Zero: 1 if last operation resulted in a 0 value
         * |||| |+--- Interrupt: Interrupt inhibit
         * |||| |       (0: /IRQ and /NMI get through; 1: only /NMI gets through)
         * |||| +---- Decimal: 1 to make ADC and SBC use binary-coded decimal arithmetic
         * ||||         (ignored on second-source 6502 like that in the NES)
         * ||++------ s: No effect, used by the stack copy, see note below
         * |+-------- Overflow: 1 if last ADC or SBC resulted in signed overflow,
         * |            or D6 from last BIT
         * +--------- Negative: Set to bit 7 of the last operation
         */
        public byte P
        {
            get
            {
                return (byte)((Convert.ToByte(flags.Carry) << 0) |
                                (Convert.ToByte(flags.Zero) << 1) |
                                (Convert.ToByte(flags.InterruptsDisabled) << 2) |
                                (Convert.ToByte(flags.DecimalMode) << 3) |
                                (Convert.ToByte(flags.IRQ) << 4) |
                                (1 << 5) |
                                (Convert.ToByte(flags.Overflow) << 6) |
                                (Convert.ToByte(flags.Negative) << 7));
            }
            set
            {
                flags.Carry = (value & 0x1) > 0;
                flags.Zero = (value & 0x2) > 0;
                flags.InterruptsDisabled = (value & 0x4) > 0;
                flags.DecimalMode = (value & 0x8) > 0;
                flags.IRQ = (value & 0x10) > 0;
                flags.Overflow = (value & 0x40) > 0;
                flags.Negative = (value & 0x80) > 0;
            }
        }

        public CPU(Emulator emulator)
        {
            this.emulator = emulator;
            Initialize();
            TextWriterTraceListener writer = new TextWriterTraceListener(System.Console.Out);
            Debug.Listeners.Add(writer);
        }

        public void Initialize()
        {
            A = 0;
            X = 0;
            Y = 0;
            SP = 0xFD;
            P = 0x24;

            PC = 0xC000;
        }

        public void Reset()
        {
            SP -= 3;
            flags.IRQ = true;
        }

        private byte NextByte()
        {
            return ReadAddress((ushort)PC++);
        }

        private ushort NextWord()
        {
            return (ushort)((NextByte()) | (NextByte() << 8));
        }

        private sbyte NextSByte() => (sbyte)NextByte();

        private int Immediate() => PC++;

        private void Branch(bool cond)
        {
            int nPC = PC + NextSByte() + 1;
            if (cond)
                PC = nPC;
        }

        public void _Execute()
        {
            int instruction = NextByte();
            if (cycle >= 4900)
                Console.WriteLine($"{(PC - 1).ToString("X4")}  {instruction.ToString("X2")}	\t\t\tA:{A.ToString("X2")} X:{X.ToString("X2")} Y:{Y.ToString("X2")} P:{P.ToString("X2")} SP:{SP.ToString("X2")}");

            switch (instruction)
            {
                case 0x4C: // JMP
                    PC = NextByte() | (NextByte() << 8);
                    break;
                case 0xA9: // LDA
                    LDA(NextByte());
                    break;
                case 0xA5: // LDA
                    LDA(ReadAddress(NextByte()));
                    break;
                case 0xAD: // LDA
                    LDA(ReadAddress(NextWord()));
                    break;
                case 0xA0: // LDY
                    LDY(NextByte());
                    break;
                case 0xA4: // LDY
                    LDY(ReadAddress(NextByte()));
                    break;
                case 0xA2: // LDX
                    LDX(NextByte());
                    break;
                case 0xA6: // LDX
                    LDX(ReadAddress(NextByte()));
                    break;
                case 0xAE: // LDX
                    LDX(ReadAddress(NextWord()));
                    break;
                case 0xAC: // LDY
                    LDY(ReadAddress(NextWord()));
                    break;
                case 0x86: // STX
                    WriteAddress(NextByte(), X);
                    break;
                case 0x8E: // STX
                    WriteAddress(NextWord(), X);
                    break;
                case 0x84: // STY
                    WriteAddress(NextByte(), Y);
                    break;
                case 0x8C: // STY
                    WriteAddress(NextWord(), Y);
                    break;
                case 0x85: // STA
                    WriteAddress(NextByte(), A);
                    break;
                case 0x8D: // STA
                    WriteAddress(NextWord(), A);
                    break;
                case 0x20: // JSR
                    int nPC = NextByte() | (NextByte() << 8);
                    PushWord(PC - 1);
                    PC = nPC;
                    break;
                case 0x60: // RTS
                    PC = PopWord() + 1;
                    break;
                case 0xEA: // NOP
                    break;
                case 0x18: // CLC
                    flags.Carry = false;
                    break;
                case 0x38: // SEC
                    flags.Carry = true;
                    break;
                case 0x58: // CLI
                    flags.InterruptsDisabled = false;
                    break;
                case 0x78: // SEI
                    flags.InterruptsDisabled = true;
                    break;
                case 0xB8: // CLV
                    flags.Overflow = false;
                    break;
                case 0xD8: // CLD
                    flags.DecimalMode = false;
                    break;
                case 0xF8: // SED
                    flags.DecimalMode = true;
                    break;
                case 0xB0: // BCS
                    Branch(flags.Carry);
                    break;
                case 0x90: // BCC
                    Branch(!flags.Carry);
                    break;
                case 0xF0: // BEQ
                    Branch(flags.Zero);
                    break;
                case 0xD0: // BNE
                    Branch(!flags.Zero);
                    break;
                case 0x70: // BVS
                    Branch(flags.Overflow);
                    break;
                case 0x50: // BVC
                    Branch(!flags.Overflow);
                    break;
                case 0x10: // BPL
                    Branch(!flags.Negative);
                    break;
                case 0x30: // BMI
                    Branch(flags.Negative);
                    break;
                case 0x24: // BIT
                    // BIT sets the Z flag as though the value in the address tested were ANDed with the accumulator.
                    // The S and V flags are set to match bits 7 and 6 respectively in the value stored at the tested address.
                    byte val = ReadAddress(NextByte());
                    flags.Zero = (val & A) == 0;
                    flags.Negative = (val & 0x80) > 0;
                    flags.Overflow = (val & 0x40) > 0;
                    break;
                case 0x2C: // BIT
                    val = ReadAddress(NextWord());
                    flags.Zero = (val & A) == 0;
                    flags.Negative = (val & 0x80) > 0;
                    flags.Overflow = (val & 0x40) > 0;
                    break;
                case 0x08: // PHP
                    bool irq = flags.IRQ;
                    flags.IRQ = true;
                    Push(P);
                    flags.IRQ = irq;
                    break;
                case 0x48: // PHA
                    Push(A);
                    break;
                case 0x28: // PLP
                    P = Pop();
                    flags.IRQ = false;
                    break;
                case 0x68: // PLA
                    A = Pop();
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x29: // AND
                    A = (byte)(A & NextByte());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x09: // OR
                    A = (byte)(A | NextByte());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x49: // EOR
                    A = (byte)(A ^ NextByte());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x69: // ADC
                    ADC(NextByte());
                    break;
                case 0xE9: // SBC
                    SBC(NextByte());
                    break;
                case 0xC9: // CMP
                    CMP(A, NextByte());
                    break;
                case 0xC0: // CPY
                    CMP(Y, NextByte());
                    break;
                case 0xE0: // CPX
                    CMP(X, NextByte());
                    break;
                case 0xC8: // INY
                    Y++;
                    flags.Zero = Y == 0;
                    flags.Negative = (Y & 0x80) > 0;
                    break;
                case 0x88: // DEY
                    Y--;
                    flags.Zero = Y == 0;
                    flags.Negative = (Y & 0x80) > 0;
                    break;
                case 0xE8: // INX
                    X++;
                    flags.Zero = X == 0;
                    flags.Negative = (X & 0x80) > 0;
                    break;
                case 0xCA: // DEX
                    X--;
                    flags.Zero = X == 0;
                    flags.Negative = (X & 0x80) > 0;
                    break;
                case 0xA8: // TAY
                    Y = A;
                    flags.Zero = Y == 0;
                    flags.Negative = (Y & 0x80) > 0;
                    break;
                case 0x98: // TYA
                    A = Y;
                    flags.Zero = A == 0;
                    flags.Negative = (A & 0x80) > 0;
                    break;
                case 0xAA: // TAX
                    X = A;
                    flags.Zero = X == 0;
                    flags.Negative = (X & 0x80) > 0;
                    break;
                case 0x8A: // TXA
                    A = X;
                    flags.Zero = A == 0;
                    flags.Negative = (A & 0x80) > 0;
                    break;
                case 0xBA: // TSX
                    X = SP;
                    flags.Zero = X == 0;
                    flags.Negative = (X & 0x80) > 0;
                    break;
                case 0x9A: // TXS
                    SP = X;
                    break;
                case 0x40: // RTI
                    P = Pop();
                    PC = PopWord();
                    break;
                case 0x4A: // LSR
                    flags.Carry = (A & 0x1) > 0;
                    A >>= 1;
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x0A: // ASL
                    flags.Carry = (A & 0x80) > 0;
                    A <<= 1;
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x6A: // ROR
                    bool c = flags.Carry;
                    flags.Carry = (A & 0x1) > 0;
                    A >>= 1;
                    if (c) A |= 0x80;
                    flags.Negative = c;
                    flags.Zero = A == 0;
                    break;
                case 0x2A: // ROL
                    c = flags.Carry;
                    flags.Carry = (A & 0x80) > 0;
                    A <<= 1;
                    if (c) A |= 0x1;
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0xA1: // LDA ind
                    int ind = IndirectX();
                    byte valx = ReadAddress(ind);
                    LDA(valx);
                    break;
                case 0x81: // STA ind
                    WriteAddress(IndirectX(), A);
                    break;
                case 0x01: // ORA ind
                    A |= ReadAddress(IndirectX());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x21: // AND ind
                    A &= ReadAddress(IndirectX());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x41: // EOR ind
                    A ^= ReadAddress(IndirectX());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x61: // ADC ind
                    ADC(ReadAddress(IndirectX()));
                    break;
                case 0xC1: // CMP ind
                    CMP(A, ReadAddress(IndirectX()));
                    break;
                case 0xE1: // SBC ind
                    SBC(ReadAddress(IndirectX()));
                    break;
                case 0x05: // ORA
                    A |= ReadAddress(NextByte());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x25: // AND
                    A &= ReadAddress(NextByte());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x45: // EOR
                    A ^= ReadAddress(NextByte());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x65: // ADC
                    ADC(ReadAddress(NextByte()));
                    break;
                case 0xE5: // SBC
                    SBC(ReadAddress(NextByte()));
                    break;
                case 0xC5: // CMP
                    CMP(A, ReadAddress(NextByte()));
                    break;
                case 0xE4: // CPX
                    CMP(X, ReadAddress(NextByte()));
                    break;
                case 0xC4: // CPY
                    CMP(Y, ReadAddress(NextByte()));
                    break;
                case 0x46: // LSR
                    LSR(NextByte());
                    break;
                case 0x06: // ASL
                    ASL(NextByte());
                    break;
                case 0x66: // ROR
                    ROR(NextByte());
                    break;
                case 0x26: // ROL
                    ROL(NextByte());
                    break;
                case 0xE6: // INC
                    INC(NextByte());
                    break;
                case 0xC6: // DEC
                    DEC(NextByte());
                    break;
                case 0x0D: // ORA
                    A |= ReadAddress(NextWord());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x2D: // AND
                    A &= ReadAddress(NextWord());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x4D: // EOR
                    A ^= ReadAddress(NextWord());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x6D: // ADC
                    ADC(ReadAddress(NextWord()));
                    break;
                case 0xED: // SBC
                    SBC(ReadAddress(NextWord()));
                    break;
                case 0xCD: // CMP
                    CMP(A, ReadAddress(NextWord()));
                    break;
                case 0xEC: // CPX
                    CMP(X, ReadAddress(NextWord()));
                    break;
                case 0xCC: // CPY
                    CMP(Y, ReadAddress(NextWord()));
                    break;
                case 0x4E: // LSR
                    LSR(NextWord());
                    break;
                case 0x0E: // ASL
                    ASL(NextWord());
                    break;
                case 0x6E: // ROR
                    ROR(NextWord());
                    break;
                case 0x2E: // ROL
                    ROL(NextWord());
                    break;
                case 0xEE: // INC
                    INC(NextWord());
                    break;
                case 0xCE: // DEC
                    DEC(NextWord());
                    break;
                case 0xB1: // LDA
                    LDA(ReadAddress(IndirectY()));
                    break;
                case 0x11: // ORA
                    A |= ReadAddress(IndirectY());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x31: // AND
                    A &= ReadAddress(IndirectY());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x51: // EOR
                    A ^= ReadAddress(IndirectY());
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0xF1: // SBC
                    SBC(ReadAddress(IndirectY()));
                    break;
                case 0x71: // ADC
                    ADC(ReadAddress(IndirectY()));
                    break;
                case 0x91: // STA
                    WriteAddress(IndirectY(), A);
                    break;
                case 0xD1: // CMP
                    CMP(A, ReadAddress(IndirectY()));
                    break;
                case 0x6C: // JMP
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
                    PC = ReadAddress(off) | (ReadAddress(hi) << 8);
                    break;
                case 0xB9: // LDA
                    LDA(ReadAddress(NextWord() + Y));
                    break;
                case 0x19: // ORA
                    A |= ReadAddress(NextWord() + Y);
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x39: // AND
                    A &= ReadAddress(NextWord() + Y);
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x59: // EOR
                    A ^= ReadAddress(NextWord() + Y);
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x79: // ADC
                    ADC(ReadAddress(NextWord() + Y));
                    break;
                case 0xF9: // SBC
                    SBC(ReadAddress(NextWord() + Y));
                    break;
                case 0xD9: // CMP
                    CMP(A, ReadAddress(NextWord() + Y));
                    break;
                case 0x99: // STA
                    WriteAddress(NextWord() + Y, A);
                    break;
                case 0xB4: // LDY
                    Y = ReadAddress((NextByte() + X) & 0xFF);
                    flags.Negative = (Y & 0x80) > 0;
                    flags.Zero = Y == 0;
                    break;
                case 0x94: // STY
                    WriteAddress((NextByte() + X) & 0xFF, Y);
                    break;
                case 0x15: // ORA
                    A |= ReadAddress((NextByte() + X) & 0xFF);
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x35: // AND
                    A &= ReadAddress((NextByte() + X) & 0xFF);
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x55: // EOR
                    A ^= ReadAddress((NextByte() + X) & 0xFF);
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x75: // ADC
                    ADC(ReadAddress((NextByte() + X) & 0xFF));
                    break;
                case 0xF5: // SBC
                    SBC(ReadAddress((NextByte() + X) & 0xFF));
                    break;
                case 0xD5: // CMP
                    CMP(A, ReadAddress((NextByte() + X) & 0xFF));
                    break;
                case 0xB5: // LDA
                    A = ReadAddress((NextByte() + X) & 0xFF);
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x95: // STA
                    WriteAddress((NextByte() + X) & 0xFF, A);
                    break;
                case 0x56: // LSR
                    LSR((NextByte() + X) & 0xFF);
                    break;
                case 0x16: // ASL
                    ASL((NextByte() + X) & 0xFF);
                    break;
                case 0x76: // ROR
                    ROR((NextByte() + X) & 0xFF);
                    break;
                case 0x36: // ROL
                    ROL((NextByte() + X) & 0xFF);
                    break;
                case 0xF6: // INC
                    INC((NextByte() + X) & 0xFF);
                    break;
                case 0xD6: // DEC
                    DEC((NextByte() + X) & 0xFF);
                    break;
                case 0xB6: // LDX
                    X = ReadAddress((NextByte() + Y) & 0xFF);
                    flags.Negative = (X & 0x80) > 0;
                    flags.Zero = X == 0;
                    break;
                case 0x96: // STX
                    WriteAddress((NextByte() + Y) & 0xFF, X);
                    break;
                case 0xBC: // LDY
                    Y = ReadAddress(NextWord() + X);
                    flags.Negative = (Y & 0x80) > 0;
                    flags.Zero = Y == 0;
                    break;
                case 0xBE: // LDX
                    X = ReadAddress(NextWord() + Y);
                    flags.Negative = (X & 0x80) > 0;
                    flags.Zero = X == 0;
                    break;
                case 0x1D: // ORA
                    A |= ReadAddress(NextWord() + X);
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x3D: // AND
                    A &= ReadAddress(NextWord() + X);
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x5D: // EOR
                    A ^= ReadAddress(NextWord() + X);
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x7D: // ADC
                    ADC(ReadAddress(NextWord() + X));
                    break;
                case 0xFD: // SBC
                    SBC(ReadAddress(NextWord() + X));
                    break;
                case 0xDD: // CMP
                    CMP(A, ReadAddress(NextWord() + X));
                    break;
                case 0xBD: // LDA
                    A = ReadAddress(NextWord() + X);
                    flags.Negative = (A & 0x80) > 0;
                    flags.Zero = A == 0;
                    break;
                case 0x9D: // STA
                    WriteAddress(NextWord() + X, A);
                    break;
                case 0x5E: // LSR
                    LSR(NextWord() + X);
                    break;
                case 0x1E: // ASL
                    ASL(NextWord() + X);
                    break;
                case 0x7E: // ROR
                    ROR(NextWord() + X);
                    break;
                case 0x3E: // ROL
                    ROL(NextWord() + X);
                    break;
                case 0xFE: // INC
                    INC(NextWord() + X);
                    break;
                case 0xDE: // DEC
                    DEC(NextWord() + X);
                    break;
                case 0x04: // Sketchy NOPs
                case 0x44:
                case 0x64:
                    ReadAddress(NextByte());
                    break;
                case 0x0C: // ???
                    ReadAddress(NextWord());
                    break;
                case 0x14:
                case 0x34:
                case 0x54:
                case 0x74:
                case 0xD4:
                case 0xF4:
                    NextByte();
                    break;
                case 0x1A:
                case 0x3A:
                case 0x5A:
                case 0x7A:
                case 0xDA:
                case 0xFA:
                    break;
                case 0x80:
                    NextByte();
                    break;
                case 0x1C:
                case 0x3C:
                case 0x5C:
                case 0x7C:
                case 0xDC:
                case 0xFC:
                    NextWord();
                    break;
                /*case 0x00: // BRK
                    NextByte();
                    PushWord(PC);
                    flags.IRQ = true;
                    Push(P);
                    PC = ReadAddress(0xFFFE) | (ReadAddress(0xFFFF) << 8);
                    break;*/
                default:
                    throw new ArgumentException(instruction.ToString("X2"));
            }
        }

        public void Execute()
        {
            for (int i = 0; i < 5200; i++)
            {
                _Execute();
                cycle++;
            }

            /* byte w;
             ushort x = 6000;
             string z = "";
             while ((w = ReadAddress(x)) != '\0')
             {
                 z += (char) w;
             }*/
            Console.WriteLine(">>> " + ReadAddress(0x02));
        }

        public int IndirectX()
        {
            int off = (NextByte() + X) & 0xFF;
            return ReadAddress(off) | (ReadAddress((off + 1) & 0xFF) << 8);
        }

        public int IndirectY()
        {
            int off = NextByte() & 0xFF;
            return ((ReadAddress(off) | (ReadAddress((off + 1) & 0xFF) << 8)) + Y) & 0xFFFF;
        }

        public byte ReadAddress(int addr)
        {
            byte read = _ReadAddress(addr);
            // Console.WriteLine($"Read from {addr.ToString("X")} = {read}");
            return read;
        }

        private byte _ReadAddress(int addr)
        {
            /*
             * Address range 	Size 	Device
             * $0000-$07FF 	    $0800 	2KB internal RAM
             * $0800-$0FFF 	    $0800 	|
             * $1000-$17FF 	    $0800   | Mirrors of $0000-$07FF
             * $1800-$1FFF 	    $0800   |
             * $2000-$2007 	    $0008 	NES PPU registers
             * $2008-$3FFF 	    $1FF8 	Mirrors of $2000-2007 (repeats every 8 bytes)
             * $4000-$4017 	    $0018 	NES APU and I/O registers
             * $4018-$401F 	    $0008 	APU and I/O functionality that is normally disabled. See CPU Test Mode.
             * $4020-$FFFF 	    $BFE0 	Cartridge space: PRG ROM, PRG RAM, and mapper registers (See Note)
             * 
             * https://wiki.nesdev.com/w/index.php/CPU_memory_map
             */
            switch (addr & 0xF000)
            {
                case 0x0000:
                case 0x1000:
                    // Wrap every 7FFh bytes
                    return ram[addr & 0x07FF];
                case 0x2000:
                case 0x3000:
                    // Wrap every 7h bytes
                    int reg = (addr & 0x7) - 0x2000;
                    return emulator.PPU.ReadRegister(reg);
                case 0x4000:
                    if (addr <= 0x401F)
                    {
                        reg = addr - 0x4000;
                        return ReadRegister(reg);
                    }
                    goto default;
                default:
                    return emulator.Mapper.ReadAddress(addr);
            }

            throw new ArgumentOutOfRangeException();
        }

        public void WriteAddress(int addr, byte val)
        {
            addr &= 0xFFFF;
            // Console.WriteLine($"Write to {addr.ToString("X")} = {val}");
            switch (addr & 0xF000)
            {
                case 0x0000:
                case 0x1000:
                    // Wrap every 7FFh bytes
                    ram[addr & 0x07FF] = val;
                    return;
                case 0x2000:
                case 0x3000:
                    // Wrap every 7h bytes
                    int reg = (addr & 0x7) - 0x2000;
                    emulator.PPU.WriteRegister(reg, val);
                    return;
                case 0x4000:
                    if (addr <= 0x401F)
                    {
                        reg = addr - 0x4000;
                        WriteRegister(reg, val);
                        return;
                    }
                    goto default;
                default:
                    emulator.Mapper.WriteAddress(addr, val);
                    return;
            }
        }

        public void WriteRegister(int reg, byte val)
        {
            throw new NotImplementedException();
        }

        public byte ReadRegister(int reg)
        {
            throw new NotImplementedException();
        }
    }
}

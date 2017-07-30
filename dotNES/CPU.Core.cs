using System;

namespace dotNES
{
    partial class CPU
    {
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
            F.BreakSource = true;
        }

        public void ExecuteSingleInstruction()
        {
            currentInstruction = NextByte();

            SetInstructionAddressingMode();

            // if (cycle >= 4900)
            //Console.WriteLine($"{(PC - 1).ToString("X4")}  {currentInstruction.ToString("X2")}	\t\t\tA:{A.ToString("X2")} X:{X.ToString("X2")} Y:{Y.ToString("X2")} P:{P.ToString("X2")} SP:{SP.ToString("X2")}");

            switch (currentInstruction)
            {
                case 0x86: // STX
                    WriteByte(NextByte(), X);
                    break;
                case 0x8E: // STX
                    WriteByte(NextWord(), X);
                    break;
                case 0x84: // STY
                    WriteByte(NextByte(), Y);
                    break;
                case 0x8C: // STY
                    WriteByte(NextWord(), Y);
                    break;
                case 0x85: // STA
                    WriteByte(NextByte(), A);
                    break;
                case 0x8D: // STA
                    WriteByte(NextWord(), A);
                    break;
                case 0x4A: // LSR
                    F.Carry = (A & 0x1) > 0;
                    A >>= 1;
                    break;
                case 0x0A: // ASL
                    F.Carry = (A & 0x80) > 0;
                    A <<= 1;
                    break;
                case 0x6A: // ROR
                    bool c = F.Carry;
                    F.Carry = (A & 0x1) > 0;
                    A >>= 1;
                    if (c) A |= 0x80;
                    break;
                case 0x2A: // ROL
                    c = F.Carry;
                    F.Carry = (A & 0x80) > 0;
                    A <<= 1;
                    if (c) A |= 0x1;
                    break;
                case 0x81: // STA ind
                    WriteByte(indirectXAddressor, A);
                    break;
                case 0x91: // STA
                    WriteByte(indirectYAddressor, A);
                    break;
                case 0x99: // STA
                    WriteByte(absoluteYAddressor, A);
                    break;
                case 0x94: // STY
                    WriteByte(zeroPageXAddressor, Y);
                    break;
                case 0x95: // STA
                    WriteByte(zeroPageXAddressor, A);
                    break;
                case 0x96: // STX
                    WriteByte(zeroPageYAddressor, X);
                    break;
                case 0x9D: // STA
                    WriteByte(absoluteXAddressor, A);
                    break;
                default:
                    Opcode op = opcodes[currentInstruction];
                    if (op == null)
                        throw new ArgumentException(currentInstruction.ToString("X2"));
                    op();
                    break;
            }
        }

        private void WriteByte(Addressor accessor, int val)
        {
            accessor.Write(val);
        }
    }
}

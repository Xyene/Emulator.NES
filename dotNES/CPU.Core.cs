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

            ResetInstructionAddressingMode();

            // if (cycle >= 4900)
            Console.WriteLine($"{(PC - 1).ToString("X4")}  {currentInstruction.ToString("X2")}	\t\t\tA:{A.ToString("X2")} X:{X.ToString("X2")} Y:{Y.ToString("X2")} P:{P.ToString("X2")} SP:{SP.ToString("X2")}");

            switch (currentInstruction)
            {
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
                default:
                    Opcode op = opcodes[currentInstruction];
                    if (op == null)
                        throw new ArgumentException(currentInstruction.ToString("X2"));
                    op();
                    break;
            }
        }
    }
}

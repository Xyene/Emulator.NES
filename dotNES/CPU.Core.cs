using System;
using System.Diagnostics;

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

            PC = ReadByte(0xFFFC) | (ReadByte(0xFFFD) << 8);
        }

        public void Reset()
        {
            SP -= 3;
            F.InterruptsDisabled = true;
        }

        public bool NMI = false;
        private int _numExecuted = 0;

        public void TickFromPPU()
        {
            if (Cycle-- > 0) return;
            ExecuteSingleInstruction();
        }

        public void ExecuteSingleInstruction()
        {
            if (NMI)
            {
                PushWord(PC);
                Push(P);
                PC = ReadByte(0xFFFA) | (ReadByte(0xFFFB) << 8);
                F.InterruptsDisabled = true;
                NMI = false;
                return;
            }
            _numExecuted++;
            currentInstruction = NextByte();

            Cycle += opcodeDefs[currentInstruction].Cycles;

            ResetInstructionAddressingMode();
            // if (_numExecuted > 10000 && PC - 1 == 0xFF61)
            //     Console.WriteLine($"{(PC - 1).ToString("X4")}  {currentInstruction.ToString("X2")}	{opcodeNames[currentInstruction]}\t\t\tA:{A.ToString("X2")} X:{X.ToString("X2")} Y:{Y.ToString("X2")} P:{P.ToString("X2")} SP:{SP.ToString("X2")}");

            Opcode op = opcodes[currentInstruction];
            if (op == null)
                throw new ArgumentException(currentInstruction.ToString("X2"));
            op();
        }

        public void TriggerNMI()
        {
            NMI = true;
        }
    }
}

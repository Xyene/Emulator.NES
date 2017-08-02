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

        private int _numExecuted = 0;
        public void ExecuteSingleInstruction()
        {
            _numExecuted++;
            currentInstruction = NextByte();

            _cycle += opcodeDefs[currentInstruction].Cycles;

            ResetInstructionAddressingMode();

            Trace.WriteLine($"{_numExecuted} {ReadByte(0x2002).ToString("X4")} {(PC - 1).ToString("X4")}  {currentInstruction.ToString("X2")}	{opcodeNames[currentInstruction]}\t\t\tA:{A.ToString("X2")} X:{X.ToString("X2")} Y:{Y.ToString("X2")} P:{P.ToString("X2")} SP:{SP.ToString("X2")} CYC:{_cycle}");

            Opcode op = opcodes[currentInstruction];
            if (op == null)
                throw new ArgumentException(currentInstruction.ToString("X2"));
            op();
        }

        public void TriggerNMI()
        {

        }
    }
}

using System;

namespace dotNES
{
    partial class CPU : IAddressable
    {
        private readonly Emulator _emulator;
        private readonly byte[] _ram = new byte[0x800];
        private long _cycle;
        private readonly Addressor imm_, zpg_, abs_, absX, absY, zpgX, zpgY, indY, indX;
        private readonly Addressor[] instructionAddressingModes;
        private int currentInstruction;
        private Addressor currentAddressor;

        public CPU(Emulator emulator)
        {
            _emulator = emulator;

            imm_ = new Addressor(this, cpu => cpu.PC++);
            zpg_ = new Addressor(this, cpu => cpu.NextByte());
            abs_ = new Addressor(this, cpu => cpu.NextWord());
            zpgX = new Addressor(this, cpu => (cpu.NextByte() + cpu.X) & 0xFF);
            zpgY = new Addressor(this, cpu => (cpu.NextByte() + cpu.Y) & 0xFF);
            absX = new Addressor(this, cpu => cpu.NextWord() + cpu.X);
            absY = new Addressor(this, cpu => cpu.NextWord() + cpu.Y);
            indY = new Addressor(this, cpu =>
            {
                int off = NextByte() & 0xFF;
                return ((ReadByte(off) | (ReadByte((off + 1) & 0xFF) << 8)) + Y) & 0xFFFF;
            });
            indX = new Addressor(this, cpu =>
            {
                int off = (NextByte() + X) & 0xFF;
                return ReadByte(off) | (ReadByte((off + 1) & 0xFF) << 8);
            });

            instructionAddressingModes = new[]
            {
                null, indX, null, null, null, zpg_, zpg_, null, null, imm_, null, null, null, abs_, abs_, null,
                null, indY, null, null, null, zpgX, zpgX, null, null, absY, null, null, null, absX, absX, null,
                null, indX, null, null, zpg_, zpg_, zpg_, null, null, imm_, null, null, abs_, abs_, abs_, null,
                null, indY, null, null, null, zpgX, zpgX, null, null, absY, null, null, null, absX, absX, null,
                null, indX, null, null, null, zpg_, zpg_, null, null, imm_, null, null, null, abs_, abs_, null,
                null, indY, null, null, null, zpgX, zpgX, null, null, absY, null, null, null, absX, absX, null,
                null, indX, null, null, null, zpg_, zpg_, null, null, imm_, null, null, null, abs_, abs_, null,
                null, indY, null, null, null, zpgX, zpgX, null, null, absY, null, null, null, absX, absX, null,
                null, indX, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                null, indY, null, null, zpgX, zpgX, zpgY, null, null, absY, null, null, null, absX, null, null,
                imm_, indX, imm_, null, zpg_, zpg_, zpg_, null, null, imm_, null, null, abs_, abs_, abs_, null,
                null, indY, null, null, zpgX, zpgX, zpgY, null, null, absY, null, null, absX, absX, absY, null,
                imm_, indX, null, null, zpg_, zpg_, zpg_, null, null, imm_, null, null, abs_, abs_, abs_, null,
                null, indY, null, null, null, zpgX, zpgX, null, null, absY, null, null, null, absX, absX, null,
                imm_, indX, null, null, zpg_, zpg_, zpg_, null, null, imm_, null, null, abs_, abs_, abs_, null,
                null, indY, null, null, null, zpgX, zpgX, null, null, absY, null, null, null, absX, absX, null
            };

            Initialize();
        }

        public void Execute()
        {
            for (int i = 0; i < 5000; i++)
            {
                ExecuteSingleInstruction();
                _cycle++;
            }

            /* 
             * byte w;
             * ushort x = 6000;
             * string z = "";
             * while ((w = ReadAddress(x)) != '\0')
             * {
             *    z += (char) w;
             * }
             */
            Console.WriteLine(">>> " + ReadByte(0x02));
        }
    }
}

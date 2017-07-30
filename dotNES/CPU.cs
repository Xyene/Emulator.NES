using System;

namespace dotNES
{
    partial class CPU : IAddressable
    {
        private readonly Emulator _emulator;
        private readonly byte[] _ram = new byte[0x800];
        private readonly Addressor[] _addressors;
        private long _cycle;
        private readonly Addressor Immediate, ZeroPage, Absolute, AbsoluteX, AbsoluteY, ZeroPageX, ZeroPageY, IndirectY, IndirectX;

        public CPU(Emulator emulator)
        {
            _emulator = emulator;
            _addressors = new[] {
                Immediate = new Addressor(this, cpu => cpu.PC++),
                ZeroPage = new Addressor(this, cpu => cpu.NextByte()),
                Absolute = new Addressor(this, cpu => cpu.NextWord()),
                ZeroPageX = new Addressor(this, cpu => (cpu.NextByte() + cpu.X) & 0xFF),
                ZeroPageY = new Addressor(this, cpu => (cpu.NextByte() + cpu.Y) & 0xFF),
                AbsoluteX = new Addressor(this, cpu => cpu.NextWord() + cpu.X),
                AbsoluteY = new Addressor(this, cpu => cpu.NextWord() + cpu.Y),
                IndirectY = new Addressor(this, cpu => {
                {
                    int off = NextByte() & 0xFF;
                    return ((ReadByte(off) | (ReadByte((off + 1) & 0xFF) << 8)) + Y) & 0xFFFF;
                }}),
                IndirectX = new Addressor(this, cpu => {
                    int off = (NextByte() + X) & 0xFF;
                    return ReadByte(off) | (ReadByte((off + 1) & 0xFF) << 8);
                })
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

            /* byte w;
             ushort x = 6000;
             string z = "";
             while ((w = ReadAddress(x)) != '\0')
             {
                 z += (char) w;
             }*/
            Console.WriteLine(">>> " + ReadByte(0x02));
        }
    }
}

using System;
using System.Linq;
using System.Reflection;

namespace dotNES
{
    partial class CPU : IAddressable
    {
        private readonly Emulator _emulator;
        private readonly byte[] _ram = new byte[0x800];
        private long _cycle;
        private readonly Addressor immediateAddressor,
                                    zeroPageAddressor,
                                    absoluteAddressor,
                                    absoluteXAddressor,
                                    absoluteYAddressor,
                                    zeroPageXAddressor,
                                    zeroPageYAddressor,
                                    indirectXAddressor,
                                    indirectYAddressor;
        private int currentInstruction;
        private Addressor currentAddressor;

        delegate void Opcode();

        private Opcode[] opcodes = new Opcode[256];
        private string[] opcodeNames = new string[256];
        private AddressingMode[] opcodeAddrModes = new AddressingMode[256];

        public CPU(Emulator emulator)
        {
            _emulator = emulator;

            immediateAddressor = new Addressor(this, cpu => cpu.PC++);
            zeroPageAddressor = new Addressor(this, cpu => cpu.NextByte());
            absoluteAddressor = new Addressor(this, cpu => cpu.NextWord());
            zeroPageXAddressor = new Addressor(this, cpu => (cpu.NextByte() + cpu.X) & 0xFF);
            zeroPageYAddressor = new Addressor(this, cpu => (cpu.NextByte() + cpu.Y) & 0xFF);
            absoluteXAddressor = new Addressor(this, cpu => cpu.NextWord() + cpu.X);
            absoluteYAddressor = new Addressor(this, cpu => cpu.NextWord() + cpu.Y);
            indirectYAddressor = new Addressor(this, cpu =>
            {
                int off = NextByte() & 0xFF;
                return ((ReadByte(off) | (ReadByte((off + 1) & 0xFF) << 8)) + Y) & 0xFFFF;
            });
            indirectXAddressor = new Addressor(this, cpu =>
            {
                int off = (NextByte() + X) & 0xFF;
                return ReadByte(off) | (ReadByte((off + 1) & 0xFF) << 8);
            });

            var opcodeDefs = from opcode in GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                             let defs = opcode.GetCustomAttributes(typeof(OpcodeDef), false)
                             where defs.Length > 0
                             select new
                             {
                                 binding = (Opcode)Delegate.CreateDelegate(typeof(Opcode), this, opcode.Name),
                                 defs = (from d in defs select (OpcodeDef)d)
                             };

            foreach (var opcode in opcodeDefs)
            {
                foreach (var def in opcode.defs)
                {
                    opcodes[def.Opcode] = opcode.binding;
                    opcodeAddrModes[def.Opcode] = def.Mode;
                }
            }

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

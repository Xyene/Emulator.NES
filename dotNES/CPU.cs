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
        private int currentInstruction;

        delegate void Opcode();

        private Opcode[] opcodes = new Opcode[256];
        private string[] opcodeNames = new string[256];
        private OpcodeDef[] opcodeDefs = new OpcodeDef[256];

        private readonly Func<int, byte> ReadMapperByte;

        public CPU(Emulator emulator)
        {
            _emulator = emulator;

            ReadMapperByte = emulator.Mapper.ReadByte;

            var opcodeDefs = from opcode in GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                             let defs = opcode.GetCustomAttributes(typeof(OpcodeDef), false)
                             where defs.Length > 0
                             select new
                             {
                                 binding = (Opcode)Delegate.CreateDelegate(typeof(Opcode), this, opcode.Name),
                                 name = opcode.Name,
                                 defs = (from d in defs select (OpcodeDef)d)
                             };

            foreach (var opcode in opcodeDefs)
            {
                foreach (var def in opcode.defs)
                {
                    opcodes[def.Opcode] = opcode.binding;
                    opcodeNames[def.Opcode] = opcode.name;
                    this.opcodeDefs[def.Opcode] = def;
                }
            }

            Initialize();
        }

        public void Execute()
        {
            for (int i = 0; i < 5000; i++)
            {
                ExecuteSingleInstruction();
            }

            
            byte w;
            ushort x = 6000;
            string z = "";
            while ((w = ReadByte(x)) != '\0')
            {
               z += (char) w;
            }
            
            Console.WriteLine(">>> " + ReadByte(0x02));
        }
    }
}

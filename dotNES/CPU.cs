using System;
using System.Linq;
using System.Reflection;

namespace dotNES
{
    partial class CPU : IAddressable
    {
        private readonly Emulator _emulator;
        private readonly byte[] _ram = new byte[0x800];
        public int Cycle;
        private uint currentInstruction;

        public delegate void Opcode();

        public delegate uint ReadDelegate(uint addr);

        public delegate void WriteDelegate(uint addr, byte val);

        private readonly Opcode[] _opcodes = new Opcode[256];
        private readonly string[] _opcodeNames = new string[256];
        private readonly OpcodeDef[] _opcodeDefs = new OpcodeDef[256];

        public CPU(Emulator emulator)
        {
            _emulator = emulator;

            InitializeOpcodes();
            InitializeMaps();
            Initialize();
        }

        private void InitializeOpcodes()
        {
            var opcodeBindings = from opcode in GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                 let defs = opcode.GetCustomAttributes(typeof(OpcodeDef), false)
                                 where defs.Length > 0
                                 select new
                                 {
                                     binding = (Opcode)Delegate.CreateDelegate(typeof(Opcode), this, opcode.Name),
                                     name = opcode.Name,
                                     defs = (from d in defs select (OpcodeDef)d)
                                 };

            foreach (var opcode in opcodeBindings)
            {
                foreach (var def in opcode.defs)
                {
                    _opcodes[def.Opcode] = opcode.binding;
                    _opcodeNames[def.Opcode] = opcode.name;
                    this._opcodeDefs[def.Opcode] = def;
                }
            }
        }

        public void Execute()
        {
            for (int i = 0; i < 5000; i++)
            {
                ExecuteSingleInstruction();
            }


            uint w;
            ushort x = 6000;
            string z = "";
            while ((w = ReadByte(x)) != '\0')
            {
                z += (char)w;
            }

            Console.WriteLine(">>> " + ReadByte(0x02));
        }
    }
}

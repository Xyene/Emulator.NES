using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class CPU : IAddressable
    {
        private Emulator Emulator;
        private byte[] RAM = new byte[0x800];

        public long Cycle { get; private set; }

        public CPU(Emulator emulator)
        {
            this.Emulator = emulator;
            Initialize();
        }

        public void Execute()
        {
            for (int i = 0; i < 5000; i++)
            {
                ExecuteSingleInstruction();
                Cycle++;
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
    }
}

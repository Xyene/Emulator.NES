using System;

namespace dotNES
{
    partial class CPU : IAddressable
    {
        private readonly Emulator Emulator;
        private readonly byte[] RAM = new byte[0x800];

        public long Cycle { get; private set; }

        public CPU(Emulator emulator)
        {
            Emulator = emulator;
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
            Console.WriteLine(">>> " + ReadByte(0x02));
        }
    }
}

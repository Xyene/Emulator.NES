using System;

namespace dotNES
{
    partial class PPU : IAddressable
    {
        private Emulator _emulator;

        public PPU(Emulator emulator)
        {
            _emulator = emulator;
        }
    }
}

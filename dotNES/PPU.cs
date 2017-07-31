using System;

namespace dotNES
{
    partial class PPU : IAddressable
    {
        private Emulator emulator;

        public PPU(Emulator emulator)
        {
            this.emulator = emulator;
        }
    }
}

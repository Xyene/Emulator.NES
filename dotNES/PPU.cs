using System;

namespace dotNES
{
    partial class PPU
    {
        private Emulator emulator;

        public PPU(Emulator emulator)
        {
            this.emulator = emulator;
        }
    }
}

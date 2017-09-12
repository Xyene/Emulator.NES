namespace dotNES
{
    partial class PPU : IAddressable
    {
        private readonly Emulator _emulator;

        public PPU(Emulator emulator)
        {
            _emulator = emulator;
            InitializeMaps();
        }
    }
}

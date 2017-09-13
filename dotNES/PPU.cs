namespace dotNES
{
    sealed partial class PPU : Addressable
    {   
        public PPU(Emulator emulator) : base(emulator, addressSpace: 0x4000)
        {
            InitializeMemoryMap();
        }
    }
}

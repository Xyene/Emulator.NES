namespace dotNES
{
    class Emulator
    {
        public Emulator()
        {
            this.Cartridge = new Cartridge(@"N:\Emulator-.NES\color_test.nes");
            this.Mapper = new NROM(this);
            this.CPU = new CPU(this);
            this.PPU = new PPU(this);
        }

        public readonly CPU CPU;

        public readonly PPU PPU;

        public readonly Memory Mapper;

        public readonly Cartridge Cartridge;
    }
}

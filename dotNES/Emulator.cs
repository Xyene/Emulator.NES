namespace dotNES
{
    class Emulator
    {
        public Emulator()
        {
            this.CPU = new CPU(this);
            this.PPU = new PPU(this);
            this.Cartridge = new Cartridge(@"C:\Users\Tudor\Documents\visual studio 2017\Projects\dotNES\nestest.nes");
            this.Mapper = new NROM(this);
        }

        public CPU CPU { get; }

        public PPU PPU { get; }

        public Memory Mapper { get; }

        public Cartridge Cartridge { get; }
    }
}

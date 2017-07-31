namespace dotNES
{
    class Emulator
    {
        public Emulator()
        {
            this.Cartridge = new Cartridge(@"C:\Users\Tudor\Documents\visual studio 2017\Projects\dotNES\color_test.nes");
            this.Mapper = new NROM(this);
            this.CPU = new CPU(this);
            this.PPU = new PPU(this);
        }

        public CPU CPU { get; }

        public PPU PPU { get; }

        public Memory Mapper { get; }

        public Cartridge Cartridge { get; }
    }
}

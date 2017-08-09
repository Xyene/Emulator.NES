namespace dotNES
{
    class Emulator
    {
        public Emulator(string path, NES001Controller controller)
        {
            Cartridge = new Cartridge(path);
            Mapper = new NROM(this);
            CPU = new CPU(this);
            PPU = new PPU(this);
            Controller = controller;
        }

        public NES001Controller Controller;

        public readonly CPU CPU;

        public readonly PPU PPU;

        public readonly Memory Mapper;

        public readonly Cartridge Cartridge;
    }
}

using System;
using System.Collections.Generic;
using dotNES.Mappers;

namespace dotNES
{
    class Emulator
    {
        private static readonly Dictionary<int, Type> Mappers = new Dictionary<int, Type> {
            {0, typeof(NROM)},
            {2, typeof(UxROM)},
            {94, typeof(Mapper094)},
            {180, typeof(Mapper180)}
        };

        public Emulator(string path, NES001Controller controller)
        {
            Cartridge = new Cartridge(path);
            Mapper = (AbstractMemory)Activator.CreateInstance(Mappers[Cartridge.MapperNumber], this);
            CPU = new CPU(this);
            PPU = new PPU(this);
            Controller = controller;
        }

        public NES001Controller Controller;

        public readonly CPU CPU;

        public readonly PPU PPU;

        public readonly IAddressable Mapper;

        public readonly Cartridge Cartridge;
    }
}

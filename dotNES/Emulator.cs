using System;
using System.Collections.Generic;
using dotNES.Mappers;

namespace dotNES
{
    class Emulator
    {
        private static readonly Dictionary<int, Type> Mappers = new Dictionary<int, Type> {
            {0, typeof(NROM)},
            {1, typeof(MMC1)},
            {2, typeof(UxROM)},
            {9, typeof(MMC2)},
            {10, typeof(MMC4)},
            {94, typeof(Mapper094)},
            {155, typeof(Mapper155)},
            {180, typeof(Mapper180)}
        };

        public Emulator(string path, NES001Controller controller)
        {
            Cartridge = new Cartridge(path);
            if (!Mappers.ContainsKey(Cartridge.MapperNumber))
                throw new NotImplementedException($"unsupported mapper {Cartridge.MapperNumber}");
            Mapper = (AbstractMapper)Activator.CreateInstance(Mappers[Cartridge.MapperNumber], this);
            CPU = new CPU(this);
            PPU = new PPU(this);
            Controller = controller;
        }

        public NES001Controller Controller;

        public readonly CPU CPU;

        public readonly PPU PPU;

        public readonly AbstractMapper Mapper;

        public readonly Cartridge Cartridge;
    }
}

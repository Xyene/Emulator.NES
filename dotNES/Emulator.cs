using System;
using System.Collections.Generic;
using System.IO;
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

        public NES001Controller Controller;

        public readonly CPU CPU;

        public readonly PPU PPU;

        public readonly AbstractMapper Mapper;

        public readonly Cartridge Cartridge;

        private string _path;

        public Emulator(string path, NES001Controller controller)
        {
            _path = path;
            Cartridge = new Cartridge(path);
            if (!Mappers.ContainsKey(Cartridge.MapperNumber))
                throw new NotImplementedException($"unsupported mapper {Cartridge.MapperNumber}");
            Mapper = (AbstractMapper)Activator.CreateInstance(Mappers[Cartridge.MapperNumber], this);
            CPU = new CPU(this);
            PPU = new PPU(this);
            Controller = controller;

            Load();
        }

        public void Save()
        {
            using (var fs = new FileStream(_path + ".sav", FileMode.Create, FileAccess.Write))
            {
                Mapper.Save(fs);
            }
        }

        public void Load()
        {
            var sav = _path + ".sav";
            if (!File.Exists(sav)) return;

            using (var fs = new FileStream(sav, FileMode.Open, FileAccess.Read))
            {
                Mapper.Load(fs);
            }
        }
    }
}

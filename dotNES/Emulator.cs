using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using dotNES.Mappers;

namespace dotNES
{
    class Emulator
    {
        private static readonly Dictionary<int, Type> Mappers = (from type in Assembly.GetExecutingAssembly().GetTypes()
                                                                 let def = (MapperDef)type.GetCustomAttributes(typeof(MapperDef), true).FirstOrDefault()
                                                                 where def != null
                                                                 select new { def, type }).ToDictionary(a => a.def.Id, a => a.type);


        public NES001Controller Controller;

        public readonly CPU CPU;

        public readonly PPU PPU;

        public readonly BaseMapper Mapper;

        public readonly Cartridge Cartridge;

        private readonly string _path;

        public Emulator(string path, NES001Controller controller)
        {
            _path = path;
            Cartridge = new Cartridge(path);
            if (!Mappers.ContainsKey(Cartridge.MapperNumber))
                throw new NotImplementedException($"unsupported mapper {Cartridge.MapperNumber}");
            Mapper = (BaseMapper)Activator.CreateInstance(Mappers[Cartridge.MapperNumber], this);
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

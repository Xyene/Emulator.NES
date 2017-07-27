using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    class Cartridge
    {
        public readonly byte[] raw;
        public readonly int PRGROMSize;
        public readonly int CHRROMSize;
        public readonly int PRGRAMSize;
        public readonly int PRGROMOffset;
        public readonly int MapperNumber;

        public Cartridge(string filename)
        {
            raw = System.IO.File.ReadAllBytes(filename);

            int header = BitConverter.ToInt32(raw, 0);
            if (header != 0x1A53454E) // "NES<EOF>"
                throw new FormatException("unexpected header value " + header.ToString("X"));

            PRGROMSize = raw[4] * 0x4000; // 16kb units
            CHRROMSize = raw[5] * 0x2000; // 8kb units
            PRGRAMSize = raw[8] * 0x2000;

            bool hasTrainer = (raw[6] & 0x2) > 0;
            PRGROMOffset = 16 + (hasTrainer ? 512 : 0);

            MapperNumber = (raw[6] >> 4) | (raw[7] & 0xF0);
        }

        public override string ToString()
        {
            return $"Cartridge{{PRGROMSize={PRGROMSize}, CHRROMSize={CHRROMSize}, PRGROMOffset={PRGROMOffset}, MapperNumber={MapperNumber}}}";
        }
    }
}

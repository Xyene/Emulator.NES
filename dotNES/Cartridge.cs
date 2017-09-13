using System;

namespace dotNES
{
    class Cartridge
    {
        public readonly byte[] Raw;
        public readonly int PRGROMSize;
        public readonly int CHRROMSize;
        public readonly int PRGRAMSize;
        public readonly int PRGROMOffset;
        public readonly int MapperNumber;
        public readonly byte[] PRGROM;
        public readonly byte[] CHRROM;
        public VRAMMirroringMode MirroringMode;

        public enum VRAMMirroringMode
        {
            Horizontal, Vertical, All, Upper, Lower
        }

        public Cartridge(string filename)
        {
            Raw = System.IO.File.ReadAllBytes(filename);

            int header = BitConverter.ToInt32(Raw, 0);
            if (header != 0x1A53454E) // "NES<EOF>"
                throw new FormatException("unexpected header value " + header.ToString("X"));

            PRGROMSize = Raw[4] * 0x4000; // 16kb units
            CHRROMSize = Raw[5] * 0x2000; // 8kb units
            PRGRAMSize = Raw[8] * 0x2000;

            bool hasTrainer = (Raw[6] & 0b100) > 0;
            PRGROMOffset = 16 + (hasTrainer ? 512 : 0);

            MirroringMode = (Raw[6] & 0x1) > 0 ? VRAMMirroringMode.Vertical : VRAMMirroringMode.Horizontal;
            if ((Raw[6] & 0x8) > 0) MirroringMode = VRAMMirroringMode.All;

            MapperNumber = (Raw[6] >> 4) | (Raw[7] & 0xF0);

            PRGROM = new byte[PRGROMSize];
            Array.Copy(Raw, PRGROMOffset, PRGROM, 0, PRGROMSize);

            if (CHRROMSize == 0)
                CHRROM = new byte[0x2000];
            else
            {
                CHRROM = new byte[CHRROMSize];
                Array.Copy(Raw, PRGROMOffset + PRGROMSize, CHRROM, 0, CHRROMSize);
            }
        }

        public override string ToString()
        {
            return $"Cartridge{{PRGROMSize={PRGROMSize}, CHRROMSize={CHRROMSize}, PRGROMOffset={PRGROMOffset}, MapperNumber={MapperNumber}}}";
        }
    }
}

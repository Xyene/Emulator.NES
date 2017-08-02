using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class PPU
    {
        private byte[] OAM = new byte[256];
        private byte[] VRAM = new byte[0x800];
        private byte[] PaletteRAM = new byte[0x20];

        private int _lastWrittenRegister;

        public void WriteRegister(int reg, byte val)
        {
            reg &= 0xF;
            _lastWrittenRegister = val & 0xFF;
            switch (reg)
            {
                case 0x0000:
                    PPUCTRL = val;
                    return;
                case 0x0001:
                    PPUMASK = val;
                    return;
                case 0x0002:
                    PPUSTATUS = val;
                    return;
                case 0x0006:
                    PPUADDR = val;
                    return;
                case 0x0007:
                    PPUDATA = val;
                    return;
            }

            throw new NotImplementedException($"{reg.ToString("X4")} = {val.ToString("X2")}");
        }

        public byte ReadRegister(int reg)
        {
            reg &= 0xF;
            switch (reg)
            {
                case 0x0000:
                    return (byte)PPUCTRL;
                case 0x0001:
                    return (byte)PPUMASK;
                case 0x0002:
                    return (byte)PPUSTATUS;
                case 0x0006:
                    return (byte)PPUADDR;
                case 0x0007:
                    return (byte)PPUDATA;
            }
            throw new NotImplementedException(reg.ToString("X2"));
        }

        public byte ReadByte(int addr)
        {
            addr &= 0xFFFF;
            throw new NotImplementedException();
        }

        public void WriteByte(int addr, int _val)
        {
            byte val = (byte)_val;

            addr &= 0xFFFF;
            switch (addr & 0xF000)
            {
                case 0x0000:
                case 0x1000:
                    emulator.Cartridge.CHRROM[addr] = val;
                    break;
                case 0x2000:
                    VRAM[(addr - 0x2000) & 0x7FF] = val;
                    break;
                case 0x3000:
                    if (addr <= 0x3EFF)
                    {
                        VRAM[addr - 0x300] = val;
                    }
                    else
                    {
                        PaletteRAM[(addr - 0x3000) & 0x19] = val;
                    }
                    break;
                default:
                    throw new NotImplementedException($"{addr.ToString("X4")} = {val.ToString("X2")}");
            }
        }
    }
}

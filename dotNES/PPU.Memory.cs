using System;
using System.Runtime.CompilerServices;

namespace dotNES
{
    partial class PPU
    {
        public byte[] OAM = new byte[0x100];
        private byte[] VRAM = new byte[0x2000];
        private byte[] PaletteRAM = new byte[0x20];

        private static readonly uint[][] VRAMMirrorLookup =
        {
            new uint[]{0, 0, 1, 1},
            new uint[]{0, 1, 0, 1},
            new uint[]{0, 1, 2, 3},
        };

        // TODO: cart-controlled modes
        private Cartridge.VRAMMirroringMode _currentMirroringMode => emulator.Cartridge.MirroringMode;

        private int _lastWrittenRegister;

        public void WriteRegister(uint reg, byte val)
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
                case 0x0002: return;
                case 0x0003:
                    OAMADDR = val;
                    return;
                case 0x0004:
                    OAMDATA = val;
                    return;
                case 0x005:
                    PPUSCROLL = val;
                    return;
                case 0x0006:
                    PPUADDR = val;
                    return;
                case 0x0007:
                    PPUDATA = val;
                    return;
            }

            throw new NotImplementedException($"{reg:X4} = {val:X2}");
        }

        public byte ReadRegister(uint reg)
        {
            reg &= 0xF;
            switch (reg)
            {
                case 0x0000: return (byte)_lastWrittenRegister;
                case 0x0001: return (byte)_lastWrittenRegister;
                case 0x0002:
                    return (byte)PPUSTATUS;
                case 0x0003:
                    return (byte)OAMADDR;
                case 0x0004:
                    return (byte)OAMDATA;
                case 0x0005: return (byte)_lastWrittenRegister;
                case 0x0006: return (byte)_lastWrittenRegister;
                case 0x0007:
                    return (byte)PPUDATA;
            }
            throw new NotImplementedException(reg.ToString("X2"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetVRAMMirror(long addr)
        {
            long entry;
            var table = Math.DivRem(addr - 0x2000, 0x400, out entry);
            return VRAMMirrorLookup[(int)_currentMirroringMode][table] * 0x400 + (uint)entry;
        }

        public uint ReadByte(uint addr)
        {
            if (0x3EFF < addr)
            {
                if (addr == 0x3F10 || addr == 0x3F14 || addr == 0x3F18 || addr == 0x3F0C)
                    addr -= 0x10;
                return PaletteRAM[(addr - 0x3F00) & 0x1F];
            }

            if (addr < 0x2000)
            {
                return emulator.Cartridge.CHRROM[addr];
            }

            if (addr < 0x3000)
            {
                return VRAM[GetVRAMMirror(addr)];
            }

            return VRAM[GetVRAMMirror(addr - 0x1000)];
        }

        public void WriteByte(uint addr, uint _val)
        {
            byte val = (byte)_val;

            switch (addr & 0xF000)
            {
                case 0x0000:
                case 0x1000:
                    emulator.Cartridge.CHRROM[addr] = val;
                    break;
                case 0x2000:
                    VRAM[GetVRAMMirror(addr)] = val;
                    break;
                case 0x3000:
                    if (addr <= 0x3EFF)
                    {
                        VRAM[GetVRAMMirror(addr - 0x1000)] = val;
                    }
                    else
                    {
                        if (addr == 0x3F10 || addr == 0x3F14 || addr == 0x3F18 || addr == 0x3F0C)
                            addr -= 0x10;
                        PaletteRAM[(addr - 0x3F00) & 0x1F] = val;
                    }
                    break;
                default:
                    throw new NotImplementedException($"{addr:X4} = {val:X2}");
            }
        }
    }
}

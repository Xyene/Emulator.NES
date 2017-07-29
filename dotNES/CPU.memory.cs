using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class CPU
    {
        private byte NextByte() => ReadAddress((ushort)PC++);

        private ushort NextWord() => (ushort)(NextByte() | (NextByte() << 8));

        private sbyte NextSByte() => (sbyte)NextByte();

        private int Immediate() => PC++;

        private void Push(int what)
        {
            WriteAddress(0x100 + SP, what);
            SP--;
        }

        private byte Pop()
        {
            SP++;
            return ReadAddress(0x100 + SP);
        }

        private void PushWord(int what)
        {
            Push(what >> 8);
            Push(what & 0xFF);
        }

        private int PopWord()
        {
            return Pop() | (Pop() << 8);
        }

        public int IndirectX()
        {
            int off = (NextByte() + X) & 0xFF;
            return ReadAddress(off) | (ReadAddress((off + 1) & 0xFF) << 8);
        }

        public int IndirectY()
        {
            int off = NextByte() & 0xFF;
            return ((ReadAddress(off) | (ReadAddress((off + 1) & 0xFF) << 8)) + Y) & 0xFFFF;
        }

        public byte ReadAddress(int addr)
        {
            byte read = _ReadAddress(addr);
            // Console.WriteLine($"Read from {addr.ToString("X")} = {read}");
            return read;
        }

        private byte _ReadAddress(int addr)
        {
            /*
             * Address range 	Size 	Device
             * $0000-$07FF 	    $0800 	2KB internal RAM
             * $0800-$0FFF 	    $0800 	|
             * $1000-$17FF 	    $0800   | Mirrors of $0000-$07FF
             * $1800-$1FFF 	    $0800   |
             * $2000-$2007 	    $0008 	NES PPU registers
             * $2008-$3FFF 	    $1FF8 	Mirrors of $2000-2007 (repeats every 8 bytes)
             * $4000-$4017 	    $0018 	NES APU and I/O registers
             * $4018-$401F 	    $0008 	APU and I/O functionality that is normally disabled. See CPU Test Mode.
             * $4020-$FFFF 	    $BFE0 	Cartridge space: PRG ROM, PRG RAM, and mapper registers (See Note)
             * 
             * https://wiki.nesdev.com/w/index.php/CPU_memory_map
             */
            switch (addr & 0xF000)
            {
                case 0x0000:
                case 0x1000:
                    // Wrap every 7FFh bytes
                    return RAM[addr & 0x07FF];
                case 0x2000:
                case 0x3000:
                    // Wrap every 7h bytes
                    int reg = (addr & 0x7) - 0x2000;
                    return Emulator.PPU.ReadRegister(reg);
                case 0x4000:
                    if (addr <= 0x401F)
                    {
                        reg = addr - 0x4000;
                        return ReadRegister(reg);
                    }
                    goto default;
                default:
                    return Emulator.Mapper.ReadAddress(addr);
            }
        }

        public void WriteAddress(int addr, int _val)
        {
            byte val = (byte)_val;
            addr &= 0xFFFF;
            // Console.WriteLine($"Write to {addr.ToString("X")} = {val}");
            switch (addr & 0xF000)
            {
                case 0x0000:
                case 0x1000:
                    // Wrap every 7FFh bytes
                    RAM[addr & 0x07FF] = val;
                    return;
                case 0x2000:
                case 0x3000:
                    // Wrap every 7h bytes
                    int reg = (addr & 0x7) - 0x2000;
                    Emulator.PPU.WriteRegister(reg, val);
                    return;
                case 0x4000:
                    if (addr <= 0x401F)
                    {
                        reg = addr - 0x4000;
                        WriteRegister(reg, val);
                        return;
                    }
                    goto default;
                default:
                    Emulator.Mapper.WriteAddress(addr, val);
                    return;
            }
        }

        public void WriteRegister(int reg, byte val)
        {
            throw new NotImplementedException();
        }

        public byte ReadRegister(int reg)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Runtime.CompilerServices;
using static dotNES.CPU.AddressingMode;

namespace dotNES
{
    partial class CPU
    {
        public enum AddressingMode
        {
            None,
            Direct,
            Immediate,
            ZeroPage,
            Absolute,
            ZeroPageX,
            ZeroPageY,
            AbsoluteX,
            AbsoluteY,
            IndirectX,
            IndirectY
        }

        private uint? _currentMemoryAddress;

        private void ResetInstructionAddressingMode() => _currentMemoryAddress = null;

        private uint _Address()
        {
            var def = opcodeDefs[currentInstruction];
            switch (def.Mode)
            {
                case Immediate:
                    return PC++;
                case ZeroPage:
                    return NextByte();
                case Absolute:
                    return NextWord();
                case ZeroPageX:
                    return (NextByte() + X) & 0xFF;
                case ZeroPageY:
                    return (NextByte() + Y) & 0xFF;
                case AbsoluteX:
                    uint addr = NextWord();
                    if (def.PageBoundary && (addr & 0xFF00) != ((addr + X) & 0xFF00)) _cycle += 1;
                    return addr + X;
                case AbsoluteY:
                    addr = NextWord();
                    if (def.PageBoundary && (addr & 0xFF00) != ((addr + Y) & 0xFF00)) _cycle += 1;
                    return addr + Y;
                case IndirectX:
                    uint off = (NextByte() + X) & 0xFF;
                    return ReadByte(off) | (ReadByte((off + 1) & 0xFF) << 8);
                case IndirectY:
                    off = NextByte() & 0xFF;
                    addr = ReadByte(off) | (ReadByte((off + 1) & 0xFF) << 8);
                    if (def.PageBoundary && (addr & 0xFF00) != ((addr + Y) & 0xFF00)) _cycle += 1;
                    return (addr + Y) & 0xFFFF;
            }
            throw new NotImplementedException();
        }

        public uint AddressRead()
        {
            if (opcodeDefs[currentInstruction].Mode == Direct) return A;
            if (_currentMemoryAddress == null) _currentMemoryAddress = _Address();
            return ReadByte((uint)_currentMemoryAddress) & 0xFF;
        }

        public void AddressWrite(uint val)
        {
            if (opcodeDefs[currentInstruction].Mode == Direct) A = val;
            else
            {
                if (_currentMemoryAddress == null) _currentMemoryAddress = _Address();
                WriteByte((uint)_currentMemoryAddress, val);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint NextByte() => ReadByte(PC++);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint NextWord() => NextByte() | (NextByte() << 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private sbyte NextSByte() => (sbyte)NextByte();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Push(uint what)
        {
            WriteByte(0x100 + SP, what);
            SP--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint Pop()
        {
            SP++;
            return ReadByte(0x100 + SP);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PushWord(uint what)
        {
            Push(what >> 8);
            Push(what & 0xFF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint PopWord() => Pop() | (Pop() << 8);

        public uint ReadByte(uint addr)
        {
            addr &= 0xFFFF;
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
            if (0x401F < addr)
            {
                return ReadMapperByte(addr);
            }

            if (addr < 0x2000)
            {
                // Wrap every 7FFh bytes
                return _ram[addr & 0x07FF];
            }

            if (addr < 0x4000)
            {
                // Wrap every 7h bytes
                return _emulator.PPU.ReadRegister((addr & 0x7) - 0x2000);
            }

            return ReadIORegister(addr - 0x4000);
        }

        public void WriteByte(uint addr, uint _val)
        {
            byte val = (byte)_val;
            addr &= 0xFFFF;
            // Console.WriteLine($"Write to {addr.ToString("X")} = {val}");
            switch (addr & 0xF000)
            {
                case 0x0000:
                case 0x1000:
                    // Wrap every 7FFh bytes
                    _ram[addr & 0x07FF] = val;
                    return;
                case 0x2000:
                case 0x3000:
                    // Wrap every 7h bytes
                    uint reg = (addr & 0x7) - 0x2000;
                    _emulator.PPU.WriteRegister(reg, val);
                    return;
                case 0x4000:
                    if (addr <= 0x401F)
                    {
                        reg = addr - 0x4000;
                        WriteIORegister(reg, val);
                        return;
                    }
                    goto default;
                default:
                    _emulator.Mapper.WriteByte(addr, val);
                    return;
            }
        }

        private void PerformDMA(uint from)
        {
            //Console.WriteLine("OAM DMA");
            from <<= 8;
            uint OAMADDR = _emulator.PPU.F.OAMAddress;
            for (uint i = 0; i <= 0xFF; i++)
            {
                _emulator.PPU.OAM[i] = (byte)ReadByte(from | ((i + OAMADDR) & 0xFF));
            }
            _cycle += 513;
        }
    }
}

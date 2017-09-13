using System;
using System.Linq;
using System.Runtime.CompilerServices;
using static dotNES.CPU.AddressingMode;

namespace dotNES
{
    sealed partial class CPU
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
        private uint _rmwValue;

        private void ResetInstructionAddressingMode() => _currentMemoryAddress = null;

        private uint _Address()
        {
            var def = _opcodeDefs[_currentInstruction];
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
                    if (def.PageBoundary && (addr & 0xFF00) != ((addr + X) & 0xFF00)) Cycle += 1;
                    return addr + X;
                case AbsoluteY:
                    addr = NextWord();
                    if (def.PageBoundary && (addr & 0xFF00) != ((addr + Y) & 0xFF00)) Cycle += 1;
                    return addr + Y;
                case IndirectX:
                    uint off = (NextByte() + X) & 0xFF;
                    return ReadByte(off) | (ReadByte((off + 1) & 0xFF) << 8);
                case IndirectY:
                    off = NextByte() & 0xFF;
                    addr = ReadByte(off) | (ReadByte((off + 1) & 0xFF) << 8);
                    if (def.PageBoundary && (addr & 0xFF00) != ((addr + Y) & 0xFF00)) Cycle += 1;
                    return (addr + Y) & 0xFFFF;
            }
            throw new NotImplementedException();
        }

        public uint AddressRead()
        {
            if (_opcodeDefs[_currentInstruction].Mode == Direct) return _rmwValue = A;
            if (_currentMemoryAddress == null) _currentMemoryAddress = _Address();
            return _rmwValue = ReadByte((uint)_currentMemoryAddress) & 0xFF;
        }

        public void AddressWrite(uint val)
        {
            if (_opcodeDefs[_currentInstruction].Mode == Direct) A = val;
            else
            {
                if (_currentMemoryAddress == null) _currentMemoryAddress = _Address();
                if (_opcodeDefs[_currentInstruction].RMW)
                    WriteByte((uint)_currentMemoryAddress, _rmwValue);
                WriteByte((uint)_currentMemoryAddress, val);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint ReadWord(uint addr) => ReadByte(addr) | (ReadByte(addr + 1) << 8);

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

        protected override void InitializeMemoryMap()
        {
            base.InitializeMemoryMap();

            MapReadHandler(0x0000, 0x1FFF, addr => _ram[addr & 0x07FF]);
            MapReadHandler(0x2000, 0x3FFF, addr => _emulator.PPU.ReadRegister((addr & 0x7) - 0x2000));
            MapReadHandler(0x4000, 0x4017, ReadIORegister);

            MapWriteHandler(0x0000, 0x1FFF, (addr, val) => _ram[addr & 0x07FF] = val);
            MapWriteHandler(0x2000, 0x3FFF, (addr, val) => _emulator.PPU.WriteRegister((addr & 0x7) - 0x2000, val));
            MapWriteHandler(0x4000, 0x401F, WriteIORegister);

            _emulator.Mapper.InitializeMemoryMap(this);
        }
    }
}

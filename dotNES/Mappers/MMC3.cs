using System;
using System.Runtime.CompilerServices;
using static dotNES.Cartridge.VRAMMirroringMode;

namespace dotNES.Mappers
{
    [MapperDef(4)]
    class MMC3 : BaseMapper
    {
        // Different PRG RAM write/enable controls
        public enum ChipType { MMC3, MMC6 }
        public enum CHRBankingMode { TwoFour, FourTwo }
        public enum PRGBankingMode { SwitchFix, FixSwitch }

        private readonly Cartridge.VRAMMirroringMode[] _mirroringModes = { Vertical, Horizontal };

        private readonly ChipType _type;
        protected CHRBankingMode _chrBankingMode;
        protected PRGBankingMode _prgBankingMode;


        protected readonly uint[] _chrBankOffsets = new uint[8];
        protected uint[] _prgBankOffsets;
        protected readonly uint[] _banks = new uint[8];
        protected uint _currentBank;

        private uint _irqReloadValue;
        private uint _irqCounter;
        protected bool _irqEnabled;

        private bool _prgRAMEnabled;

        public MMC3(Emulator emulator) : this(emulator, ChipType.MMC3)
        {

        }

        public MMC3(Emulator emulator, ChipType chipType) : base(emulator)
        {
            _type = chipType;
            _prgBankOffsets = new uint[] { 0, 0x2000, _lastBankOffset, _lastBankOffset + 0x2000 };
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x6000, 0x7FFF, addr => _prgRAM[addr - 0x6000]);
            cpu.MapReadHandler(0x8000, 0xFFFF, addr => _prgROM[_prgBankOffsets[(addr - 0x8000) / 0x2000] + addr % 0x2000]);

            cpu.MapWriteHandler(0x6000, 0xFFFF, WriteByte);
        }

        public override void InitializeMemoryMap(PPU ppu)
        {
            ppu.MapReadHandler(0x0000, 0x1FFF, addr => _chrROM[_chrBankOffsets[addr / 0x400] + addr % 0x400]);
            ppu.MapWriteHandler(0x0000, 0x1FFF, (addr, val) => _chrROM[_chrBankOffsets[addr / 0x400] + addr % 0x400] = val);
        }

        public override void ProcessCycle(int scanline, int cycle)
        {
            if (_emulator.PPU.F.RenderingEnabled && cycle == 260 && (0 <= scanline && scanline < 240 || scanline == -1))
            {
                if (_irqCounter == 0)
                {
                    _irqCounter = _irqReloadValue;
                }
                else
                {
                    _irqCounter--;
                    if (_irqEnabled && _irqCounter == 0) _emulator.CPU.TriggerInterrupt(CPU.InterruptType.IRQ);
                }
            }
        }

        protected void WriteByte(uint addr, byte value)
        {
            bool even = (addr & 0x1) == 0;

            if (addr < 0x8000)
            {
                if (_prgRAMEnabled)
                    _prgRAM[addr - 0x6000] = value;
            }
            else if (addr < 0xA000)
            {
                if (even)
                {
                    _currentBank = value & 0x7u;
                    _prgBankingMode = (PRGBankingMode)((value >> 6) & 0x1);
                    _chrBankingMode = (CHRBankingMode)((value >> 7) & 0x1);
                }
                else
                {
                    _banks[_currentBank] = value;
                }
                UpdateOffsets();
            }
            else if (addr < 0xC000)
            {
                if (even)
                    _emulator.Cartridge.MirroringMode = _mirroringModes[value & 0x1];
                else
                    _prgRAMEnabled = (value & 0xC0) == 0x80;
            }
            else if (addr < 0xE000)
            {
                if (even)
                    _irqReloadValue = value;
                else
                    _irqCounter = 0;
            }
            else
            {
                _irqEnabled = !even;
            }
        }

        protected void UpdateOffsets()
        {
            switch (_prgBankingMode)
            {
                case PRGBankingMode.SwitchFix:
                    _prgBankOffsets[0] = _banks[6] * 0x2000;
                    _prgBankOffsets[1] = _banks[7] * 0x2000;
                    _prgBankOffsets[2] = _lastBankOffset;
                    _prgBankOffsets[3] = _lastBankOffset + 0x2000;
                    break;
                case PRGBankingMode.FixSwitch:
                    _prgBankOffsets[0] = _lastBankOffset;
                    _prgBankOffsets[1] = _banks[7] * 0x2000;
                    _prgBankOffsets[2] = _banks[6] * 0x2000;
                    _prgBankOffsets[3] = _lastBankOffset + 0x2000;
                    break;
            }

            switch (_chrBankingMode)
            {
                case CHRBankingMode.TwoFour:
                    _chrBankOffsets[0] = _banks[0] & 0xFE;
                    _chrBankOffsets[1] = _banks[0] | 0x01;
                    _chrBankOffsets[2] = _banks[1] & 0xFE;
                    _chrBankOffsets[3] = _banks[1] | 0x01;
                    _chrBankOffsets[4] = _banks[2];
                    _chrBankOffsets[5] = _banks[3];
                    _chrBankOffsets[6] = _banks[4];
                    _chrBankOffsets[7] = _banks[5];
                    break;
                case CHRBankingMode.FourTwo:
                    _chrBankOffsets[0] = _banks[2];
                    _chrBankOffsets[1] = _banks[3];
                    _chrBankOffsets[2] = _banks[4];
                    _chrBankOffsets[3] = _banks[5];
                    _chrBankOffsets[4] = _banks[0] & 0xFE;
                    _chrBankOffsets[5] = _banks[0] | 0x01;
                    _chrBankOffsets[6] = _banks[1] & 0xFE;
                    _chrBankOffsets[7] = _banks[1] | 0x01;
                    break;
            }

            for (int i = 0; i < _prgBankOffsets.Length; i++)
                _prgBankOffsets[i] %= (uint)_prgROM.Length;

            for (int i = 0; i < _chrBankOffsets.Length; i++)
                _chrBankOffsets[i] = (uint) (_chrBankOffsets[i] * 0x400 % _chrROM.Length);
        }
    }
}

using System;
using System.Runtime.CompilerServices;
using static dotNES.Cartridge.VRAMMirroringMode;

namespace dotNES.Mappers
{
    [MapperDef(1)]
    class MMC1 : BaseMapper
    {
        // TODO: are MMC1 and MMC1A even different chip types?
        public enum ChipType { MMC1, MMC1A, MMC1B, MMC1C }
        public enum CHRBankingMode { Single, Double }
        public enum PRGBankingMode { Switch32Kb, Switch16KbFixFirst, Switch16KbFixLast }

        private readonly Cartridge.VRAMMirroringMode[] _mirroringModes = { Lower, Upper, Vertical, Horizontal };

        private readonly ChipType _type;
        private CHRBankingMode _chrBankingMode;
        private PRGBankingMode _prgBankingMode;

        private uint _serialData;
        private int _serialPos;

        private uint _control;

        private readonly uint[] _chrBankOffsets = new uint[2];
        private readonly uint[] _chrBanks = new uint[2];

        private readonly uint[] _prgBankOffsets = new uint[2];
        private uint _prgBank;

        private bool _prgRAMEnabled;

        private uint? _lastWritePC;

        public MMC1(Emulator emulator) : this(emulator, ChipType.MMC1B)
        {

        }

        public MMC1(Emulator emulator, ChipType chipType) : base(emulator)
        {
            _type = chipType;
            if (chipType == ChipType.MMC1B) _prgRAMEnabled = true;
            UpdateControl(0x0F);
            _emulator.Cartridge.MirroringMode = Horizontal;
        }

        public override void InitializeMemoryMap(CPU cpu)
        {
            cpu.MapReadHandler(0x6000, 0x7FFF, addr => _prgRAM[addr - 0x6000]);
            cpu.MapReadHandler(0x8000, 0xFFFF, addr => _prgROM[_prgBankOffsets[(addr - 0x8000) / 0x4000] + addr % 0x4000]);

            cpu.MapWriteHandler(0x6000, 0x7FFF, (addr, val) =>
            {
                // PRG RAM is always enabled on MMC1A
                if (_type == ChipType.MMC1A || _prgRAMEnabled)
                    _prgRAM[addr - 0x6000] = val;
            });
            cpu.MapWriteHandler(0x8000, 0xFFFF, (addr, val) =>
            {
                // Explicitly ignore the second write happening on consecutive cycles
                // of an RMW instruction
                var cycle = _emulator.CPU.PC;
                if (cycle == _lastWritePC)
                    return;
                _lastWritePC = cycle;

                if ((val & 0x80) > 0)
                {
                    _serialData = 0;
                    _serialPos = 0;
                    UpdateControl(_control | 0x0C);
                }
                else
                {
                    _serialData |= (uint)((val & 0x1) << _serialPos);
                    _serialPos++;

                    if (_serialPos == 5)
                    {
                        // Address is incompletely decoded
                        addr &= 0x6000;
                        if (addr == 0x0000)
                            UpdateControl(_serialData);
                        else if (addr == 0x2000)
                            UpdateCHRBank(0, _serialData);
                        else if (addr == 0x4000)
                            UpdateCHRBank(1, _serialData);
                        else if (addr == 0x6000)
                            UpdatePRGBank(_serialData);

                        _serialData = 0;
                        _serialPos = 0;
                    }
                }
            });
        }

        public override void InitializeMemoryMap(PPU ppu)
        {
            ppu.MapReadHandler(0x0000, 0x1FFF, addr => _chrROM[_chrBankOffsets[addr / 0x1000] + addr % 0x1000]);
            ppu.MapWriteHandler(0x0000, 0x1FFF, (addr, val) => _chrROM[_chrBankOffsets[addr / 0x1000] + addr % 0x1000] = val);
        }

        private void UpdateControl(uint value)
        {
            _control = value;

            _emulator.Cartridge.MirroringMode = _mirroringModes[value & 0x3];

            _chrBankingMode = (CHRBankingMode)((value >> 4) & 0x1);

            var prgMode = (value >> 2) & 0x3;
            // Both 0 and 1 are 32Kb switch
            if (prgMode == 0) prgMode = 1;
            _prgBankingMode = (PRGBankingMode)(prgMode - 1);

            UpdateCHRBank(1, _chrBanks[1]);
            UpdateCHRBank(0, _chrBanks[0]);
            UpdatePRGBank(_prgBank);
        }

        private void UpdatePRGBank(uint value)
        {
            _prgBank = value;

            _prgRAMEnabled = (value & 0x10) == 0;
            value &= 0xF;

            switch (_prgBankingMode)
            {
                case PRGBankingMode.Switch32Kb:
                    value >>= 1;
                    value *= 0x4000;
                    _prgBankOffsets[0] = value;
                    _prgBankOffsets[1] = value + 0x4000;
                    break;
                case PRGBankingMode.Switch16KbFixFirst:
                    _prgBankOffsets[0] = 0;
                    _prgBankOffsets[1] = value * 0x4000;
                    break;
                case PRGBankingMode.Switch16KbFixLast:
                    _prgBankOffsets[0] = value * 0x4000;
                    _prgBankOffsets[1] = _lastBankOffset;
                    break;
            }
        }

        private void UpdateCHRBank(uint bank, uint value)
        {
            _chrBanks[bank] = value;

            // TODO FIXME: I feel like this branch should only be taken
            // when bank == 0, but this breaks Final Fantasy
            // When can banking mode change without UpdateCHRBank being called?
            if (_chrBankingMode == CHRBankingMode.Single)
            {
                value = _chrBanks[0];
                value >>= 1;
                value *= 0x1000;
                _chrBankOffsets[0] = value;
                _chrBankOffsets[1] = value + 0x1000;
            }
            else
            {
                _chrBankOffsets[bank] = value * 0x1000;
            }
        }
    }
}

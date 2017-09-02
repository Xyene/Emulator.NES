using System;
using System.Runtime.CompilerServices;

namespace dotNES.Mappers
{
    class MMC1 : AbstractMapper
    {
        public enum ChipType { MMC1, MMC1A, MMC1B, MMC1C }
        public enum CHRBankingMode { Single, Double }
        public enum PRGBankingMode { Switch32Kb, Switch16KbFixFirst, Switch16KbFixLast }

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
        private readonly byte[] _prgRAM = new byte[0x2000];

        // Set to MaxValue in case a RMW happens in first cycle -- is that even possible?
        private uint _lastWriteCycle = uint.MaxValue;

        public MMC1(Emulator emulator) : this(emulator, ChipType.MMC1)
        {

        }

        public MMC1(Emulator emulator, ChipType chipType) : base(emulator)
        {
            _type = chipType;
            if (chipType == ChipType.MMC1B) _prgRAMEnabled = true;
            UpdateControl(0x0F);
            _emulator.Cartridge.MirroringMode = Cartridge.VRAMMirroringMode.HORIZONTAL;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override uint ReadBytePPU(uint addr)
        {
            if (addr < 0x2000)
            {
                return _chrROM[_chrBankOffsets[addr / 0x1000] + addr % 0x1000];
            }
            throw new NotImplementedException();
        }

        public override uint ReadByte(uint addr)
        {
            if (0x6000 <= addr && addr < 0x8000)
            {
                return _prgRAM[addr - 0x6000];
            }
            if (addr >= 0x8000)
            {
                return _prgROM[_prgBankOffsets[(addr - 0x8000) / 0x4000] + addr % 0x4000];
            }
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void WriteBytePPU(uint addr, uint value)
        {
            if (addr < 0x2000)
            {
                _chrROM[_chrBankOffsets[addr / 0x1000] + addr % 0x1000] = (byte)value;
            }
            else throw new NotImplementedException();
        }

        public override void WriteByte(uint addr, uint value)
        {
            // Explicitly ignore the second write happening on consecutive cycles
            // of an RMW instruction
            var cycle = _emulator.CPU.PC;
            if (cycle == _lastWriteCycle) return;
            _lastWriteCycle = cycle;

            if (0x6000 <= addr && addr < 0x8000)
            {
                // PRG RAM is always enabled on MMC1A
                if (_type == ChipType.MMC1A || _prgRAMEnabled) _prgRAM[addr - 0x6000] = (byte)value;
            }
            else if (addr >= 0x8000)
            {
                if ((value & 0x80) > 0)
                {
                    _serialData = 0;
                    _serialPos = 0;
                    UpdateControl(_control | 0x0C);
                }
                else
                {
                    _serialData |= (value & 0x1) << _serialPos;
                    _serialPos++;

                    if (_serialPos == 5)
                    {
                        if (addr < 0xA000)
                            UpdateControl(_serialData);
                        else if (addr < 0xC000)
                            UpdateCHRBank(0, _chrBanks[0] = _serialData);
                        else if (addr < 0xE000)
                            UpdateCHRBank(1, _chrBanks[1] = _serialData);
                        else
                            UpdatePRGBank(_serialData);

                        _serialData = 0;
                        _serialPos = 0;
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void UpdateControl(uint value)
        {
            _control = value;

            switch (value & 0x03)
            {
                case 0:
                    _emulator.Cartridge.MirroringMode = Cartridge.VRAMMirroringMode.LOWER;
                    break;
                case 1:
                    _emulator.Cartridge.MirroringMode = Cartridge.VRAMMirroringMode.UPPER;
                    break;
                case 2:
                    _emulator.Cartridge.MirroringMode = Cartridge.VRAMMirroringMode.VERTICAL;
                    break;
                case 3:
                    _emulator.Cartridge.MirroringMode = Cartridge.VRAMMirroringMode.HORIZONTAL;
                    break;
            }

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

            _prgRAMEnabled = (value & 0x10) > 0;
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

            if (_chrBankingMode == CHRBankingMode.Single)
            {
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

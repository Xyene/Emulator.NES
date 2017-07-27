using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    class PPU : IAddressable
    {
        private Emulator emulator;

        public PPU(Emulator emulator)
        {
            this.emulator = emulator;
        }

        public void WriteRegister(int reg, byte val)
        {
            switch (reg)
            {
                case 0x0000:
                    PPUCTRL = val;
                    return;
            }
            throw new NotImplementedException();
        }

        public byte ReadRegister(int reg)
        {
            switch (reg)
            {
                case 0x0000:
                    return PPUCTRL;
            }
            throw new NotImplementedException();
        }

        public byte ReadAddress(ushort addr)
        {
            throw new NotImplementedException();
        }

        public void WriteAddress(ushort addr, byte val)
        {
            throw new NotImplementedException();
        }

        private bool PPUCTRL_NMIEnabled => (PPUCTRL & 0x80) > 0;

        private bool PPUCTRL_IsMaster => (PPUCTRL & 0x40) > 0;

        private bool PPUCTRL_TallSpritesEnabled => (PPUCTRL & 0x20) > 0;

        private ushort PPUCTRL_PatternTableAddress => (ushort)((PPUCTRL & 0x10) > 0 ? 0x1000 : 0x0000);

        private ushort PPUCTRL_SpriteTableAddress => (ushort)((PPUCTRL & 0x08) > 0 ? 0x1000 : 0x0000);

        private bool PPUCTRL_VRAMIncrementMode => (PPUCTRL & 0x04) > 0;

        private ushort PPUCTRL_NametableAddress
        {
            get
            {
                switch (PPUCTRL & 0x3)
                {
                    case 0:
                        return 0x2000;
                    case 1:
                        return 0x2400;
                    case 2:
                        return 0x2800;
                    case 3:
                        return 0x2C00;
                    default:
                        return 0; // Impossible
                }
            }
        }

        public byte PPUCTRL { get; private set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class PPU
    {
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class PPU
    {
        public class PPUFlags
        {
            /* PPUCTRL register */
            public bool NMIEnabled;
            public bool IsMaster;
            public bool TallSpritesEnabled;
            public int PatternTableAddress;
            public int SpriteTableAddress;
            public int VRAMIncrement;

            /* PPUMASK register */
            public bool GrayscaleEnabled;
            public bool DrawLeftBackground;
            public bool DrawLeftSprites;
            public bool DrawBackground;
            public bool DrawSprites;
            // Flipped for PAL/Dendy
            public bool EmphasizeRed;
            public bool EmphasizeGreen;
            public bool EmphasizeBlue;

            /* PPUSTATUS register */
            public bool VBlankStarted;
            public bool Sprite0Hit;
            public bool SpriteOverflow;
            public bool AddressLatch;

            /* PPUADDR register */
            private int _busAddress;
            public int BusAddress
            {
                get => _busAddress;
                set => _busAddress = value & 0x3FFF;
            }

            /* PPUDATA register */
            public int BusData;

            /* OAMADDR register */
            private int _oamAddress;
            public int OAMAddress
            {
                get => _oamAddress;
                set => _oamAddress = value & 0xFF;
            }

            /* PPUSCROLL registers */
            [Obsolete]
            public int ScrollX;
            [Obsolete]
            public int ScrollY;
        }

        public PPUFlags F = new PPUFlags();

        private int _v;
        public int V
        {
            get => _v;
            set => _v = value & 0x7FFF;
        }
        public int T, X;

        public int CoarseX => V & 0x1F;

        public int CoarseY => (V >> 5) & 0x1F;

        public int FineY => (V >> 12) & 0x7;

        public void ReloadScrollX() => V = (V & 0xFBE0) | (T & 0x041F);

        public void ReloadScrollY() => V = (V & 0x841F) | (T & 0x7BE0);

        public void IncrementScrollX()
        {
            if ((V & 0x001F) == 31) // if coarse X == 31
            {
                V &= ~0x001F; // coarse X = 0
                V ^= 0x0400; // switch horizontal nametable
            }
            else
                V += 1; // increment coarse X
        }

        public void IncrementScrollY()
        {
            if ((V & 0x7000) != 0x7000) // if fine Y < 7
                V += 0x1000; // increment fine Y
            else
            {
                V &= ~0x7000; // fine Y = 0

                int y = (V & 0x03E0) >> 5; // let y = coarse Y
                if (y == 29)
                {
                    y = 0; // coarse Y = 0
                    V ^= 0x0800;
                }
                // switch vertical nametable
                else if (y == 31)
                    y = 0; // coarse Y = 0, nametable not switched
                else
                    y += 1; // increment coarse Y
                V = (V & 0xFC1F) | (y << 5); // put coarse Y back into v
            }
        }

        public int PPUCTRL
        {
            set
            {
                F.NMIEnabled = (value & 0x80) > 0;
                F.IsMaster = (value & 0x40) > 0;
                F.TallSpritesEnabled = (value & 0x20) > 0;
                F.PatternTableAddress = (value & 0x10) > 0 ? 0x1000 : 0x0000;
                F.SpriteTableAddress = (value & 0x08) > 0 ? 0x1000 : 0x0000;
                F.VRAMIncrement = (value & 0x04) > 0 ? 32 : 1;
                // yyy NN YYYYY XXXXX
                // ||| || ||||| +++++--coarse X scroll
                // ||| || +++++--------coarse Y scroll
                // ||| ++--------------nametable select
                // +++-----------------fine Y scroll
                T = (T & 0xF3FF) | ((value & 0x3) << 10); // Bits 10-11 hold the base address of the nametable minus $2000
            }
        }

        public int PPUMASK
        {
            set
            {
                F.GrayscaleEnabled = (value & 0x1) > 0;
                F.DrawLeftBackground = (value & 0x2) > 0;
                F.DrawLeftSprites = (value & 0x4) > 0;
                F.DrawBackground = (value & 0x8) > 0;
                F.DrawSprites = (value & 0x10) > 0;
                F.EmphasizeRed = (value & 0x20) > 0;
                F.EmphasizeGreen = (value & 0x40) > 0;
                F.EmphasizeBlue = (value & 0x80) > 0;
            }
        }

        /** $2002 **/
        public int PPUSTATUS
        {
            get
            {
                F.AddressLatch = false;
                F.BusAddress = 0;
                var ret = (F.VBlankStarted.AsByte() << 7) |
                    (F.Sprite0Hit.AsByte() << 6) |
                    (F.SpriteOverflow.AsByte() << 5) |
                    (_lastWrittenRegister & 0x1F);
                F.VBlankStarted = false;
                return ret;
            }
        }

        /** $2006 **/
        public int PPUADDR
        {
            set
            {
                if (F.AddressLatch)
                {
                    T = (T & 0xFF00) | value;
                    F.BusAddress = T;
                }
                else
                    T = (T & 0x80FF) | ((value & 0x3F) << 8);
                F.AddressLatch ^= true;
            }
        }

        /** $2005 **/
        public int PPUSCROLL
        {
            set
            {
                if (F.AddressLatch)
                {
                    F.ScrollY = value;
                    T = (T & 0x8FFF) | ((value & 0x7) << 12);
                    T = (T & 0xFC1F) | (value & 0xF8) << 2;
                }
                else
                {
                    F.ScrollX = value;
                    X = value & 0x7;
                    T = (T & 0xFFE0) | (value >> 3);
                }
                F.AddressLatch ^= true;
            }
        }

        private byte _readBuffer;
        public int PPUDATA
        {
            get
            {
                byte ret = ReadByte(F.BusAddress);
                if (F.BusAddress < 0x3EFF)
                {
                    byte temp = _readBuffer;
                    _readBuffer = ret;
                    ret = temp;
                }
                F.BusAddress += F.VRAMIncrement;
                return ret;
            }
            set
            {
                F.BusData = value;
                WriteByte(F.BusAddress, value);
                F.BusAddress += F.VRAMIncrement;
            }
        }

        public int OAMADDR
        {
            get => F.OAMAddress;
            set => F.OAMAddress = value;
        }

        public int OAMDATA
        {
            get => OAM[F.OAMAddress];
            set
            {
                OAM[F.OAMAddress] = (byte)value;
                F.OAMAddress++;
            }
        }
    }
}

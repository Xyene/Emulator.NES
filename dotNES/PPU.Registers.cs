using System;

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
            public uint PatternTableAddress;
            public uint SpriteTableAddress;
            public uint VRAMIncrement;

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
            private uint _busAddress;
            public uint BusAddress
            {
                get => _busAddress;
                set => _busAddress = value & 0x3FFF;
            }

            /* PPUDATA register */
            public uint BusData;

            /* OAMADDR register */
            private uint _oamAddress;
            public uint OAMAddress
            {
                get => _oamAddress;
                set => _oamAddress = value & 0xFF;
            }

            /* PPUSCROLL registers */
            [Obsolete]
            public uint ScrollX;
            [Obsolete]
            public uint ScrollY;

            public bool RenderingEnabled => DrawBackground || DrawSprites;
        }

        public PPUFlags F = new PPUFlags();

        private uint _v;
        public uint V
        {
            get => _v;
            set => _v = value & 0x7FFF;
        }
        public uint T, X;

        public uint CoarseX => V & 0x1F;

        public uint CoarseY => (V >> 5) & 0x1F;

        public uint FineY => (V >> 12) & 0x7;

        public void ReloadScrollX() => V = (V & 0xFBE0) | (T & 0x041F);

        public void ReloadScrollY() => V = (V & 0x841F) | (T & 0x7BE0);

        public void IncrementScrollX()
        {
            if ((V & 0x001F) == 31) // if coarse X == 31
            {
                V &= ~0x001Fu; // coarse X = 0
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
                V &= ~0x7000u; // fine Y = 0

                uint y = (V & 0x03E0) >> 5; // let y = coarse Y
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
                V = (V & ~0x03E0u) | (y << 5); // put coarse Y back into v
            }
        }

        public uint PPUCTRL
        {
            set
            {
                F.NMIEnabled = (value & 0x80) > 0;
                F.IsMaster = (value & 0x40) > 0;
                F.TallSpritesEnabled = (value & 0x20) > 0;
                F.PatternTableAddress = (value & 0x10) > 0 ? 0x1000u : 0x0000;
                F.SpriteTableAddress = (value & 0x08) > 0 ? 0x1000u : 0x0000;
                F.VRAMIncrement = (value & 0x04) > 0 ? 32u : 1;
                // yyy NN YYYYY XXXXX
                // ||| || ||||| +++++--coarse X scroll
                // ||| || +++++--------coarse Y scroll
                // ||| ++--------------nametable select
                // +++-----------------fine Y scroll
                T = (T & 0xF3FF) | ((value & 0x3) << 10); // Bits 10-11 hold the base address of the nametable minus $2000
            }
        }

        public uint PPUMASK
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
        public uint PPUSTATUS
        {
            get
            {
                F.AddressLatch = false;
                var ret = (F.VBlankStarted.AsByte() << 7) |
                    (F.Sprite0Hit.AsByte() << 6) |
                    (F.SpriteOverflow.AsByte() << 5) |
                    (_lastWrittenRegister & 0x1F);
                F.VBlankStarted = false;
                return (uint)ret;
            }
        }

        /** $2006 **/
        public uint PPUADDR
        {
            set
            {
                if (F.AddressLatch)
                {
                    T = (T & 0xFF00) | value;
                    F.BusAddress = T;
                    V = T;
                }
                else
                    T = (T & 0x80FF) | ((value & 0x3F) << 8);
                F.AddressLatch ^= true;
            }
        }

        /** $2005 **/
        public uint PPUSCROLL
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

        private uint _readBuffer;
        public uint PPUDATA
        {
            get
            {
                uint ret = ReadByte(F.BusAddress);
                if (F.BusAddress < 0x3F00)
                {
                    uint temp = _readBuffer;
                    _readBuffer = ret;
                    ret = temp;
                }
                else
                {
                    // Palette read should also read VRAM into read buffer
                    _readBuffer = ReadByte(F.BusAddress - 0x1000);
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

        public uint OAMADDR
        {
            get => F.OAMAddress;
            set => F.OAMAddress = value;
        }

        public uint OAMDATA
        {
            get => _oam[F.OAMAddress];
            set
            {
                _oam[F.OAMAddress] = (byte)value;
                F.OAMAddress++;
            }
        }
    }
}

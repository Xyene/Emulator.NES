using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class PPU
    {
        const int GameWidth = 256, GameHeight = 240;
        public uint bufferPos;
        public uint[] rawBitmap = new uint[GameWidth * GameHeight];
        public int[] priority = new int[GameWidth * GameHeight];

        // TODO: use real chroma/luma decoding
        private uint[] Palette = {
            0x7C7C7C, 0x0000FC, 0x0000BC, 0x4428BC, 0x940084, 0xA80020, 0xA81000, 0x881400,
            0x503000, 0x007800, 0x006800, 0x005800, 0x004058, 0x000000, 0x000000, 0x000000,
            0xBCBCBC, 0x0078F8, 0x0058F8, 0x6844FC, 0xD800CC, 0xE40058, 0xF83800, 0xE45C10,
            0xAC7C00, 0x00B800, 0x00A800, 0x00A844, 0x008888, 0x000000, 0x000000, 0x000000,
            0xF8F8F8, 0x3CBCFC, 0x6888FC, 0x9878F8, 0xF878F8, 0xF85898, 0xF87858, 0xFCA044,
            0xF8B800, 0xB8F818, 0x58D854, 0x58F898, 0x00E8D8, 0x787878, 0x000000, 0x000000,
            0xFCFCFC, 0xA4E4FC, 0xB8B8F8, 0xD8B8F8, 0xF8B8F8, 0xF8A4C0, 0xF0D0B0, 0xFCE0A8,
            0xF8D878, 0xD8F878, 0xB8F8B8, 0xB8F8D8, 0x00FCFC, 0xF8D8F8, 0x000000, 0x000000
        };
        private int ScanlineCount = 261;
        private int VBlankSetLine = 241;
        private int VBlankClearedLine = 20;
        private int CyclesPerLine = 341;
        private int CPUSyncCounter = 2;
        private int[] scanlineOAM = new int[8 * 4];
        private bool[] isSprite0 = new bool[8];
        private int spriteCount;

        private long tileShiftRegister;
        private int _currentNametableByte;
        private int _currentHighTile, _currentLowTile;
        private int _currentColor;

        public void ProcessPixel(int x, int y)
        {
            ProcessBackgroundForPixel(x, y);
            if (F.DrawSprites)
                ProcessSpritesForPixel(x, y);

            if (y != -1) bufferPos++;
        }

        private void CountSpritesOnLine(int scanline)
        {
            spriteCount = 0;
            int height = F.TallSpritesEnabled ? 16 : 8;

            for (int idx = 0; idx < OAM.Length; idx += 4)
            {
                int y = OAM[idx] + 1;

                if (scanline >= y && scanline < y + height)
                {
                    isSprite0[spriteCount] = idx == 0;
                    scanlineOAM[spriteCount * 4 + 0] = OAM[idx + 0];
                    scanlineOAM[spriteCount * 4 + 1] = OAM[idx + 1];
                    scanlineOAM[spriteCount * 4 + 2] = OAM[idx + 2];
                    scanlineOAM[spriteCount * 4 + 3] = OAM[idx + 3];
                    spriteCount++;
                }

                if (spriteCount == 8) break;
            }
        }

        private void NextNametableByte()
        {
            _currentNametableByte = ReadByte(0x2000 | (V & 0x0FFF));
        }

        private void NextTileByte(bool hi)
        {
            int tileIdx = _currentNametableByte * 16;

            int address = F.PatternTableAddress + tileIdx + FineY;

            if (hi)
                _currentHighTile = ReadByte(address + 8);
            else
                _currentLowTile = ReadByte(address);
        }

        private void NextAttributeByte()
        {
            // Bless nesdev
            int addr = 0x23C0 | (V & 0x0C00) | ((V >> 4) & 0x38) | ((V >> 2) & 0x07);

            _currentColor = (ReadByte(addr) >> ((CoarseX & 2) | ((CoarseY & 2) << 1))) & 0x3;
        }

        private void ShiftTileRegister()
        {
            for (int x = 0; x < 8; x++)
            {
                int palette = ((_currentHighTile & 0x80) >> 6) | ((_currentLowTile & 0x80) >> 7);
                tileShiftRegister |= (uint)(palette + _currentColor * 4) << ((7 - x) * 4);
                _currentLowTile <<= 1;
                _currentHighTile <<= 1;
            }
        }

        private void ProcessBackgroundForPixel(int cycle, int scanline)
        {
            if (cycle < 8 && !F.DrawLeftBackground || !F.DrawBackground && scanline != -1)
            {
                // Maximally sketchy: if current address is in the PPU palette, then it draws that palette entry if rendering is disabled
                // Otherwise, it draws $3F00 (universal bg color)
                // https://www.romhacking.net/forum/index.php?topic=20554.0
                rawBitmap[bufferPos] = Palette[ReadByte(0x3F00 + ((F.BusAddress & 0x3F00) == 0x3F00 ? F.BusAddress & 0x001F : 0)) & 0x3F];
                return;
            }

            int paletteEntry = (int)(tileShiftRegister >> 32 >> ((7 - X) * 4)) & 0x0F;
            if (paletteEntry % 4 == 0) paletteEntry = 0;

            if (scanline != -1)
            {
                priority[bufferPos] = paletteEntry;
                rawBitmap[bufferPos] = Palette[ReadByte(0x3F00 + paletteEntry) & 0x3F];
            }
        }

        private void ProcessSpritesForPixel(int x, int scanline)
        {
            for (int idx = 0; idx < spriteCount * 4; idx += 4)
            {
                int spriteX = scanlineOAM[idx + 3];
                int spriteY = scanlineOAM[idx] + 1;

                // Don't draw this sprite if...
                if (spriteY == 0 || // it's located at y = 0
                    spriteY > 239 || // it's located past y = 239 ($EF)
                    x >= spriteX + 8 || // it's behind the current dot
                    x < spriteX || // it's ahead of the current dot
                    x < 8 && !F.DrawLeftSprites) // it's in the clip area, and clipping is enabled
                    continue;

                // amusingly enough, the PPU's palette handling is basically identical
                // to that of the Gameboy / Gameboy Color, so I've sort of just copy/pasted
                // handling code wholesale from my GBC emulator at
                // https://github.com/Xyene/Nitrous-Emulator/blob/master/src/main/java/nitrous/lcd/LCD.java#L642
                int tileIdx = scanlineOAM[idx + 1] * 16;
                int attrib = scanlineOAM[idx + 2] & 0xE3;

                int palette = attrib & 0x3;
                bool front = (attrib & 0x20) == 0;
                bool flipX = (attrib & 0x40) > 0;
                bool flipY = (attrib & 0x80) > 0;

                int px = x - spriteX;
                int line = scanline - spriteY;

                // here we handle the x and y flipping by tweaking the indices we are accessing
                int logicalX = flipX ? 7 - px : px;
                int logicalLine = flipY ? 7 - line : line;
                int address = F.SpriteTableAddress + tileIdx + logicalLine;

                // this looks bad, but it's about as readable as it's going to get
                int color =
                    (
                        (
                            (
                                // fetch upper bit from 2nd bit plane
                                ReadByte(address + 8) & (0x80 >> logicalX)
                            ) >> (7 - logicalX)
                        ) << 1 // this is the upper bit of the color number
                    ) |
                    (
                        (
                            ReadByte(address) & (0x80 >> logicalX)
                        ) >> (7 - logicalX)
                    ); // << 0, this is the lower bit of the color number

                if (color > 0)
                {
                    int backgroundPixel = priority[bufferPos];
                    // Sprite 0 hits...
                    if (!(!isSprite0[idx / 4] || // do not occur on not-0 sprite
                          x < 8 && !F.DrawLeftSprites || // or if left clipping is enabled
                          backgroundPixel == 0 || // or if bg pixel is transparent
                          F.Sprite0Hit || // or if it fired this frame already
                          x == 255)) // or if x is 255, "for an obscure reason related to the pixel pipeline"
                        F.Sprite0Hit = true;
                    if (front || backgroundPixel == 0)
                    {
                        if (scanline != -1)
                        {
                            rawBitmap[bufferPos] = Palette[ReadByte(0x3F10 + palette * 4 + color) & 0x3F];
                        }
                    }
                }
            }
        }

        public void ProcessFrame()
        {
            rawBitmap.Fill((uint)0);
            priority.Fill(0);
            bufferPos = 0;

            for (int i = -1; i < ScanlineCount; i++)
                ProcessScanline(i);
        }

        public void ProcessScanline(int line)
        {
            for (int i = 0; i < CyclesPerLine; i++)
                ProcessCycle(line, i);
        }

        public void ProcessCycle(int scanline, int cycle)
        {
            bool visibleCycle = 1 <= cycle && cycle <= 256;
            bool prefetchCycle = 321 <= cycle && cycle <= 336;
            bool fetchCycle = visibleCycle || prefetchCycle;

            if (0 <= scanline && scanline < 240 || scanline == -1)
            {
                if (visibleCycle)
                    ProcessPixel(cycle - 1, scanline);

                // During pixels 280 through 304 of this scanline, the vertical scroll bits are reloaded TODO: if rendering is enabled.
                if (scanline == -1 && 280 <= cycle && cycle <= 304)
                    ReloadScrollY();

                if (fetchCycle)
                {
                    tileShiftRegister <<= 4;

                    // Begin rendering a brand new tile
                    if ((cycle & 7) == 0)
                        ShiftTileRegister();

                    // Takes 8 cycles for tile to be read, 2 per "step"
                    switch (cycle & 0x3)
                    {
                        case 0: // NT
                            NextNametableByte();
                            break;
                        case 1: // AT
                            NextAttributeByte();
                            break;
                        case 2: // Tile low
                            NextTileByte(false);
                            break;
                        case 3: // Tile high
                            NextTileByte(true);
                            break;
                    }

                    if (cycle % 8 == 0)
                        IncrementScrollX();
                }

                if (cycle == 256)
                    IncrementScrollY();

                if (cycle == 257)
                {
                    ReloadScrollX();
                    // 257 - 320
                    // The tile data for the sprites on the next scanline are fetched here.
                    // TODO: stagger this over all the cycles as opposed to only on 257
                    CountSpritesOnLine(scanline + 1);
                }
            }

            if (cycle == 1)
            {
                if (scanline == VBlankSetLine)
                {
                    F.VBlankStarted = true;
                    if (F.NMIEnabled)
                        emulator.CPU.TriggerNMI();
                }

                if (scanline == VBlankClearedLine)
                {
                    F.VBlankStarted = false;
                    F.Sprite0Hit = false;
                }
            }

            if (CPUSyncCounter + 1 == 3)
            {
                emulator.CPU.TickFromPPU();
                CPUSyncCounter = 0;
            }
            else CPUSyncCounter++;
        }
    }
}

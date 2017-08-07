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
        private int ScanlineCount = 262;
        private int VBlankSetLine = 241;
        private int VBlankClearedLine = 20;
        private int CyclesPerLine = 341;
        private int CPUSyncCounter = 2;
        private int[] scanlineOAM = new int[8 * 4];
        private int spriteCount;

        public void ProcessPixel(int x, int y)
        {
            ProcessBackgroundForPixel(x, y);
            ProcessSpritesForPixel(x, y);
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
                    scanlineOAM[spriteCount * 4 + 0] = OAM[idx + 0];
                    scanlineOAM[spriteCount * 4 + 1] = OAM[idx + 1];
                    scanlineOAM[spriteCount * 4 + 2] = OAM[idx + 2];
                    scanlineOAM[spriteCount * 4 + 3] = OAM[idx + 3];
                    spriteCount++;
                }

                if (spriteCount == 8) break;
            }
        }

        private void ProcessBackgroundForPixel(int x, int y)
        {
            // TODO: scroll?
            int tileX = x / 8;
            int tileY = y / 8;

            // TODO: handle mirroring etc.
            int nametableAddressBase = 0x2000;
            int attributeTableAddressBase = nametableAddressBase + 0x3C0; // 960 bytes followed by attribs

            byte attributeTableEntry = ReadByte(attributeTableAddressBase + (tileY >> 2) * 8 + (tileX >> 2));

            // 7654 3210
            // |||| || ++- Color bits 3 - 2 for top left quadrant of this byte
            // |||| ++---  Color bits 3 - 2 for top right quadrant of this byte
            // || ++------ Color bits 3 - 2 for bottom left quadrant of this byte
            // ++--------  Color bits 3 - 2 for bottom right quadrant of this byte
            // value = (topleft << 0) | (topright << 2) | (bottomleft << 4) | (bottomright << 6)   

            int palette = (attributeTableEntry >> (((tileX & 1) << 1) | (tileY & 1) << 2)) & 0x3;

            int tileIdx = ReadByte(nametableAddressBase + tileY * 32 + tileX) * 16;

            int logicalX = x & 7;
            int logicalLine = y & 7;
            int address = F.PatternTableAddress + tileIdx + logicalLine;

            int color =
                (
                    (
                        ReadByte(address + 8) >> (6 - logicalX)
                    ) & 0x2 // this is the upper bit of the color number
                )
                |
                (
                    (
                        ReadByte(address) >> (7 - logicalX)
                    ) & 0x1 // this is the lower bit of the color number
                );

            if (color == 0)
            {
                palette = 0;
            }

            priority[y * GameWidth + x] = color;
            rawBitmap[y * GameWidth + x] = Palette[ReadByte(0x3F00 + palette * 4 + color) & 0x3F];
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
                            ReadByte(address + 8) >> (6 - logicalX)
                        ) & 0x2 // this is the upper bit of the color number
                    )
                        |
                    (
                        (
                            ReadByte(address) >> (7 - logicalX)
                        ) & 0x1 // this is the lower bit of the color number
                    ); 

                if (color > 0)
                {
                    int backgroundPixel = priority[scanline * GameWidth + x];
                    // Sprite 0 hits...
                    if (!(idx != 0 || // do not occur on not-0 sprite (TODO: this isn't the real sprite 0)
                          x < 8 && !F.DrawLeftSprites || // or if left clipping is enabled
                          backgroundPixel == 0 || // or if bg pixel is transparent
                          F.Sprite0Hit || // or if it fired this frame already
                          x == 255)) // or if x is 255, "for an obscure reason related to the pixel pipeline"
                        F.Sprite0Hit = true;

                    if (front || backgroundPixel == 0)
                    {
                        rawBitmap[scanline * GameWidth + x] = Palette[ReadByte(0x3F10 + palette * 4 + color) & 0x3F];
                    }
                }
            }
        }

        public void ProcessFrame()
        {
            // Console.WriteLine("---Frame---");
            for (int i = 0; i < ScanlineCount; i++)
                ProcessScanline(i);
        }

        public void ProcessScanline(int line)
        {
            // Console.WriteLine("---Scanline---")
            for (int i = 0; i < CyclesPerLine; i++)
                ProcessCycle(line, i);
        }

        public void ProcessCycle(int scanline, int cycle)
        {
            if (scanline < 240 && cycle < 256)
                ProcessPixel(cycle, scanline);

            if (cycle == 256)
            {
                CountSpritesOnLine(scanline + 1);
            }

            if (cycle == 0)
            {
                if (scanline == VBlankSetLine)
                {
                    // Console.WriteLine("---VBlank Set---");
                    F.VBlankStarted = true;
                    if (F.NMIEnabled)
                    {
                        this.emulator.CPU.TriggerNMI();
                    }
                }

                if (scanline == VBlankClearedLine)
                {
                    // Console.WriteLine("---VBlank End---");
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

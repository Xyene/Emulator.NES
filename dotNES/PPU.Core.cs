using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotNES
{
    partial class PPU
    {
        private int ScanlineCount = 262;
        private int VBlankSetLine = 241;
        private int VBlankClearedLine = 20;
        private int CyclesPerLine = 341;
        private int CPUSyncCounter = 2;

        public void ProcessFrame()
        {
            Console.WriteLine("---Frame---");
            for (int i = 0; i < ScanlineCount; i++)
                ProcessScanline(i);
        }

        public void ProcessScanline(int line)
        {
            Console.WriteLine("---Scanline---");
            for (int i = 0; i < CyclesPerLine; i++)
                ProcessCycle(line, i);
        }

        public void ProcessCycle(int scanline, int cycle)
        {
            if (scanline == VBlankSetLine && cycle == 0)
            {
                Console.WriteLine("---VBlank Set---");
                F.VBlankStarted = true;
            }
            if (scanline == VBlankClearedLine && cycle == 0)
            {
                Console.WriteLine("---VBlank End---");
                F.VBlankStarted = false;
            }

            if (CPUSyncCounter + 1 == 3)
            {
                emulator.CPU.ExecuteSingleInstruction();
                CPUSyncCounter = 0;
            } else CPUSyncCounter++;
        }
    }
}

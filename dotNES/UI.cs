using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dotNES
{
    public partial class UI : Form
    {
        private bool rendererRunning = true;
        private Thread renderer;
        private NES001Controller controller = new NES001Controller();

        public UI()
        {
            InitializeComponent();
        }

        private void UI_Load(object sender, EventArgs e)
        {
            renderer = new Thread(() =>
            {
                string[] args = Environment.GetCommandLineArgs();
                string rom = args.Length > 1 ? args[1] : @"N:\Emulator-.NES\oam_read.nes";
                Emulator emu = new Emulator(rom, controller);
                Console.WriteLine(emu.Cartridge);
                Stopwatch s = new Stopwatch();
                while (rendererRunning)
                {
                    s.Restart();
                    for (int i = 0; i < 60; i++)
                    {
                        emu.PPU.ProcessFrame();
                        rawBitmap = emu.PPU.rawBitmap;
                        Invoke((MethodInvoker)Draw);
                        Thread.Sleep(500/60);
                    }
                    s.Stop();
                    Console.WriteLine($"60 frames in {s.ElapsedMilliseconds}ms");
                }
            });
            renderer.Start();
        }

        private void UI_FormClosing(object sender, FormClosingEventArgs e)
        {
            rendererRunning = false;
            renderer.Abort();
        }

        private void UI_KeyDown(object sender, KeyEventArgs e)
        {
            controller.PressKey(e);
        }

        private void UI_KeyUp(object sender, KeyEventArgs e)
        {
            controller.ReleaseKey(e);
        }
    }
}

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

        public UI()
        {
            InitializeComponent();
        }

        private void UI_Load(object sender, EventArgs e)
        {
            renderer = new Thread(() =>
            {
                Emulator emu = new Emulator(@"N:\Emulator-.NES\donkeykong.nes");
                Console.WriteLine(emu.Cartridge);
                while (rendererRunning)
                {
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    for (int i = 0; i < 60; i++)
                    {
                        emu.PPU.ProcessFrame();
                        rawBitmap = emu.PPU.rawBitmap;
                        Invoke((MethodInvoker)delegate { Draw(); });
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
    }
}

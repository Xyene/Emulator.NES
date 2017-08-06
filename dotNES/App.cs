using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace dotNES
{
    static class App
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            UI ui = new UI();
            ui.InitRendering();
            Thread renderer = new Thread(() =>
            {
                Emulator emu = new Emulator();
                Console.WriteLine(emu.Cartridge);
                while (true)
                {
                    Stopwatch s = new Stopwatch();
                    s.Start();
                    for (int i = 0; i < 60; i++)
                    {
                        emu.PPU.ProcessFrame();
                        ui.rawBitmap = emu.PPU.rawBitmap;
                        ui.Invoke((MethodInvoker) delegate { ui.Draw(); });
                    }
                    s.Stop();
                    Console.WriteLine($"60 frames in {s.ElapsedMilliseconds}ms");
                }
            });
            renderer.Start();
            Application.ApplicationExit += delegate
            {
                renderer.Abort();
                Environment.Exit(0);
            };
            Application.Run(ui);
            //Emulator emu = new Emulator();
            //Console.WriteLine(emu.Cartridge);
            //for (int i = 0; i < 10000; i++)
            //{
            //   emu.CPU.Execute();
            //}
            //emu.PPU.ProcessFrame();
            //emu.PPU.ProcessFrame();
            //emu.PPU.ProcessFrame();
            //emu.PPU.ProcessFrame();
            //emu.PPU.ProcessFrame();
        }
    }
}

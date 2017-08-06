using System;
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
            Application.Idle += delegate {
                Emulator emu = new Emulator();
                Console.WriteLine(emu.Cartridge);
                emu.PPU.ProcessFrame();
                emu.PPU.ProcessFrame();
                emu.PPU.ProcessFrame();
                emu.PPU.ProcessFrame();
                ui.rawBitmap = emu.PPU.rawBitmap;
                ui.Invalidate();
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

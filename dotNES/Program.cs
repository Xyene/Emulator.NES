using System;

namespace dotNES
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            Emulator emu = new Emulator();
            Console.WriteLine(emu.Cartridge);
            //for (int i = 0; i < 10000; i++)
            //{
               emu.CPU.Execute();
            //}
        }
    }
}

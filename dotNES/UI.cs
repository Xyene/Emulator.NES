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
using System.Windows.Forms.Design;

namespace dotNES
{
    public partial class UI : Form
    {
        private bool rendererRunning = true;
        private Thread renderer;
        private NES001Controller controller = new NES001Controller();

        class SeparatorItem : MenuItem
        {
            public SeparatorItem() : base("-") { }
        }

        class Item : MenuItem
        {
            public Item(string title, Action<Item> build = null) : base(title)
            {
                build?.Invoke(this);
            }

            public void Add(MenuItem item) => MenuItems.Add(item);
        }

        class RadioItem : Item
        {
            public RadioItem(string title, Action<Item> build = null) : base(title, build)
            {
                RadioCheck = true;
            }
        }

        private string[] filters = { "Linear", "Bilinear" };
        private string activeFilter = "Linear";
        private string[] speeds = { "1x", "2x", "4x", "8x", "16x" };
        private string activeSpeed = "1x";
        private string[] sizes = { "1x", "2x", "4x", "8x" };
        private string activeSize = "2x";

        public UI()
        {
            InitializeComponent();
        }

        private void UI_Load(object sender, EventArgs e)
        {
            renderer = new Thread(() =>
            {
                string[] args = Environment.GetCommandLineArgs();
                string rom = args.Length > 1 ? args[1] : @"C:\dev\nes\Emulator-.NES\senjou-no-ookami.nes";
                Emulator emu = new Emulator(rom, controller);
                Console.WriteLine(emu.Cartridge);
                Stopwatch s = new Stopwatch();
                Stopwatch s0 = new Stopwatch();
                while (rendererRunning)
                {
                    s.Restart();
                    for (int i = 0; i < 60; i++)
                    {
                        s0.Restart();
                        emu.PPU.ProcessFrame();
                        rawBitmap = emu.PPU.rawBitmap;
                        Invoke((MethodInvoker)Draw);
                        s0.Stop();
                        Thread.Sleep(Math.Max((int)(980 / 60.0 - s0.ElapsedMilliseconds), 0));
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

        private void UI_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            ContextMenu cm = new ContextMenu
            {
                MenuItems =
            {
                new Item("Renderer", self =>
                {
                    self.Add(new RadioItem("Direct3D")
                    {
                        Checked = true
                    });
                }),
                new Item("Filter", x =>
                {
                    foreach (var filter in filters)
                        x.Add(new RadioItem(filter, y =>
                        {
                            y.Checked = filter == activeFilter;
                            y.Click += delegate { activeFilter = filter; };
                        }));
                }),
                new SeparatorItem(),
                new Item("&Pause"),
                new Item("Speed", x =>
                {
                    foreach (var speed in speeds)
                        x.Add(new RadioItem(speed, y =>
                        {
                            y.Checked = speed == activeSpeed;
                            y.Click += delegate { activeSpeed = speed; };
                        }));
                }),
                new Item("&Mute"),
                new Item("Volume")
                {
                    MenuItems = {"TODO"}
                },
                new Item("&Fullscreen"),
                new Item("Magnification", x =>
                {
                    foreach (var size in sizes)
                    {
                        x.Add(new RadioItem(size, y =>
                        {
                            y.Checked = size == activeSize;
                            y.Click += delegate { activeSize = size; };
                        }));
                    }
                }),
                new Item("&Reset..."),
                new Item("Keybindings...")
            }
            };
            cm.Show(this, new Point(e.X, e.Y));
        }
    }
}

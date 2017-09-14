using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using dotNES.Controllers;
using dotNES.Drawers;

namespace dotNES
{
    public partial class UI : Form
    {
        private bool _rendererRunning = true;
        private Thread _renderer;
        private IController _controller = new NES001Controller();

        public const int GameWidth = 256;
        public const int GameHeight = 240;
        public uint[] rawBitmap = new uint[GameWidth * GameHeight];
        public bool ready;
        public IRenderer renderer;

        public enum FilterMode
        {
            NearestNeighbor, Linear
        }

        public FilterMode _filterMode = FilterMode.Linear;

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

        private string[] speeds = { "1x", "2x", "4x", "8x", "16x" };
        private string activeSpeed = "1x";
        private string[] sizes = { "1x", "2x", "4x", "8x" };
        private string activeSize = "2x";
        private Emulator emu;
        private bool suspended;
        public bool gameStarted;

        public UI()
        {
            InitializeComponent();
            renderer = new SoftwareRenderer();

            Controls.Add(renderer);
            renderer.Dock = DockStyle.Fill;
            renderer.TabStop = false;
            renderer.InitRendering(this);
        }

        private void BootCartridge(string rom)
        {
            emu = new Emulator(rom, _controller);
            _renderer = new Thread(() =>
            {
                gameStarted = true;
                Console.WriteLine(emu.Cartridge);
                Stopwatch s = new Stopwatch();
                Stopwatch s0 = new Stopwatch();
                while (_rendererRunning)
                {
                    if (suspended)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    s.Restart();
                    for (int i = 0; i < 60 && !suspended; i++)
                    {
                        s0.Restart();
                        emu.PPU.ProcessFrame();
                        rawBitmap = emu.PPU.RawBitmap;
                        Invoke((MethodInvoker)renderer.Draw);
                        s0.Stop();
                        Thread.Sleep(Math.Max((int)(980 / 60.0 - s0.ElapsedMilliseconds), 0));
                    }
                    s.Stop();
                    Console.WriteLine($"60 frames in {s.ElapsedMilliseconds}ms");
                }
            });
            _renderer.Start();
        }

        private void UI_Load(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
                BootCartridge(args[1]);
        }

        private void Screenshot()
        {
            var bitmap = new Bitmap(GameWidth, GameHeight, PixelFormat.Format32bppArgb);

            for (int y = 0; y < GameHeight; y++)
            {
                for (int x = 0; x < GameWidth; x++)
                {
                    bitmap.SetPixel(x, y, Color.FromArgb((int)(rawBitmap[y * GameWidth + x] | 0xff000000)));
                }
            }

            Clipboard.SetImage(bitmap);
        }

        private void UI_FormClosing(object sender, FormClosingEventArgs e)
        {
            _rendererRunning = false;
            _renderer?.Abort();
            emu?.Save();
        }

        private void UI_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F12:
                    Screenshot();
                    break;
                case Keys.F2:
                    suspended = false;
                    break;
                case Keys.F3:
                    suspended = true;
                    break;
                default:
                    _controller.PressKey(e);
                    break;
            }
        }

        private void UI_KeyUp(object sender, KeyEventArgs e)
        {
            _controller.ReleaseKey(e);
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
                    var filters = new Dictionary<string, FilterMode>()
                    {
                        {"None", FilterMode.NearestNeighbor},
                        {"Linear", FilterMode.Linear},
                    };
                    foreach (var filter in filters)
                        x.Add(new RadioItem(filter.Key, y =>
                        {
                            y.Checked = filter.Value == _filterMode;
                            y.Click += delegate { _filterMode = filter.Value; };
                        }));
                }),
                new SeparatorItem(),
                new Item("&Screenshot (F12)", x =>
                {
                    x.Click += delegate { Screenshot(); };
                }),
                new Item(suspended ? "&Play (F2)" : "&Pause (F3)", x =>
                {
                    x.Click += delegate { suspended ^= true; };
                }),
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

        private void UI_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                BootCartridge(files[0]);
                AllowDrop = false;
                try
                {
                }
                catch (Exception)
                {
                    MessageBox.Show("Error loading ROM file; either corrupt or unsupported");
                }
            }
        }

        private void UI_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void UI_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }
    }
}

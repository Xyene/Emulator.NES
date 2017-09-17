using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using dotNES.Controllers;
using dotNES.Renderers;

namespace dotNES
{
    public partial class UI : Form
    {
        private bool _rendererRunning = true;
        private Thread _renderThread;
        private IController _controller = new NES001Controller();

        public const int GameWidth = 256;
        public const int GameHeight = 240;
        public uint[] rawBitmap = new uint[GameWidth * GameHeight];
        public bool ready;
        public IRenderer _renderer;

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

        private int[] speeds = { 1, 2, 4, 8, 16 };
        private int activeSpeed = 1;
        private string[] sizes = { "1x", "2x", "4x", "8x" };
        private string activeSize = "2x";
        private Emulator emu;
        private bool suspended;
        public bool gameStarted;

        private Type[] possibleRenderers = { typeof(SoftwareRenderer), /* typeof(OpenGLRenderer),  */ typeof(Direct3DRenderer) };
        private List<IRenderer> availableRenderers = new List<IRenderer>();

        public UI()
        {
            InitializeComponent();

            FindRenderers();
            SetRenderer(availableRenderers.Last());
        }

        private void SetRenderer(IRenderer renderer)
        {
            if (_renderer == renderer) return;

            if (_renderer != null)
            {
                var oldCtrl = (Control)renderer;
                oldCtrl.MouseClick -= UI_MouseClick;
                oldCtrl.KeyUp -= UI_KeyUp;
                oldCtrl.KeyDown -= UI_KeyDown;
                oldCtrl.PreviewKeyDown -= UI_PreviewKeyDown;
                _renderer.EndRendering();
                Controls.Remove((Control)_renderer);
            }
            _renderer = renderer;
            var ctrl = (Control)renderer;
            ctrl.Dock = DockStyle.Fill;
            ctrl.TabStop = false;
            ctrl.MouseClick += UI_MouseClick;
            ctrl.KeyUp += UI_KeyUp;
            ctrl.KeyDown += UI_KeyDown;
            ctrl.PreviewKeyDown += UI_PreviewKeyDown;
            Controls.Add(ctrl);
            renderer.InitRendering(this);
        }

        private void FindRenderers()
        {
            foreach (var renderType in possibleRenderers)
            {
                try
                {
                    var renderer = (IRenderer)Activator.CreateInstance(renderType);
                    renderer.InitRendering(this);
                    renderer.EndRendering();
                    availableRenderers.Add(renderer);
                }
                catch (Exception)
                {
                    Console.WriteLine($"{renderType} failed to initialize");
                }
            }
        }

        private void BootCartridge(string rom)
        {
            emu = new Emulator(rom, _controller);
            _renderThread = new Thread(() =>
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
                        Invoke((MethodInvoker)_renderer.Draw);
                        s0.Stop();
                        Thread.Sleep(Math.Max((int)(980 / 60.0 - s0.ElapsedMilliseconds), 0) / activeSpeed);
                    }
                    s.Stop();
                    Console.WriteLine($"60 frames in {s.ElapsedMilliseconds}ms");
                }
            });
            _renderThread.Start();
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
            _renderThread?.Abort();
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
                        foreach (var renderer in availableRenderers)
                        {
                            self.Add(new RadioItem(renderer.RendererName, y => {
                                y.Checked = renderer == _renderer;
                                y.Click += delegate { SetRenderer(renderer); };
                            }));
                        }
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
                    new Item("Screenshot (F12)", x =>
                    {
                        x.Click += delegate { Screenshot(); };
                    }),
                    new Item(suspended ? "&Play (F2)" : "&Pause (F3)", x =>
                    {
                        x.Click += delegate { suspended ^= true; };
                    }),
                    new Item("&Speed", x =>
                    {
                        foreach (var speed in speeds)
                            x.Add(new RadioItem($"{speed}x", y =>
                            {
                                y.Checked = speed == activeSpeed;
                                y.Click += delegate { activeSpeed = speed; };
                            }));
                    }),
                    new Item("&Reset..."),
                }
            };
            cm.Show(this, new Point(e.X, e.Y));
        }

        private void UI_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                try
                {
                    BootCartridge(files[0]);
                    AllowDrop = false;
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Windows.Forms;

using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.DXGI.Factory;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace dotNES
{
    partial class UI
    {
        Device device;
        SwapChain swapChain;
        RenderTarget d2dRenderTarget;
        Bitmap gameBitmap;

        const int GameWidth = 256, GameHeight = 240;
        public uint[] rawBitmap = new uint[GameWidth * GameHeight];

        public void InitRendering()
        {
            ResizeRedraw = true;
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(ClientSize.Width, ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device.CreateWithSwapChain(DriverType.Hardware,
                                       DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug,
                                       new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 },
                                       desc,
                                       out device,
                                       out swapChain);

            var d2dFactory = new SharpDX.Direct2D1.Factory();
            
            Factory factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAll);
            
            Texture2D backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            
            Surface surface = backBuffer.QueryInterface<Surface>();

            d2dRenderTarget = new RenderTarget(d2dFactory, surface,
                                                            new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));

            var bitmapProperties = new BitmapProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Ignore));
            gameBitmap = new Bitmap(d2dRenderTarget, new Size2(GameWidth, GameHeight), bitmapProperties);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            InitRendering();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            d2dRenderTarget.BeginDraw();
            d2dRenderTarget.Clear(Color.Transparent);

            int stride = GameWidth * 4;
            gameBitmap.CopyFromMemory(rawBitmap, stride);

            d2dRenderTarget.DrawBitmap(gameBitmap, new RawRectangleF()
            {
                Left = 0,
                Top = 0,
                Bottom = ClientSize.Height,
                Right = ClientSize.Width
            }, 1f, BitmapInterpolationMode.NearestNeighbor);

            d2dRenderTarget.EndDraw();
            swapChain.Present(0, PresentFlags.None);
            base.OnPaint(e);
        }
    }
}

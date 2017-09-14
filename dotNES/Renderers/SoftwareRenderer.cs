using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace dotNES.Renderers
{
    class SoftwareRenderer : Control, IRenderer
    {
        private UI _ui;
        private Bitmap _gameBitmap;

        public string RendererName => "Software";

        public void InitRendering(UI ui)
        {
            if (ui == null) return;
            _ui = ui;

            BackColor = Color.Gray;
            DoubleBuffered = true;
        }

        public void EndRendering()
        {
        }

        protected override void OnResize(EventArgs e)
        {
            InitRendering(_ui);
            base.OnResize(e);
        }

        public void Draw()
        {
            var locked = GCHandle.Alloc(_ui.rawBitmap, GCHandleType.Pinned);

            _gameBitmap?.Dispose();
            _gameBitmap = new Bitmap(UI.GameWidth, UI.GameHeight, UI.GameWidth * 4, PixelFormat.Format32bppPArgb, locked.AddrOfPinnedObject());

            Invalidate();
            Update();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_gameBitmap == null || _ui == null || !_ui.gameStarted) return;

            Graphics _renderTarget = e.Graphics;
            _renderTarget.CompositingMode = CompositingMode.SourceCopy;
            _renderTarget.InterpolationMode = _ui._filterMode == UI.FilterMode.Linear ? InterpolationMode.Bilinear : InterpolationMode.NearestNeighbor;
            _renderTarget.DrawImage(_gameBitmap, 0, 0, Size.Width, Size.Height);
        }
    }
}

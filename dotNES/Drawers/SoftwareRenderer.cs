using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace dotNES.Drawers
{
    class SoftwareRenderer : IRenderer
    {
        private UI _ui;
        private Bitmap _gameBitmap;

        public override void InitRendering(UI ui)
        {
            if (ui == null) return;
            _ui = ui;

            _gameBitmap = new Bitmap(UI.GameWidth, UI.GameHeight, PixelFormat.Format24bppRgb);

            BackColor = Color.Gray;
            DoubleBuffered = true;
        }

        protected override void OnResize(EventArgs e)
        {
            InitRendering(_ui);
            base.OnResize(e);
        }

        public override unsafe void Draw()
        {
            BitmapData _frameData = _gameBitmap.LockBits(new Rectangle(0, 0, 256, 240), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            byte* ptr = (byte*)_frameData.Scan0;

            int bufferPos = 0;
            for (int y = 0; y < UI.GameHeight; y++)
                for (int x = 0; x < UI.GameWidth; x++)
                {
                    uint raw = _ui.rawBitmap[bufferPos / 3];
                    ptr[bufferPos + 0] = (byte)((raw >> 0) & 0xFF);
                    ptr[bufferPos + 1] = (byte)((raw >> 8) & 0xFF);
                    ptr[bufferPos + 2] = (byte)((raw >> 16) & 0xFF);
                    
                    bufferPos += 3;
                }
            _gameBitmap.UnlockBits(_frameData);
            Invalidate();
            Update();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_ui == null || !_ui.gameStarted) return;

            Graphics _renderTarget = e.Graphics;
            _renderTarget.CompositingMode = CompositingMode.SourceCopy;
            _renderTarget.InterpolationMode = _ui._filterMode == UI.FilterMode.Linear ? InterpolationMode.Bilinear : InterpolationMode.NearestNeighbor;
            _renderTarget.DrawImage(_gameBitmap, 0, 0, Size.Width, Size.Height);
        }
    }
}

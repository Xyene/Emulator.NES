using System.Windows.Forms;

namespace dotNES.Drawers
{
    public abstract class IRenderer : Control
    {
        public abstract string RendererName { get; }

        public abstract void Draw();

        public abstract void InitRendering(UI ui);

        public abstract void EndRendering();
    }
}

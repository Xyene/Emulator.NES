using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dotNES.Drawers
{
    public abstract class IRenderer : Panel
    {
        public abstract void Draw();
        public abstract void InitRendering(UI ui);
    }
}

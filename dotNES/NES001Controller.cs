using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dotNES
{
    class NES001Controller
    {
        private int data;
        private int serialData;
        private bool strobing;

        // bit:   	 7     6     5     4     3     2     1     0
        // button:	 A B  Select Start  Up Down  Left 

        private Dictionary<Keys, int> keyMapping = new Dictionary<Keys, int>()
        {
            {Keys.A, 7},
            {Keys.B, 6},
            {Keys.R, 5},
            {Keys.T, 4},
            {Keys.Up, 3},
            {Keys.Down, 2},
            {Keys.Left, 1},
            {Keys.Right, 0},
        };

        public void Strobe(bool on)
        {
            serialData = data;
            strobing = on;
        }

        public int ReadState()
        {
            int ret = ((serialData & 0x80) > 0).AsByte();
            if (!strobing)
            {
                serialData <<= 1;
                serialData &= 0xFF;
            }
            return ret;
        }

        public void PressKey(KeyEventArgs e)
        {
            if (!keyMapping.ContainsKey(e.KeyCode)) return;
            data |= (1 << keyMapping[e.KeyCode]);
            Console.WriteLine(data.ToString("X4"));
        }

        public void ReleaseKey(KeyEventArgs e)
        {
            if (!keyMapping.ContainsKey(e.KeyCode)) return;
            data &= ~(1 << keyMapping[e.KeyCode]);
        }
    }
}

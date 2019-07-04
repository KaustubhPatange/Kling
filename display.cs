using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kling
{
    public partial class display : Form
    {
        int WM_NCHITTEST = 0x84, HTTRANSPARENT = -1;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)WM_NCHITTEST)
                m.Result = (IntPtr)HTTRANSPARENT;
            else
                base.WndProc(ref m);
        }
        public void SetText(string Text)
        {
            label1.Text = Text;
        }
        public display()
        {
            InitializeComponent();
            AllowTransparency = true;
            Opacity = 0.6;
            ShowInTaskbar = false;
            TopMost = true;
        }
    }
}

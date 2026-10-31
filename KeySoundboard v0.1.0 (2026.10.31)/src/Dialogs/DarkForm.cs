using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal class DarkForm : Form
    {
        public DarkForm()
        {
            BackColor = Theme.Background;
            ForeColor = Theme.Text;
            Font = Theme.FontBody;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;
            Padding = new Padding(0);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            NativeMethods.EnableDarkTitleBar(Handle);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
        }
    }
}

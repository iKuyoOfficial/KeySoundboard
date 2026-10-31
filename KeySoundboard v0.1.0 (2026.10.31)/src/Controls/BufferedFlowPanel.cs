using System;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class BufferedFlowPanel : FlowLayoutPanel
    {
        public BufferedFlowPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw, true);
            FlowDirection = FlowDirection.LeftToRight;
            WrapContents = true;
            AutoScroll = true;
            BackColor = Theme.Background;
            Margin = new Padding(0);
            Padding = new Padding(0);
        }
    }
}

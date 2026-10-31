using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class StatusPill : Control
    {
        private Color dotColor = Theme.Success;

        public StatusPill()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Font = Theme.FontSmallBold;
            Size = new Size(142, 28);
            TabStop = false;
        }

        public void SetStatus(string text, Color color)
        {
            Text = text ?? "";
            dotColor = color;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme.Prepare(e.Graphics);
            Color parentColor = Parent != null ? Parent.BackColor : Theme.Background;
            if (parentColor.A < 255) parentColor = Theme.Background;
            e.Graphics.Clear(parentColor);

            RectangleF bounds = new RectangleF(0.5f, 0.5f, Width - 1f, Height - 1f);
            using (var brush = new SolidBrush(Theme.Alpha(dotColor, 22)))
            {
                Theme.FillRounded(e.Graphics, brush, bounds, bounds.Height / 2f);
            }
            using (var pen = new Pen(Theme.Alpha(dotColor, 80), 1f))
            {
                Theme.DrawRounded(e.Graphics, pen, bounds, bounds.Height / 2f);
            }

            float dotSize = Theme.Px(this, 6f);
            float dotX = Theme.Px(this, 11);
            float dotY = (Height - dotSize) / 2f;
            using (var brush = new SolidBrush(dotColor))
            {
                e.Graphics.FillEllipse(brush, dotX, dotY, dotSize, dotSize);
            }

            Rectangle textRect = new Rectangle(
                (int)Math.Round(dotX + dotSize + Theme.Px(this, 7)),
                0,
                Width - (int)Math.Round(dotX + dotSize + Theme.Px(this, 12)),
                Height);
            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                textRect,
                Theme.Blend(dotColor, Color.White, 0.35f),
                TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }
    }
}

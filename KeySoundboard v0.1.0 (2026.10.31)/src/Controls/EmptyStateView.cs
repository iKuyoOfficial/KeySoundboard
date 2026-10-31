using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class EmptyStateView : Control
    {
        private readonly Font iconFont;
        private bool hovered;

        public event EventHandler AddRequested;

        public EmptyStateView()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.Selectable, true);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            TabStop = true;
            Size = new Size(800, 330);
            iconFont = new Font("Segoe UI Symbol", 30f, FontStyle.Regular, GraphicsUnit.Point);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme.Prepare(e.Graphics);
            Color parentColor = Parent != null ? Parent.BackColor : Theme.Background;
            if (parentColor.A < 255) parentColor = Theme.Background;
            e.Graphics.Clear(parentColor);

            RectangleF bounds = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            Color background = hovered ? Theme.Blend(Theme.Surface, Theme.AccentDim, 0.55f) : Theme.Surface;
            Color border = hovered ? Theme.BorderHover : Theme.Border;

            using (var brush = new SolidBrush(background))
            {
                Theme.FillRounded(e.Graphics, brush, bounds, Theme.Px(this, 14));
            }
            using (var pen = new Pen(border, 1f))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                Theme.DrawRounded(e.Graphics, pen, bounds, Theme.Px(this, 14));
            }

            int circle = Theme.Px(this, 68);
            RectangleF circleRect = new RectangleF((Width - circle) / 2f, Theme.Px(this, 44), circle, circle);
            using (var brush = new SolidBrush(Theme.AccentDim))
            {
                e.Graphics.FillEllipse(brush, circleRect);
            }
            using (var pen = new Pen(Theme.AccentSoft, Theme.Px(this, 1.4f)))
            {
                e.Graphics.DrawEllipse(pen, circleRect);
            }

            TextRenderer.DrawText(
                e.Graphics,
                "♪",
                iconFont,
                Rectangle.Round(circleRect),
                Theme.Accent,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

            TextRenderer.DrawText(
                e.Graphics,
                "还没有音效按钮",
                Theme.FontHeading,
                new Rectangle(0, Theme.Px(this, 126), Width, Theme.Px(this, 34)),
                Theme.Text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

            TextRenderer.DrawText(
                e.Graphics,
                "点击“添加音效”选择音频文件，再为它设置一个按键。\r\n游戏在前台时，按下的按键也能触发播放。",
                Theme.FontBody,
                new Rectangle(Theme.Px(this, 40), Theme.Px(this, 168), Width - Theme.Px(this, 80), Theme.Px(this, 54)),
                Theme.TextMuted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top |
                TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);

            RectangleF cta = new RectangleF(
                (Width - Theme.Px(this, 190)) / 2f,
                Theme.Px(this, 240),
                Theme.Px(this, 190),
                Theme.Px(this, 38));
            using (var brush = new SolidBrush(hovered ? Theme.Accent : Theme.AccentDim))
            {
                Theme.FillRounded(e.Graphics, brush, cta, Theme.Px(this, 9));
            }
            using (var pen = new Pen(hovered ? Theme.AccentHover : Theme.AccentSoft, 1f))
            {
                Theme.DrawRounded(e.Graphics, pen, cta, Theme.Px(this, 9));
            }
            TextRenderer.DrawText(
                e.Graphics,
                "＋  添加第一个音效",
                Theme.FontBodyBold,
                Rectangle.Round(cta),
                hovered ? Color.White : Theme.Blend(Theme.Accent, Color.White, 0.3f),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

            if (Focused)
            {
                ControlPaint.DrawFocusRectangle(
                    e.Graphics,
                    new Rectangle(6, 6, Width - 12, Height - 12),
                    Theme.Text,
                    background);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            hovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hovered = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left) Focus();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            RequestAdd();
        }

        private void RequestAdd()
        {
            EventHandler handler = AddRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                RequestAdd();
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Space || keyData == Keys.Enter) return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) iconFont.Dispose();
            base.Dispose(disposing);
        }
    }
}

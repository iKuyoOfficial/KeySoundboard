using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class AddCard : Control
    {
        private bool hovered;
        private bool pressed;

        public event EventHandler AddRequested;

        public AddCard()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.Selectable, true);
            BackColor = Color.Transparent;
            Font = Theme.FontCardTitle;
            Cursor = Cursors.Hand;
            TabStop = true;
            Size = new Size(208, 124);
            AccessibleRole = AccessibleRole.PushButton;
        }

        protected override Size DefaultSize
        {
            get { return new Size(208, 124); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme.Prepare(e.Graphics);
            Color parentColor = Parent != null ? Parent.BackColor : Theme.Background;
            if (parentColor.A < 255) parentColor = Theme.Background;
            e.Graphics.Clear(parentColor);

            RectangleF bounds = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            int radius = Theme.Px(this, 12);
            Color background = pressed ? Theme.SurfacePressed : (hovered ? Theme.Blend(Theme.Surface, Theme.AccentDim, 0.55f) : Theme.Surface);
            Color border = hovered ? Theme.Accent : Theme.Border;

            using (var brush = new SolidBrush(background))
            {
                Theme.FillRounded(e.Graphics, brush, bounds, radius);
            }
            using (var pen = new Pen(border, Theme.Px(this, hovered ? 1.6f : 1f)))
            {
                pen.DashStyle = hovered ? DashStyle.Solid : DashStyle.Dash;
                Theme.DrawRounded(e.Graphics, pen, bounds, radius);
            }

            int plusBox = Theme.Px(this, 32);
            RectangleF plusRect = new RectangleF(
                (Width - plusBox) / 2f,
                Theme.Px(this, 28),
                plusBox,
                plusBox);
            using (var brush = new SolidBrush(hovered ? Theme.Accent : Theme.AccentDim))
            {
                Theme.FillRounded(e.Graphics, brush, plusRect, Theme.Px(this, 9));
            }

            using (var pen = new Pen(hovered ? Color.White : Theme.Accent, Theme.Px(this, 2f)))
            {
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                float cx = plusRect.X + plusRect.Width / 2f;
                float cy = plusRect.Y + plusRect.Height / 2f;
                float arm = Theme.Px(this, 7f);
                e.Graphics.DrawLine(pen, cx - arm, cy, cx + arm, cy);
                e.Graphics.DrawLine(pen, cx, cy - arm, cx, cy + arm);
            }

            TextRenderer.DrawText(
                e.Graphics,
                "添加音效",
                Font,
                new Rectangle(0, Theme.Px(this, 70), Width, Theme.Px(this, 24)),
                hovered ? Theme.Text : Theme.Blend(Theme.Text, Theme.TextMuted, 0.25f),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

            TextRenderer.DrawText(
                e.Graphics,
                "选择音频文件",
                Theme.FontSmall,
                new Rectangle(0, Theme.Px(this, 92), Width, Theme.Px(this, 20)),
                Theme.TextMuted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

            if (Focused)
            {
                ControlPaint.DrawFocusRectangle(
                    e.Graphics,
                    new Rectangle(4, 4, Width - 8, Height - 8),
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
            pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            pressed = true;
            Focus();
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left) return;
            bool activate = pressed && ClientRectangle.Contains(e.Location);
            pressed = false;
            Invalidate();
            if (activate) RequestAdd();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
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
    }
}

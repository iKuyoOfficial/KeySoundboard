using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal enum AccentButtonStyle
    {
        Primary,
        Secondary,
        Danger,
        Ghost
    }

    internal sealed class AccentButton : Control, IButtonControl
    {
        private bool hovered;
        private bool pressed;
        private bool isDefault;
        private AccentButtonStyle style = AccentButtonStyle.Secondary;
        private DialogResult dialogResult = DialogResult.None;

        public AccentButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.Selectable, true);
            BackColor = Color.Transparent;
            Font = Theme.FontBodyBold;
            Cursor = Cursors.Hand;
            TabStop = true;
            Size = new Size(96, 34);
        }

        public AccentButtonStyle Style
        {
            get { return style; }
            set
            {
                if (style == value) return;
                style = value;
                Invalidate();
            }
        }

        public int CornerRadius { get; set; }

        public DialogResult DialogResult
        {
            get { return dialogResult; }
            set { dialogResult = value; }
        }

        protected override Size DefaultSize
        {
            get { return new Size(96, 34); }
        }

        public void NotifyDefault(bool value)
        {
            if (isDefault == value) return;
            isDefault = value;
            Invalidate();
        }

        public void PerformClick()
        {
            if (!Enabled || !Visible) return;
            OnClick(EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme.Prepare(e.Graphics);
            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = CornerRadius > 0 ? Theme.Px(this, CornerRadius) : Theme.Px(this, 9);
            Color parentColor = Parent != null ? Parent.BackColor : Theme.Background;
            if (parentColor.A < 255) parentColor = Theme.Background;
            e.Graphics.Clear(parentColor);

            Color back;
            Color border;
            Color foreground;

            switch (style)
            {
                case AccentButtonStyle.Primary:
                    back = pressed ? Theme.Blend(Theme.Accent, Color.Black, 0.18f) : (hovered ? Theme.AccentHover : Theme.Accent);
                    border = back;
                    foreground = Color.White;
                    break;
                case AccentButtonStyle.Danger:
                    back = pressed ? Theme.Blend(Theme.Danger, Color.Black, 0.12f) : (hovered ? Theme.Danger : Theme.DangerDim);
                    border = Theme.Danger;
                    foreground = hovered || pressed ? Color.White : Theme.Danger;
                    break;
                case AccentButtonStyle.Ghost:
                    back = pressed ? Theme.SurfacePressed : (hovered ? Theme.SurfaceHover : parentColor);
                    border = hovered ? Theme.BorderHover : parentColor;
                    foreground = hovered ? Theme.Text : Theme.TextMuted;
                    break;
                default:
                    back = pressed ? Theme.SurfacePressed : (hovered ? Theme.SurfaceHover : Theme.Surface);
                    border = hovered ? Theme.BorderHover : Theme.Border;
                    foreground = Theme.Text;
                    break;
            }

            if (!Enabled)
            {
                back = Theme.Blend(back, Theme.Background, 0.55f);
                border = Theme.Blend(border, Theme.Background, 0.55f);
                foreground = Theme.TextDim;
            }

            using (var brush = new SolidBrush(back))
            {
                Theme.FillRounded(e.Graphics, brush, bounds, radius);
            }

            if (border != parentColor && border != back)
            {
                using (var pen = new Pen(border, Theme.Px(this, 1f)))
                {
                    Theme.DrawRounded(e.Graphics, pen, new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), radius);
                }
            }

            if (isDefault && Enabled && style != AccentButtonStyle.Primary)
            {
                using (var pen = new Pen(Theme.Accent, Theme.Px(this, 1.4f)))
                {
                    Theme.DrawRounded(e.Graphics, pen, new RectangleF(1.2f, 1.2f, Width - 2.4f, Height - 2.4f), radius);
                }
            }

            TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                                    TextFormatFlags.VerticalCenter |
                                    TextFormatFlags.SingleLine |
                                    TextFormatFlags.EndEllipsis |
                                    TextFormatFlags.NoPrefix;
            Rectangle textRect = new Rectangle(Theme.Px(this, 6), 0, Width - Theme.Px(this, 12), Height);
            TextRenderer.DrawText(e.Graphics, Text, Font, textRect, foreground, flags);

            if (Focused && ShowFocusCues)
            {
                Rectangle focus = new Rectangle(3, 3, Width - 6, Height - 6);
                ControlPaint.DrawFocusRectangle(e.Graphics, focus, foreground, back);
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
            bool click = pressed && ClientRectangle.Contains(e.Location);
            pressed = false;
            Invalidate();
            if (click) PerformClick();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                PerformClick();
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

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }
    }
}

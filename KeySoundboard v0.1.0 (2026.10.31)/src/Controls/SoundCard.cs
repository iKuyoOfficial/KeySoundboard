using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class SoundCard : Control
    {
        private readonly AudioEngine engine;
        private readonly Timer flashTimer;
        private SoundButtonConfig config;
        private SoundSlot slot;
        private bool hovered;
        private bool pressed;
        private bool moreHovered;
        private bool flash;

        public event EventHandler PlayRequested;
        public event EventHandler MenuRequested;

        public SoundCard(AudioEngine engine, SoundButtonConfig config)
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.Selectable, true);
            this.engine = engine;
            this.config = config;
            BackColor = Color.Transparent;
            Font = Theme.FontBody;
            Cursor = Cursors.Hand;
            TabStop = true;
            Size = new Size(208, 124);
            AccessibleRole = AccessibleRole.PushButton;

            flashTimer = new Timer { Interval = 230 };
            flashTimer.Tick += FlashTimerTick;
            AttachSlot();
        }

        public SoundButtonConfig Config
        {
            get { return config; }
        }

        public string ButtonId
        {
            get { return config != null ? config.Id : ""; }
        }

        public void SetConfig(SoundButtonConfig value)
        {
            config = value;
            AttachSlot();
            Invalidate();
        }

        private void AttachSlot()
        {
            if (slot != null)
            {
                slot.StateChanged -= SlotStateChanged;
                slot = null;
            }
            if (config == null) return;
            slot = engine.GetSlot(config.Id);
            if (slot != null) slot.StateChanged += SlotStateChanged;
        }

        private void SlotStateChanged(object sender, EventArgs e)
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new EventHandler(SlotStateChanged), sender, e);
                }
                catch
                {
                }
                return;
            }
            Invalidate();
        }

        public bool PlayNow()
        {
            if (config == null || engine == null) return false;
            bool played = engine.Play(config.Id);
            if (played) Flash();
            return played;
        }

        public void Flash()
        {
            flash = true;
            flashTimer.Stop();
            flashTimer.Start();
            Invalidate();
        }

        private void FlashTimerTick(object sender, EventArgs e)
        {
            flashTimer.Stop();
            flash = false;
            Invalidate();
        }

        private Rectangle MoreRect()
        {
            int size = Theme.Px(this, 26);
            return new Rectangle(Width - size - Theme.Px(this, 8), Theme.Px(this, 8), size, size);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme.Prepare(e.Graphics);
            Color parentColor = Parent != null ? Parent.BackColor : Theme.Background;
            if (parentColor.A < 255) parentColor = Theme.Background;
            e.Graphics.Clear(parentColor);

            RectangleF bounds = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            int radius = Theme.Px(this, 12);

            Color background = Theme.Surface;
            if (hovered) background = Theme.SurfaceHover;
            if (pressed) background = Theme.SurfacePressed;
            if (flash) background = Theme.Blend(Theme.Surface, Theme.AccentDim, 0.75f);

            using (var brush = new SolidBrush(background))
            {
                Theme.FillRounded(e.Graphics, brush, bounds, radius);
            }

            Color border = Theme.Border;
            float borderWidth = 1f;
            if (hovered) border = Theme.BorderHover;
            if (flash)
            {
                border = Theme.Accent;
                borderWidth = 1.8f;
            }
            if (Focused)
            {
                border = Theme.Accent;
                borderWidth = 1.6f;
            }

            using (var pen = new Pen(border, Theme.Px(this, borderWidth)))
            {
                Theme.DrawRounded(e.Graphics, pen, bounds, radius);
            }

            DrawKeyBadge(e.Graphics);
            DrawMoreButton(e.Graphics);
            DrawTexts(e.Graphics);
        }

        private void DrawKeyBadge(Graphics g)
        {
            bool bound = config.VirtualKey != 0;
            string keyText = bound ? KeyNames.Get(config.VirtualKey) : "未绑定";
            Size textSize = TextRenderer.MeasureText(
                g,
                keyText,
                Theme.FontBadge,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

            int width = Math.Max(Theme.Px(this, bound ? 46 : 64), textSize.Width + Theme.Px(this, 16));
            width = Math.Min(width, Theme.Px(this, 96));
            int height = Theme.Px(this, 24);
            RectangleF badge = new RectangleF(Theme.Px(this, 12), Theme.Px(this, 12), width, height);

            Color fill = bound ? Theme.AccentDim : Theme.SurfacePressed;
            Color stroke = bound ? Theme.AccentSoft : Theme.Border;
            Color textColor = bound ? Theme.Blend(Theme.Accent, Color.White, 0.25f) : Theme.TextDim;

            using (var brush = new SolidBrush(fill))
            {
                Theme.FillRounded(g, brush, badge, Theme.Px(this, 6));
            }
            using (var pen = new Pen(stroke, 1f))
            {
                Theme.DrawRounded(g, pen, badge, Theme.Px(this, 6));
            }

            Rectangle textRect = new Rectangle(
                (int)badge.X + 4,
                (int)badge.Y,
                (int)badge.Width - 8,
                (int)badge.Height);
            TextRenderer.DrawText(
                g,
                keyText,
                Theme.FontBadge,
                textRect,
                textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private void DrawMoreButton(Graphics g)
        {
            Rectangle rect = MoreRect();
            if (moreHovered)
            {
                using (var brush = new SolidBrush(Theme.SurfacePressed))
                {
                    Theme.FillRounded(g, brush, rect, Theme.Px(this, 6));
                }
            }

            string glyph = "···";
            TextRenderer.DrawText(
                g,
                glyph,
                Theme.FontBodyBold,
                rect,
                moreHovered ? Theme.Text : Theme.TextMuted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }

        private void DrawTexts(Graphics g)
        {
            string title = string.IsNullOrWhiteSpace(config.Name) ? "未命名音效" : config.Name.Trim();
            Rectangle titleRect = new Rectangle(
                Theme.Px(this, 14),
                Theme.Px(this, 44),
                Width - Theme.Px(this, 28),
                Theme.Px(this, 24));
            TextRenderer.DrawText(
                g,
                title,
                Theme.FontCardTitle,
                titleRect,
                Theme.Text,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            string status;
            Color statusColor = Theme.TextMuted;
            Color dotColor = Theme.TextDim;

            if (config == null || string.IsNullOrEmpty(config.File))
            {
                status = "点击 ··· 选择音频";
            }
            else if (slot == null)
            {
                status = "正在准备…";
                dotColor = Theme.Warning;
            }
            else if (!string.IsNullOrEmpty(slot.Error))
            {
                status = slot.Error;
                statusColor = Theme.Danger;
                dotColor = Theme.Danger;
            }
            else if (config.VirtualKey == 0)
            {
                status = "点 ··· 设置按键";
                dotColor = Theme.Warning;
            }
            else
            {
                string extension = Path.GetExtension(config.File);
                if (!string.IsNullOrEmpty(extension)) extension = extension.TrimStart('.').ToUpperInvariant();
                if (slot.Duration > TimeSpan.Zero)
                {
                    status = slot.FormatDuration() + "  ·  " + extension;
                    dotColor = Theme.Success;
                }
                else
                {
                    status = "正在加载…";
                    dotColor = Theme.Warning;
                }
            }

            int statusY = Height - Theme.Px(this, 34);
            int dotSize = Theme.Px(this, 6);
            float dotX = Theme.Px(this, 15);
            float dotY = statusY + Theme.Px(this, 7);
            using (var brush = new SolidBrush(dotColor))
            {
                g.FillEllipse(brush, dotX, dotY, dotSize, dotSize);
            }

            Rectangle statusRect = new Rectangle(
                Theme.Px(this, 27),
                statusY,
                Width - Theme.Px(this, 40),
                Theme.Px(this, 20));
            TextRenderer.DrawText(
                g,
                status,
                Theme.FontSmall,
                statusRect,
                statusColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool overMore = MoreRect().Contains(e.Location);
            if (overMore != moreHovered)
            {
                moreHovered = overMore;
                Invalidate();
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
            moreHovered = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Right)
            {
                Focus();
                RaiseMenu();
                return;
            }
            if (e.Button != MouseButtons.Left) return;
            pressed = true;
            Focus();
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left) return;
            bool wasPressed = pressed;
            pressed = false;
            Invalidate();
            if (!wasPressed || !ClientRectangle.Contains(e.Location)) return;
            if (MoreRect().Contains(e.Location)) RaiseMenu();
            else RaisePlay();
        }

        private void RaisePlay()
        {
            EventHandler handler = PlayRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void RaiseMenu()
        {
            EventHandler handler = MenuRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                RaisePlay();
            }
            else if (e.KeyCode == Keys.Apps)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                RaiseMenu();
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Space || keyData == Keys.Enter || keyData == Keys.Apps) return true;
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
            if (disposing)
            {
                flashTimer.Stop();
                flashTimer.Dispose();
                if (slot != null) slot.StateChanged -= SlotStateChanged;
            }
            base.Dispose(disposing);
        }
    }
}

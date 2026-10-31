using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class ToggleSwitch : Control
    {
        private readonly Timer animationTimer;
        private bool isChecked;
        private bool hovered;
        private float animation;
        private float animationTarget;

        public event EventHandler CheckedChanged;

        public ToggleSwitch()
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
            Size = new Size(46, 24);
            AccessibleRole = AccessibleRole.CheckButton;

            animationTimer = new Timer { Interval = 15 };
            animationTimer.Tick += AnimationTick;
        }

        public bool Checked
        {
            get { return isChecked; }
            set { SetChecked(value, true); }
        }

        public void SetChecked(bool value, bool raiseEvent)
        {
            if (isChecked == value) return;
            isChecked = value;
            animationTarget = value ? 1f : 0f;
            animationTimer.Start();
            if (raiseEvent)
            {
                EventHandler handler = CheckedChanged;
                if (handler != null) handler(this, EventArgs.Empty);
            }
            Invalidate();
        }

        protected override Size DefaultSize
        {
            get { return new Size(46, 24); }
        }

        private void AnimationTick(object sender, EventArgs e)
        {
            float distance = animationTarget - animation;
            if (Math.Abs(distance) < 0.03f)
            {
                animation = animationTarget;
                animationTimer.Stop();
            }
            else
            {
                animation += distance * 0.42f;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme.Prepare(e.Graphics);
            Color parentColor = Parent != null ? Parent.BackColor : Theme.Background;
            if (parentColor.A < 255) parentColor = Theme.Background;
            e.Graphics.Clear(parentColor);

            float trackHeight = Height - Theme.Px(this, 4);
            float trackWidth = Width - Theme.Px(this, 2);
            if (trackHeight < 8f || trackWidth < 16f) return;

            RectangleF track = new RectangleF(
                Theme.Px(this, 1),
                (Height - trackHeight) / 2f,
                trackWidth,
                trackHeight);

            Color offColor = hovered ? Theme.BorderHover : Theme.Border;
            Color trackColor = Theme.Blend(offColor, Theme.Accent, animation);
            if (!Enabled)
            {
                trackColor = Theme.Blend(trackColor, Theme.Background, 0.6f);
            }

            using (var brush = new SolidBrush(trackColor))
            {
                Theme.FillRounded(e.Graphics, brush, track, trackHeight / 2f);
            }

            float knobSize = trackHeight - Theme.Px(this, 4);
            float left = track.X + Theme.Px(this, 2);
            float right = track.Right - knobSize - Theme.Px(this, 2);
            float knobX = left + (right - left) * animation;
            float knobY = track.Y + (track.Height - knobSize) / 2f;

            using (var shadow = new SolidBrush(Theme.Alpha(Color.Black, 70)))
            {
                e.Graphics.FillEllipse(shadow, knobX + 0.6f, knobY + 1f, knobSize, knobSize);
            }

            Color knobColor = Enabled ? Color.FromArgb(245, 248, 255) : Color.FromArgb(160, 167, 180);
            using (var brush = new SolidBrush(knobColor))
            {
                e.Graphics.FillEllipse(brush, knobX, knobY, knobSize, knobSize);
            }

            if (Focused)
            {
                using (var pen = new Pen(Theme.Accent, Theme.Px(this, 1.2f)))
                {
                    Theme.DrawRounded(e.Graphics, pen, new RectangleF(0.6f, 0.6f, Width - 1.2f, Height - 1.2f), Height / 2f);
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            if (!Enabled) return;
            Checked = !Checked;
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                Checked = !Checked;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animationTimer.Stop();
                animationTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

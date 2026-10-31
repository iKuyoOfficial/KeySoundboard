using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class VolumeSlider : Control
    {
        private int sliderValue = 100;
        private bool dragging;
        private bool hovered;

        public event EventHandler ValueChanged;

        public VolumeSlider()
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
            Size = new Size(150, 26);
            AccessibleRole = AccessibleRole.Slider;
        }

        public int Value
        {
            get { return sliderValue; }
            set { SetValue(value, true); }
        }

        public void SetValue(int value, bool raiseEvent)
        {
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            if (value == sliderValue) return;
            sliderValue = value;
            if (raiseEvent)
            {
                EventHandler handler = ValueChanged;
                if (handler != null) handler(this, EventArgs.Empty);
            }
            Invalidate();
        }

        protected override Size DefaultSize
        {
            get { return new Size(150, 26); }
        }

        private int TrackMargin
        {
            get { return Theme.Px(this, 8); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme.Prepare(e.Graphics);
            Color parentColor = Parent != null ? Parent.BackColor : Theme.Background;
            if (parentColor.A < 255) parentColor = Theme.Background;
            e.Graphics.Clear(parentColor);

            float trackHeight = Theme.Px(this, 5f);
            float left = TrackMargin;
            float right = Width - TrackMargin;
            float trackWidth = right - left;
            if (trackWidth <= 0f) return;

            float trackY = (Height - trackHeight) / 2f;
            RectangleF backTrack = new RectangleF(left, trackY, trackWidth, trackHeight);
            using (var brush = new SolidBrush(hovered || dragging ? Theme.BorderHover : Theme.Border))
            {
                Theme.FillRounded(e.Graphics, brush, backTrack, trackHeight / 2f);
            }

            float amount = sliderValue / 100f;
            float filledWidth = trackWidth * amount;
            if (filledWidth > 0.5f)
            {
                RectangleF fill = new RectangleF(left, trackY, filledWidth, trackHeight);
                using (var brush = new SolidBrush(Enabled ? Theme.Accent : Theme.TextDim))
                {
                    Theme.FillRounded(e.Graphics, brush, fill, trackHeight / 2f);
                }
            }

            float thumbRadius = Theme.Px(this, hovered || dragging ? 8f : 7f);
            float thumbX = left + filledWidth;
            thumbX = Math.Max(left, Math.Min(right, thumbX));
            float thumbY = Height / 2f;
            Color thumbColor = Enabled ? Color.FromArgb(245, 248, 255) : Color.FromArgb(150, 156, 170);

            using (var shadow = new SolidBrush(Theme.Alpha(Color.Black, 90)))
            {
                e.Graphics.FillEllipse(shadow, thumbX - thumbRadius + 0.5f, thumbY - thumbRadius + 1.2f, thumbRadius * 2f, thumbRadius * 2f);
            }
            using (var brush = new SolidBrush(thumbColor))
            {
                e.Graphics.FillEllipse(brush, thumbX - thumbRadius, thumbY - thumbRadius, thumbRadius * 2f, thumbRadius * 2f);
            }
            using (var pen = new Pen(Theme.Accent, Theme.Px(this, 1.6f)))
            {
                e.Graphics.DrawEllipse(pen, thumbX - thumbRadius, thumbY - thumbRadius, thumbRadius * 2f, thumbRadius * 2f);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || !Enabled) return;
            Focus();
            dragging = true;
            Capture = true;
            SetFromPoint(e.X);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!dragging) return;
            SetFromPoint(e.X);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left) return;
            if (dragging)
            {
                SetFromPoint(e.X);
                dragging = false;
                Capture = false;
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
            Invalidate();
        }

        private void SetFromPoint(int x)
        {
            int left = TrackMargin;
            int width = Width - TrackMargin * 2;
            if (width <= 0) return;
            double ratio = (x - left) / (double)width;
            Value = (int)Math.Round(ratio * 100.0);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            int step = e.Shift ? 10 : 5;
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Down:
                    Value -= step;
                    e.Handled = true;
                    break;
                case Keys.Right:
                case Keys.Up:
                    Value += step;
                    e.Handled = true;
                    break;
                case Keys.PageDown:
                    Value -= 10;
                    e.Handled = true;
                    break;
                case Keys.PageUp:
                    Value += 10;
                    e.Handled = true;
                    break;
                case Keys.Home:
                    Value = 0;
                    e.Handled = true;
                    break;
                case Keys.End:
                    Value = 100;
                    e.Handled = true;
                    break;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.Home:
                case Keys.End:
                case Keys.PageUp:
                case Keys.PageDown:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!Enabled) return;
            Value += e.Delta > 0 ? 5 : -5;
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
            Invalidate();
        }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class KeyCaptureForm : DarkForm
    {
        private readonly KeyDisplay keyDisplay;
        private readonly Label hintLabel;
        private readonly AccentButton clearButton;
        private readonly AccentButton cancelButton;

        public int CapturedVirtualKey { get; private set; }

        public KeyCaptureForm(int currentKey)
        {
            CapturedVirtualKey = currentKey;
            Text = "设置按键";
            ClientSize = new Size(470, 318);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            KeyPreview = true;
            ImeMode = ImeMode.Disable;

            var title = new Label
            {
                Text = "设置按键",
                Font = Theme.FontHeading,
                ForeColor = Theme.Text,
                AutoSize = true,
                Location = new Point(30, 20)
            };
            Controls.Add(title);

            var subtitle = new Label
            {
                Text = "请按下你想使用的按键，按下后立即生效。",
                Font = Theme.FontBody,
                ForeColor = Theme.TextMuted,
                AutoSize = true,
                Location = new Point(30, 58)
            };
            Controls.Add(subtitle);

            keyDisplay = new KeyDisplay
            {
                Location = new Point(30, 92),
                Size = new Size(410, 92),
                ImeMode = ImeMode.Disable
            };
            keyDisplay.SetKey(currentKey);
            Controls.Add(keyDisplay);

            hintLabel = new Label
            {
                Text = "只支持单个按键，不支持组合键；Esc 取消。",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextMuted,
                Location = new Point(30, 196),
                Size = new Size(410, 38)
            };
            Controls.Add(hintLabel);

            clearButton = new AccentButton
            {
                Text = "清除按键",
                Style = AccentButtonStyle.Secondary,
                Size = new Size(110, 36),
                Location = new Point(220, 252)
            };
            clearButton.Click += ClearClicked;
            Controls.Add(clearButton);

            cancelButton = new AccentButton
            {
                Text = "取消",
                Style = AccentButtonStyle.Ghost,
                Size = new Size(92, 36),
                Location = new Point(340, 252),
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(cancelButton);
            CancelButton = cancelButton;

            Shown += delegate { Activate(); Focus(); };
        }

        private void ClearClicked(object sender, EventArgs e)
        {
            CapturedVirtualKey = 0;
            DialogResult = DialogResult.OK;
            Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return TryCapture(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled && TryCapture(e.KeyData))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            base.OnKeyDown(e);
        }

        private bool TryCapture(Keys keyData)
        {
            Keys code = keyData & Keys.KeyCode;
            Keys modifiers = keyData & Keys.Modifiers;
            AppLog.Write("Key capture: keyData=" + (int)keyData + " code=" + (int)code +
                         " modifiers=" + (int)modifiers + " bindable=" + KeyNames.IsBindable((int)code));

            if (code == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return true;
            }

            if (IsModifierKey(code))
            {
                hintLabel.ForeColor = Theme.Warning;
                hintLabel.Text = "这是修饰键，请再按一个普通按键。";
                return true;
            }

            if (modifiers != Keys.None)
            {
                hintLabel.ForeColor = Theme.Warning;
                hintLabel.Text = "不支持组合键，请只按一个键。";
                return true;
            }

            if (!KeyNames.IsBindable((int)code))
            {
                hintLabel.ForeColor = Theme.Warning;
                hintLabel.Text = "这个按键不能用作音效触发键，请换一个。";
                return true;
            }

            CapturedVirtualKey = (int)code;
            DialogResult = DialogResult.OK;
            Close();
            return true;
        }

        private static bool IsModifierKey(Keys key)
        {
            switch (key)
            {
                case Keys.ShiftKey:
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                case Keys.ControlKey:
                case Keys.LControlKey:
                case Keys.RControlKey:
                case Keys.Menu:
                case Keys.LMenu:
                case Keys.RMenu:
                case Keys.LWin:
                case Keys.RWin:
                    return true;
                default:
                    return false;
            }
        }
    }

    internal sealed class KeyDisplay : Control
    {
        private int virtualKey;

        public KeyDisplay()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        public void SetKey(int key)
        {
            virtualKey = key;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme.Prepare(e.Graphics);
            Color parentColor = Parent != null ? Parent.BackColor : Theme.Background;
            if (parentColor.A < 255) parentColor = Theme.Background;
            e.Graphics.Clear(parentColor);

            RectangleF bounds = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (var brush = new SolidBrush(Theme.Surface))
            {
                Theme.FillRounded(e.Graphics, brush, bounds, Theme.Px(this, 12));
            }

            bool bound = virtualKey != 0;
            using (var pen = new Pen(bound ? Theme.AccentSoft : Theme.Border, Theme.Px(this, 1.4f)))
            {
                Theme.DrawRounded(e.Graphics, pen, bounds, Theme.Px(this, 12));
            }

            string text = bound ? KeyNames.Get(virtualKey) : "等待按键…";
            Color color = bound ? Theme.Text : Theme.TextDim;
            Font font = Theme.FontKeyDisplay;

            Size measured = TextRenderer.MeasureText(
                e.Graphics,
                text,
                font,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            if (measured.Width > Width - Theme.Px(this, 30))
            {
                font = Theme.FontHeading;
            }

            TextRenderer.DrawText(
                e.Graphics,
                text,
                font,
                ClientRectangle,
                color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

            if (bound)
            {
                Rectangle bottom = new Rectangle(
                    Theme.Px(this, 12),
                    Height - Theme.Px(this, 28),
                    Width - Theme.Px(this, 24),
                    Theme.Px(this, 18));
                TextRenderer.DrawText(
                    e.Graphics,
                    "按键代码 VK " + virtualKey.ToString("X2"),
                    Theme.FontSmall,
                    bottom,
                    Theme.TextDim,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            }
        }
    }
}

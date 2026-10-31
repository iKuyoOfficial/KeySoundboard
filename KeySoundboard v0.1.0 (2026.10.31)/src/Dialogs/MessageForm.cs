using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class MessageForm : DarkForm
    {
        public MessageForm(string title, string message, bool error)
        {
            Text = title;
            int lines = 3 + (message == null ? 0 : message.Length / 52);
            int height = Math.Max(250, Math.Min(430, 150 + lines * 18));
            ClientSize = new Size(500, height);

            var box = new TextBox
            {
                Text = message ?? "",
                Font = Theme.FontBody,
                BackColor = Theme.Surface,
                ForeColor = error ? Theme.Blend(Theme.Danger, Theme.Text, 0.35f) : Theme.Text,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true,
                ReadOnly = true,
                WordWrap = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(26, 24),
                Size = new Size(448, height - 118)
            };
            Controls.Add(box);

            var okButton = new AccentButton
            {
                Text = "知道了",
                Style = error ? AccentButtonStyle.Danger : AccentButtonStyle.Primary,
                Size = new Size(108, 36),
                Location = new Point(366, height - 58)
            };
            okButton.Click += delegate
            {
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(okButton);
            AcceptButton = okButton;

            Shown += delegate
            {
                box.SelectionStart = 0;
                box.SelectionLength = 0;
                okButton.Focus();
            };
        }

        public static void ShowMessage(IWin32Window owner, string title, string message, bool error)
        {
            using (var form = new MessageForm(title, message, error))
            {
                form.ShowDialog(owner);
            }
        }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class ConfirmForm : DarkForm
    {
        public ConfirmForm(string title, string message, string confirmText, bool danger)
        {
            Text = title;
            ClientSize = new Size(440, 236);

            var messageLabel = new Label
            {
                Text = message,
                Font = Theme.FontBody,
                ForeColor = Theme.Text,
                Location = new Point(26, 24),
                Size = new Size(388, 112)
            };
            Controls.Add(messageLabel);

            var cancelButton = new AccentButton
            {
                Text = "取消",
                Style = AccentButtonStyle.Ghost,
                Size = new Size(92, 36),
                Location = new Point(218, 168),
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(cancelButton);
            CancelButton = cancelButton;

            var okButton = new AccentButton
            {
                Text = string.IsNullOrEmpty(confirmText) ? "确定" : confirmText,
                Style = danger ? AccentButtonStyle.Danger : AccentButtonStyle.Primary,
                Size = new Size(100, 36),
                Location = new Point(318, 168)
            };
            okButton.Click += delegate
            {
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(okButton);
            AcceptButton = okButton;
        }

        public static bool Ask(IWin32Window owner, string title, string message, string confirmText, bool danger)
        {
            using (var form = new ConfirmForm(title, message, confirmText, danger))
            {
                return form.ShowDialog(owner) == DialogResult.OK;
            }
        }
    }
}

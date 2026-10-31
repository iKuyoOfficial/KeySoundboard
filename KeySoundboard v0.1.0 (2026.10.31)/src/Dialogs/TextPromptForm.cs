using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class TextPromptForm : DarkForm
    {
        private readonly TextBox input;
        private readonly Label errorLabel;
        private readonly AccentButton okButton;
        private readonly AccentButton cancelButton;
        private readonly string promptText;

        public string Value
        {
            get { return input.Text.Trim(); }
        }

        public TextPromptForm(string title, string prompt, string initialValue, string okText)
        {
            promptText = prompt ?? "";
            Text = title;
            ClientSize = new Size(440, 228);

            var promptLabel = new Label
            {
                Text = promptText,
                Font = Theme.FontBody,
                ForeColor = Theme.TextMuted,
                Location = new Point(26, 22),
                Size = new Size(388, 36)
            };
            Controls.Add(promptLabel);

            input = new TextBox
            {
                Text = initialValue ?? "",
                Font = Theme.FontBody,
                BackColor = Theme.Surface,
                ForeColor = Theme.Text,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(26, 66),
                Size = new Size(388, 30),
                MaxLength = 80
            };
            input.KeyDown += InputKeyDown;
            Controls.Add(input);

            errorLabel = new Label
            {
                Text = "",
                Font = Theme.FontSmall,
                ForeColor = Theme.Danger,
                Location = new Point(26, 102),
                Size = new Size(388, 22)
            };
            Controls.Add(errorLabel);

            okButton = new AccentButton
            {
                Text = string.IsNullOrEmpty(okText) ? "确定" : okText,
                Style = AccentButtonStyle.Primary,
                Size = new Size(92, 36),
                Location = new Point(226, 162)
            };
            okButton.Click += OkClicked;
            Controls.Add(okButton);
            AcceptButton = okButton;

            cancelButton = new AccentButton
            {
                Text = "取消",
                Style = AccentButtonStyle.Ghost,
                Size = new Size(92, 36),
                Location = new Point(326, 162),
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(cancelButton);
            CancelButton = cancelButton;

            Shown += delegate
            {
                input.Focus();
                input.SelectAll();
            };
        }

        private void InputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OkClicked(sender, EventArgs.Empty);
            }
        }

        private void OkClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(input.Text))
            {
                errorLabel.Text = "名称不能为空。";
                input.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        public static string Prompt(IWin32Window owner, string title, string prompt, string initialValue, string okText)
        {
            using (var form = new TextPromptForm(title, prompt, initialValue, okText))
            {
                return form.ShowDialog(owner) == DialogResult.OK ? form.Value : null;
            }
        }
    }
}

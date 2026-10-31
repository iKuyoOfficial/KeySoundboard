using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class HelpForm : DarkForm
    {
        public HelpForm()
        {
            Text = "使用说明";
            ClientSize = new Size(640, 650);
            FormBorderStyle = FormBorderStyle.FixedDialog;

            var title = new Label
            {
                Text = "使用说明",
                Font = Theme.FontTitle,
                ForeColor = Theme.Text,
                AutoSize = true,
                Location = new Point(28, 20)
            };
            Controls.Add(title);

            var subtitle = new Label
            {
                Text = "音效键盘 · 在游戏前台也能触发的音效板",
                Font = Theme.FontBody,
                ForeColor = Theme.TextMuted,
                AutoSize = true,
                Location = new Point(30, 58)
            };
            Controls.Add(subtitle);

            var panel = new Panel
            {
                Location = new Point(26, 92),
                Size = new Size(588, 486),
                BackColor = Theme.Surface
            };
            Controls.Add(panel);

            var content = new RichTextBox
            {
                Location = new Point(14, 12),
                Size = new Size(560, 462),
                BackColor = Theme.Surface,
                ForeColor = Theme.Text,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                WordWrap = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                DetectUrls = false,
                Font = Theme.FontBody
            };
            panel.Controls.Add(content);

            FillContent(content);

            var closeButton = new AccentButton
            {
                Text = "关闭",
                Style = AccentButtonStyle.Primary,
                Size = new Size(100, 36),
                Location = new Point(514, 596),
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(closeButton);
            CancelButton = closeButton;

            var hint = new Label
            {
                Text = "按 Esc 关闭",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextDim,
                AutoSize = true,
                Location = new Point(30, 604)
            };
            Controls.Add(hint);
        }

        private static void FillContent(RichTextBox box)
        {
            AppendHeading(box, "快速开始");
            AppendBody(box,
                "1. 点击「添加音效」，选择 WAV、MP3、WMA、M4A、AAC 或 FLAC 文件。音频会复制到软件目录的 sounds 文件夹。\r\n" +
                "2. 在卡片右上角点「···」→「设置按键」，然后按下你想使用的按键。\r\n" +
                "3. 点击卡片，或按下对应按键即可播放。软件在后台、游戏在前台时也能触发。\r\n\r\n");

            AppendHeading(box, "界面说明");
            AppendBody(box,
                "· 点击卡片：播放音效。卡片会短暂高亮。\r\n" +
                "· 「···」菜单：设置按键、更换音频、重命名、在文件夹中显示、删除。\r\n" +
                "· 底部滑块：调节总音量；右侧开关：开启或关闭全局按键。关闭后只有点击卡片才会播放。\r\n" +
                "· 关闭窗口：默认最小化到系统托盘。右键托盘图标可以重新显示主界面或退出。\r\n\r\n");

            AppendHeading(box, "常见问题");
            AppendBody(box,
                "· 按键没反应：如果游戏以管理员身份运行，请右键本程序 →「以管理员身份运行」。普通权限的程序无法接收管理员窗口的按键。\r\n" +
                "· 部分游戏使用独占全屏或反作弊保护，可能拦截全局按键；可以尝试游戏的「无边框窗口」模式。\r\n" +
                "· 音频无法播放：建议把音频转换为 WAV 或 MP3 后重新添加。\r\n" +
                "· 多个音效同时按：不同音效可以同时播放；同一个音效再次按下会从头播放。\r\n" +
                "· 按键只支持单个键，不支持 Ctrl / Alt / Shift 组合键。\r\n\r\n");

            AppendHeading(box, "数据位置");
            AppendBody(box,
                "配置保存在软件目录的 config.xml，音频文件在 sounds 文件夹。\r\n" +
                "修改会自动保存；配置损坏时会自动备份为 config.xml.bad-日期时间.xml。");
        }

        private static void AppendHeading(RichTextBox box, string text)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionFont = Theme.FontHeading;
            box.SelectionColor = Theme.Accent;
            box.AppendText(text + "\r\n");
            box.SelectionFont = Theme.FontBody;
            box.SelectionColor = Theme.Text;
        }

        private static void AppendBody(RichTextBox box, string text)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionFont = Theme.FontBody;
            box.SelectionColor = Theme.Text;
            box.AppendText(text);
        }
    }
}

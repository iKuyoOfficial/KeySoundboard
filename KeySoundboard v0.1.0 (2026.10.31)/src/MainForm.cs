using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal sealed class MainForm : Form
    {
        private readonly bool debug;
        private readonly AppConfig config;
        private readonly AudioEngine audio;
        private readonly KeyboardHook hook;
        private readonly List<SoundCard> cards = new List<SoundCard>();

        private BufferedFlowPanel flow;
        private Panel header;
        private Panel footer;
        private StatusPill statusPill;
        private AccentButton addButton;
        private AccentButton folderButton;
        private AccentButton helpButton;
        private VolumeSlider volumeSlider;
        private Label volumeValueLabel;
        private ToggleSwitch hotkeyToggle;

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private readonly Timer saveTimer;
        private readonly Timer statusTimer;
        private bool loadingUi;
        private bool exitRequested;
        private bool trayHintShown;
        private string startupWarning;

        public MainForm(bool debug)
        {
            this.debug = debug;
            config = ConfigStore.Load(AppPaths.ConfigPath, out startupWarning);
            audio = new AudioEngine(config.MasterVolume);
            hook = new KeyboardHook();
            hook.KeyDown += OnGlobalKeyDown;

            saveTimer = new Timer { Interval = 600 };
            saveTimer.Tick += delegate
            {
                saveTimer.Stop();
                SaveConfig(false);
            };

            statusTimer = new Timer { Interval = 2400 };
            statusTimer.Tick += delegate
            {
                statusTimer.Stop();
                UpdateStatus();
            };

            AppPaths.EnsureDirectories();
            BuildUi();
            InitializeTray();
            ApplyUiValues();
            RebuildCards();

            Load += delegate
            {
                NativeMethods.EnableDarkTitleBar(Handle);
                NativeMethods.ApplyDarkScrollbar(flow.Handle);
            };

            Shown += OnShown;
            FormClosing += OnFormClosing;
            FormClosed += OnFormClosed;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_SHOW_MAIN)
            {
                ShowMainWindow();
                return;
            }
            base.WndProc(ref m);
        }

        private void BuildUi()
        {
            SuspendLayout();
            Text = NativeMethods.MainWindowTitle;
            BackColor = Theme.Background;
            ForeColor = Theme.Text;
            Font = Theme.FontBody;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(980, 660);
            MinimumSize = new Size(780, 560);
            DoubleBuffered = true;
            KeyPreview = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            flow = new BufferedFlowPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(Theme.Px(this, 28), Theme.Px(this, 12), Theme.Px(this, 18), Theme.Px(this, 14)),
                Margin = new Padding(0)
            };
            flow.Resize += delegate { ResizeEmptyState(); };

            footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Width = ClientSize.Width,
                Height = Theme.Px(this, 78),
                BackColor = Theme.Background
            };
            footer.Paint += FooterPaint;
            BuildFooter();

            header = new Panel
            {
                Dock = DockStyle.Top,
                Width = ClientSize.Width,
                Height = Theme.Px(this, 98),
                BackColor = Theme.Background
            };
            header.Paint += HeaderPaint;
            BuildHeader();

            Controls.Add(flow);
            Controls.Add(footer);
            Controls.Add(header);

            ResumeLayout(true);
        }

        private void BuildHeader()
        {
            var title = new Label
            {
                Text = "音效键盘",
                Font = Theme.FontTitle,
                ForeColor = Theme.Text,
                AutoSize = true,
                Location = new Point(Theme.Px(this, 30), Theme.Px(this, 20))
            };
            header.Controls.Add(title);

            var subtitle = new Label
            {
                Text = "按键盘触发音效 · 游戏在前台也能用",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextMuted,
                AutoSize = true,
                Location = new Point(Theme.Px(this, 32), Theme.Px(this, 58))
            };
            header.Controls.Add(subtitle);

            int addWidth = Theme.Px(this, 132);
            int addHeight = Theme.Px(this, 38);
            int statusWidth = Theme.Px(this, 154);
            int statusHeight = Theme.Px(this, 30);
            int right = Theme.Px(this, 30);

            addButton = new AccentButton
            {
                Text = "＋  添加音效",
                Style = AccentButtonStyle.Primary,
                Size = new Size(addWidth, addHeight),
                Location = new Point(ClientSize.Width - right - statusWidth - Theme.Px(this, 14) - addWidth, Theme.Px(this, 30)),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            addButton.Click += delegate { AddSounds(); };
            header.Controls.Add(addButton);

            statusPill = new StatusPill
            {
                Size = new Size(statusWidth, statusHeight),
                Location = new Point(ClientSize.Width - right - statusWidth, Theme.Px(this, 34)),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            header.Controls.Add(statusPill);
        }

        private void BuildFooter()
        {
            int y = (footer.Height - Theme.Px(this, 26)) / 2;

            var volumeCaption = new Label
            {
                Text = "音量",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextMuted,
                AutoSize = true,
                Location = new Point(Theme.Px(this, 30), y + Theme.Px(this, 4))
            };
            footer.Controls.Add(volumeCaption);

            volumeSlider = new VolumeSlider
            {
                Size = new Size(Theme.Px(this, 150), Theme.Px(this, 26)),
                Location = new Point(Theme.Px(this, 70), y)
            };
            volumeSlider.ValueChanged += VolumeChanged;
            footer.Controls.Add(volumeSlider);

            volumeValueLabel = new Label
            {
                Text = "100%",
                Font = Theme.FontSmallBold,
                ForeColor = Theme.Text,
                AutoSize = false,
                Size = new Size(Theme.Px(this, 46), Theme.Px(this, 26)),
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(Theme.Px(this, 228), y)
            };
            footer.Controls.Add(volumeValueLabel);

            var divider = new Panel
            {
                BackColor = Theme.Border,
                Size = new Size(1, Theme.Px(this, 30)),
                Location = new Point(Theme.Px(this, 294), (footer.Height - Theme.Px(this, 30)) / 2)
            };
            footer.Controls.Add(divider);

            var hotkeyCaption = new Label
            {
                Text = "全局按键",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextMuted,
                AutoSize = true,
                Location = new Point(Theme.Px(this, 316), y + Theme.Px(this, 4))
            };
            footer.Controls.Add(hotkeyCaption);

            hotkeyToggle = new ToggleSwitch
            {
                Size = new Size(Theme.Px(this, 46), Theme.Px(this, 24)),
                Location = new Point(Theme.Px(this, 388), y + Theme.Px(this, 1))
            };
            hotkeyToggle.CheckedChanged += HotkeyToggled;
            footer.Controls.Add(hotkeyToggle);

            var hotkeyHint = new Label
            {
                Text = "关闭后按键不再触发",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextDim,
                AutoSize = true,
                Location = new Point(Theme.Px(this, 444), y + Theme.Px(this, 4))
            };
            footer.Controls.Add(hotkeyHint);

            int folderWidth = Theme.Px(this, 116);
            int helpWidth = Theme.Px(this, 92);
            int buttonHeight = Theme.Px(this, 34);
            int buttonY = (footer.Height - buttonHeight) / 2;
            int right = Theme.Px(this, 30);

            helpButton = new AccentButton
            {
                Text = "使用说明",
                Style = AccentButtonStyle.Ghost,
                Size = new Size(helpWidth, buttonHeight),
                Location = new Point(footer.Width - right - helpWidth, buttonY),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            helpButton.Click += delegate { ShowHelp(); };
            footer.Controls.Add(helpButton);

            folderButton = new AccentButton
            {
                Text = "打开文件夹",
                Style = AccentButtonStyle.Secondary,
                Size = new Size(folderWidth, buttonHeight),
                Location = new Point(footer.Width - right - helpWidth - Theme.Px(this, 10) - folderWidth, buttonY),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            folderButton.Click += delegate { AppPaths.OpenFolder(AppPaths.SoundDir); };
            footer.Controls.Add(folderButton);
        }

        private void HeaderPaint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Theme.Border, 1f))
            {
                e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
            }
        }

        private void FooterPaint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Theme.Border, 1f))
            {
                e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
            }
        }

        private void InitializeTray()
        {
            trayMenu = Theme.CreateMenu();
            trayMenu.Items.Add(Theme.MenuItem("显示主界面", delegate { ShowMainWindow(); }, false, true));
            trayMenu.Items.Add(Theme.MenuItem("打开音效文件夹", delegate { AppPaths.OpenFolder(AppPaths.SoundDir); }, false, false));
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(Theme.MenuItem("退出", delegate { ExitApplication(); }, true, false));

            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "音效键盘",
                Visible = true,
                ContextMenuStrip = trayMenu
            };
            trayIcon.DoubleClick += delegate { ShowMainWindow(); };
        }

        private void ApplyUiValues()
        {
            loadingUi = true;
            volumeSlider.Value = config.MasterVolume;
            volumeValueLabel.Text = config.MasterVolume.ToString() + "%";
            hotkeyToggle.Checked = config.GlobalHotkeysEnabled;
            loadingUi = false;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (config == null) return;
            if (!config.GlobalHotkeysEnabled)
            {
                statusPill.SetStatus("全局按键已关闭", Theme.TextMuted);
            }
            else if (hook.IsInstalled)
            {
                statusPill.SetStatus("全局按键已启用", Theme.Success);
            }
            else
            {
                statusPill.SetStatus("全局按键未启动", Theme.Warning);
            }
        }

        private void ShowTransientStatus(string text, Color color)
        {
            statusPill.SetStatus(text, color);
            statusTimer.Stop();
            statusTimer.Start();
        }

        private void OnShown(object sender, EventArgs e)
        {
            try
            {
                hook.Install();
                hook.Enabled = config.GlobalHotkeysEnabled;
                AppLog.Write("Keyboard hook installed.");
            }
            catch (Exception ex)
            {
                hook.Enabled = false;
                AppLog.Write("Keyboard hook failed: " + ex.Message);
                ShowTransientStatus("全局按键不可用", Theme.Danger);
                MessageForm.ShowMessage(
                    this,
                    "全局按键不可用",
                    "无法安装全局键盘钩子，按键触发功能将不可用。\n\n" + ex.Message +
                    "\n\n你仍然可以点击卡片来播放音效。",
                    true);
            }

            UpdateStatus();

            if (!string.IsNullOrEmpty(startupWarning))
            {
                BeginInvoke(new Action(delegate
                {
                    MessageForm.ShowMessage(this, "配置提示", startupWarning, false);
                }));
            }
        }

        private void OnGlobalKeyDown(int virtualKey)
        {
            if (!config.GlobalHotkeysEnabled) return;
            AppLog.Write("Hook key down: " + virtualKey + " (" + KeyNames.Get(virtualKey) + ")");
            if (!IsHandleCreated || IsDisposed) return;
            try
            {
                BeginInvoke(new Action<int>(HandleGlobalKeyOnUi), virtualKey);
            }
            catch
            {
            }
        }

        private void HandleGlobalKeyOnUi(int virtualKey)
        {
            if (!config.GlobalHotkeysEnabled) return;
            SoundButtonConfig target = null;
            foreach (SoundButtonConfig button in config.Buttons)
            {
                if (button.VirtualKey == virtualKey)
                {
                    target = button;
                    break;
                }
            }

            if (target == null) return;
            PlayButton(target, true);
        }

        private void PlayButton(SoundButtonConfig button, bool fromHotkey)
        {
            if (button == null) return;
            SoundCard card = FindCard(button.Id);
            bool played = card != null && card.PlayNow();
            if (played)
            {
                AppLog.Write("Played: " + button.Name + " (" + (fromHotkey ? "hotkey" : "click") + ")");
                ShowTransientStatus("已播放 · " + Shorten(button.Name, 5), Theme.Accent);
            }
            else
            {
                AppLog.Write("Play failed: " + button.Name);
                ShowTransientStatus("播放失败", Theme.Danger);
            }
        }

        private void VolumeChanged(object sender, EventArgs e)
        {
            if (loadingUi) return;
            config.MasterVolume = volumeSlider.Value;
            audio.SetMasterVolume(config.MasterVolume);
            volumeValueLabel.Text = config.MasterVolume.ToString() + "%";
            ScheduleSave();
        }

        private void HotkeyToggled(object sender, EventArgs e)
        {
            if (loadingUi) return;
            config.GlobalHotkeysEnabled = hotkeyToggle.Checked;
            hook.Enabled = config.GlobalHotkeysEnabled;
            if (config.GlobalHotkeysEnabled && !hook.IsInstalled)
            {
                try
                {
                    hook.Install();
                }
                catch (Exception ex)
                {
                    AppLog.Write("Keyboard hook install failed: " + ex.Message);
                    hotkeyToggle.Checked = false;
                    config.GlobalHotkeysEnabled = false;
                    hook.Enabled = false;
                    MessageForm.ShowMessage(this, "全局按键不可用", "无法安装全局键盘钩子。\n\n" + ex.Message, true);
                }
            }

            UpdateStatus();
            SaveConfig(false);
        }

        private void ScheduleSave()
        {
            saveTimer.Stop();
            saveTimer.Start();
        }

        private bool SaveConfig(bool showError)
        {
            string error;
            if (ConfigStore.Save(AppPaths.ConfigPath, config, out error)) return true;
            AppLog.Write("Save failed: " + error);
            if (showError)
            {
                MessageForm.ShowMessage(this, "保存失败", "无法保存配置：\n" + error, true);
            }
            return false;
        }

        private void RebuildCards()
        {
            audio.Sync(config.Buttons);

            foreach (SoundCard card in cards)
            {
                card.PlayRequested -= CardPlayRequested;
                card.MenuRequested -= CardMenuRequested;
                flow.Controls.Remove(card);
                card.Dispose();
            }
            cards.Clear();

            while (flow.Controls.Count > 0)
            {
                Control control = flow.Controls[0];
                flow.Controls.RemoveAt(0);
                control.Dispose();
            }

            int cardWidth = Theme.Px(this, 208);
            int cardHeight = Theme.Px(this, 124);
            int gap = Theme.Px(this, 12);

            if (config.Buttons.Count == 0)
            {
                var empty = new EmptyStateView
                {
                    Size = new Size(Math.Max(Theme.Px(this, 440), flow.ClientSize.Width - flow.Padding.Horizontal - Theme.Px(this, 14)), Theme.Px(this, 330)),
                    Margin = new Padding(0, 0, gap, gap)
                };
                empty.AddRequested += delegate { AddSounds(); };
                flow.Controls.Add(empty);
                return;
            }

            foreach (SoundButtonConfig button in config.Buttons)
            {
                var card = new SoundCard(audio, button)
                {
                    Size = new Size(cardWidth, cardHeight),
                    Margin = new Padding(0, 0, gap, gap)
                };
                card.PlayRequested += CardPlayRequested;
                card.MenuRequested += CardMenuRequested;
                cards.Add(card);
                flow.Controls.Add(card);
            }

            var add = new AddCard
            {
                Size = new Size(cardWidth, cardHeight),
                Margin = new Padding(0, 0, gap, gap)
            };
            add.AddRequested += delegate { AddSounds(); };
            flow.Controls.Add(add);
        }

        private void ResizeEmptyState()
        {
            foreach (Control control in flow.Controls)
            {
                EmptyStateView empty = control as EmptyStateView;
                if (empty == null) continue;
                int width = flow.ClientSize.Width - flow.Padding.Horizontal - Theme.Px(this, 14);
                if (flow.VerticalScroll.Visible) width -= SystemInformation.VerticalScrollBarWidth;
                empty.Width = Math.Max(Theme.Px(this, 420), width);
            }
        }

        private SoundCard FindCard(string id)
        {
            foreach (SoundCard card in cards)
            {
                if (string.Equals(card.ButtonId, id, StringComparison.OrdinalIgnoreCase)) return card;
            }
            return null;
        }

        private void CardPlayRequested(object sender, EventArgs e)
        {
            var card = sender as SoundCard;
            if (card == null) return;
            SoundButtonConfig button = FindButton(card.ButtonId);
            if (button != null) PlayButton(button, false);
        }

        private void CardMenuRequested(object sender, EventArgs e)
        {
            var card = sender as SoundCard;
            if (card == null) return;
            ShowCardMenu(card, Cursor.Position);
        }

        private void ShowCardMenu(SoundCard card, Point screenPoint)
        {
            SoundButtonConfig button = FindButton(card.ButtonId);
            if (button == null) return;

            ContextMenuStrip menu = Theme.CreateMenu();
            string titleText = string.IsNullOrWhiteSpace(button.Name) ? "未命名音效" : button.Name;
            var headerItem = new ToolStripMenuItem(titleText)
            {
                Enabled = false,
                Font = Theme.FontBodyBold,
                ForeColor = Theme.Text,
                Padding = new Padding(8, 5, 8, 5)
            };
            menu.Items.Add(headerItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(Theme.MenuItem("播放", delegate { PlayButton(button, false); card.Flash(); }, false, true));
            menu.Items.Add(Theme.MenuItem("设置按键（" + KeyNames.Get(button.VirtualKey) + "）", delegate { SetKey(button); }, false, false));
            menu.Items.Add(Theme.MenuItem("更换音频…", delegate { ReplaceSound(button); }, false, false));
            menu.Items.Add(Theme.MenuItem("重命名…", delegate { RenameSound(button); }, false, false));
            menu.Items.Add(Theme.MenuItem("在文件夹中显示", delegate { ShowSoundInFolder(button); }, false, false));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(Theme.MenuItem("删除", delegate { DeleteSound(button); }, true, false));
            menu.Closed += delegate
            {
                try
                {
                    menu.BeginInvoke(new Action(delegate
                    {
                        if (!menu.IsDisposed) menu.Dispose();
                    }));
                }
                catch
                {
                }
            };
            menu.Show(screenPoint);
            menu.Focus();
        }

        private SoundButtonConfig FindButton(string id)
        {
            foreach (SoundButtonConfig button in config.Buttons)
            {
                if (string.Equals(button.Id, id, StringComparison.OrdinalIgnoreCase)) return button;
            }
            return null;
        }

        private void AddSounds()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "选择音效文件";
                dialog.Filter = AppPaths.FileDialogFilter;
                dialog.Multiselect = true;
                dialog.CheckFileExists = true;
                dialog.RestoreDirectory = true;
                if (!string.IsNullOrEmpty(config.LastFolder) && Directory.Exists(config.LastFolder))
                {
                    dialog.InitialDirectory = config.LastFolder;
                }
                else
                {
                    dialog.InitialDirectory = AppPaths.SoundDir;
                }

                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                var errors = new List<string>();
                int added = 0;
                foreach (string file in dialog.FileNames)
                {
                    string error;
                    string relative = AppPaths.ImportSound(file, out error);
                    if (string.IsNullOrEmpty(relative))
                    {
                        if (!string.IsNullOrEmpty(error)) errors.Add(error);
                        continue;
                    }

                    var button = new SoundButtonConfig
                    {
                        Name = UniqueName(Path.GetFileNameWithoutExtension(file)),
                        File = relative
                    };
                    config.Buttons.Add(button);
                    added++;
                }

                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    try
                    {
                        config.LastFolder = Path.GetDirectoryName(dialog.FileName);
                    }
                    catch
                    {
                    }
                }

                if (added > 0)
                {
                    SaveConfig(true);
                    RebuildCards();
                    ShowTransientStatus("已添加 · 请设置按键", Theme.Success);
                    AppLog.Write("Added " + added + " sound(s).");
                }

                if (errors.Count > 0)
                {
                    MessageForm.ShowMessage(this, "部分文件未添加", string.Join("\n\n", errors.ToArray()), true);
                }
            }
        }

        private string UniqueName(string baseName)
        {
            string name = string.IsNullOrWhiteSpace(baseName) ? "未命名音效" : baseName.Trim();
            string candidate = name;
            int counter = 2;
            while (NameExists(candidate))
            {
                candidate = name + " (" + counter.ToString() + ")";
                counter++;
            }
            return candidate;
        }

        private bool NameExists(string name)
        {
            foreach (SoundButtonConfig button in config.Buttons)
            {
                if (string.Equals(button.Name, name, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private void SetKey(SoundButtonConfig button)
        {
            hook.Suspended = true;
            try
            {
                using (var form = new KeyCaptureForm(button.VirtualKey))
                {
                    if (form.ShowDialog(this) != DialogResult.OK) return;
                    int captured = form.CapturedVirtualKey;
                    foreach (SoundButtonConfig other in config.Buttons)
                    {
                        if (other != button && other.VirtualKey == captured) other.VirtualKey = 0;
                    }
                    button.VirtualKey = captured;
                    SaveConfig(true);
                    RebuildCards();
                    if (captured == 0)
                    {
                        ShowTransientStatus("已清除按键", Theme.TextMuted);
                    }
                    else
                    {
                        ShowTransientStatus("按键已设为 " + KeyNames.Get(captured), Theme.Success);
                    }
                }
            }
            finally
            {
                hook.Suspended = false;
            }
        }

        private void ReplaceSound(SoundButtonConfig button)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "更换音频";
                dialog.Filter = AppPaths.FileDialogFilter;
                dialog.Multiselect = false;
                dialog.CheckFileExists = true;
                dialog.RestoreDirectory = true;
                if (!string.IsNullOrEmpty(config.LastFolder) && Directory.Exists(config.LastFolder))
                {
                    dialog.InitialDirectory = config.LastFolder;
                }
                else
                {
                    dialog.InitialDirectory = AppPaths.SoundDir;
                }

                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                string error;
                string relative = AppPaths.ImportSound(dialog.FileName, out error);
                if (string.IsNullOrEmpty(relative))
                {
                    MessageForm.ShowMessage(this, "无法更换音频", error ?? "未知错误", true);
                    return;
                }

                string oldFile = button.File;
                button.File = relative;
                try
                {
                    config.LastFolder = Path.GetDirectoryName(dialog.FileName);
                }
                catch
                {
                }
                SaveConfig(true);
                RebuildCards();
                DeleteUnusedSound(oldFile);
                ShowTransientStatus("已更换音频", Theme.Success);
            }
        }

        private void RenameSound(SoundButtonConfig button)
        {
            string value = TextPromptForm.Prompt(
                this,
                "重命名音效",
                "给这个音效起一个容易识别的名字：",
                button.Name,
                "保存");
            if (value == null) return;
            if (value == button.Name) return;
            button.Name = value;
            SaveConfig(true);
            RebuildCards();
            ShowTransientStatus("已重命名", Theme.Success);
        }

        private void ShowSoundInFolder(SoundButtonConfig button)
        {
            string full = AppPaths.GetFullPath(button.File);
            if (!string.IsNullOrEmpty(full) && File.Exists(full))
            {
                try
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + full + "\"") { UseShellExecute = true });
                    return;
                }
                catch
                {
                }
            }
            AppPaths.OpenFolder(AppPaths.SoundDir);
        }

        private void DeleteSound(SoundButtonConfig button)
        {
            string message = "确定删除音效“" + button.Name + "”吗？\n\n这个按钮会从列表中移除。";
            string full = AppPaths.GetFullPath(button.File);
            bool deleteFile = !string.IsNullOrEmpty(full) && File.Exists(full) && AppPaths.IsInsideBase(full);
            if (deleteFile)
            {
                message += "\n音频文件也会从软件目录中删除。";
            }

            if (!ConfirmForm.Ask(this, "删除音效", message, "删除", true)) return;

            config.Buttons.Remove(button);
            audio.Sync(config.Buttons);
            SaveConfig(true);
            RebuildCards();
            if (deleteFile) DeleteUnusedSound(button.File);
            ShowTransientStatus("已删除", Theme.TextMuted);
        }

        private void DeleteUnusedSound(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;
            foreach (SoundButtonConfig button in config.Buttons)
            {
                if (string.Equals(button.File, relativePath, StringComparison.OrdinalIgnoreCase)) return;
            }

            string full = AppPaths.GetFullPath(relativePath);
            if (!AppPaths.IsInsideBase(full)) return;
            if (!full.StartsWith(AppPaths.SoundDir, StringComparison.OrdinalIgnoreCase)) return;
            try
            {
                if (File.Exists(full)) File.Delete(full);
            }
            catch (Exception ex)
            {
                AppLog.Write("Delete sound failed: " + ex.Message);
            }
        }

        private void ShowHelp()
        {
            using (var form = new HelpForm())
            {
                form.ShowDialog(this);
            }
        }

        private void ShowMainWindow()
        {
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            Show();
            Activate();
            BringToFront();
        }

        private void ExitApplication()
        {
            exitRequested = true;
            Close();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && config.CloseToTray && !exitRequested)
            {
                e.Cancel = true;
                Hide();
                if (!trayHintShown)
                {
                    trayHintShown = true;
                    try
                    {
                        trayIcon.ShowBalloonTip(2500, "音效键盘仍在运行", "程序已最小化到托盘。右键托盘图标可以退出。", ToolTipIcon.Info);
                    }
                    catch
                    {
                    }
                }
                return;
            }

            saveTimer.Stop();
            statusTimer.Stop();
            SaveConfig(false);
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            hook.Dispose();
            audio.Dispose();
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
                trayIcon = null;
            }
            if (trayMenu != null)
            {
                trayMenu.Dispose();
                trayMenu = null;
            }
            saveTimer.Dispose();
            statusTimer.Dispose();
            AppLog.Write("Application closed.");
        }

        private static string Shorten(string text, int maxChars)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.Length <= maxChars) return text;
            return text.Substring(0, maxChars) + "…";
        }
    }
}

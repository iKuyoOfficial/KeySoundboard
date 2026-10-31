using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace KeySoundboard
{
    internal sealed class SoundSlot : IDisposable
    {
        private readonly MediaPlayer player;
        private bool opened;
        private bool pendingPlay;
        private bool disposed;
        private int masterVolume;
        private TimeSpan duration;

        public SoundButtonConfig Config { get; private set; }
        public string FilePath { get; private set; }
        public string Error { get; private set; }
        public bool IsOpened { get { return opened; } }
        public TimeSpan Duration { get { return duration; } }

        public event EventHandler StateChanged;

        public SoundSlot(SoundButtonConfig config, int volume)
        {
            Config = config;
            masterVolume = volume;
            player = new MediaPlayer();
            player.MediaOpened += OnMediaOpened;
            player.MediaFailed += OnMediaFailed;
            player.MediaEnded += OnMediaEnded;
            Load();
        }

        public void Load()
        {
            if (disposed) return;
            opened = false;
            pendingPlay = false;
            Error = null;
            duration = TimeSpan.Zero;
            FilePath = "";

            try
            {
                player.Close();
            }
            catch
            {
            }

            if (Config == null || string.IsNullOrEmpty(Config.File))
            {
                RaiseStateChanged();
                return;
            }

            string fullPath = AppPaths.GetFullPath(Config.File);
            if (!File.Exists(fullPath))
            {
                Error = "音频文件不存在";
                RaiseStateChanged();
                return;
            }

            FilePath = fullPath;
            try
            {
                player.Open(new Uri(fullPath, UriKind.Absolute));
                player.Volume = masterVolume / 100.0;
            }
            catch (Exception ex)
            {
                Error = "无法加载音频：" + ex.Message;
            }
            RaiseStateChanged();
        }

        public void UpdateConfig(SoundButtonConfig config)
        {
            bool fileChanged = Config == null ||
                               !string.Equals(Config.File, config.File, StringComparison.OrdinalIgnoreCase);
            Config = config;
            if (fileChanged) Load();
        }

        public bool Play()
        {
            if (disposed || Config == null) return false;
            if (!string.IsNullOrEmpty(Error)) return false;
            if (string.IsNullOrEmpty(Config.File)) return false;

            try
            {
                if (opened)
                {
                    player.Position = TimeSpan.Zero;
                    player.Play();
                }
                else
                {
                    pendingPlay = true;
                    if (player.Source == null) Load();
                }
                return true;
            }
            catch (Exception ex)
            {
                Error = "播放失败：" + ex.Message;
                return false;
            }
        }

        public void Stop()
        {
            if (disposed) return;
            try
            {
                player.Stop();
            }
            catch
            {
            }
        }

        public void SetMasterVolume(int volume)
        {
            masterVolume = volume;
            if (disposed) return;
            try
            {
                player.Volume = masterVolume / 100.0;
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            try
            {
                player.MediaOpened -= OnMediaOpened;
                player.MediaFailed -= OnMediaFailed;
                player.MediaEnded -= OnMediaEnded;
                player.Close();
            }
            catch
            {
            }
        }

        private void RaiseStateChanged()
        {
            EventHandler handler = StateChanged;
            if (handler == null) return;
            try
            {
                handler(this, EventArgs.Empty);
            }
            catch
            {
            }
        }

        private void OnMediaOpened(object sender, EventArgs e)
        {
            if (disposed) return;
            opened = true;
            Error = null;
            try
            {
                if (player.NaturalDuration.HasTimeSpan)
                {
                    duration = player.NaturalDuration.TimeSpan;
                }

                if (pendingPlay)
                {
                    pendingPlay = false;
                    player.Position = TimeSpan.Zero;
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                Error = "播放失败：" + ex.Message;
            }
            RaiseStateChanged();
        }

        private void OnMediaFailed(object sender, ExceptionEventArgs e)
        {
            if (disposed) return;
            opened = false;
            pendingPlay = false;
            string message = e != null && e.ErrorException != null
                ? e.ErrorException.Message
                : "无法解码音频";
            Error = "音频无法播放：" + message;
            RaiseStateChanged();
        }

        private void OnMediaEnded(object sender, EventArgs e)
        {
            if (disposed) return;
            // 不要在 MediaEnded 里把 Position 归零：
            // WPF MediaPlayer 会在播放结束后因这次 seek 自动重新播放，听起来像循环。
            try
            {
                player.Stop();
            }
            catch
            {
            }
        }

        public string FormatDuration()
        {
            if (duration <= TimeSpan.Zero) return "";
            if (duration.TotalHours >= 1)
            {
                return ((int)duration.TotalHours).ToString() + ":" + duration.ToString("mm\\:ss");
            }
            return duration.ToString("m\\:ss");
        }
    }

    internal sealed class AudioEngine : IDisposable
    {
        private readonly Dictionary<string, SoundSlot> slots =
            new Dictionary<string, SoundSlot>(StringComparer.OrdinalIgnoreCase);

        public int MasterVolume { get; private set; }
        public bool Muted { get; set; }

        public AudioEngine(int masterVolume)
        {
            MasterVolume = Clamp(masterVolume);
        }

        public void SetMasterVolume(int volume)
        {
            MasterVolume = Clamp(volume);
            foreach (SoundSlot slot in slots.Values)
            {
                slot.SetMasterVolume(EffectiveVolume());
            }
        }

        public void SetMuted(bool muted)
        {
            Muted = muted;
            foreach (SoundSlot slot in slots.Values)
            {
                slot.SetMasterVolume(EffectiveVolume());
            }
        }

        private int EffectiveVolume()
        {
            return Muted ? 0 : MasterVolume;
        }

        public void Sync(IEnumerable<SoundButtonConfig> buttons)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (SoundButtonConfig button in buttons)
            {
                if (button == null || string.IsNullOrEmpty(button.Id)) continue;
                seen.Add(button.Id);

                SoundSlot slot;
                if (slots.TryGetValue(button.Id, out slot))
                {
                    slot.UpdateConfig(button);
                }
                else
                {
                    slot = new SoundSlot(button, EffectiveVolume());
                    slots[button.Id] = slot;
                }
            }

            var stale = new List<string>();
            foreach (string id in slots.Keys)
            {
                if (!seen.Contains(id)) stale.Add(id);
            }

            foreach (string id in stale)
            {
                slots[id].Dispose();
                slots.Remove(id);
            }
        }

        public SoundSlot GetSlot(string id)
        {
            SoundSlot slot;
            if (!string.IsNullOrEmpty(id) && slots.TryGetValue(id, out slot)) return slot;
            return null;
        }

        public bool Play(string id)
        {
            SoundSlot slot = GetSlot(id);
            return slot != null && slot.Play();
        }

        public void Reload(string id)
        {
            SoundSlot slot = GetSlot(id);
            if (slot != null) slot.Load();
        }

        public void StopAll()
        {
            foreach (SoundSlot slot in slots.Values) slot.Stop();
        }

        public void Dispose()
        {
            foreach (SoundSlot slot in slots.Values) slot.Dispose();
            slots.Clear();
        }

        private static int Clamp(int volume)
        {
            if (volume < 0) return 0;
            if (volume > 100) return 100;
            return volume;
        }
    }
}

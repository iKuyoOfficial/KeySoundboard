using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace KeySoundboard
{
    [XmlRoot("Soundboard")]
    public sealed class AppConfig
    {
        public int Version { get; set; }

        public bool GlobalHotkeysEnabled { get; set; }

        public int MasterVolume { get; set; }

        public bool CloseToTray { get; set; }

        public string LastFolder { get; set; }

        [XmlArray("Buttons")]
        [XmlArrayItem("Button")]
        public List<SoundButtonConfig> Buttons { get; set; }

        public AppConfig()
        {
            Version = 1;
            GlobalHotkeysEnabled = true;
            MasterVolume = 100;
            CloseToTray = true;
            LastFolder = "";
            Buttons = new List<SoundButtonConfig>();
        }
    }

    public sealed class SoundButtonConfig
    {
        [XmlAttribute]
        public string Id { get; set; }

        [XmlAttribute]
        public int VirtualKey { get; set; }

        public string Name { get; set; }

        public string File { get; set; }

        public SoundButtonConfig()
        {
            Id = Guid.NewGuid().ToString("N");
            Name = "";
            File = "";
        }
    }

    internal static class ConfigStore
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(AppConfig));

        public static AppConfig Load(string path, out string warning)
        {
            warning = null;
            if (!File.Exists(path))
            {
                return new AppConfig();
            }

            try
            {
                AppConfig config;
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    config = (AppConfig)Serializer.Deserialize(stream);
                }

                if (config == null) throw new InvalidDataException("配置文件内容为空。");
                Normalize(config, out warning);
                return config;
            }
            catch (Exception ex)
            {
                string backup = null;
                try
                {
                    backup = path + ".bad-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".xml";
                    File.Copy(path, backup, true);
                }
                catch
                {
                    backup = null;
                }

                warning = "配置文件读取失败，已使用新的配置。";
                if (!string.IsNullOrEmpty(backup))
                {
                    warning += "\n原文件已备份为：\n" + backup;
                }
                warning += "\n\n" + ex.Message;
                return new AppConfig();
            }
        }

        public static bool Save(string path, AppConfig config, out string error)
        {
            error = null;
            string temp = path + ".tmp";
            try
            {
                string folder = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);

                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    Encoding = new UTF8Encoding(false),
                    NewLineChars = "\r\n"
                };

                using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    Serializer.Serialize(writer, config);
                }

                if (File.Exists(path))
                {
                    try
                    {
                        File.Copy(path, path + ".bak", true);
                    }
                    catch
                    {
                    }

                    try
                    {
                        File.Replace(temp, path, null);
                        return true;
                    }
                    catch
                    {
                        // Fall through to a normal copy on file systems that do not support Replace.
                    }
                }

                File.Copy(temp, path, true);
                TryDelete(temp);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                TryDelete(temp);
                return false;
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
            }
        }

        private static void Normalize(AppConfig config, out string warning)
        {
            warning = null;
            if (config.MasterVolume < 0) config.MasterVolume = 0;
            if (config.MasterVolume > 100) config.MasterVolume = 100;
            if (config.Version <= 0) config.Version = 1;
            if (config.LastFolder == null) config.LastFolder = "";
            if (config.Buttons == null) config.Buttons = new List<SoundButtonConfig>();

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var keys = new HashSet<int>();
            bool cleaned = false;

            for (int i = config.Buttons.Count - 1; i >= 0; i--)
            {
                SoundButtonConfig button = config.Buttons[i];
                if (button == null)
                {
                    config.Buttons.RemoveAt(i);
                    cleaned = true;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(button.Id) || ids.Contains(button.Id))
                {
                    button.Id = Guid.NewGuid().ToString("N");
                    cleaned = true;
                }
                ids.Add(button.Id);

                if (button.Name == null) button.Name = "";
                if (button.File == null) button.File = "";

                if (button.VirtualKey != 0)
                {
                    if (!KeyNames.IsBindable(button.VirtualKey) || keys.Contains(button.VirtualKey))
                    {
                        button.VirtualKey = 0;
                        cleaned = true;
                    }
                    else
                    {
                        keys.Add(button.VirtualKey);
                    }
                }
            }

            if (cleaned)
            {
                warning = "配置中有无效或重复的按键，已自动清理。";
            }
        }
    }
}

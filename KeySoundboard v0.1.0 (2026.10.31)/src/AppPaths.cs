using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace KeySoundboard
{
    internal static class AppPaths
    {
        public static readonly string BaseDir = NormalizeDirectory(AppDomain.CurrentDomain.BaseDirectory);
        public static readonly string SoundDir = Path.Combine(BaseDir, "sounds");
        public static readonly string ConfigPath = Path.Combine(BaseDir, "config.xml");

        public static readonly string[] SupportedExtensions =
        {
            ".wav", ".mp3", ".wma", ".m4a", ".aac", ".flac"
        };

        public static readonly string FileDialogFilter =
            "音频文件|*.wav;*.mp3;*.wma;*.m4a;*.aac;*.flac|所有文件|*.*";

        private static string NormalizeDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return AppDomain.CurrentDomain.BaseDirectory;
            return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }

        public static void EnsureDirectories()
        {
            if (!Directory.Exists(SoundDir)) Directory.CreateDirectory(SoundDir);
        }

        public static bool IsSupported(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string extension = Path.GetExtension(path);
            foreach (string supported in SupportedExtensions)
            {
                if (string.Equals(extension, supported, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static string GetFullPath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return "";
            if (Path.IsPathRooted(relativePath)) return relativePath;
            return Path.Combine(BaseDir, relativePath);
        }

        public static string GetRelativePath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return "";
            string full = Path.GetFullPath(fullPath);
            string root = Path.GetFullPath(BaseDir);
            if (!root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                root += Path.DirectorySeparatorChar;
            }

            if (full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                return full.Substring(root.Length);
            }
            return full;
        }

        public static bool IsInsideBase(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return false;
            string full = Path.GetFullPath(fullPath);
            string root = Path.GetFullPath(BaseDir);
            if (!root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                root += Path.DirectorySeparatorChar;
            }
            return full.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }

        public static string ImportSound(string sourcePath, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
            {
                error = "找不到音频文件。";
                return null;
            }

            if (!IsSupported(sourcePath))
            {
                error = "不支持这个文件格式。支持的格式：WAV、MP3、WMA、M4A、AAC、FLAC。";
                return null;
            }

            try
            {
                EnsureDirectories();
                string sourceFull = Path.GetFullPath(sourcePath);

                if (IsInsideBase(sourceFull))
                {
                    return GetRelativePath(sourceFull);
                }

                string baseName = Path.GetFileNameWithoutExtension(sourceFull);
                string extension = Path.GetExtension(sourceFull).ToLowerInvariant();
                string safeName = MakeSafeFileName(baseName);
                string candidate = Path.Combine(SoundDir, safeName + extension);
                int counter = 2;

                while (File.Exists(candidate))
                {
                    if (FilesEqual(sourceFull, candidate))
                    {
                        return GetRelativePath(candidate);
                    }
                    candidate = Path.Combine(SoundDir, safeName + " (" + counter.ToString() + ")" + extension);
                    counter++;
                    if (counter > 999)
                    {
                        candidate = Path.Combine(SoundDir, Guid.NewGuid().ToString("N").Substring(0, 8) + extension);
                        break;
                    }
                }

                File.Copy(sourceFull, candidate);
                return GetRelativePath(candidate);
            }
            catch (Exception ex)
            {
                error = "复制音频文件失败：" + ex.Message;
                return null;
            }
        }

        private static bool FilesEqual(string a, string b)
        {
            try
            {
                var infoA = new FileInfo(a);
                var infoB = new FileInfo(b);
                return infoA.Length == infoB.Length &&
                       string.Equals(infoA.Name, infoB.Name, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static string MakeSafeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "sound";
            var invalid = new HashSet<char>(Path.GetInvalidFileNameChars());
            var builder = new StringBuilder(name.Length);
            foreach (char c in name.Trim())
            {
                builder.Append(invalid.Contains(c) ? '_' : c);
            }

            string result = builder.ToString().Trim().TrimEnd('.');
            if (string.IsNullOrEmpty(result)) result = "sound";
            if (result.Length > 80) result = result.Substring(0, 80).TrimEnd();
            return result;
        }

        public static void OpenFolder(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) path = BaseDir;
                if (!Directory.Exists(path)) path = BaseDir;
                Process.Start(new ProcessStartInfo("explorer.exe", "\"" + path + "\"") { UseShellExecute = true });
            }
            catch
            {
            }
        }

        public static string FileName(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return "";
            try
            {
                return Path.GetFileName(relativePath);
            }
            catch
            {
                return relativePath;
            }
        }
    }
}

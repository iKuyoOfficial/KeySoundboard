using System;
using System.IO;
using System.Text;

namespace KeySoundboard
{
    internal static class AppLog
    {
        private static readonly object Sync = new object();
        public static readonly string LogPath = Path.Combine(AppPaths.BaseDir, "debug.log");

        public static bool Enabled { get; set; }

        public static void Write(string message)
        {
            if (!Enabled) return;
            try
            {
                lock (Sync)
                {
                    File.AppendAllText(
                        LogPath,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "  " + (message ?? "") + "\r\n",
                        new UTF8Encoding(false));
                }
            }
            catch
            {
            }
        }
    }
}

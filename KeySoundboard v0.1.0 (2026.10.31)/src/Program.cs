using System;
using System.Threading;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            bool debug = false;
            foreach (string arg in args)
            {
                if (string.Equals(arg, "--debug", StringComparison.OrdinalIgnoreCase)) debug = true;
            }

            AppLog.Enabled = debug;
            NativeMethods.EnsureDpiAwareness();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
            {
                AppLog.Write("UI exception: " + e.Exception);
                try
                {
                    MessageBox.Show(
                        "程序遇到一个错误：\n\n" + e.Exception.Message,
                        "音效键盘",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                catch
                {
                }
            };
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                AppLog.Write("Unhandled exception: " + e.ExceptionObject);
            };

            const string mutexName = "Local\\KeySoundboard.SingleInstance";
            bool createdNew;
            using (var mutex = new Mutex(true, mutexName, out createdNew))
            {
                if (!createdNew)
                {
                    for (int i = 0; i < 24; i++)
                    {
                        if (NativeMethods.ActivateExistingWindow()) return;
                        Thread.Sleep(125);
                    }
                    return;
                }

                try
                {
                    AppPaths.EnsureDirectories();
                    AppLog.Write("Starting KeySoundboard. Debug logging enabled.");
                    Application.Run(new MainForm(debug));
                }
                catch (Exception ex)
                {
                    AppLog.Write("Fatal startup error: " + ex);
                    MessageBox.Show(
                        "音效键盘启动失败：\n\n" + ex.Message,
                        "音效键盘",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}

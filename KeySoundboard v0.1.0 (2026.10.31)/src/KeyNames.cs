using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal static class KeyNames
    {
        private static readonly Dictionary<int, string> Names = BuildNames();
        private static readonly int[] BindableKeys = BuildBindableKeys();

        public static bool IsBindable(int virtualKey)
        {
            if (virtualKey <= 0) return false;
            switch (virtualKey)
            {
                case (int)Keys.ShiftKey:
                case (int)Keys.LShiftKey:
                case (int)Keys.RShiftKey:
                case (int)Keys.ControlKey:
                case (int)Keys.LControlKey:
                case (int)Keys.RControlKey:
                case (int)Keys.Menu:
                case (int)Keys.LMenu:
                case (int)Keys.RMenu:
                case (int)Keys.LWin:
                case (int)Keys.RWin:
                case (int)Keys.CapsLock:
                case (int)Keys.NumLock:
                case (int)Keys.Scroll:
                case (int)Keys.PrintScreen:
                case (int)Keys.Pause:
                    return false;
            }

            if ((virtualKey >= (int)Keys.A && virtualKey <= (int)Keys.Z) ||
                (virtualKey >= (int)Keys.D0 && virtualKey <= (int)Keys.D9) ||
                (virtualKey >= (int)Keys.F1 && virtualKey <= (int)Keys.F24) ||
                (virtualKey >= (int)Keys.NumPad0 && virtualKey <= (int)Keys.Divide))
            {
                return true;
            }

            switch (virtualKey)
            {
                case (int)Keys.Back:
                case (int)Keys.Tab:
                case (int)Keys.Enter:
                case (int)Keys.Escape:
                case (int)Keys.Space:
                case (int)Keys.Insert:
                case (int)Keys.Delete:
                case (int)Keys.Home:
                case (int)Keys.End:
                case (int)Keys.PageUp:
                case (int)Keys.PageDown:
                case (int)Keys.Up:
                case (int)Keys.Down:
                case (int)Keys.Left:
                case (int)Keys.Right:
                case (int)Keys.OemSemicolon:
                case (int)Keys.Oemplus:
                case (int)Keys.Oemcomma:
                case (int)Keys.OemMinus:
                case (int)Keys.OemPeriod:
                case (int)Keys.OemQuestion:
                case (int)Keys.Oemtilde:
                case (int)Keys.OemOpenBrackets:
                case (int)Keys.OemPipe:
                case (int)Keys.OemCloseBrackets:
                case (int)Keys.OemQuotes:
                case (int)Keys.Oem8:
                case (int)Keys.Oem102:
                case (int)Keys.BrowserBack:
                case (int)Keys.BrowserForward:
                case (int)Keys.BrowserRefresh:
                case (int)Keys.BrowserStop:
                case (int)Keys.BrowserSearch:
                case (int)Keys.BrowserFavorites:
                case (int)Keys.BrowserHome:
                case (int)Keys.VolumeMute:
                case (int)Keys.VolumeDown:
                case (int)Keys.VolumeUp:
                case (int)Keys.MediaNextTrack:
                case (int)Keys.MediaPreviousTrack:
                case (int)Keys.MediaStop:
                case (int)Keys.MediaPlayPause:
                case (int)Keys.LaunchMail:
                case (int)Keys.LaunchApplication1:
                case (int)Keys.LaunchApplication2:
                    return true;
            }

            return false;
        }

        public static string Get(int virtualKey)
        {
            string name;
            if (Names.TryGetValue(virtualKey, out name)) return name;
            if (virtualKey <= 0) return "未绑定";
            return "VK 0x" + virtualKey.ToString("X2");
        }

        public static string GetLongName(int virtualKey)
        {
            string name;
            if (LongNames.TryGetValue(virtualKey, out name)) return name;
            return Get(virtualKey);
        }

        private static readonly Dictionary<int, string> LongNames = BuildLongNames();

        public static bool TryParse(string text, out int virtualKey)
        {
            virtualKey = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            text = text.Trim();

            foreach (int vk in BindableKeys)
            {
                if (string.Equals(Get(vk), text, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(GetLongName(vk), text, StringComparison.OrdinalIgnoreCase))
                {
                    virtualKey = vk;
                    return true;
                }
            }

            switch (text.ToLowerInvariant())
            {
                case "escape": virtualKey = (int)Keys.Escape; return true;
                case "return": virtualKey = (int)Keys.Enter; return true;
                case "del": virtualKey = (int)Keys.Delete; return true;
                case "ins": virtualKey = (int)Keys.Insert; return true;
                case "pgup": virtualKey = (int)Keys.PageUp; return true;
                case "pgdn": virtualKey = (int)Keys.PageDown; return true;
                case "control": virtualKey = 0; return false;
                case "ctrl": virtualKey = 0; return false;
            }

            return false;
        }

        private static Dictionary<int, string> BuildNames()
        {
            var names = new Dictionary<int, string>
            {
                { 0, "未绑定" }
            };

            for (int i = (int)Keys.A; i <= (int)Keys.Z; i++) names[i] = ((char)i).ToString();
            for (int i = (int)Keys.D0; i <= (int)Keys.D9; i++) names[i] = ((char)i).ToString();
            for (int i = (int)Keys.F1; i <= (int)Keys.F24; i++) names[i] = "F" + (i - (int)Keys.F1 + 1);
            for (int i = (int)Keys.NumPad0; i <= (int)Keys.NumPad9; i++) names[i] = "Num" + (i - (int)Keys.NumPad0);

            names[(int)Keys.Back] = "退格";
            names[(int)Keys.Tab] = "Tab";
            names[(int)Keys.Enter] = "回车";
            names[(int)Keys.Escape] = "Esc";
            names[(int)Keys.Space] = "空格";
            names[(int)Keys.Insert] = "Ins";
            names[(int)Keys.Delete] = "Del";
            names[(int)Keys.Home] = "Home";
            names[(int)Keys.End] = "End";
            names[(int)Keys.PageUp] = "PgUp";
            names[(int)Keys.PageDown] = "PgDn";
            names[(int)Keys.Up] = "↑";
            names[(int)Keys.Down] = "↓";
            names[(int)Keys.Left] = "←";
            names[(int)Keys.Right] = "→";

            names[(int)Keys.Multiply] = "Num*";
            names[(int)Keys.Add] = "Num+";
            names[(int)Keys.Subtract] = "Num-";
            names[(int)Keys.Decimal] = "Num.";
            names[(int)Keys.Divide] = "Num/";

            names[(int)Keys.OemSemicolon] = ";";
            names[(int)Keys.Oemplus] = "=";
            names[(int)Keys.Oemcomma] = ",";
            names[(int)Keys.OemMinus] = "-";
            names[(int)Keys.OemPeriod] = ".";
            names[(int)Keys.OemQuestion] = "/";
            names[(int)Keys.Oemtilde] = "`";
            names[(int)Keys.OemOpenBrackets] = "[";
            names[(int)Keys.OemPipe] = "\\";
            names[(int)Keys.OemCloseBrackets] = "]";
            names[(int)Keys.OemQuotes] = "'";
            names[(int)Keys.Oem8] = "OEM8";
            names[(int)Keys.Oem102] = "OEM102";

            names[(int)Keys.BrowserBack] = "浏览器后退";
            names[(int)Keys.BrowserForward] = "浏览器前进";
            names[(int)Keys.BrowserRefresh] = "浏览器刷新";
            names[(int)Keys.BrowserStop] = "浏览器停止";
            names[(int)Keys.BrowserSearch] = "浏览器搜索";
            names[(int)Keys.BrowserFavorites] = "浏览器收藏";
            names[(int)Keys.BrowserHome] = "浏览器主页";
            names[(int)Keys.VolumeMute] = "静音键";
            names[(int)Keys.VolumeDown] = "音量-";
            names[(int)Keys.VolumeUp] = "音量+";
            names[(int)Keys.MediaNextTrack] = "下一曲";
            names[(int)Keys.MediaPreviousTrack] = "上一曲";
            names[(int)Keys.MediaStop] = "停止播放";
            names[(int)Keys.MediaPlayPause] = "播放/暂停";
            names[(int)Keys.LaunchMail] = "邮件键";
            names[(int)Keys.LaunchApplication1] = "应用键1";
            names[(int)Keys.LaunchApplication2] = "应用键2";

            return names;
        }

        private static Dictionary<int, string> BuildLongNames()
        {
            var names = new Dictionary<int, string>
            {
                { (int)Keys.Back, "Backspace" },
                { (int)Keys.Enter, "Enter" },
                { (int)Keys.Escape, "Escape" },
                { (int)Keys.Space, "Space" },
                { (int)Keys.Insert, "Insert" },
                { (int)Keys.Delete, "Delete" },
                { (int)Keys.PageUp, "PageUp" },
                { (int)Keys.PageDown, "PageDown" },
                { (int)Keys.Up, "Up" },
                { (int)Keys.Down, "Down" },
                { (int)Keys.Left, "Left" },
                { (int)Keys.Right, "Right" },
                { (int)Keys.OemSemicolon, "Semicolon" },
                { (int)Keys.Oemplus, "Plus" },
                { (int)Keys.Oemcomma, "Comma" },
                { (int)Keys.OemMinus, "Minus" },
                { (int)Keys.OemPeriod, "Period" },
                { (int)Keys.OemQuestion, "Slash" },
                { (int)Keys.Oemtilde, "Tilde" },
                { (int)Keys.OemOpenBrackets, "OpenBracket" },
                { (int)Keys.OemPipe, "Backslash" },
                { (int)Keys.OemCloseBrackets, "CloseBracket" },
                { (int)Keys.OemQuotes, "Quote" }
            };
            return names;
        }

        private static int[] BuildBindableKeys()
        {
            var list = new List<int>();
            for (int vk = 1; vk < 255; vk++)
            {
                if (IsBindable(vk)) list.Add(vk);
            }
            return list.ToArray();
        }
    }
}


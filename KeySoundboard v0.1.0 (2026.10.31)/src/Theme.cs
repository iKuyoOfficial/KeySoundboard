using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace KeySoundboard
{
    internal static class Theme
    {
        public static readonly Color Background = Color.FromArgb(13, 15, 19);
        public static readonly Color Surface = Color.FromArgb(23, 26, 33);
        public static readonly Color SurfaceHover = Color.FromArgb(30, 34, 44);
        public static readonly Color SurfacePressed = Color.FromArgb(35, 40, 52);
        public static readonly Color Border = Color.FromArgb(38, 43, 54);
        public static readonly Color BorderHover = Color.FromArgb(66, 77, 99);
        public static readonly Color Text = Color.FromArgb(240, 243, 248);
        public static readonly Color TextMuted = Color.FromArgb(140, 149, 168);
        public static readonly Color TextDim = Color.FromArgb(98, 107, 126);
        public static readonly Color Accent = Color.FromArgb(91, 140, 255);
        public static readonly Color AccentHover = Color.FromArgb(124, 163, 255);
        public static readonly Color AccentDim = Color.FromArgb(31, 43, 72);
        public static readonly Color AccentSoft = Color.FromArgb(45, 61, 98);
        public static readonly Color Success = Color.FromArgb(61, 220, 151);
        public static readonly Color SuccessDim = Color.FromArgb(18, 36, 29);
        public static readonly Color Warning = Color.FromArgb(255, 184, 77);
        public static readonly Color WarningDim = Color.FromArgb(44, 35, 22);
        public static readonly Color Danger = Color.FromArgb(255, 92, 114);
        public static readonly Color DangerDim = Color.FromArgb(47, 24, 30);
        public static readonly Color DangerHover = Color.FromArgb(255, 122, 140);

        public static readonly Font FontTitle;
        public static readonly Font FontHeading;
        public static readonly Font FontCardTitle;
        public static readonly Font FontBody;
        public static readonly Font FontBodyBold;
        public static readonly Font FontSmall;
        public static readonly Font FontSmallBold;
        public static readonly Font FontBadge;
        public static readonly Font FontKeyDisplay;

        static Theme()
        {
            string fontName = "Microsoft YaHei UI";
            foreach (string candidate in new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Segoe UI" })
            {
                using (var test = new Font(candidate, 9f))
                {
                    if (string.Equals(test.Name, candidate, StringComparison.OrdinalIgnoreCase))
                    {
                        fontName = candidate;
                        break;
                    }
                }
            }

            FontTitle = new Font(fontName, 20f, FontStyle.Bold, GraphicsUnit.Point);
            FontHeading = new Font(fontName, 15f, FontStyle.Bold, GraphicsUnit.Point);
            FontCardTitle = new Font(fontName, 11.25f, FontStyle.Bold, GraphicsUnit.Point);
            FontBody = new Font(fontName, 9.5f, FontStyle.Regular, GraphicsUnit.Point);
            FontBodyBold = new Font(fontName, 9.5f, FontStyle.Bold, GraphicsUnit.Point);
            FontSmall = new Font(fontName, 8.5f, FontStyle.Regular, GraphicsUnit.Point);
            FontSmallBold = new Font(fontName, 8.5f, FontStyle.Bold, GraphicsUnit.Point);
            FontBadge = new Font(fontName, 8.75f, FontStyle.Bold, GraphicsUnit.Point);
            FontKeyDisplay = new Font(fontName, 22f, FontStyle.Bold, GraphicsUnit.Point);
        }

        public static int Px(Control control, int value)
        {
            if (control == null) return value;
            return (int)Math.Round(value * control.DeviceDpi / 96.0);
        }

        public static int Px(Control control, float value)
        {
            if (control == null) return (int)Math.Round(value);
            return (int)Math.Round(value * control.DeviceDpi / 96.0);
        }

        public static Color Blend(Color a, Color b, float t)
        {
            if (t < 0f) t = 0f;
            if (t > 1f) t = 1f;
            return Color.FromArgb(
                (int)Math.Round(a.A + (b.A - a.A) * t),
                (int)Math.Round(a.R + (b.R - a.R) * t),
                (int)Math.Round(a.G + (b.G - a.G) * t),
                (int)Math.Round(a.B + (b.B - a.B) * t));
        }

        public static Color Alpha(Color color, int alpha)
        {
            if (alpha < 0) alpha = 0;
            if (alpha > 255) alpha = 255;
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        public static GraphicsPath RoundedRect(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            if (rect.Width <= 0f || rect.Height <= 0f)
            {
                path.AddRectangle(rect);
                return path;
            }

            float diameter = Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height));
            if (diameter <= 1f)
            {
                path.AddRectangle(rect);
                return path;
            }

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180f, 90f);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270f, 90f);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        public static void FillRounded(Graphics g, Brush brush, RectangleF rect, float radius)
        {
            using (GraphicsPath path = RoundedRect(rect, radius))
            {
                g.FillPath(brush, path);
            }
        }

        public static void DrawRounded(Graphics g, Pen pen, RectangleF rect, float radius)
        {
            using (GraphicsPath path = RoundedRect(rect, radius))
            {
                g.DrawPath(pen, path);
            }
        }

        public static void Prepare(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }

        public static ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip
            {
                BackColor = Surface,
                ForeColor = Text,
                Font = FontBody,
                ShowImageMargin = false,
                ShowCheckMargin = false,
                Padding = new Padding(6, 6, 6, 6),
                Renderer = new ToolStripProfessionalRenderer(new DarkMenuColorTable()) { RoundedEdges = false },
                DropShadowEnabled = true
            };
            return menu;
        }

        public static ToolStripMenuItem MenuItem(string text, EventHandler onClick, bool danger, bool bold)
        {
            var item = new ToolStripMenuItem(text)
            {
                ForeColor = danger ? Danger : Text,
                Font = bold ? FontBodyBold : FontBody,
                Padding = new Padding(8, 5, 8, 5)
            };
            if (onClick != null) item.Click += onClick;
            return item;
        }
    }

    internal sealed class DarkMenuColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground { get { return Theme.Surface; } }
        public override Color ImageMarginGradientBegin { get { return Theme.Surface; } }
        public override Color ImageMarginGradientMiddle { get { return Theme.Surface; } }
        public override Color ImageMarginGradientEnd { get { return Theme.Surface; } }
        public override Color MenuBorder { get { return Theme.Border; } }
        public override Color MenuItemBorder { get { return Theme.AccentSoft; } }
        public override Color MenuItemSelected { get { return Theme.SurfaceHover; } }
        public override Color MenuItemSelectedGradientBegin { get { return Theme.SurfaceHover; } }
        public override Color MenuItemSelectedGradientEnd { get { return Theme.SurfaceHover; } }
        public override Color SeparatorDark { get { return Theme.Border; } }
        public override Color SeparatorLight { get { return Theme.Border; } }
        public override Color CheckBackground { get { return Theme.AccentDim; } }
        public override Color CheckSelectedBackground { get { return Theme.AccentDim; } }
        public override Color CheckPressedBackground { get { return Theme.AccentDim; } }
    }
}

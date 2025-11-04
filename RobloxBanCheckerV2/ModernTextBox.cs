using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RobloxBanCheckerV2
{
    public class ModernTextBox : TextBox
    {
        private Color borderColor = Color.Gray;
        private int borderWidth = 2;
        private int cornerRadius = 8;

        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        public int BorderWidth
        {
            get => borderWidth;
            set { borderWidth = value; Invalidate(); }
        }

        public int CornerRadius
        {
            get => cornerRadius;
            set { cornerRadius = value; Invalidate(); }
        }

        public string PlaceholderText { get; set; } = "";

        public ModernTextBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint, true);

            BorderStyle = BorderStyle.None;
            BackColor = Color.FromArgb(30, 30, 40);
            ForeColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var path = GetRoundedPath(rect, cornerRadius);

            // Hintergrund
            using (var brush = new SolidBrush(BackColor))
                g.FillPath(brush, path);

            // Border
            using (var pen = new Pen(borderColor, borderWidth))
                g.DrawPath(pen, path);

            // Text zeichnen
            var textRect = new Rectangle(
                Padding.Left,
                Padding.Top,
                Width - Padding.Left - Padding.Right,
                Height - Padding.Top - Padding.Bottom
            );

            using (var brush = new SolidBrush(ForeColor))
            {
                if (string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(PlaceholderText) && !Focused)
                {
                    using (var placeholderBrush = new SolidBrush(Color.FromArgb(150, 150, 150)))
                    {
                        g.DrawString(PlaceholderText, Font, placeholderBrush, textRect);
                    }
                }
                else
                {
                    g.DrawString(Text, Font, brush, textRect);
                }
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
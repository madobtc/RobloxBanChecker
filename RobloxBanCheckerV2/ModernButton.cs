using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RobloxBanCheckerV2
{
    public class ModernButton : Button
    {
        private bool isHovered = false;
        private bool isPressed = false;

        public Color BorderColor { get; set; } = Color.Gray;
        public int CornerRadius { get; set; } = 8;
        public Color HoverColor { get; set; } = Color.FromArgb(50, 255, 255, 255);
        public Color PressColor { get; set; } = Color.FromArgb(30, 0, 0, 0);

        public ModernButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint, true);

            Cursor = Cursors.Hand;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            isPressed = true;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            isPressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var path = GetRoundedPath(rect, CornerRadius);

            // Hintergrund
            using (var brush = new SolidBrush(BackColor))
                g.FillPath(brush, path);

            // Hover/Press Effekt
            if (isPressed)
            {
                using (var brush = new SolidBrush(PressColor))
                    g.FillPath(brush, path);
            }
            else if (isHovered)
            {
                using (var brush = new SolidBrush(HoverColor))
                    g.FillPath(brush, path);
            }

            // Border
            using (var pen = new Pen(BorderColor, 2))
                g.DrawPath(pen, path);

            // Text
            using (var brush = new SolidBrush(ForeColor))
            {
                var format = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(Text, Font, brush, rect, format);
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
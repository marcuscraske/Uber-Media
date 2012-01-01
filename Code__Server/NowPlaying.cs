using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace UberMediaServer
{
    public partial class NowPlaying : Form
    {
        public const int fadeTime = 200; // The amount of time before the pane begins to fade away (10ms  * this variable)
        private int count = 0; // Stores timing for fading
        private string nowPlaying = null;
        public NowPlaying()
        {
            InitializeComponent();
            BackColor = Color.Black;
            TransparencyKey = BackColor;
            Show();
            BringToFront();
            TopMost = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }
        Font font = new Font("Arial", 20.0f, FontStyle.Bold);
        Font font_position = new Font("Arial", 16.0f, FontStyle.Bold);
        Font fontNowPlaying = new Font("Arial", 14.0f, FontStyle.Bold);
        SolidBrush brush = new SolidBrush(Color.White);
        string message = null;
        double[] seconds = null; // <current, total>
        public void DisplayMessage(string msg)
        {
            message = msg;
            Fade();
        }
        public void UpdatePosition(double secondsCurrent, double secondsTotal)
        {
            seconds = new double[] { secondsCurrent, secondsTotal };
        }
        public void UpdatePositionEnd()
        {
            seconds = null;
        }
        public void Fade()
        {
            Invoke((MethodInvoker)delegate()
            {
                Opacity = 1;
                count = 0;
            });
        }
        private void NowPlaying_Paint(object sender, PaintEventArgs e)
        {
            // Draw gradient overlay
            Rectangle area = new Rectangle(0, 0, Width, Height);
            LinearGradientBrush brush_grad = new LinearGradientBrush(area, Color.FromArgb(100, 255, 255, 255), Color.Transparent, 90f);
            e.Graphics.FillRectangle(brush_grad, area);
            try
            {
                // Draw message
                if (message != null) e.Graphics.DrawString(message, font, brush, 25, 40);
                // Draw position
                if (seconds != null)
                {
                    // Draw enclosing rectangle
                    e.Graphics.DrawRectangle(new Pen(brush, 1), 5, 10, Width - 10, 10);
                    // Draw marker
                    double multiplier = seconds[0] / seconds[1];
                    e.Graphics.FillEllipse(brush, 15 + (int)(multiplier * (Width - 65)), 5, 25, 25);
                    // Draw position info
                    string info = "";
                    TimeSpan ts = TimeSpan.FromSeconds(seconds[0]);
                    info = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00") + "/";
                    ts = TimeSpan.FromSeconds(seconds[1]);
                    info += ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                    e.Graphics.DrawString(info, font_position, brush, (float)Width - e.Graphics.MeasureString(info, font_position).Width, 25);
                }
                // Draw the current item being played
                string _nowPlaying = nowPlaying ?? "No item is currently playing.";
                e.Graphics.DrawString(_nowPlaying, fontNowPlaying, brush, (float)Width - e.Graphics.MeasureString(_nowPlaying, fontNowPlaying).Width, Height - (fontNowPlaying.Height + 10));
            }
            catch { }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (count < fadeTime)
                count ++;
            else if (Opacity > 0)
                Opacity -= 0.01;
            else
                message = null;
            Invalidate();
        }
        public void UpdateNowPlaying(string text)
        {
            nowPlaying = text;
        }
    }
}

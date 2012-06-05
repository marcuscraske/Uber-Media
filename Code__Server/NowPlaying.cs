/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /NowPlaying.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 * A user-interface for informing the user of messages such as the current item playing or the
 * change in the state of the terminal e.g. volume.
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Net;
using System.Threading;
using System.IO;

namespace UberMediaServer
{
    public partial class NowPlaying : Form
    {
        #region "Constants"
        public const int fadeTime = 200; // The amount of time before the pane begins to fade away (10ms  * this variable)
        #endregion

        #region "Variables"
        private Main main;
        private Font font = new Font("Arial", 20.0f, FontStyle.Bold);
        private Font font_position = new Font("Arial", 16.0f, FontStyle.Bold);
        private Font fontNowPlaying = new Font("Arial", 14.0f, FontStyle.Bold);
        private SolidBrush brush = new SolidBrush(Color.White);
        private string message = null;
        private double[] seconds = null; // <current, total>
        private int count = 0; // Stores timing for fading
        private string nowPlaying = null;
        private Image image = null;
        #endregion

        #region "Methods - Constructors"
        public NowPlaying(Main main)
        {
            InitializeComponent();
            this.main = main;
            BackColor = Color.Black;
            TransparencyKey = BackColor;
            Show();
            BringToFront();
            TopMost = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }
        #endregion

        #region "Methods - Events"
        private void NowPlaying_Paint(object sender, PaintEventArgs e)
        {
            if (Opacity == 0) return;
            // Draw gradient overlay
            Rectangle area = new Rectangle(0, 0, Width, Height);
            LinearGradientBrush brush_grad = new LinearGradientBrush(area, Color.FromArgb(100, 255, 255, 255), Color.Transparent, 90f);
            e.Graphics.FillRectangle(brush_grad, area);
            // Draw message
            if (message != null) e.Graphics.DrawString(message, font, brush, 25, 40);
            // Draw position
            if (seconds != null && seconds.Length == 2)
            {
                // Draw enclosing rectangle
                e.Graphics.DrawRectangle(new Pen(brush, 1), 5, 10, Width - 10, 10);
                // Draw marker
                double multiplier = seconds[0] != 0 && seconds[1] != 0 ? seconds[0] / seconds[1] : 0; // Divide by zero protection
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
            SizeF textSizeCalc = e.Graphics.MeasureString(_nowPlaying, fontNowPlaying);
            float x = (float)Width - textSizeCalc.Width;
            float y = Height - (fontNowPlaying.Height + 10);
            e.Graphics.DrawString(_nowPlaying, fontNowPlaying, brush, x, y);
            if (image != null)
                e.Graphics.DrawImage(image, x - image.Width - 5, y + ((textSizeCalc.Height / 2) - (image.Height / 2)));
        }
        /// <summary>
        /// Used to fade the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (count < fadeTime)
            {   // Check if the period before fading has been surpassed
                count++;
                Invalidate();
            }
            else if (Opacity > 0)
            {   // Slowly fade the window
                Opacity -= 0.01;
                Invalidate();
            }
            else if(message != null)
            {   // Reset the message displayed to the user
                message = null;
                Invalidate();
            }
            Invalidate();
        }
        #endregion

        #region "Methods"
        /// <summary>
        /// Displays a message and fades the window; used to state e.g. the volume has been changed.
        /// </summary>
        /// <param name="msg"></param>
        public void displayMessage(string msg)
        {
            message = msg;
            fade();
        }
        /// <summary>
        /// Updates the position of the on-screen seeker/ball.
        /// </summary>
        /// <param name="secondsCurrent"></param>
        /// <param name="secondsTotal"></param>
        public void updatePosition(double secondsCurrent, double secondsTotal)
        {
            seconds = new double[] { secondsCurrent, secondsTotal };
        }
        /// <summary>
        /// Removes the seeker due to a medium ending.
        /// </summary>
        public void updatePositionEnd()
        {
            seconds = null;
        }
        /// <summary>
        /// Causes the window to fade away.
        /// </summary>
        public void fade()
        {
            Invoke((MethodInvoker)delegate()
            {
                Opacity = 1;
                count = 0;
            });
        }
        /// <summary>
        /// Changes the title of the current item playing.
        /// </summary>
        /// <param name="text"></param>
        public void updateNowPlaying(string text, string vitemid)
        {
            nowPlaying = text;
            image = null;
            if (text != null)
            {
                nowPlaying = text;
                Thread th = new Thread(delegate()
                {
                    fetchImage(this, vitemid);
                });
                th.Start();
            }
        }
        private const int THUMBNAIL_WIDTH_MAX = 48;
        private const int THUMBNAIL_HEIGHT_MAX = 36;
        public static void fetchImage(NowPlaying np, string vitemid)
        {
            try
            {
                WebClient wc = new WebClient();
                byte[] data = wc.DownloadData(np.main.libraryURL + "/thumbnail/" + vitemid);
                System.Diagnostics.Debug.WriteLine(np.main.libraryURL + "/thumbnail/" + vitemid);
                MemoryStream ms = new MemoryStream(data);
                Image i = Image.FromStream(ms);
                ms.Dispose();
                int width;
                int height;
                if (i.Width > i.Height)
                {
                    width = THUMBNAIL_WIDTH_MAX;
                    height = (int)(((float)THUMBNAIL_WIDTH_MAX / (float)i.Width) * (float)i.Height);
                }
                else
                {
                    width = (int)(((float)THUMBNAIL_HEIGHT_MAX / (float)i.Height) * (float)i.Width);
                    height = THUMBNAIL_HEIGHT_MAX;
                }
                np.image = i.GetThumbnailImage(width, height, null, IntPtr.Zero);
                i.Dispose();
                ms.Dispose();
            }
            catch { }
        }
        #endregion
    }
}

/*
 * License:     Creative Commons Attribution-ShareAlike 3.0 unported
 * File:        Interfaces/YouTube/video_youtube.cs
 * Author(s):   limpygnome
 * 
 * Provides an interface for playing YouTube.com (video-sharing site) media.
 * 
 * Improvements/bugs:
 *          -   If the video cannot be embedded, we could use an RTSP stream embedded in a web-page; for instance if we visit:
 *              http://m.youtube.com/watch?v=ylLzyHk54Z0
 *      
 *              The "Watch Video" link contains an RTSP URI, which could be played in an embedded page using an object element.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace UberMediaServer.Interfaces
{
    public class images : Interface
    {
        #region "Variables"
        private Bitmap img;
        private Rectangle drawRegion;
        public override event Interface._Error Error;
        #endregion

        #region "Methods - Constructors"
        public images(Main main, string path, string vitemid) : base(main, path, vitemid)
        {
            try
            {
                // Load the image
                img = new Bitmap(path);
                // Calculate the drawing size
                drawRegion = new Rectangle();
                double boundsWidth = main.Width;
                double boundsHeight = main.Height;
                double ratioFitToScreen = 1 - img.Width > img.Height ? (double)boundsWidth / (double)img.Width : (double)boundsHeight / (double)img.Height;
                drawRegion.Width = (int)(ratioFitToScreen * (double)img.Width);
                drawRegion.Height = (int)(ratioFitToScreen * (double)img.Height);
                // Calculate the drawing co-ordinates
                drawRegion.X = ((int)boundsWidth - drawRegion.Width) / 2;
                drawRegion.Y = ((int)boundsHeight - drawRegion.Height) / 2;
                // Begin animation and painting of image
                ImageAnimator.Animate(img, new EventHandler(OnFrameChanged));
                formMain.Invoke((MethodInvoker)delegate()
                {
                    main.Paint += new PaintEventHandler(main_Paint);
                    formMain.panel1.Visible = false;
                });
            }
            catch
            {
                Error("Could not load image!");
            }
        }
        #endregion

        #region "Methods - Events"
        void main_Paint(object sender, PaintEventArgs e)
        {
            ImageAnimator.UpdateFrames();
            e.Graphics.DrawImage(img, drawRegion);
        }
        private void OnFrameChanged(object o, EventArgs e)
        {
            formMain.Invoke((MethodInvoker)delegate()
            {
                formMain.Invalidate();
            });
        }
        #endregion

        #region "Methods - Overrides"
        public override void Dispose()
        {
            formMain.Invoke((MethodInvoker)delegate()
            {
                formMain.panel1.Visible = true;
                formMain.Paint -= new PaintEventHandler(main_Paint);
            });
            if (img != null)
            {
                img.Dispose();
                img = null;
            }
        }
        #endregion
    }
}
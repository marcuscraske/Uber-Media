using System;
using System.Collections.Generic;
using System.Web;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.IO;
using UberLib.Connector;

namespace UberMedia
{
    /// <summary>
    /// Responsible for generating thumbnails of media items.
    /// </summary>
    public static class ThumbnailGeneratorService
    {
        private static bool ShutdownOverride = false; // Causes the threads to safely terminate
        public static string Status = "Uninitialised";
        private static Thread DelegatorThread = null;
        public static List<Thread> Threads = new List<Thread>();
        public static List<string[]> Queue = new List<string[]>(); // Path of media, output path, handler

        public static void Delegator_Start()
        {
            if (DelegatorThread != null) return;
            Status = "Creating pool of threads";
            // Create thread pool
            Thread th;
            for (int i = 0; i < int.Parse(Core.Cache_Settings["thumbnail_threads"]); i++)
            {
                th = new Thread(new ParameterizedThreadStart(ThreadMethod));
                Threads.Add(th);
            }
            Status = "Starting delegator";
            // Recreate the cache folder if it exists
            if (!Directory.Exists(Core.basePath + "\\Content\\Cache"))
                Directory.CreateDirectory(Core.basePath + "\\Content\\Cache");
            // Initialise delegator/manager of threads
            DelegatorThread = new Thread(new ParameterizedThreadStart(Delegator));
            DelegatorThread.Start();
            Status = "Delegator started";
        }
        public static void Delegator_Stop()
        {
            Status = "Delegator shutting down";
            if (DelegatorThread != null)
            {
                DelegatorThread.Abort();
                DelegatorThread = null;
            }
            ShutdownOverride = true;
            foreach (Thread th in Threads) th.Abort();
            lock (processes)
                try { foreach (Process p in processes) p.Kill(); }
                catch { }
            Threads.Clear();
            ShutdownOverride = false;
            Status = "Delegator and pool terminated";
        }

        /// <summary>
        /// Controls the threads.
        /// </summary>
        private static void Delegator(object o)
        {
            while (true && !ShutdownOverride)
            {
                lock (Threads)
                {
                    Thread th;
                    for (int i = 0; i < Threads.Count; i++)
                    {
                        th = Threads[i];
                        if (th.ThreadState != System.Threading.ThreadState.WaitSleepJoin && th.ThreadState != System.Threading.ThreadState.Running && Queue.Count != 0)
                            lock (Queue)
                            {
                                Threads[i] = new Thread(new ParameterizedThreadStart(ThreadMethod));
                                Threads[i].Start(Queue[0]);
                                Queue.Remove(Queue[0]);
                            }
                    }

                }
                Thread.Sleep(100);
                Status = "Successfully looped at " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            }
        }
        public static void AddToQueue(string path_media, string vitemid, string handler)
        {
            lock (Queue) Queue.Add(new string[] { path_media, vitemid, handler });
        }
        /// <summary>
        /// Responsible for any processors executed by the thumbnail service; this is to ensure the service can be suddenly shutdown.
        /// </summary>
        public static List<Process> processes = new List<Process>();
        private static void ThreadMethod(object o)
        {
            string[] data = (string[])o;
            try
            {
                typeof(ThumbnailGeneratorService).GetMethod("ThumbnailProcessor__" + data[2], System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .Invoke(null, new object[] { data[0], data[1] });
            }
            catch { }
        }
        public static void ThumbnailProcessor__ffmpeg(string path, string vitemid)
        {
            string cachePath = Core.basePath + "\\Content\\Cache\\" + vitemid + ".png";
            // Initialize ffmpeg
            Process p = new Process();
            p.StartInfo.FileName = "ffmpeg.exe";
            p.StartInfo.WorkingDirectory = Core.basePath + @"\bin";
            p.StartInfo.Arguments = "-itsoffset -" + Core.Cache_Settings["thumbnail_screenshot_media_time"].ToString() + "  -i \"" + path + "\" -vcodec mjpeg -vframes 1 -an -f rawvideo -s " + Core.Cache_Settings["thumbnail_width"] + "x" + Core.Cache_Settings["thumbnail_height"] + " -y \"" + cachePath + "\"";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            processes.Add(p);
            // Wait for ffmpeg to extract the screen-shot
            try
            {

                while (!p.WaitForExit(int.Parse(Core.Cache_Settings["thumbnail_thread_ttl"])) && !ShutdownOverride)
                {
                    Thread.Sleep(20);
                }
                p.Kill();
            }
            catch { }
            // Remove the process
            processes.Remove(p);
            // Read the image into a byte-array and delete the cache file
            if (File.Exists(cachePath))
            {
                try
                {
                    // Read the file into a byte-array
                    MemoryStream ms = new MemoryStream();
                    Image img = Image.FromFile(cachePath);
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    img.Dispose();
                    byte[] data = ms.ToArray();
                    ms.Dispose();
                    // Upload to the database
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("thumbnail", data);
                    parameters.Add("vitemid", vitemid);
                    Core.GlobalConnector.Query_Execute_Parameters("UPDATE virtual_items SET thumbnail_data=@thumbnail WHERE vitemid=@vitemid", parameters);
                }
                catch
                {
                }
                try
                {
                    // Delete the cache file
                    File.Delete(cachePath);
                }
                catch {}
            }
        }
        public static void ThumbnailProcessor__youtube(string path, string vitemid)
        {
            try
            {
                // Get the ID
                string vid = File.ReadAllText(path);
                // Download thumbnail from YouTube
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://img.youtube.com/vi/" + vid + "/1.jpg");
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                if (resp.ContentLength == 0) return;
                // Resize and draw the thumbnail
                Bitmap img = (Bitmap)Bitmap.FromStream(resp.GetResponseStream());
                Bitmap thumbnail = new Bitmap(int.Parse(Core.Cache_Settings["thumbnail_width"]), int.Parse(Core.Cache_Settings["thumbnail_height"]));
                Graphics g = Graphics.FromImage(thumbnail);
                g.FillRectangle(new SolidBrush(Color.DarkGray), 0, 0, thumbnail.Width, thumbnail.Height);
                double fitToThumbnailRatio = (img.Width > img.Height ? (double)thumbnail.Width / (double)img.Width : (double)thumbnail.Height / (double)img.Height);
                Rectangle drawArea = new Rectangle();
                drawArea.Width = (int)(fitToThumbnailRatio * (double)img.Width);
                drawArea.Height = (int)(fitToThumbnailRatio * (double)img.Height);
                drawArea.X = (int)(((double)thumbnail.Width - (double)drawArea.Width) / 2);
                drawArea.Y = (int)(((double)thumbnail.Height - (double)drawArea.Height) / 2);
                g.DrawImage(img, drawArea);
                g.Dispose();
                // Convert to a byte array
                MemoryStream ms = new MemoryStream();
                thumbnail.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                thumbnail.Dispose();
                img.Dispose();
                resp.Close();
                byte[] data = ms.ToArray();
                ms.Dispose();
                // Upload to the database
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("thumbnail", data);
                parameters.Add("vitemid", vitemid);
                Core.GlobalConnector.Query_Execute_Parameters("UPDATE virtual_items SET thumbnail_data=@thumbnail WHERE vitemid=@vitemid", parameters);
            }
            catch
            {
            }
        }
        public static void ThumbnailProcessor__image(string path, string vitemid)
        {
            try
            {
                Bitmap img = new Bitmap(path);
                Bitmap thumbnail = new Bitmap(int.Parse(Core.Cache_Settings["thumbnail_width"]), int.Parse(Core.Cache_Settings["thumbnail_height"]));
                Graphics g = Graphics.FromImage(thumbnail);
                g.FillRectangle(new SolidBrush(Color.DarkGray), 0, 0, thumbnail.Width, thumbnail.Height);
                double fitToThumbnailRatio = (img.Width > img.Height ? (double)thumbnail.Width / (double)img.Width : (double)thumbnail.Height / (double)img.Height);
                Rectangle drawArea = new Rectangle();
                drawArea.Width = (int)(fitToThumbnailRatio * (double)img.Width);
                drawArea.Height = (int)(fitToThumbnailRatio * (double)img.Height);
                drawArea.X = (int)(((double)thumbnail.Width - (double)drawArea.Width) / 2);
                drawArea.Y = (int)(((double)thumbnail.Height - (double)drawArea.Height) / 2);
                g.DrawImage(img, drawArea);
                g.Dispose();
                // Convert to a byte array
                MemoryStream ms = new MemoryStream();
                thumbnail.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                thumbnail.Dispose();
                img.Dispose();
                byte[] data = ms.ToArray();
                ms.Dispose();
                // Upload to the database
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("thumbnail", data);
                parameters.Add("vitemid", vitemid);
                Core.GlobalConnector.Query_Execute_Parameters("UPDATE virtual_items SET thumbnail_data=@thumbnail WHERE vitemid=@vitemid", parameters);
            }
            catch
            {
            }
        }
    }
}
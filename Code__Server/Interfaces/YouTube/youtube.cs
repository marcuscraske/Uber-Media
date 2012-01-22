/*
 * License:     Creative Commons Attribution-ShareAlike 3.0 unported
 * File:        Interfaces/YouTube/video_youtube.cs
 * Author(s):   limpygnome
 * 
 * Provides an interface for playing YouTube.com (video-sharing site) media.
 * 
 * Improvements/bugs:
 *          -   Needs thorough testing, odd bugs can occur.
 *          -   If the video cannot be embedded, we could use an RTSP stream embedded in a web-page; for instance if we visit:
 *              http://m.youtube.com/watch?v=ylLzyHk54Z0
 *      
 *              The "Watch Video" link contains an RTSP URI, which could be played in an embedded page using an object element.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace UberMediaServer.Interfaces
{
    public class youtube : Interface
    {
        States state = States.Loading;
        Main mainForm;
        WebBrowser browser = null;
        string videoid = null;
        public youtube(Main main, string path, string vitemid) : base(main, path, vitemid)
        {
            mainForm = main;
            // Pass the path variable, which contains the ID of the YouTube video
            videoid = File.ReadAllText(path);
            System.Diagnostics.Debug.WriteLine("Playing YouTube ID: '" + videoid + "'!");
            main.Invoke((MethodInvoker)delegate()
            {
                // Create and attach web browser to inner panel
                browser = new WebBrowser();
                main.panel1.Controls.Add(browser);
                // Set browser properties
                browser.ScriptErrorsSuppressed = true;
                browser.Dock = DockStyle.Fill;
                browser.DocumentTitleChanged += new EventHandler(browser_DocumentTitleChanged);
                // Load YouTube player page
                browser.Navigate("file://" + Application.StartupPath + "\\Interfaces\\YouTube\\YouTube.htm");
            });
        }
        /// <summary>
        /// This method is hooked to an event-handler of the browser control; this will be used to communicate with the page without using
        /// a timer or additional threads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void browser_DocumentTitleChanged(object sender, EventArgs e)
        {
            switch (browser.DocumentTitle)
            {
                case "LOADING":
                    System.Diagnostics.Debug.WriteLine(videoid);
                    browser.Document.InvokeScript("YT_Load", new object[] { videoid });
                    break;
                case "PLAYING":
                    state = States.Playing;
                    break;
                case "PAUSED":
                    state = States.Paused;
                    break;
            }
            // An error has occurred, decide what to do based on the error-code
            if (browser.DocumentTitle.StartsWith("ERROR"))
            {
                string error_code = browser.DocumentTitle.Remove(0, 5);
                switch (error_code)
                {
                    case "101":
                    case "150":
                        // Navigate to normal YouTube page since the video cannot be embedded
                        mainForm.np.DisplayMessage("Video cannot be embedded, displaying YouTube page instead...");
                        browser.Navigate("http://www.youtube.com/watch?v=" + videoid + "&fmt=22&wide=1&feature=watch-now-button");
                        break;
                    default:
                        // Unknown error - inform the user
                        browser.DocumentText = "<h1>Unknown error occurred: " + error_code +"</h1>";
                        break;
                }
            }
        }
        public override void Play()
        {
            mainForm.Invoke((MethodInvoker)delegate()
            {
                if (browser != null)
                    browser.Document.InvokeScript("YT_Action_Play");
            });
        }
        public override void Pause()
        {
            mainForm.Invoke((MethodInvoker)delegate()
            {
                if (browser != null)
                    browser.Document.InvokeScript("YT_Action_Pause");
            });
        }
        public override void Stop()
        {
            mainForm.Invoke((MethodInvoker)delegate()
            {
                if (browser != null)
                    browser.Document.InvokeScript("YT_Action_Stop");
            });
        }
        public override void Mute()
        {
            mainForm.Invoke((MethodInvoker)delegate()
            {
                if (browser != null)
                    browser.Document.InvokeScript("YT_Action_Mute");
            });
        }
        public override void Unmute()
        {
            mainForm.Invoke((MethodInvoker)delegate()
            {
                if (browser != null)
                    browser.Document.InvokeScript("YT_Action_Unmute");
            });
        }
        delegate bool youtubeBool();
        public override bool IsMuted()
        {
            return (bool)mainForm.Invoke((youtubeBool)delegate()
            {
                if (browser != null)
                    return (bool)browser.Document.InvokeScript("YT_Query_IsMuted");
                else return true;
            });
        }
        delegate double youtubeDouble();
        public override double Duration
        {
            get
            {
                try
                {
                    return (double)mainForm.Invoke((youtubeDouble)delegate()
                    {

                        if (browser != null)
                            return (double)browser.Document.InvokeScript("YT_Query_Duration");
                        else return 0;
                    });
                }
                catch { return 0; }
            }
        }
        public override Interface.States State()
        {
            return state;
        }
        public override double Position
        {
            get
            {
                try
                {
                    return (double)mainForm.Invoke((youtubeDouble)delegate()
                    {
                        if (browser != null)
                            return (double)browser.Document.InvokeScript("YT_Query_CurrentPosition");
                        else return 0;
                    });
                }
                catch { return 0; }
            }
            set
            {
                mainForm.Invoke((MethodInvoker)delegate()
                {
                    if (browser != null)
                        browser.Document.InvokeScript("YT_Action_Position", new object[]{ value });
                });
            }
        }
        public override double Volume
        {
            get
            {
                try
                {
                    return (double)mainForm.Invoke((youtubeDouble)delegate()
                    {
                        if (browser != null)
                        {
                            object o = browser.Document.InvokeScript("YT_Query_Volume");
                            if (o != null)
                                return (int)o / 100.0;
                            else return 0;
                        }
                        else return 0;
                    });
                }
                catch { return 0; }
            }
            set
            {
                mainForm.Invoke((MethodInvoker)delegate()
                {
                    if (browser != null)
                        browser.Document.InvokeScript("YT_Action_SetVolume", new object[] { value * 100 });
                });
            }
        }
        public override void Dispose()
        {
            mainForm.Invoke((MethodInvoker)delegate()
            {
                mainForm.panel1.Controls.Remove(browser);
                browser.Dispose();
                browser = null;
            });
        }
    }
}
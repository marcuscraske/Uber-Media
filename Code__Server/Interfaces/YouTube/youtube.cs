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
using Microsoft.Win32;

namespace UberMediaServer.Interfaces
{
    public class youtube : Interface
    {
        #region "Class Variables"
        public static bool checkedFlashInstall = false;
        public static bool flashInstalled = false;
        #endregion

        #region "Variables"
        private States state = States.Loading;
        private Main mainForm;
        private WebBrowser browser = null;
        private string videoid = null;
        #endregion

        #region "Methods - Constructors"
        public youtube(Main main, string path, string vitemid) : base(main, path, vitemid)
        {
            mainForm = main;
            // Check Adobe Flash is installed
            if (!checkedFlashInstall)
            {
                flashInstalled = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Macromedia\FlashPlayer") != null;
                checkedFlashInstall = true;
                System.Diagnostics.Debug.WriteLine("Adobe Flash Player installed: " + flashInstalled);
            }
            // Create browser object
            main.Invoke((MethodInvoker)delegate()
            {
                // Create and attach web browser to inner panel
                browser = new WebBrowser();
                main.panel1.Controls.Add(browser);
                // Set browser properties
                browser.ScriptErrorsSuppressed = true;
                browser.Dock = DockStyle.Fill;
                browser.DocumentTitleChanged += new EventHandler(browser_DocumentTitleChanged);
                browser.ScrollBarsEnabled = false;
                // Load YouTube player if flash is installed - else inform the user
                if(flashInstalled)
                    browser.Navigate("file://" + Application.StartupPath + "\\Interfaces\\YouTube\\YouTube.html");
                else
                    browser.Navigate("file://" + Application.StartupPath + "\\Interfaces\\YouTube\\FlashPlayer.html");
            });
            // Pass the path variable, which contains the ID of the YouTube video
            try
            {
                videoid = File.ReadAllText(path);
            }
            catch { }
            System.Diagnostics.Debug.WriteLine("Playing YouTube ID: '" + videoid + "'!");
        }
        #endregion

        #region "Methods - Events"
        /// <summary>
        /// This method is hooked to an event-handler of the browser control; this will be used to communicate with the page without using
        /// a timer or additional threads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void browser_DocumentTitleChanged(object sender, EventArgs e)
        {
            if (!flashInstalled) return; // Ignore the title change - it's not the YouTube player
            switch (browser.DocumentTitle)
            {
                case "LOADING":
                    browser.Document.InvokeScript("YT_Load", new object[] { videoid });
                    Volume = mainForm.currentVolume; // Audio-fix
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
                        mainForm.np.displayMessage("Video cannot be embedded, displaying YouTube page instead...");
                        browser.ScrollBarsEnabled = true;
                        browser.Navigate("http://www.youtube.com/watch?v=" + videoid + "&fmt=22&wide=1&feature=watch-now-button");
                        break;
                    default:
                        // Unknown error - inform the user
                        browser.DocumentText = "<h1>Unknown error occurred: " + error_code +"</h1>";
                        break;
                }
            }
        }
        #endregion

        #region "Methods - Overrides"
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
                try
                {
                    if (browser != null)
                        return (bool)browser.Document.InvokeScript("YT_Query_IsMuted");
                    else return true;
                }
                catch { return true; }
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
                        try
                        {
                            if (browser != null)
                                return (double)browser.Document.InvokeScript("YT_Query_Duration");
                            else return 0;
                        }
                        catch
                        {
                            return 0;
                        }
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
                        try
                        {
                            if (browser != null)
                                return (double)browser.Document.InvokeScript("YT_Query_CurrentPosition");
                            else return 0;
                        }
                        catch { return 0; }
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
        #endregion
    }
}
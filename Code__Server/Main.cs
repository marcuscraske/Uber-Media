/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /Main.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 * Responsible for displaying media to the end-user on the physical machine through a borderless
 * window; this is also responsible for processing terminal commands and loading configuration.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using System.IO;
using System.Diagnostics;
using UberMedia.Shared;
using System.Runtime.InteropServices;

namespace UberMediaServer
{
    public partial class Main : Form
    {
        #region "Constants"
        /// <summary>
        /// Used for identifying this application in error-files.
        /// </summary>
        public const string APPLICATION_ID = "UM_TERMINAL";
        #endregion

        #region "Variables"
        private Interfaces.Interface _currentInterface = null;
        private string vitemid = string.Empty;
        public string terminalID;
        public string libraryURL;
        private XmlDocument settings;
        private Thread workerProcessor = null;
        public NowPlaying np = null;
        public double currentVolume = 1.0f;
        private List<string> playedItems = new List<string>();
        private int playedItemsOffset = 0;
        private RenderIdle renderIdle = null;
        #endregion

        #region "Form - Init"
        public Main()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Responsible for loading the terminal's settings and connecting it to the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_Shown(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            // Check if the configuration file exists, else show the configurator
            if (!File.Exists(Application.StartupPath + "\\Settings.xml"))
            {
                ConfigGenerator cg = new ConfigGenerator();
                cg.Show();
                return;
            }
            try
            {
                // Create information pane
                np = new NowPlaying(this);
                np.Location = new Point(Location.X, Location.Y + Height - np.Height);
                np.Width = Width;
                // Load configuration
                settings = new XmlDocument();
                string rawxml;
                try
                {
                    rawxml = File.ReadAllText(Application.StartupPath + "\\Settings.xml");
                    settings.LoadXml(rawxml);
                }
                catch(Exception ex)
                {
                    SetErrorMessage("Could not load configuration!");
                    Misc.dumpError(APPLICATION_ID, ex);
                    return;
                }
                try
                {
                    // Establish connection settings
                    terminalID = settings["settings"]["connection"]["id"].InnerText;
                    libraryURL = settings["settings"]["connection"]["url"].InnerText;
                }
                catch(Exception ex)
                {
                    SetErrorMessage("Invalid configuration!");
                    Misc.dumpError(APPLICATION_ID, ex);
                    return;
                }
                // Start work processor
                workerProcessor = new Thread(new ParameterizedThreadStart(WorkProcessor));
                workerProcessor.Start(this);
                // Bring to front and hide cursor etc
                Refocus();
                cursorHide();
                np.displayMessage("Welcome!");
                // Init idle class
                renderIdle = new RenderIdle(this);
            }
            catch (Exception ex)
            {
                SetErrorMessage("Failed to load terminal:\r\n" + ex.Message);
                Misc.dumpError(APPLICATION_ID, ex);
                return;
            }
        }
        #endregion

        #region "Screensaver Headers"
        // Credit: Lincoln_MA - http://social.msdn.microsoft.com/Forums/br/csharpgeneral/thread/b951fc9f-8996-47a4-aaab-1131447d06ea
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE flags);
        [Flags]

        public enum EXECUTION_STATE : uint
        {
            ES_CONTINUOUS = 0x80000000
        }
        #endregion

        public static bool libraryIsAccessible = false;
        public static void WorkProcessor(object obj)
        {
            // Begin reading commands
            Main m = (Main)obj;
            StringBuilder url;
            int splitterIndex;
            string response = null;
            string command = null;
            string args = null;
            bool commandQueued = false;
            while (true)
            {
                // Tell windows to keep the screensaver disabled by resetting the timer
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                try
                {
                    // Update current position in information pane
                    try
                    {
                        if (m._currentInterface != null) m.np.updatePosition(m._currentInterface.Position, m._currentInterface.Duration);
                        else m.np.updatePositionEnd();
                    }
                    catch { System.Diagnostics.Debug.WriteLine("Failed to get position and duration!"); }
                    // Check if to get queued commands only
                    commandQueued = m._currentInterface == null; // Only use queued commands if the current media has finished
                    // Build the URL for updating the status etc
                    url = new StringBuilder();
                    url.Append(m.libraryURL + "/terminal/update?")     // Append base URL
                    // -- Append parameters of the request
                        .Append("&tid=").Append(m.terminalID)
                        .Append("&q=").Append(commandQueued ? "1" : "0");
                    if(m._currentInterface != null)
                        url
                        .Append("&state=").Append((int)m._currentInterface.State())
                        .Append("&volume=").Append(m._currentInterface.Volume)
                        .Append("&muted=").Append(m._currentInterface.IsMuted() ? "1" : "0")
                        .Append("&vitemid=").Append(m.vitemid)
                        .Append("&pos=").Append(m._currentInterface.Position)
                        .Append("&dur=").Append(m._currentInterface.Duration);
                    else
                        url
                        .Append("&state=").Append(13)
                        .Append("&volume=").Append(0)
                        .Append("&muted=").Append(1)
                        .Append("&vitemid=")
                        .Append("&pos=").Append(0)
                        .Append("&dur=").Append(0);

                    // Update the status of the terminal and get the latest command
                    try
                    {
                        response = Library.fetchData(url.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to update: " + ex.Message + " - " + ex.StackTrace + " ## Base: " + ex.GetBaseException().Message + " - " + ex.GetBaseException().StackTrace);
                        response = null;
                    }
                    // Check if we got a response - hence the library online
                    libraryIsAccessible = response != null;
                    // Process the response data
                    if (response != null && (splitterIndex = response.IndexOf(':')) != -1 && splitterIndex < response.Length)
                    {
                        // Parse the data
                        command = response.Substring(0, splitterIndex);
                        args = response.Substring(splitterIndex + 1);
                    }
                    // Fetch the next command
                    if (command != null && args != null)
                    {
                        // Process command
                        switch (command)
                        {
                            // Shutdown this terminal
                            // <no args>
                            case "shutdown":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlShutdown();
                                });
                                break;
                            // Play the current media
                            // <no args>
                            case "play":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlPlay();
                                });
                                break;
                            // Toggles play/pause of the current media
                            // <no args>
                            case "play_toggle":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlPlayToggle();
                                });
                                break;
                            // Stop the current media
                            // <no args>
                            case "stop":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlStop();
                                });
                                break;
                            // Pause the current media
                            // <no args>
                            case "pause":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlPause();
                                });
                                break;
                            // Sets the current volume of the interface.
                            // <volume from 0.0 to 1.0>
                            case "volume":
                                double value = -1;
                                double.TryParse(args, out value);
                                if (value != -1)
                                {
                                    m.Invoke((MethodInvoker)delegate()
                                    {
                                        m.controlVolume(value);
                                        m.currentVolume = value;
                                    });
                                }
                                break;
                            // Mutes the volume of the current media.
                            // <no args>
                            case "mute":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlVolumeMute();
                                });
                                break;
                            // Unmutes the volume of the current media.
                            // <no args>
                            case "unmute":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlVolumeUnmute();
                                });
                                break;
                            // Toggles the mute of the current media.
                            // <no args>
                            case "mute_toggle":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlVolumeMuteToggle();
                                });
                                break;
                            // Sets the position of the current media.
                            // <seconds>
                            case "position":
                                double value2 = -1;
                                double.TryParse(args, out value2);
                                if (value2 >= 0)
                                    m.Invoke((MethodInvoker)delegate()
                                    {
                                        m.controlPosition(value2);
                                    });
                                break;
                            // Plays the previous item in the playlist
                            // <no args>
                            case "previous":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlItemPrevious();
                                });
                                break;
                            // Plays the next item in the playlist
                            // <no args>
                            case "next":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlItemNext();
                                });
                                break;
                            // Skips the current media backwards by a few seconds
                            // <no args>
                            case "skip_backward":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlSkipBackward();
                                });
                                break;
                            // Skips the current media forward by a few seconds
                            // <no args>
                            case "skip_forward":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlSkipForward();
                                });
                                break;
                            // Change media
                            // <vitemid>
                            case "media":
                                // Add to history
                                m.playedItems.Add(args);
                                // If not queued, play now (reset the history index)
                                if (!commandQueued) m.playedItemsOffset = 0;
                                else if (m.playedItemsOffset > 0) m.playedItemsOffset++;
                                // Play media
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlMedia(m.playedItems[m.playedItems.Count - 1 - m.playedItemsOffset]);
                                });
                                break;
                            // API command
                            // <data>
                            case "api":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlAPI(args);
                                });
                                break;
                            // Restart the terminal
                            case "restart":
                                m.Invoke((MethodInvoker)delegate()
                                {
                                    m.controlRestart();
                                });
                                break;
                            // Unknown command - hault application and inform the user, could be critical!
                            default: m.np.displayMessage("Received unknown command '" + command + "'!"); break;
                        }
                    }
                    // No command executed, sleep for 200 m/s
                    else System.Threading.Thread.Sleep(200);

                }
                catch (Exception ex)
                {
                    libraryIsAccessible = false;
#if DEBUG
                    m.np.displayMessage("Worker processor failure, check output!");
                    System.Diagnostics.Debug.WriteLine(ex.Message + ": " + ex.StackTrace);
#endif
                    Misc.dumpError(APPLICATION_ID, ex);
                }
                finally
                {
                    command = null;
                    args = null;
                }
            }
        }

        #region "Event Hooking"
        /// <summary>
        /// Used to hook interfaces from the work processor to capture when the interface ends.
        /// </summary>
        public void HookInterfaceEvent_End()
        {
            try
            {
                if (_currentInterface != null)
                    lock (_currentInterface)
                        _currentInterface.MediaEnd += new Interfaces.Interface._MediaEnd(intf_MediaEnd);
            }
            catch(Exception ex)
            {
                np.displayMessage("Error: failed to unhook previous interface!");
                Misc.dumpError(APPLICATION_ID, ex);
            }
        }
        /// <summary>
        /// This methid is invoked when an interface media ends.
        /// </summary>
        void intf_MediaEnd()
        {
            Invoke((MethodInvoker)delegate()
            {
                try
                {
                    // Clear the information pane now playing text
                    np.updateNowPlaying(null, null);
                    // Dispose the interface
                    DisposeCurrentInterface();
                    // Play the next item if in an offset
                    if (playedItemsOffset > 0)
                        controlItemNext();
                }
                catch (Exception ex)
                {
                    np.displayMessage("Error: media-ended, error occurred!");
                    Misc.dumpError(APPLICATION_ID, ex);
                }
            });
        }
        #endregion

        #region "Misc Methods"
        /// <summary>
        /// Destroys the current interface; this will cause the next item to play as well.
        /// </summary>
        public void DisposeCurrentInterface()
        {
            if (_currentInterface == null) return;
            try
            {
                _currentInterface.MediaEnd -= new Interfaces.Interface._MediaEnd(intf_MediaEnd);
                _currentInterface.Dispose();
                _currentInterface = null;
            }
            catch (Exception ex)
            {
                Misc.dumpError(APPLICATION_ID, ex);
            }
        }
        /// <summary>
        /// Displays an error-message to the user.
        /// </summary>
        /// <param name="error"></param>
        public void SetErrorMessage(string error)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ERROR: " + error);
                DisposeCurrentInterface();
                np.displayMessage(error);
            }
            catch
            { }
        }
        /// <summary>
        /// Brings this form to the top of the application and focuses it - required for catching tv input.
        /// </summary>
        public void Refocus()
        {
            this.Invoke((MethodInvoker)delegate()
            {
                BringToFront();
                Focus();
            });
        }
        #endregion

        #region "Form - Input (Remote, keyboard etc)"
        const int WM_APPCOMMAND = 0x0319;
        const int APPCOMMAND_MEDIA_NEXTTRACK = 11;
        const int APPCOMMAND_MEDIA_PLAY_PAUSE = 14;
        const int APPCOMMAND_MEDIA_STOP = 13;
        const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 12;
        const int APPCOMMAND_VOLUME_UP = 10;
        const int APPCOMMAND_VOLUME_DOWN = 9;
        const int APPCOMMAND_MEDIA_PLAY = 46;
        const int APPCOMMAND_MEDIA_PAUSE = 47;
        const int APPCOMMAND_VOLUME_MUTE = 8;
        const int SPECIAL_FASTFORWARD = 3211264;
        const int SPECIAL_REWIND = 3276800;
        /// <summary>
        /// Captures global window messages used for e.g. tv remotes currently.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            // Thanks to http://richardhopton.blogspot.com/2009/08/responding-to-mediapresentation-buttons.html for an explanation on reading the LParam
            switch (m.Msg)
            {
                case WM_APPCOMMAND:
                    switch ((int)((uint)m.LParam >> 16 & ~0xf000))
                    {
                        case APPCOMMAND_MEDIA_NEXTTRACK:
                            controlItemNext();
                            break;
                        case APPCOMMAND_MEDIA_PLAY_PAUSE:
                            controlPlayToggle();
                            break;
                        case APPCOMMAND_MEDIA_STOP:
                            controlStop();
                            break;
                        case APPCOMMAND_MEDIA_PREVIOUSTRACK:
                            controlItemPrevious();
                            break;
                        case APPCOMMAND_VOLUME_DOWN:
                            if (_currentInterface != null) _currentInterface.Volume = _currentInterface.Volume < 0.05 ? 0 : _currentInterface.Volume - 0.05;
                            np.displayMessage("Volume: " + Math.Round(100 * _currentInterface.Volume, 0) + "%");
                            break;
                        case APPCOMMAND_VOLUME_UP:
                            if (_currentInterface != null) _currentInterface.Volume = _currentInterface.Volume > 0.95 ? 1 : _currentInterface.Volume + 0.05;
                            np.displayMessage("Volume: " + Math.Round(100 * _currentInterface.Volume, 0) + "%");
                            break;
                        case APPCOMMAND_MEDIA_PLAY:
                            if (_currentInterface != null) { _currentInterface.Play(); np.displayMessage("Playing..."); } break;
                        case APPCOMMAND_MEDIA_PAUSE:
                            if (_currentInterface != null) { _currentInterface.Pause(); np.displayMessage("Paused."); } break;
                        case APPCOMMAND_VOLUME_MUTE:
                            if (_currentInterface != null)
                                if (_currentInterface.IsMuted()) { _currentInterface.Unmute(); np.displayMessage("Volume: unmuted."); }
                                else { _currentInterface.Mute(); np.displayMessage("Volume: muted."); }
                            break;
                    }
                    switch (m.LParam.ToInt32())
                    {
                        case SPECIAL_FASTFORWARD:
                            if (_currentInterface != null) _currentInterface.Position += 5; np.fade(); break;
                        case SPECIAL_REWIND:
                            if (_currentInterface != null) _currentInterface.Position -= 5; np.fade(); break;
                    }
                    break;
            }
            //System.Diagnostics.Debug.WriteLine("WndProc: " + m.Msg + " - " + m.LParam.ToInt32() + " - " + m.WParam.ToInt32());
            base.WndProc(ref m);
        }
        /// <summary>
        /// Currently responsible for keyboard input.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.MediaPreviousTrack:
                    controlItemPrevious();
                    break;
                case Keys.MediaNextTrack:
                    controlItemNext();
                    break;
                case Keys.MediaPlayPause:
                    controlPlayToggle();
                    break;
                case Keys.MediaStop:
                    controlStop();
                    break;
            }
        }
        #endregion

        #region "Interface Control"
        public void controlPlay()
        {
            try
            { if (_currentInterface != null) _currentInterface.Play(); np.displayMessage("Playing..."); }
            catch { }
        }
        public void controlPause()
        {
            try
            { if (_currentInterface != null) _currentInterface.Pause(); np.displayMessage("Paused."); }
            catch { }
        }
        public void controlPlayToggle()
        {
            try
            {
                if (_currentInterface != null)
                {
                    if (_currentInterface.State() == Interfaces.Interface.States.Playing)
                    { _currentInterface.Pause(); np.displayMessage("Paused."); }
                    else
                    { _currentInterface.Play(); np.displayMessage("Playing..."); }
                }
            }
            catch { }
        }
        public void controlItemPrevious()
        {
            if (playedItemsOffset < playedItems.Count - 1) playedItemsOffset++;
            controlMedia(playedItems[playedItems.Count - 1 - playedItemsOffset]);
        }
        public void controlItemNext()
        {
            if (playedItemsOffset > 0)
            {
                playedItemsOffset--;
                // Play the next item in the history array
                controlMedia(playedItems[playedItems.Count - 1 - playedItemsOffset]);
            }
            else
                // Dispose the current interface - this will cause the work processor to grab new media
                DisposeCurrentInterface();
        }
        public void controlSkipBackward()
        {
            try
            { if (_currentInterface != null) _currentInterface.Position -= 10; np.displayMessage("Skipping backwards..."); }
            catch { }
        }
        public void controlSkipForward()
        {
            try
            { if (_currentInterface != null) _currentInterface.Position += 10; np.displayMessage("Skipping forwards..."); }
            catch { }
        }
        public void controlStop()
        {
            try
            { if (_currentInterface != null) _currentInterface.Stop(); np.displayMessage("Stopped."); }
            catch { }
        }
        public void controlShutdown()
        {
            // Dispose interface
            try
            { DisposeCurrentInterface(); }
            catch
            { }
            // Shutdown
            np.displayMessage("Shutting down...");
            Thread.Sleep(2500);
            Windows.shutdown(true);
            Environment.Exit(0);
        }
        public void controlRestart()
        {
            // Dispose interface
            DisposeCurrentInterface();
            // Restart
            np.displayMessage("Restarting terminal...");
            Thread.Sleep(500);
            Windows.applicationRestart();
        }
        public void controlPosition(double seconds)
        {
            try
            {
                if (seconds >= 0 && _currentInterface != null) _currentInterface.Position = seconds;
                np.fade(); // Show the pane
            }
            catch { }
        }
        public void controlVolume(double value)
        {
            try
            {
                if (_currentInterface != null && value >= 0.0 && value <= 1.0)
                {
                    _currentInterface.Volume = value;
                    np.displayMessage("Volume: " + Math.Round(value * 100, 0) + "%");
                }
            }
            catch { } 
        }
        public void controlVolumeMute()
        {
            try
            { if (_currentInterface != null) _currentInterface.Mute(); np.displayMessage("Muted."); }
            catch { }
        }
        public void controlVolumeUnmute()
        {
            try
            { if (_currentInterface != null) _currentInterface.Unmute(); np.displayMessage("Unmuted..."); }
            catch { }
        }
        public void controlVolumeMuteToggle()
        {
            try
            {
                if (_currentInterface != null)
                    if (_currentInterface.IsMuted()) controlVolumeUnmute();
                    else controlVolumeMute();
            }
            catch { }
        }
        public void controlAPI(string data)
        {
            try
            { if (_currentInterface != null) _currentInterface.API(data); }
            catch { }
        }
        private object controlMediaLock = new object();
        /// <summary>
        /// Changes the media being played to a different item.
        /// </summary>
        /// <param name="vitemid"></param>
        public void controlMedia(string _vitemid)
        {
            // Dispose previous interface
            DisposeCurrentInterface();
            // Create new interface - thread-lock this procedure to avoid multi-threading issues
            lock (controlMediaLock)
            {
                // Fetch the virtual item details
                string mediaTitle = null;
                string mediaInterface = null;
                string mediaPath = null;
                string data = null;
                try
                {
                    data = Library.fetchData(libraryURL + "/terminal/media?vitemid=" + _vitemid);
                    WebPacket wp = new WebPacket(data);
                    if (wp.errorCode == WebPacket.ErrorCode.ERROR) throw new Exception("Web packet error '" + wp.errorMessage + "'!");
                    else if (wp.arguments.Count != 3)
                        throw new LibraryConnectionFailure("Malformed argument count!");
                    else
                    {
                        // -- Interface, Path, Title
                        mediaTitle = wp.arguments[2];
                        mediaPath = wp.arguments[1];
                        mediaInterface = wp.arguments[0];
                    }
                }
                catch (Exception ex)
                {
                    Misc.dumpError(APPLICATION_ID, ex);
                    throw new LibraryConnectionFailure("Failed to retrieve media information: '" + ex.Message + "' - data: '" + data + "'!", ex);
                }
                try
                {
                    System.Diagnostics.Debug.WriteLine("Playing new item: " + mediaTitle + " : " + mediaInterface + " : " + mediaPath);
                    _currentInterface = (Interfaces.Interface)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance("UberMediaServer.Interfaces." + mediaInterface, false, System.Reflection.BindingFlags.CreateInstance, null,
                        new object[] { this, mediaPath, vitemid }, null, null);
                    HookInterfaceEvent_End();
                    // Set the volume
                    _currentInterface.Volume = currentVolume;
                    // Update information pane
                    np.displayMessage("Now playing:\t\t" + mediaTitle);
                    np.updateNowPlaying(mediaTitle, _vitemid);
                    vitemid = _vitemid;
                    Refocus();
                }
                catch (Exception ex)
                {
                    Misc.dumpError(APPLICATION_ID, ex);
                    System.Diagnostics.Debug.WriteLine(ex.Message + ": " + ex.StackTrace);
                    DisposeCurrentInterface();
                    np.displayMessage("Could not play item '" + vitemid + "' - " + ex.Message + "!");
                }
            }
        }
        #endregion

        #region "Methods - Form"
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
        private const string paintLibraryUnconnected = "Unable to connect to the media library...";
        private static Font paintFont = new Font("Arial", 30.0f, FontStyle.Regular, GraphicsUnit.Pixel);
        private static Font paintFontTime = new Font("Arial", 50.0f, FontStyle.Bold, GraphicsUnit.Pixel);
        private static Brush paintBrush = new SolidBrush(Color.White);
        private string paintCacheDate = string.Empty;
        private string paintCacheTime = string.Empty;
        private void Main_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                // If the current interface is null, draw the idle information
                if (_currentInterface == null)
                {
                    // Draw our magic balls :D - only if the library is accessible
                    if (renderIdle != null && libraryIsAccessible)
                        renderIdle.draw(e.Graphics);
                    SizeF sizeDate = e.Graphics.MeasureString(paintCacheDate, paintFont);
                    // Draw date
                    e.Graphics.DrawString(paintCacheDate, paintFont, paintBrush, 10, 10);
                    // Draw time
                    e.Graphics.DrawString(paintCacheTime, paintFontTime, paintBrush, 10, 10 + sizeDate.Height + 5);
                    // Draw online
                    if (!libraryIsAccessible)
                    {
                        SizeF sizeAccessible = e.Graphics.MeasureString(paintLibraryUnconnected, paintFont);
                        e.Graphics.DrawString(paintLibraryUnconnected, paintFont, paintBrush, Width - (10 + sizeAccessible.Width), Height - (10 + sizeAccessible.Height));
                    }
                }
            }
            catch (Exception ex)
            {
                Misc.dumpError(APPLICATION_ID, ex);
            }
        }
        private void invalidateTimer_Tick(object sender, EventArgs e)
        {
            if (_currentInterface == null && renderIdle != null)
            {
                renderIdle.logic();
                paintCacheDate = DateTime.Now.ToString("dd/MM/yyyy");
                paintCacheTime = DateTime.Now.ToString("HH:mm:ss");
                Invalidate();
            }
        }
        public void cursorShow()
        {
            Cursor.Show();
        }
        public void cursorHide()
        {
            Cursor.Hide();
        }
        #endregion
    }
}
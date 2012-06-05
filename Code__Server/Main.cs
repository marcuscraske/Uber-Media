/*
 * License:     Creative Commons Attribution-ShareAlike 3.0 unported
 * File:        Main.cs
 * Author(s):   limpygnome
 * 
 * Responsible for displaying media to the end-user on the physical machine through a borderless
 * window; this is also responsible for processing terminal commands and loading configuration.
 * 
 * Improvements/bugs:
 *          -   none.
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

namespace UberMediaServer
{
    public partial class Main : Form
    {
        #region "Variables"
        private Interfaces.Interface _currentInterface = null;
        private string vitemid = string.Empty;
        public string terminalID;
        public string libraryURL;
        private XmlDocument settings;
        private Thread workerProcessor = null;
        public NowPlaying np = null;
        public double currentVolume = 1.0f;
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
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Settings.xml"))
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
                    rawxml = File.ReadAllText("Settings.xml");
                    settings.LoadXml(rawxml);
                }
                catch
                {
                    SetErrorMessage("Could not load configuration!");
                    return;
                }
                try
                {
                    // Establish connection settings
                    terminalID = settings["settings"]["connection"]["id"].InnerText;
                    libraryURL = settings["settings"]["connection"]["url"].InnerText;
                }
                catch
                {
                    SetErrorMessage("Invalid configuration!");
                    return;
                }
                // Start work processor
                workerProcessor = new Thread(new ParameterizedThreadStart(WorkProcessor));
                workerProcessor.Start(this);
                // Bring to front and hide cursor etc
                Refocus();
                Cursor.Hide();
                np.displayMessage("Welcome!");
            }
            catch (Exception ex)
            {
                SetErrorMessage("Failed to load terminal:\r\n" + ex.Message);
            }
        }
        #endregion

        public static void WorkProcessor(object obj)
        {
            Main m = (Main)obj;
            StringBuilder url;
            int splitterIndex;
            string response = null;
            string command = null;
            string args = null;
            while (true)
            {
                try
                {
                    // Update current position in information pane
                    try
                    {
                        if (m._currentInterface != null) m.np.updatePosition(m._currentInterface.Position, m._currentInterface.Duration);
                        else m.np.updatePositionEnd();
                    }
                    catch { System.Diagnostics.Debug.WriteLine("Failed to get position and duration!"); }
                    // Build the URL for updating the status etc
                    url = new StringBuilder();
                    url.Append(m.libraryURL + "/terminal/update?")     // Append base URL
                    // -- Append parameters of the request
                        .Append("&tid=").Append(m.terminalID)
                        .Append("&q=").Append(m._currentInterface == null || m._currentInterface.State() == Interfaces.Interface.States.Idle ? "1" : "0");
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
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to update: " + ex.Message + " - " + ex.StackTrace + " ## Base: " + ex.GetBaseException().Message + " - " + ex.GetBaseException().StackTrace);
                        response = null;
                    }
                    // Process the response data
                    if (response != null && (splitterIndex = response.IndexOf(':')) != -1 && splitterIndex < response.Length)
                    {
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
                                m.controlShutdown();
                                break;
                            // Play the current media
                            // <no args>
                            case "play":
                                m.controlPlay();
                                break;
                            // Toggles play/pause of the current media
                            // <no args>
                            case "play_toggle":
                                m.controlPlayToggle();
                                break;
                            // Stop the current media
                            // <no args>
                            case "stop":
                                m.controlStop();
                                break;
                            // Pause the current media
                            // <no args>
                            case "pause":
                                m.controlPause();
                                break;
                            // Sets the current volume of the interface.
                            // <volume from 0.0 to 1.0>
                            case "volume":
                                double value = -1;
                                double.TryParse(args, out value);
                                if (value != -1)
                                {
                                    m.controlVolume(value);
                                    m.currentVolume = value;
                                }
                                break;
                            // Mutes the volume of the current media.
                            // <no args>
                            case "mute":
                                m.controlVolumeMute();
                                break;
                            // Unmutes the volume of the current media.
                            // <no args>
                            case "unmute":
                                m.controlVolumeUnmute();
                                break;
                            // Toggles the mute of the current media.
                            // <no args>
                            case "mute_toggle":
                                m.controlVolumeMuteToggle();
                                break;
                            // Sets the position of the current media.
                            // <seconds>
                            case "position":
                                double value2 = -1;
                                double.TryParse(args, out value2);
                                m.controlPosition(value2);
                                break;
                            // Plays the previous item in the playlist
                            // <no args>
                            case "previous":
                                m.controlItemPrevious();
                                break;
                            // Plays the next item in the playlist
                            // <no args>
                            case "next":
                                m.controlItemNext();
                                break;
                            // Skips the current media backwards by a few seconds
                            // <no args>
                            case "skip_backward":
                                m.controlSkipBackward();
                                break;
                            // Skips the current media forward by a few seconds
                            // <no args>
                            case "skip_forward":
                                m.controlSkipForward();
                                break;
                            // Change media
                            // <vitemid>
                            case "media":
                                m.controlMedia(args);
                                break;
                            // API command
                            // <data>
                            case "api":
                                m.controlAPI(args);
                                break;
                            // Restart the terminal
                            case "restart":
                                m.controlRestart();
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
#if DEBUG
                    m.np.displayMessage("Worker processor failure, check output!");
                    System.Diagnostics.Debug.WriteLine(ex.Message + ": " + ex.StackTrace);
#endif
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
            if(_currentInterface != null) _currentInterface.MediaEnd += new Interfaces.Interface._MediaEnd(intf_MediaEnd);
        }
        /// <summary>
        /// This methid is invoked when an interface media ends.
        /// </summary>
        void intf_MediaEnd()
        {
            // Clear the information pane now playing text
            np.updateNowPlaying(null, null);
            // Play the next item
            controlItemNext();
        }
        #endregion

        #region "Misc Methods"
        /// <summary>
        /// Destroys the current interface; this will cause the next item to play as well.
        /// </summary>
        public void DisposeCurrentInterface()
        {
            if (_currentInterface == null) return;
            _currentInterface.Dispose();
            _currentInterface = null;
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
            catch { }
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
            // Not implemented yet.
        }
        public void controlItemNext()
        {
            try
            { if(_currentInterface != null) DisposeCurrentInterface(); }
            catch { }
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
            DisposeCurrentInterface();
            // Shutdown worker processor
            workerProcessor.Abort();
            workerProcessor = null;
            // Dispose and shutdown
            np.displayMessage("Shutting down...");
            Thread.Sleep(2500);
#if !DEBUG
                                System.Diagnostics.Process.Start("shutdown", "-s -t 0");
#endif
            Environment.Exit(0);
        }
        public void controlRestart()
        {
            // Dispose interface
            DisposeCurrentInterface();
            // Shutdown worker processor
            workerProcessor.Abort();
            workerProcessor = null;
            // Restart
            np.displayMessage("Restarting terminal...");
            Thread.Sleep(500);
            Application.Restart();
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
        /// <summary>
        /// Changes the media being played to a different item.
        /// </summary>
        /// <param name="vitemid"></param>
        public void controlMedia(string _vitemid)
        {
            DisposeCurrentInterface();
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
            catch(Exception ex)
            {
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
                System.Diagnostics.Debug.WriteLine(ex.Message + ": " + ex.StackTrace);
                DisposeCurrentInterface();
                throw new Exception("Could not play item '" + vitemid + "': " + ex.Message + "!");
            }
        }
        #endregion

        #region "Form"
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
        #endregion
    }
}
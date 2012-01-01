using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using UberLib.Connector;
using UberLib.Connector.Connectors;
using System.Threading;
using System.IO;

namespace UberMediaServer
{
    public partial class Main : Form
    {
        #region "Variables"
        Interfaces.Interface _currentInterface = null;
        string vitemid = "";
        string terminalKey = null;
        XmlDocument settings;
        Connector db = null;
        Thread workerProcessor = null;
        NowPlaying np = null;
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
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Settings.xml"))
            {
                ConfigGenerator cg = new ConfigGenerator();
                cg.Show();
                return;
            }
            try
            {
                // Create information pane
                np = new NowPlaying();
                np.Location = new Point(Location.X, Location.Y + Height - np.Height);
                np.Width = Width;
                // Load configuration
                settings = new XmlDocument();
                try
                {
                    string rawxml = File.ReadAllText("Settings.xml");
                    settings.LoadXml(rawxml);
                    // Establish terminal settings
                    terminalKey = settings["settings"]["terminal"]["key"].InnerText;
                    // Establish database settings
                    if (settings["settings"]["database"]["type"].InnerText == "mysql")
                    {
                        MySQL _db = new MySQL();
                        _db.Settings_Host = settings["settings"]["database"]["host"].InnerText;
                        _db.Settings_Port = int.Parse(settings["settings"]["database"]["port"].InnerText);
                        _db.Settings_User = settings["settings"]["database"]["user"].InnerText;
                        _db.Settings_Pass = settings["settings"]["database"]["pass"].InnerText;
                        _db.Settings_Database = settings["settings"]["database"]["db"].InnerText;
                        db = _db;
                    }
                }
                catch
                {
                    SetErrorMessage("Invalid configuration!");
                    return;
                }
                try
                {
                    // Connect to the database
                    db.Connect();
                }
                catch
                {
                    SetErrorMessage("Could not connect to database server...");
                    return;
                }
                // Check connection is working by polling commands table
                try
                {
                    db.Query_Execute("SELECT * FROM terminal_buffer");
                }
                catch
                {
                    SetErrorMessage("Could not access database...");
                    return;
                }
                // Start work processor
                workerProcessor = new Thread(new ParameterizedThreadStart(WorkProcessor));
                workerProcessor.Start(this);
                // Bring to front and hide cursor etc
                Refocus();
                Cursor.Hide();
                np.DisplayMessage("Welcome!");
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
            Result data;
            ResultRow command = null;
            string preQuery = String.Empty;
            string terminalKey = Utils.Escape(m.terminalKey);
            while (true)
            {
                try
                {
                    // Ensure the database connection is still open, just in-case it failed
                    m.db.CheckConnectionIsReady();
                    // Update current position in information pane
                    try
                    {
                        if (m._currentInterface != null) m.np.UpdatePosition(m._currentInterface.Position, m._currentInterface.Duration);
                        else m.np.UpdatePositionEnd();
                    }
                    catch { }
                    // Update the database with the terminals status
                    try
                    {
                        if (m._currentInterface != null)
                            m.db.Query_Execute("UPDATE terminals SET status_state='" + Utils.Escape(((int)m._currentInterface.State()).ToString()) + "', status_volume='" + Utils.Escape(m._currentInterface.Volume.ToString()) + "', status_volume_muted='" + Utils.Escape(m._currentInterface.IsMuted() ? "1" : "0") + "', status_vitemid='" + Utils.Escape(m.vitemid) + "', status_position='" + Utils.Escape(m._currentInterface.Position.ToString()) + "', status_duration='" + Utils.Escape(m._currentInterface.Duration.ToString()) + "', status_updated=NOW() WHERE tkey='" + terminalKey + "'");
                        else
                            m.db.Query_Execute("UPDATE terminals SET status_state='13', status_volume='0', status_volume_muted='1', status_vitemid='', status_position='0', status_duration='0', status_updated=NOW() WHERE tkey='" + terminalKey + "'");
                    }
                    catch { }
                    // Fetch the next command
                    data = m.db.Query_Read(preQuery + "SELECT tb.* FROM terminal_buffer AS tb, terminals AS t WHERE " + (m._currentInterface == null || m._currentInterface.State() == Interfaces.Interface.States.Idle ? "" :  "queue='0' AND ") + "tb.terminalid=t.terminalid AND t.tkey='" + terminalKey + "' ORDER BY tb.cid ASC LIMIT 1");
                    preQuery = String.Empty; // Reset pre-query
                    if (data.Rows.Count > 0)
                    {
                        // Process command
                        command = data[0];
                        switch (command["command"])
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
                                double.TryParse(command["arguments"], out value);
                                m.controlVolume(value);
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
                                double.TryParse(command["arguments"], out value2);
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
                                m.controlMedia(command["arguments"]);
                                break;
                            // API command
                            // <data>
                            case "api":
                                m.controlAPI(command["arguments"]);
                                break;
                            // Unknown command - hault application and inform the user, could be critical!
                            default: m.np.DisplayMessage("Received unknown command '" + command["command"] + "'!"); break;
                        }
                    }
                    // No command executed, sleep for 200 m/s
                    else System.Threading.Thread.Sleep(200);
                }
                catch (Exception ex)
                {
#if DEBUG
                    m.np.DisplayMessage("Worker processor failure, check output!");
                    System.Diagnostics.Debug.WriteLine(ex.Message + ": " + ex.StackTrace);
#endif
                }
                finally
                {
                    // Delete command - add this to a buffer to execute it next time we poll
                    if (command != null)
                    {
                        preQuery = "DELETE FROM terminal_buffer WHERE cid='" + Utils.Escape(command["cid"]) + "';";
                        command = null;
                    }
                }
            }
        }

        #region "Event Hooking"
        /// <summary>
        /// Used to hook interfaces from the work processor to capture when the interface ends.
        /// </summary>
        public void HookInterfaceEvent_End()
        {
            _currentInterface.MediaEnd += new Interfaces.Interface._MediaEnd(intf_MediaEnd);
        }
        /// <summary>
        /// This methid is invoked when an interface media ends.
        /// </summary>
        void intf_MediaEnd()
        {
            // Clear the information pane now playing text
            np.UpdateNowPlaying(null);
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
                np.DisplayMessage(error);
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
                            np.DisplayMessage("Volume: " + Math.Round(100 * _currentInterface.Volume, 0) + "%");
                            break;
                        case APPCOMMAND_VOLUME_UP:
                            if (_currentInterface != null) _currentInterface.Volume = _currentInterface.Volume > 0.95 ? 1 : _currentInterface.Volume + 0.05;
                            np.DisplayMessage("Volume: " + Math.Round(100 * _currentInterface.Volume, 0) + "%");
                            break;
                        case APPCOMMAND_MEDIA_PLAY:
                            if (_currentInterface != null) { _currentInterface.Play(); np.DisplayMessage("Playing..."); } break;
                        case APPCOMMAND_MEDIA_PAUSE:
                            if (_currentInterface != null) { _currentInterface.Pause(); np.DisplayMessage("Paused."); } break;
                        case APPCOMMAND_VOLUME_MUTE:
                            if (_currentInterface != null)
                                if (_currentInterface.IsMuted()) { _currentInterface.Unmute(); np.DisplayMessage("Volume: unmuted."); }
                                else { _currentInterface.Mute(); np.DisplayMessage("Volume: muted."); }
                            break;
                    }
                    switch (m.LParam.ToInt32())
                    {
                        case SPECIAL_FASTFORWARD:
                            if (_currentInterface != null) _currentInterface.Position += 5; np.Fade(); break;
                        case SPECIAL_REWIND:
                            if (_currentInterface != null) _currentInterface.Position -= 5; np.Fade(); break;
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
            { if (_currentInterface != null) _currentInterface.Play(); np.DisplayMessage("Playing..."); }
            catch { }
        }
        public void controlPause()
        {
            try
            { if (_currentInterface != null) _currentInterface.Pause(); np.DisplayMessage("Paused."); }
            catch { }
        }
        public void controlPlayToggle()
        {
            try
            {
                if (_currentInterface != null)
                {
                    System.Diagnostics.Debug.WriteLine(_currentInterface.State());
                    if (_currentInterface.State() == Interfaces.Interface.States.Playing)
                    { _currentInterface.Pause(); np.DisplayMessage("Paused."); }
                    else
                    { _currentInterface.Play(); np.DisplayMessage("Playing..."); }
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
            { if (_currentInterface != null) _currentInterface.Position -= 10; np.DisplayMessage("Skipping backwards..."); }
            catch { }
        }
        public void controlSkipForward()
        {
            try
            { if (_currentInterface != null) _currentInterface.Position += 10; np.DisplayMessage("Skipping forwards..."); }
            catch { }
        }
        public void controlStop()
        {
            try
            { if (_currentInterface != null) _currentInterface.Stop(); np.DisplayMessage("Stopped."); }
            catch { }
        }
        public void controlShutdown()
        {
            // Dispose interface
            DisposeCurrentInterface();
            // Delete all commands
            try
            { db.Query_Execute("DELETE FROM terminal_buffer WHERE EXISTS(SELECT terminalid FROM terminals WHERE tkey='" + terminalKey + "' AND terminalid=terminal_buffer.terminalid);"); db.Disconnect(); }
            catch { }
            // Shutdown worker processor
            workerProcessor.Abort();
            workerProcessor = null;
            // Dispose and shutdown
            np.DisplayMessage("Shutting down...");
            Thread.Sleep(2500);
#if !DEBUG
                                System.Diagnostics.Process.Start("shutdown", "-s -t 0");
#endif
            Environment.Exit(0);
        }
        public void controlPosition(double seconds)
        {
            try
            {
                if (seconds >= 0 && _currentInterface != null) _currentInterface.Position = seconds;
                np.Fade(); // Show the pane
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
                    np.DisplayMessage("Volume: " + Math.Round(value * 100, 0) + "%");
                }
            }
            catch { } 
        }
        public void controlVolumeMute()
        {
            try
            { if (_currentInterface != null) _currentInterface.Mute(); np.DisplayMessage("Muted."); }
            catch { }
        }
        public void controlVolumeUnmute()
        {
            try
            { if (_currentInterface != null) _currentInterface.Unmute(); np.DisplayMessage("Unmuted..."); }
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
            Result data = db.Query_Read("SELECT it.interface, (CONCAT(pf.physicalpath, vi.phy_path)) AS path, vi.title, vi.vitemid FROM (virtual_items AS vi, physical_folders AS pf) LEFT OUTER JOIN item_types AS it ON it.uid=vi.type_uid WHERE pf.pfolderid=vi.pfolderid AND vi.vitemid='" + Utils.Escape(_vitemid) + "';");
            try
            {
                ResultRow query = data[0];
                _currentInterface = (Interfaces.Interface)System.Reflection.Assembly.GetExecutingAssembly().CreateInstance("UberMediaServer.Interfaces." + query["interface"], false, System.Reflection.BindingFlags.CreateInstance, null,
                    new object[] { this, query["path"], vitemid }, null, null);
                HookInterfaceEvent_End();
                // Update information pane
                np.DisplayMessage("Now playing:\t\t" + query["title"]);
                np.UpdateNowPlaying(query["title"]);
                vitemid = query["vitemid"];
                Refocus();
                // Update views
                db.Query_Execute("UPDATE virtual_items SET views = views + 1 WHERE vitemid='" + Utils.Escape(data[0]["vitemid"]) + "';");
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
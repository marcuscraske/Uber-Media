/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /Settings.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 * Used to configure the launcher.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using UberMedia.Shared;

namespace WebsiteLauncher
{
    public partial class Settings : Form
    {
        #region "Constants"
        public string PATH_MYSQLCONFIG_RAW = Application.StartupPath + "\\Database\\my.ini.raw";
        public string PATH_MYSQLCONFIG = Application.StartupPath + "\\Database\\my.ini";
        public const int PORT_MIN = 1;
        public const int PORT_MAX = 65535;
        #endregion

        #region "Variables"
        private bool firstTimeConfig;
        #endregion

        #region "Methods - Constructors"
        public Settings(bool firstTimeConfig)
        {
            this.firstTimeConfig = firstTimeConfig;
            InitializeComponent();
        }
        #endregion

        #region "Methods - Events"
        // Based on http://www.codeproject.com/Articles/20379/Disabling-Close-Button-on-Forms
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }
        private void buttExit_MouseClick(object sender, MouseEventArgs e)
        {
            if (firstTimeConfig)
                Application.Exit();
            else
                Hide();
        }
        private void buttContinue_MouseClick(object sender, MouseEventArgs e)
        {
            string error = null;

            bool launchWeb = cbLaunchWeb.Checked;
            bool launchDb = cbLaunchDatabase.Checked;
            bool launchHide = cbHideWindows.Checked;
            bool createShortcut = cbShortcut.Checked;
            string rawPortWeb = txtPortWeb.Text;
            string rawPortDb = txtPortDatabase.Text;
            // Validate
            int portWeb, portDb;
            if (!int.TryParse(rawPortWeb, out portWeb))
                error = "Invalid web port!";
            else if (!int.TryParse(rawPortDb, out portDb))
                error = "Invalid database port!";
            else if(portWeb < PORT_MIN || portWeb > PORT_MAX)
                error = "Web port must be inclusive of " + PORT_MIN + " to " + PORT_MAX + "!";
            else if (portDb < PORT_MIN || portDb > PORT_MAX)
                error = "Database port must be inclusive of " + PORT_MIN + " to " + PORT_MAX + "!";
            else
            {
                try
                {
                    // Write database port
                    StringBuilder data = new StringBuilder(File.ReadAllText(PATH_MYSQLCONFIG_RAW));
                    data.Replace("%PORT%", portDb.ToString());
                    File.WriteAllText(PATH_MYSQLCONFIG, data.ToString());
                }
                catch (Exception ex)
                {
                    error = "Failed to write database port configuration, the following error occurred:\r\n" + ex.Message + "\r\n\r\nStack-trace:\r\n" + ex.StackTrace;
                }
                try
                {
                    // Write Config.xml
                    StringWriter sw = new StringWriter();
                    XmlWriter xw = XmlWriter.Create(sw);
                    // Start document
                    xw.WriteStartDocument();
                    xw.WriteStartElement("settings");

                    // Launch info
                    xw.WriteStartElement("launch");
                    // -- Web
                    xw.WriteStartElement("web");
                    xw.WriteCData(launchWeb ? "1" : "0");
                    xw.WriteEndElement();
                    // -- Database
                    xw.WriteStartElement("database");
                    xw.WriteCData(launchDb ? "1" : "0");
                    xw.WriteEndElement();

                    xw.WriteEndElement();

                    // Web server port
                    xw.WriteStartElement("web_port");
                    xw.WriteCData(portWeb.ToString());
                    xw.WriteEndElement();

                    // Database server port
                    xw.WriteStartElement("database_port");
                    xw.WriteCData(portWeb.ToString());
                    xw.WriteEndElement();

                    // Hide windows
                    xw.WriteStartElement("hide_windows");
                    xw.WriteCData(launchHide ? "1" : "0");
                    xw.WriteEndElement();

                    // End document
                    xw.WriteEndElement();
                    xw.WriteEndDocument();
                    xw.Close();
                    // Write the config to file
                    File.WriteAllText(Program.PATH_CONFIG, sw.ToString());
                }
                catch (Exception ex)
                {
                    error = "Failed to write Launcher.xml configuration, the following error occurred:\r\n" + ex.Message + "\r\n\r\nStack-trace:\r\n" + ex.StackTrace;
                }
                if (createShortcut)
                {
                    try
                    {
                        // Create desktop shortcut
                        Misc.createShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "UberMedia Media Library", Application.ExecutablePath, "http://" + Environment.MachineName + ":" + portWeb + "/", true, true);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Failed to create desktop shortcut; this is not critical and has been ignored. Error:\r\n" + ex.Message + "\r\n\r\nStack-trace:\r\n" + ex.StackTrace, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                if (firstTimeConfig)
                {
                    // Display the completion form
                    new Complete(portWeb).Show();
                    // Hide this form - no longer required
                    Hide();
                }
                else
                    Application.Restart();
            }
            // Check if an error has occurred
            if (error != null)
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void Settings_Load(object sender, EventArgs e)
        {
            // If this is not the first time configuring the launcher, show previous settings
            if (!firstTimeConfig)
            {
                // Disable creating a shortcut
                cbShortcut.Checked = false;
                // Load existing settings to determine the current values
                XmlDocument doc;
                try
                {
                    string config = File.ReadAllText(Program.PATH_CONFIG);
                    doc = new XmlDocument();
                    doc.LoadXml(config);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load configuration! If this persists, delete the file 'Launcher.xml' in the same directory as this application (" + Application.StartupPath + ")...\r\n\r\nError:\r\n" + ex.Message + "\r\n\r\nStack-trace:\r\n" + ex.StackTrace, "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    return;
                }
                // Hide windows
                try
                {
                    cbHideWindows.Checked = doc["settings"]["hide_windows"].InnerText == "1";
                }
                catch { }
                // Web startup
                try
                {
                    cbLaunchWeb.Checked = doc["settings"]["launch"]["web"].InnerText == "1";
                }
                catch { }
                // Database startup
                try
                {
                    cbLaunchDatabase.Checked = doc["settings"]["launch"]["database"].InnerText == "1";
                }
                catch { }
                // Database port
                try
                {
                    txtPortWeb.Text = doc["settings"]["database_port"].InnerText;
                }
                catch { }
                // Web port
                try
                {
                    txtPortWeb.Text = doc["settings"]["web_port"].InnerText;
                }
                catch { }
            }
        }
        #endregion
    }
}

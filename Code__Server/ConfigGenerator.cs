/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /ConfigGenerator.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 *A user-interface for setting-up the terminal.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using Microsoft.Win32;
using System.Xml;
using UberMedia.Shared;

namespace UberMediaServer
{
    public partial class ConfigGenerator : Form
    {
        #region "Constants - Copied from website project"
        const int URL_LENGTH_MIN = 8;
        const int URL_LENGTH_MAX = 2000;
        const int TERMINAL_TITLE_MIN = 1;
        const int TERMINAL_TITLE_MAX = 28;
        const int WINDOWS_PASSWORD_MAX = 127; // Based on http://exchangepedia.com/2007/01/what-is-the-real-maximum-password-length.html
        #endregion

        public ConfigGenerator()
        {
            InitializeComponent();
        }
        private void buttContinue_MouseClick(object sender, MouseEventArgs e)
        {
            string error = null;
            
            string terminalTitle = txtTerminalTitle.Text;
            string website = txtWebsite.Text;
            string windowsDomain = txtWindowsDomain.Text;
            string windowsUsername = txtWindowsUsername.Text;
            string windowsPassword = txtWindowsPassword.Text;
            bool autoStart = cbAutoStartup.Checked;
            bool autoLogon = cbAutoLogon.Checked;
            bool desktopShortcut = cbDesktopShortcut.Checked;
            // Format the fields
            if(website.EndsWith("/")) website = website.Remove(website.Length - 1, 1);
            // Validate the fields
            if (terminalTitle.Length < TERMINAL_TITLE_MIN || terminalTitle.Length > TERMINAL_TITLE_MAX)
                error = "Terminal title must be " + TERMINAL_TITLE_MIN + " to " + TERMINAL_TITLE_MIN + " characters in length!";
            else if (website.Length < URL_LENGTH_MIN || website.Length > URL_LENGTH_MAX)
                error = "Invalid URL!";
            else if (windowsPassword.Length > WINDOWS_PASSWORD_MAX)
                error = "Invalid Windows password!";
            else
            {
                // Attempt to register the terminal
                try
                {
                    string response = Library.fetchData(website + "/terminal/register?title=" + terminalTitle);
                    if (response.StartsWith("ERROR:") && response.Length > 6)
                        // Error thrown by the media library
                        error = "Media library server threw the following error whilst registering:\r\n\r\n" + response.Substring(6);
                    else if (response.StartsWith("SUCCESS:") && response.Length > 8)
                    {
                        // Successfully registered, parse and save the terminal ID
                        int tid;
                        if (!int.TryParse(response.Substring(8), out tid))
                            error = "Invalid terminal identifier returned! Response from server:\r\n\r\n" + response;
                        else
                        {

                            StringWriter sw = new StringWriter();
                            XmlWriter xw = XmlWriter.Create(sw);
                            // Start document
                            xw.WriteStartDocument();
                            xw.WriteStartElement("settings");

                            // Connection info
                            xw.WriteStartElement("connection");
                            // -- Terminal identifier
                            xw.WriteStartElement("id");
                            xw.WriteCData(tid.ToString());
                            xw.WriteEndElement();
                            // -- URL of the media library
                            xw.WriteStartElement("url");
                            xw.WriteCData(website);
                            xw.WriteEndElement();

                            xw.WriteEndElement();
                            // End document
                            xw.WriteEndElement();
                            xw.WriteEndDocument();
                            xw.Close();
                            // Write the config to file
                            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/Settings.xml", sw.ToString());
                            // Check if to automatically start-up
                            try
                            {
                                if (autoStart)
                                {
                                    RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                                    if (reg == null)
                                        error = "Could not make this application automatically start-up!";
                                    else
                                    {
                                        reg.SetValue("Uber Media - Server", Application.ExecutablePath);
                                        reg.Close();
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                error = "Error occurred trying to make the application auto-start:\r\n\r\n" + ex.Message + "\r\n\r\nStack-trace:\r\n" + ex.GetBaseException().StackTrace;
                            }
                            // Check if to automatically logon
                            try
                            {
                                if (autoLogon)
                                {
                                    RegistryKey reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
                                    if (reg == null)
                                        error = "Cannot access Winlogon registry subkey for automatic logon!";
                                    else
                                    {
                                        reg.SetValue("DefaultUserName", windowsUsername);
                                        reg.SetValue("DefaultDomainName", windowsDomain);
                                        reg.SetValue("DefaultPassword", windowsPassword);
                                        reg.SetValue("AutoAdminLogon", "1");
                                        reg.Close();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                error = "Error occurred trying to make the application auto-logon:\r\n\r\n" + ex.Message + "\r\n\r\nStack-trace:\r\n" + ex.GetBaseException().StackTrace;
                            }
                            if (error != null)
                                MessageBox.Show(error, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            // Check if to make a desktop shortcut
                            if (desktopShortcut)
                            {
                                try
                                {
                                    // Create desktop shortcut
                                    Misc.createShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "UberMedia Media Terminal", Application.ExecutablePath, Application.ExecutablePath, true, false);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Failed to create desktop shortcut; this is not critical and has been ignored. Error:\r\n" + ex.Message + "\r\n\r\nStack-trace:\r\n" + ex.StackTrace, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                            // Restart the application
                            Application.Restart();
                        }
                    }
                    else
                        // Unknown response
                        error = "Unknown response from URL, most likely not a media library server or a server error occurred!\r\n\r\nReceived:\r\n" + response;
                }
                catch (Exception ex)
                {
                    error = "Failed to register terminal, check the website of your media library is accessible from this terminal!\r\n\r\nError details:\r\n" + ex.Message + "\r\n\r\nStack-trace:\r\n" + ex.GetBaseException().StackTrace;
                }
            }
            // Check if an error has occurred
            if (error != null)
                MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void buttExit_MouseClick(object sender, MouseEventArgs e)
        {
            Application.Exit();
        }
        private void ConfigGenerator_Load(object sender, EventArgs e)
        {
            // Set the boxers for the automatic logon with the current user
            txtWindowsDomain.Text = Environment.UserDomainName;
            txtWindowsUsername.Text = Environment.UserName;
        }
    }
}

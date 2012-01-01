using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using UberLib.Connector;
using UberLib.Connector.Connectors;
using Microsoft.Win32;
using System.Xml;

namespace UberMediaServer
{
    public partial class ConfigGenerator : Form
    {
        #region "Constants - Copied from website project"
        const int TERMINAL_KEY_MIN = 3;
        const int TERMINAL_KEY_MAX = 20;
        const int TERMINAL_TITLE_MIN = 1;
        const int TERMINAL_TITLE_MAX = 28;
        #endregion

        public ConfigGenerator()
        {
            InitializeComponent();
        }
        private void ConfigGenerator_FormClosing(object sender, FormClosingEventArgs e)
        {
            groupBox1.Enabled = false;
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            MessageBox.Show("You wil need to restart the application...", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Environment.Exit(0);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void ConfigGenerator_Load(object sender, EventArgs e)
        {
            // Check if the portable web server exists - else disable the option
            if(!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Website/Webserver"))
                startupWebserver.Enabled = false;
            // Same for the database server
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Website/Database"))
                startupDatabase.Enabled = false;
            // Fill-in the host with the current IP for this computer
            IPHostEntry iphe = Dns.GetHostEntry(Dns.GetHostName());
            if (iphe.AddressList.Length > 0)
                for (int i = 0; i < iphe.AddressList.Length; i++)
                {
                    dbHost.Items.Add(iphe.AddressList[i].ToString());
                    if (iphe.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        dbHost.SelectedIndex = dbHost.Items.Count - 1;
                }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // Validate the input
            if (Uri.CheckHostName(dbHost.Text) == UriHostNameType.Unknown)
            {
                Msg("Invalid database host!");
                return;
            }
            else if (!ValidPort(dbPort.Text))
            {
                Msg("Invalid database port!");
                return;
            }
            else if (dbUser.Text.Length < 1 || dbUser.Text.Length > 16)
            {
                Msg("Invalid database username!");
                return;
            }
            else if (!ValidPort(startupWebserverPort.Text))
            {
                Msg("Invalid web server port!");
                return;
            }
            // Based on: http://exchangepedia.com/2007/01/what-is-the-real-maximum-password-length.html
            else if (startupLogonPassword.Text.Length > 127)
            {
                Msg("Invalid automatic logon password!");
                return;
            }
            else if (terminalKey.Text.Length < TERMINAL_KEY_MIN || terminalKey.Text.Length > TERMINAL_KEY_MAX)
            {
                Msg("Terminal key must be " + TERMINAL_KEY_MIN + " to " + TERMINAL_KEY_MAX + " characters in length!");
                return;
            }
            else if (terminalTitle.Text.Length < TERMINAL_TITLE_MIN || terminalTitle.Text.Length > TERMINAL_TITLE_MAX)
            {
                Msg("Terminal title must be " + TERMINAL_TITLE_MIN + " to " + TERMINAL_TITLE_MAX + " characters in length!");
                return;
            }
            else
            {
                string key = terminalKey.Text; // Copied from the website admin panel
                for (int i = 0; i < key.Length; i++)
                {
                    if (!(key[i] >= 'A' && key[i] <= 'Z') && !(key[i] >= 'a' && key[i] <= 'z') && !(key[i] >= '0' && key[i] <= '9'))
                    {
                        Msg("Invalid key - must be alpha-numeric (alphabet and number characters - no spaces etc!");
                        return;
                    }
                }
            }
            // Validate connectivity
            MySQL mysql = null;
            try
            {
                // Test if we can connect to the database source
                mysql = new MySQL();
                mysql.Settings_Host = dbHost.Text;
                mysql.Settings_Port = int.Parse(dbPort.Text);
                mysql.Settings_User = dbUser.Text;
                mysql.Settings_Pass = dbPass.Text;
                mysql.Settings_Database = dbDatabase.Text;
                mysql.Connect();
            }
            catch(Exception ex)
            {
                Msg("Could not connect to the database - " + ex.Message + "!");
                return;
            }
            try
            {
                // Test if we can access the database and pull the version
                if (mysql.Query_Read("SELECT * FROM settings").Rows.Count == 0)
                {
                    Msg("Invalid database!");
                    return;
                }
                // Test if the media computer is already added, if so we'll update the title
                string autoJoin = (string)(mysql.Query_Scalar("SELECT value FROM settings WHERE keyid='terminals_automatic_register'") ?? string.Empty);
                if (mysql.Query_Count("SELECT COUNT('') FROM terminals WHERE tkey='" + Utils.Escape(terminalKey.Text) + "'") == 1)
                    mysql.Query_Execute("UPDATE terminals SET title='" + Utils.Escape(terminalTitle.Text) + "' WHERE tkey='" + Utils.Escape(terminalKey.Text) + "'"); // Update the title
                else if (autoJoin.Equals("1")) // Doesnt exist, check we can automatically join
                    mysql.Query_Execute("INSERT INTO terminals (title, tkey) VALUES('" + Utils.Escape(terminalTitle.Text) + "', '" + Utils.Escape(terminalKey.Text) + "')"); // Join
                else
                {
                    Msg("This media computer has not been added to your media website/portal and automatic joining is disabled; please add the key and try again!");
                    return;
                }
            }
            catch
            {
                Msg("Invalid database!");
                return;
            }
            // Write database configuration to file
            StringWriter sw = new StringWriter();
            XmlWriter xw = XmlWriter.Create(sw);
            xw.WriteStartDocument();
            xw.WriteStartElement("settings");

            // -- Database
            xw.WriteStartElement("database");

            xw.WriteStartElement("type");
            xw.WriteCData("mysql");
            xw.WriteEndElement();

            xw.WriteStartElement("host");
            xw.WriteCData(dbHost.Text);
            xw.WriteEndElement();

            xw.WriteStartElement("port");
            xw.WriteCData(dbPort.Text);
            xw.WriteEndElement();

            xw.WriteStartElement("user");
            xw.WriteCData(dbUser.Text);
            xw.WriteEndElement();

            xw.WriteStartElement("pass");
            xw.WriteCData(dbPass.Text);
            xw.WriteEndElement();

            xw.WriteStartElement("db");
            xw.WriteCData(dbDatabase.Text);
            xw.WriteEndElement();

            xw.WriteEndElement();
            // -- Terminal
            xw.WriteStartElement("terminal");

            xw.WriteStartElement("key");
            xw.WriteCData(terminalKey.Text);
            xw.WriteEndElement();

            xw.WriteEndElement();
                
            xw.WriteEndElement();
            xw.WriteEndDocument();
            xw.Close();
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/Settings.xml", sw.ToString());
            // Settings are valid, now to handle startup options
            if (startupDatabase.Checked || startupLogon.Checked || startupServer.Checked || startupWebserver.Checked)
            {
                RegistryKey reg = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (startupServer.Checked)
                    reg.SetValue("Uber Media - Server", Application.ExecutablePath);
                if (startupDatabase.Checked)
                    reg.SetValue("Uber Media - Database", AppDomain.CurrentDomain.BaseDirectory + "\\Website\\Database\\bin\\mysqld.exe");
                if (startupWebserver.Checked)
                    reg.SetValue("Uber Media - Webserver", AppDomain.CurrentDomain.BaseDirectory + "\\Website\\Webserver\\UltiDevCassinWebServer2a.exe /run \"" + AppDomain.CurrentDomain.BaseDirectory + "\\Website" + "\" \"Default.aspx\" \"" + startupWebserverPort.Text + "\" nobrowser");
                if (startupLogon.Checked)
                {
                    reg.Close();
                    reg = Registry.LocalMachine.OpenSubKey(@"\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true);
                    reg.SetValue("DefaultUserName", Environment.UserName);
                    reg.SetValue("DefaultDomainName", Environment.UserDomainName);
                    reg.SetValue("DefaultPassword", startupLogonPassword.Text);
                    reg.SetValue("AutoAdminLogon", "1");
                    reg.Close();
                }
            }
            // End the application
            Close();
        }
        void Msg(string text)
        {
            MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        bool ValidPort(string text)
        {
            try
            {
                int i = int.Parse(text);
                return i >= 1 && i <= 49151;
            }
            catch { return false; }
        }
    }
}

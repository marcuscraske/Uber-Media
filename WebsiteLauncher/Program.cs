/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /Program.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 * Used to check the installation is somewhat valid and decides if this is the
 * first time the launcher has been executed.
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace WebsiteLauncher
{
    static class Program
    {
        public static string PATH_WEB = Application.StartupPath + "\\Webserver";
        public static string PATH_DB = Application.StartupPath + "\\Database";
        public static string PATH_CONFIG = Application.StartupPath + "\\Launcher.xml";
        public static string PATH_WEBSITE = Application.StartupPath + "\\Website";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Launch the updater
            try
            { Process.Start(Environment.CurrentDirectory + "\\Updater.exe"); }
            catch { }
            // Check the database and web-server both exist and are valid
            if (!Directory.Exists(PATH_WEB))
                MessageBox.Show("The directory with the web-server is missing, cannot continue!", "Critical Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (!Directory.Exists(PATH_DB))
                MessageBox.Show("The directory with the database-server is missing, cannot continue!", "Critical Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (!Directory.Exists(PATH_WEBSITE))
                MessageBox.Show("The directory with the website is missing, cannot continue!", "Critical Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (!File.Exists(PATH_CONFIG))
                // Launch configurator
                Application.Run(new Settings(true));
            else
                // Launch the actual launcher
                Application.Run(new Launcher());
        }
    }
}

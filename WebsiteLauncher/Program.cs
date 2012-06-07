using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace WebsiteLauncher
{
    static class Program
    {
        public static string PATH_WEB = Application.StartupPath + "\\Webserver";
        public static string PATH_DB = Application.StartupPath + "\\Database";
        public static string PATH_CONFIG = Application.StartupPath + "\\Launcher.xml";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check the database and web-server both exist and are valid
            if (!Directory.Exists(PATH_WEB))
                MessageBox.Show("The directory with the web-server is missing, cannot continue!", "Critical Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (!Directory.Exists(PATH_DB))
                MessageBox.Show("The directory with the database-server is missing, cannot continue!", "Critical Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (!File.Exists(PATH_CONFIG))
                // Launch configurator
                Application.Run(new Settings(true));
            else
                // Launch the actual launcher
                Application.Run(new Launcher());
        }
    }
}

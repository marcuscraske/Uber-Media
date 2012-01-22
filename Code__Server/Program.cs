/*
 * License:     Creative Commons Attribution-ShareAlike 3.0 unported
 * File:        Program.cs
 * Author(s):   limpygnome
 * 
 * Template-generated class for launching the application.
 * 
 * Improvements/bugs:
 *          -   none.
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace UberMediaServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}

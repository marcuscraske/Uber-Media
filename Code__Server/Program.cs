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
 * Template-generated class for launching the application.
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

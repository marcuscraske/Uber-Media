/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media - UberMediaSharedLibrary
 * File:                    /Misc.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 * Shared code used between the Ubermedia projects.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace UberMedia.Shared
{
    public static class Misc
    {
        /// <summary>
        /// Creates a new shortcut.
        /// </summary>
        /// <param name="creationPath">The directory of where the shortcut will be created</param>
        /// <param name="creationFileName">The name of the shortcut, excluding extension.</param>
        /// <param name="iconPath">Full-path of the icon - this can be an executable.</param>
        /// <param name="executablePath">The application executed by the shortcut.</param>
        public static void createShortcut(string creationPath, string creationFileName, string iconPath, string executablePath, bool deleteIfExists, bool webShortcut)
        { // With help from http://stackoverflow.com/questions/4897655/create-shortcut-on-desktop-c-sharp
            string path = creationPath + "\\" + creationFileName + ".url";
            if (File.Exists(path))
                if (deleteIfExists)
                    File.Delete(path);
                else
                    return; // We cannot do anything ;_;
            // Create the shortcut
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=" + (webShortcut ? string.Empty : "file:///") + executablePath);
                writer.WriteLine("IconIndex=0");
                writer.WriteLine("IconFile=" + iconPath.Replace("\\", "/"));
                writer.Flush();
            }
        }
    }
}
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

        public static void dumpError(string applicationID, Exception ex)
        {
            try
            {
                // Create directory
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Errors"))
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Errors");
                // Open stream for writing
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Errors\\" + applicationID + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".dump", false);
                // Write header
                sw.Write("<error>\r\n");
                // Write info data
                sw.Write("  <info>\r\n");
                sw.Write("      <app-id>" + dumpErrorEncodeValue(applicationID) + "</app-id>\r\n");
                sw.Write("      <time>" + dumpErrorEncodeValue(DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss:ffff")) + "</time>\r\n");
                sw.Write("      <machine>" + dumpErrorEncodeValue(Environment.MachineName) + "</machine>\r\n");
                sw.Write("      <user>" + dumpErrorEncodeValue(Environment.UserName) + "</user>\r\n");
                sw.Write("      <domain>" + dumpErrorEncodeValue(Environment.UserDomainName) + "</domain>\r\n");
                sw.Write("      <basedir>" + dumpErrorEncodeValue(AppDomain.CurrentDomain.BaseDirectory) + "</basedir>\r\n");
                sw.Write("      <appdomain-name>" + dumpErrorEncodeValue(AppDomain.CurrentDomain.FriendlyName) + "</appdomain-name>\r\n");
                sw.Write("      <windows>" + dumpErrorEncodeValue(Environment.OSVersion.VersionString) + "</windows>\r\n");
                sw.Write("      <system-ticks>" + dumpErrorEncodeValue(Environment.TickCount.ToString()) + "</system-ticks>\r\n");
                sw.Write("      <args>" + dumpErrorEncodeValue(Environment.CommandLine) + "</args>\r\n");
                sw.Write("  </info>\r\n");
                // Write exception
                sw.Write("  <exception>\r\n");
                sw.Write("      <message>" + dumpErrorEncodeValue(ex.Message) + "</message>\r\n");
                sw.Write("      <stack-trace>" + dumpErrorEncodeValue(ex.StackTrace) + "</stack-trace>\r\n");
                sw.Write("      <source>" + dumpErrorEncodeValue(ex.Source) + "</source>\r\n");
                sw.Write("  </exception>\r\n");
                // Write base exception
                sw.Write("  <base-exception>\r\n");
                sw.Write("      <message>" + dumpErrorEncodeValue(ex.GetBaseException().Message) + "</message>\r\n");
                sw.Write("      <stack-trace>" + dumpErrorEncodeValue(ex.GetBaseException().StackTrace) + "</stack-trace>\r\n");
                sw.Write("      <source>" + dumpErrorEncodeValue(ex.GetBaseException().Source) + "</source>\r\n");
                sw.Write("  </base-exception>\r\n");
                // Write footer
                sw.Write("</error>");
                // Flush the stream and dispose it
                sw.Flush();
                sw.Dispose();
            }
            catch
            {
            }
        }
        public static string dumpErrorEncodeValue(string text)
        {
            return text.Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
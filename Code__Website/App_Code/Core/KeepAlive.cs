/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /App_Code/KeepAlive.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 *                          Paul Wilson                     http://www.wilsondotnet.com/
 * To-do/bugs:              none
 * 
 * This keeps the application pool running until manually terminated, which gets around the
 * issue of the application pool being shutdown after a user typically not visiting the site
 * for 15 mins.
 */
/* Original header: */
// Derives from:
// http://authors.aspalliance.com/PaulWilson/Articles/?id=12
//
// This keeps the site active since the app-pool is automatically closed when no users are present at the site;
// by keeping the pool active, the site will load faster and carry out cycling and automatic backups through
// prolonged periods of inactivity. This web application should be safe to avoid recycling, however a manual
// forced recycle should be implemented on the web-server to occur at a periodic rate daily.
//
// Original author: Paul Wilson
// http://www.wilsondotnet.com/
using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.UI;

namespace Wilson.WebCompile
{
    public class GlobalBase : System.Web.HttpApplication
    {
        private static bool needsCompile = true;
        private static string applicationPath = "";
        private static string physicalPath = "";
        private static string applicationURL = "";
        private static System.Threading.Thread thread = null;
        private static System.Timers.Timer timer = null;
        public event EventHandler Elapsed;
        protected virtual int KeepAliveMinutes
        {
            get { return 10; }
        }
        protected virtual string SkipFiles
        {
            get { return @""; }
        }
        protected virtual string SkipFolders
        {
            get { return @""; }
        }
        public override void Init()
        {
            if (GlobalBase.needsCompile)
            {
                GlobalBase.needsCompile = false;
                applicationPath = HttpContext.Current.Request.ApplicationPath;
                if (!applicationPath.EndsWith("/")) { applicationPath += "/"; }
                string server = HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
                bool https = HttpContext.Current.Request.ServerVariables["HTTPS"] != "off";
                applicationURL = (https ? "https://" : "http://") + server + applicationPath;
                physicalPath = HttpContext.Current.Request.PhysicalApplicationPath;
                thread = new System.Threading.Thread(new System.Threading.ThreadStart(CompileApp));
                thread.Start();
                if (this.KeepAliveMinutes > 0)
                {
                    timer = new System.Timers.Timer(60000 * this.KeepAliveMinutes);
                    timer.Elapsed += new System.Timers.ElapsedEventHandler(KeepAlive);
                    timer.Start();
                }
            }
        }
        private void KeepAlive(Object sender, System.Timers.ElapsedEventArgs e)
        {
            // Check if the services are processing anything, else we can let the application die by not keeping the app alive
            if (UberMedia.Indexer.serviceIsActive || UberMedia.ThumbnailGeneratorService.serviceIsActive ||
                UberMedia.FilmInformation.serviceIsActive || UberMedia.ConversionService.serviceIsActive)
            {
                timer.Enabled = false;
                if (this.Elapsed != null) { this.Elapsed(this, e); }
                timer.Enabled = true;
                string url = applicationURL;
                using (HttpWebRequest.Create(url).GetResponse()) { }
                System.Diagnostics.Debug.WriteLine("Keep-alive request made!");
            }
        }
        private void CompileApp()
        {
            CompileFolder(physicalPath);
        }
        private void CompileFolder(string Folder)
        {
            foreach (string file in Directory.GetFiles(Folder, "*.as?x"))
            {
                CompileFile(file);
            }
            foreach (string folder in Directory.GetDirectories(Folder))
            {
                bool skipFolder = false;
                foreach (string item in this.SkipFolders.Split(';'))
                {
                    if (item != "" && folder.ToUpper().EndsWith(item.ToUpper()))
                    {
                        skipFolder = true;
                        break;
                    }
                }
                if (!skipFolder)
                {
                    CompileFolder(folder);
                }
            }
        }
        private void CompileFile(string File)
        {
            bool skipFile = false;
            foreach (string item in this.SkipFiles.Split(';'))
            {
                if (item != "" && File.ToUpper().EndsWith(item.ToUpper()))
                {
                    skipFile = true;
                    break;
                }
            }
            if (!skipFile)
            {
                string path = File.Remove(0, physicalPath.Length);
                if (File.ToLower().EndsWith(".ascx"))
                {
                    string virtualPath = applicationPath + path.Replace(@"\", "/");
                    Page controlLoader = new Page();
                    try
                    {
                        controlLoader.LoadControl(virtualPath);
                    }
                    finally
                    {
                        System.Diagnostics.Debug.WriteLine(virtualPath, "Control");
                    }
                }
                else if (!File.ToLower().EndsWith(".asax"))
                {
                    string url = applicationURL + path.Replace(@"\", "/");
                    using (HttpWebRequest.Create(url).GetResponse()) { }
                    System.Diagnostics.Debug.WriteLine(url, "Page");
                }
            }
        }
    }
}
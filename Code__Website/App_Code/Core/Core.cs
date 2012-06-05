/*
 * License:     Creative Commons Attribution-ShareAlike 3.0 unported
 * File:        App_Code/Core.cs
 * Author(s):   limpygnome
 * 
 * Contains multiple classes responsible for core functions of the web application:
 * - Core
 *      Responsible for loading settings and executing background services
 *      such as drive indexing and thumbnail generation.
 *      
 *  - ThreadIndexerAttribs
 *      Contains variables to be passed to an indexer thread.
 *      
 *  - Indexer
 *      Responsible for running in the background and indexing each drive/folder
 *      added by the user; the files in the drives are virtually indexed in the database
 *      if their extension matches the type of file to be indexed in the folder (specified
 *      by the user). The indexer may also invoke the thumbnail generation service to generate
 *      thumbnails of media.
 *      
 *  - ThumbnailGeneratorService
 *      Responsible for generating thumbnails of media; this uses reflection to
 *      generate thumbnails for different types of media formats.
 *      
 *  - FilmInformation
 *      Responsible for grabbing information about media based on their title's from third-party
 *      sources. Currently this utilizes IMDB and RottenTomatoes.
 * 
 * Improvements/bugs:
 *          -   The downloaded IMDB database could be cached in an e.g. SQLite file for faster
 *              and more efficient access of data.
 */

using System;
using System.Collections.Generic;
using System.Web;
using UberLib.Connector;
using UberLib.Connector.Connectors;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Xml;
using System.Reflection;
using System.Drawing;

namespace UberMedia
{
    /// <summary>
    /// Responsible for core settings and functionality of the website side of the media library; this area is critical and the most sensitive. If this fails to start,
    /// the entire application will not be allowed to continue.
    /// </summary>
    public static class Core
    {
        #region "Versioning"
        /* The below variables are used by the installer and hence should not be tampered! Version methodology used:
         * -- major - any major changes e.g. due to the platform or language, maybe a rebuild of major code.
         * -- minor - any minor changes e.g. new features implemented.
         * -- build - any e.g. bug-fixes applied, but no new features etc.
         */

        public const int versionMajor = 1;  // Remember to update SQL as well (settings table)!!!! This is hard-written to ensure the database data matches the version data etc etc
        public const int versionMinor = 0;
        public const int versionBuild = 0;
        #endregion

        #region "Consts"
        /// <summary>
        /// For some reason using the e.g. VS web-server causes the base-dir to end with \; this variable is only used by affected areas.
        /// </summary>
        public static string basePath = AppDomain.CurrentDomain.BaseDirectory.EndsWith("\\") ? AppDomain.CurrentDomain.BaseDirectory.Remove(AppDomain.CurrentDomain.BaseDirectory.Length - 1, 1) : AppDomain.CurrentDomain.BaseDirectory;
        #endregion

        #region "Core Status"
        /// <summary>
        /// This boolean should be used by all web-components to ensure the core is operational.
        /// </summary>
        public static CoreState State = CoreState.NotRunning;

        public enum CoreState
        {
            NotInstalled,
            NotRunning,
            Running,
            Error,
            InvalidVersion,
        }
        #endregion

        #region "Core Variables"
        public static Connector GlobalConnector = null;
        public static Dictionary<string, string> Cache_HtmlTemplates = null;
        public static Dictionary<string, string> Cache_Settings = null;
        public static Exception failureError = null;
        #endregion

        #region "Methods - Core Start/End"
        public static void Core_Start()
        {
            // Check the site has actually been installed before we continue
            if (!File.Exists(basePath + "/Config.xml"))
            {
                State = CoreState.NotInstalled;
                return;
            }
            // Load the core
            try
            {
                // Cache any settings and create the global-connector
                CacheSettings_Reload(true);
                // Check version matches hard-written version information
                if (!Cache_Settings["major"].Equals(versionMajor.ToString()) ||
                    !Cache_Settings["minor"].Equals(versionMinor.ToString()) ||
                    !Cache_Settings["build"].Equals(versionBuild.ToString()))
                {
                    State = CoreState.InvalidVersion;
                    return;
                }
                // Load HTML templates cache
                HtmlTemplates_Reload();
                // Start the thumbnail service
                ThumbnailGeneratorService.Delegator_Start();
                // Index each drive
                foreach (ResultRow drive in GlobalConnector.Query_Read("SELECT pfolderid, physicalpath, allow_web_synopsis FROM physical_folders ORDER BY title ASC")) Indexer.IndexDrive(drive["pfolderid"], drive["physicalpath"], drive["allow_web_synopsis"].Equals("1"));
                // Update the core status to running
                State = CoreState.Running;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Core failed to start - " + ex.Message + "!\n\nStack-trace:\n" + ex.StackTrace);
                failureError = ex;
                State = CoreState.Error;
            }
        }
        public static void Core_Stop()
        {
            State = CoreState.NotRunning;
            // Terminate thread pool of indexer
            Indexer.TerminateThreadPool();
            // Terminate thumbnail service
            ThumbnailGeneratorService.Delegator_Stop();
            // Dispose local core
            Cache_HtmlTemplates = null;
            Cache_Settings = null;
            GlobalConnector = null;
        }
        #endregion

        #region "Methods - Misc"
        public static Connector Connector_Create(bool persistant)
        {
            switch (Cache_Settings["db_type"])
            {
                case "mysql":
                    MySQL mconn = new MySQL();
                    mconn.Settings_Host = Cache_Settings["db_host"];
                    mconn.Settings_Port = int.Parse(Cache_Settings["db_port"]);
                    mconn.Settings_Database = Cache_Settings["db_database"];
                    mconn.Settings_User = Cache_Settings["db_user"];
                    mconn.Settings_Pass = Cache_Settings["db_pass"];
                    if (persistant) mconn.Settings_Timeout_Connection = 0;
                    else mconn.Settings_Timeout_Connection = 1000;
                    mconn.Connect();
                    return mconn;
                case "sqlite":
                    SQLite sconn = new SQLite();
                    sconn.ChangeDatabase(Core.basePath + "/local_database");
                    return sconn;
            }
            return null;
        }
        public static void Log_External_Request(string reason, string url, Connector connector)
        {
            GlobalConnector.CheckConnectionIsReady();
            connector.Query_Execute("INSERT INTO external_requests (reason, url, datetime) VALUES('" + Utils.Escape(reason) + "', '" + Utils.Escape(url) + "', NOW());");
        }
        public static void HtmlTemplates_Reload()
        {
            GlobalConnector.CheckConnectionIsReady();
            Cache_HtmlTemplates = new Dictionary<string, string>();
            lock (Cache_HtmlTemplates)
                foreach (ResultRow r in GlobalConnector.Query_Read("SELECT hkey, html FROM html_templates ORDER BY hkey ASC"))
                    Cache_HtmlTemplates.Add(r["hkey"], r["html"]);
        }
        /// <summary>
        /// Reloads the core's settings; this will also initialize the global connector if null.
        /// </summary>
        public static void CacheSettings_Reload(bool loadDatabaseSettings)
        {
            Cache_Settings = new Dictionary<string, string>();
            lock (Cache_Settings)
            {
                // Load static settings
                XmlDocument doc = new XmlDocument();
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/Config.xml");
                foreach (XmlNode node in doc["settings"].ChildNodes) Cache_Settings[node.Name] = node.InnerText;
                if (loadDatabaseSettings)
                {
                    // Check the global connector is ready
                    if (GlobalConnector == null)
                        GlobalConnector = Connector_Create(true);
                    else
                        GlobalConnector.CheckConnectionIsReady();
                    // Load settings stored in the database
                    if (GlobalConnector.Query_Read("SELECT keyid, value FROM settings ORDER BY keyid ASC") == null) throw new Exception("Global connector is null/not set!");
                    foreach (ResultRow setting in GlobalConnector.Query_Read("SELECT keyid, value FROM settings ORDER BY keyid ASC")) Cache_Settings[setting["keyid"]] = setting["value"];
                }
            }
        }
        #endregion
    }
}
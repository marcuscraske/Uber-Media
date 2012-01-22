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
using Jayrock.Json;
using Jayrock.Json.Conversion;
using System.Net;
using System.Xml;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Core;
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
        #endregion

        #region "Methods - Core Start/End"
        public static void Core_Start()
        {
            // Check the site has actually been installed before we continue
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Config.xml"))
            {
                State = CoreState.NotInstalled;
                return;
            }
            // Load the core
            try
            {
                // Cache any settings and create the global-connector
                CacheSettings_Reload();
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
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Core failed to start - " + ex.Message + "!\n\nStack-trace:\n" + ex.StackTrace);
                State = CoreState.Error;
                return;
            }
            State = CoreState.Running;
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
            MySQL Connector = new MySQL();
            Connector.Settings_Host = Cache_Settings["db_host"];
            Connector.Settings_Port = int.Parse(Cache_Settings["db_port"]);
            Connector.Settings_Database = Cache_Settings["db_database"];
            Connector.Settings_User = Cache_Settings["db_user"];
            Connector.Settings_Pass = Cache_Settings["db_pass"];
            if (persistant) Connector.Settings_Timeout_Connection = 0;
            else Connector.Settings_Timeout_Connection = 1000;
            Connector.Connect();
            return Connector;
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
        public static void CacheSettings_Reload()
        {
            Cache_Settings = new Dictionary<string, string>();
            lock (Cache_Settings)
            {
                // Load database settings
                XmlDocument doc = new XmlDocument();
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/Config.xml");
                Cache_Settings["db_host"] = doc["settings"]["db_host"].InnerText;
                Cache_Settings["db_port"] = doc["settings"]["db_port"].InnerText;
                Cache_Settings["db_database"] = doc["settings"]["db_database"].InnerText;
                Cache_Settings["db_user"] = doc["settings"]["db_user"].InnerText;
                Cache_Settings["db_pass"] = doc["settings"]["db_pass"].InnerText;
                // Check the global connector is ready
                if (GlobalConnector == null)
                    GlobalConnector = Connector_Create(true);
                else
                    GlobalConnector.CheckConnectionIsReady();
                // Load settings stored in the database
                foreach (ResultRow setting in GlobalConnector.Query_Read("SELECT keyid, value FROM settings ORDER BY keyid ASC")) Cache_Settings[setting["keyid"]] = setting["value"];
            }
        }
        #endregion
    }
    /// <summary>
    /// Used to pass values to an indexer thread.
    /// </summary>
    public struct ThreadIndexerAttribs
    {
        public Connector Connector;
        public string pfolderid;
        public string base_path;
        public bool allow_web_synopsis;
    }
    /// <summary>
    /// Responsible for indexing media files in a virtual file-system for faster searching and for the collation and manipulation of media data.
    /// </summary>
    public static class Indexer
    {
        public static bool Terminate = false;
        public static Dictionary<string, string> ThreadPoolStatus = new Dictionary<string, string>(); // Stores the status's of drive-indexing
        private static void UpdateStatus(string pfolderid, string text)
        {
            lock (ThreadPoolStatus)
            {
                if (ThreadPoolStatus.ContainsKey(pfolderid)) ThreadPoolStatus[pfolderid] = text;
                else ThreadPoolStatus.Add(pfolderid, text);
            }
        }
        public static string GetStatus(string pfolderid)
        {
            return ThreadPoolStatus.ContainsKey(pfolderid) ? ThreadPoolStatus[pfolderid] : "Thread has not executed";
        }
        public static Dictionary<string, Thread> ThreadPool = new Dictionary<string, Thread>(); // Stores all the active threads; string = pfolderid.

        public static void IndexDrive(string pfolderid, string path, bool allow_web_synopsis)
        {
            if (!Directory.Exists(path) || Terminate) return; // Doesnt exist/down? Leave...
            lock (ThreadPool)
            {
                // Check if a pre-existing thread exists; if yes and not active, remove the thread and pool a new one.
                if (ThreadPool.ContainsKey(pfolderid))
                {
                    if (ThreadPool[pfolderid].ThreadState == System.Threading.ThreadState.Running) return;
                    else ThreadPool.Remove(pfolderid);
                }
                UpdateStatus(pfolderid, "Initializing thread for " + pfolderid);
                // Build the args to pass to the thread
                ThreadIndexerAttribs tia = new ThreadIndexerAttribs();
                tia.base_path = path;
                tia.pfolderid = pfolderid;
                tia.allow_web_synopsis = allow_web_synopsis;
                tia.Connector = Core.Connector_Create(false);
                // Create and start thread
                Thread th = new Thread(new ParameterizedThreadStart(ThreadIndexer));
                th.Start(tia);
                // Add thread to pool
                ThreadPool.Add(pfolderid, th);
            }
        }
        static void ThreadIndexer(object obj)
        {
            if (Terminate) return;
            ThreadIndexerAttribs tia = (ThreadIndexerAttribs)obj;
            UpdateStatus(tia.pfolderid, "Mapping extensions to type UID");
            tia.pfolderid = Utils.Escape(tia.pfolderid);
            // Build extensions->typeid map and a list of processible (thumbnail-wise) items
            Dictionary<string, string> ExtensionsMap = new Dictionary<string, string>();
            // Builds thumbnail extensions -> thumbnail handler/processor
            Dictionary<string, string> ThumbnailExts = new Dictionary<string, string>();
            string[] t;
            foreach (ResultRow type in tia.Connector.Query_Read("SELECT it.uid, it.extensions, it.thumbnail FROM physical_folder_types AS pf LEFT OUTER JOIN item_types AS it ON it.typeid=pf.typeid WHERE pf.pfolderid='" + tia.pfolderid + "'"))
            {
                if (Terminate) return;
                t = type["extensions"].Split(',');
                foreach (string s in t) if (s.Length > 0 && !ExtensionsMap.ContainsKey(s))
                    {
                        if (type["thumbnail"].Length > 0) ThumbnailExts.Add(s.ToLower(), type["thumbnail"]);
                        ExtensionsMap.Add(s.ToLower(), type["uid"]);
                    }
            }
            // Cache for parent addresses
            Dictionary<string, string> Cache = new Dictionary<string, string>();
            // Index each folder
            UpdateStatus(tia.pfolderid, "Indexing directories");
            foreach (string directory in Directory.GetDirectories(tia.base_path, "*", SearchOption.AllDirectories))
            {
                if (Terminate) return;
                if (tia.Connector.Query_Count("SELECT COUNT('') FROM virtual_items WHERE pfolderid='" + tia.pfolderid + "' AND type_uid='100' AND phy_path='" + Utils.Escape(directory.Remove(0, tia.base_path.Length)) + "'") == 0)
                {
                    // Create the virtual folder
                    tia.Connector.Query_Execute("INSERT INTO virtual_items (pfolderid, type_uid, title, phy_path, parent, date_added) VALUES('" + tia.pfolderid + "', '100', '" + Utils.Escape(Path.GetFileName(directory)) + "', '" + Utils.Escape(directory.Remove(0, tia.base_path.Length)) + "', '" + Utils.Escape(GetParentVITEMID(tia.pfolderid, directory.Remove(0, tia.base_path.Length), ref Cache, tia.Connector)) + "', NOW());");
                }
            }
            // Index each file
            UpdateStatus(tia.pfolderid, "Indexing files");
            string filename, ext, title, desc;
            foreach (string file in Directory.GetFiles(tia.base_path, "*", SearchOption.AllDirectories))
            {
                if (Terminate) return;
                filename = file.Replace("/", "\\").Substring(file.LastIndexOf('\\') + 1);
                ext = filename.LastIndexOf('.') != -1 ? filename.Substring(filename.LastIndexOf('.') + 1).ToLower() : "";
                title = ext.Length > 0 ? filename.Remove(filename.Length - (ext.Length + 1), (ext.Length + 1)) : filename;
                if (tia.allow_web_synopsis)
                    desc = FilmInformation.FilmSynopsis(title, tia.Connector);
                else desc = "";
                if (ext.Length > 0 && ExtensionsMap.ContainsKey(ext) && tia.Connector.Query_Count("SELECT COUNT('') FROM virtual_items WHERE pfolderid='" + tia.pfolderid + "' AND type_uid != '100' AND phy_path='" + Utils.Escape(file.Remove(0, tia.base_path.Length)) + "'") == 0)
                {
                    string vitemid = tia.Connector.Query_Scalar("INSERT INTO virtual_items (pfolderid, type_uid, title, description, phy_path, parent, date_added) VALUES('" + tia.pfolderid + "', '" + Utils.Escape(ExtensionsMap[ext]) + "', '" + Utils.Escape(title) + "', '" + Utils.Escape(desc) + "', '" + Utils.Escape(file.Remove(0, tia.base_path.Length)) + "', '" + Utils.Escape(GetParentVITEMID(tia.pfolderid, file.Remove(0, tia.base_path.Length), ref Cache, tia.Connector)) + "', NOW()); SELECT LAST_INSERT_ID();").ToString();
                    if (ThumbnailExts.ContainsKey(ext)) ThumbnailGeneratorService.AddToQueue(file, AppDomain.CurrentDomain.BaseDirectory + "/Content/Thumbnails/" + vitemid.ToString() + ".png", ThumbnailExts[ext]);
                }
            }
            UpdateStatus(tia.pfolderid, "Verifying index integrity");
            string invalid_vi = "DELETE FROM virtual_items WHERE "; // Query for deleting invalid virtual items
            // Verify the integrity of every item (unless the path is empty - meaning it's a virtual item e.g. youtube)
            foreach (ResultRow r in tia.Connector.Query_Read("SELECT vitemid, type_uid, phy_path FROM virtual_items WHERE pfolderid='" + Utils.Escape(tia.pfolderid) + "' AND phy_path != ''"))
            {
                if (Terminate) return;
                if ((r["type_uid"] == "100" && !Directory.Exists(tia.base_path + r["phy_path"])) || (r["type_uid"] != "100" && !File.Exists(tia.base_path + r["phy_path"]))) invalid_vi += "vitemid='" + Utils.Escape(r["vitemid"]) + "' OR ";
            }
            if (invalid_vi.Length > 32) tia.Connector.Query_Execute(invalid_vi.Remove(invalid_vi.Length - 3, 3));
            UpdateStatus(tia.pfolderid, "Removing self from pool");
            // Remove self from pool
            lock (ThreadPool) if (ThreadPool.ContainsKey(tia.pfolderid)) ThreadPool.Remove(tia.pfolderid);
            UpdateStatus(tia.pfolderid, "Thread finished");
        }
        public static string GetParentVITEMID(string pfolderid, string pathofmedium, ref Dictionary<string, string> Cache, Connector Connector)
        {
            if (Cache.ContainsKey(pathofmedium)) return Cache[pathofmedium];
            int lsep = pathofmedium.IndexOf('\\');
            int rsep = pathofmedium.LastIndexOf('\\');
            if (lsep == rsep) return "0"; // Item is at the top
            else
            {
                string t = Connector.Query_Scalar("SELECT vitemid FROM virtual_items WHERE type_uid='100' AND pfolderid='" + Utils.Escape(pfolderid) + "' AND phy_path='" + Utils.Escape(pathofmedium.Substring(0, rsep)) + "'").ToString();
                Cache.Add(pathofmedium, t);
                return t;
            }
        }
        public static void TerminateThreadPool()
        {
            Terminate = true;
            Thread.Sleep(100);
            lock (ThreadPool)
            {
                foreach (KeyValuePair<string, Thread> thr in ThreadPool) thr.Value.Abort();
                ThreadPool.Clear();
            }
            Terminate = false;
        }
        public static void TerminateIndexer(string pfolderid)
        {
            lock (ThreadPool)
            {
                if (ThreadPool.ContainsKey(pfolderid))
                {
                    ThreadPool[pfolderid].Abort();
                    ThreadPool.Remove(pfolderid);
                }
            }
        }
    }
    /// <summary>
    /// Responsible for generating thumbnails of media items.
    /// </summary>
    public static class ThumbnailGeneratorService
    {
        private static bool ShutdownOverride = false; // Causes the threads to safely terminate
        public static string Status = "Uninitialised";
        private static Thread DelegatorThread = null;
        public static List<Thread> Threads = new List<Thread>();
        public static List<string[]> Queue = new List<string[]>(); // Path of media, output path, handler

        public static void Delegator_Start()
        {
            if (DelegatorThread != null) return;
            Status = "Creating pool of threads";
            // Create thread pool
            Thread th;
            for (int i = 0; i < int.Parse(Core.Cache_Settings["thumbnail_threads"]); i++)
            {
                th = new Thread(new ParameterizedThreadStart(ThreadMethod));
                Threads.Add(th);
            }
            Status = "Starting delegator";
            // Initialise delegator/manager of threads
            DelegatorThread = new Thread(new ParameterizedThreadStart(Delegator));
            DelegatorThread.Start();
            Status = "Delegator started";
        }
        public static void Delegator_Stop()
        {
            Status = "Delegator shutting down";
            if (DelegatorThread != null)
            {
                DelegatorThread.Abort();
                DelegatorThread = null;
            }
            ShutdownOverride = true;
            foreach (Thread th in Threads) th.Abort();
            lock(processes)
                try { foreach (Process p in processes) p.Kill(); } catch { }
            Threads.Clear();
            ShutdownOverride = false;
            Status = "Delegator and pool terminated";
        }

        /// <summary>
        /// Controls the threads.
        /// </summary>
        private static void Delegator(object o)
        {
            while (true && !ShutdownOverride)
            {
                lock (Threads)
                {
                    Thread th;
                    for (int i = 0; i < Threads.Count; i++)
                    {
                        th = Threads[i];
                        if (th.ThreadState != System.Threading.ThreadState.WaitSleepJoin && th.ThreadState != System.Threading.ThreadState.Running && Queue.Count != 0)
                            lock (Queue)
                            {
                                //Status = "Added item '" + Queue[0][0] + "' to queue";
                                Threads[i] = new Thread(new ParameterizedThreadStart(ThreadMethod));
                                Threads[i].Start(Queue[0]);
                                Queue.Remove(Queue[0]);
                            }
                    }

                }
                Thread.Sleep(100);
                Status = "Successfully looped at " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            }
        }
        public static void AddToQueue(string path_media, string path_output, string handler)
        {
            lock (Queue) Queue.Add(new string[]{ path_media, path_output, handler});
        }
        /// <summary>
        /// Responsible for any processors executed by the thumbnail service; this is to ensure the service can be suddenly shutdown.
        /// </summary>
        public static List<Process> processes = new List<Process>();
        private static void ThreadMethod(object o)
        {
            string[] data = (string[])o;
            try
            {
                typeof(ThumbnailGeneratorService).GetMethod("ThumbnailProcessor__" + data[2], System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .Invoke(null, new object[] { data[0], data[1] });
            }
            catch { }
        }
        public static void ThumbnailProcessor__ffmpeg(string path, string thumbnail_path)
        {
            
            Process p = new Process();
            p.StartInfo.FileName = "ffmpeg.exe";
            p.StartInfo.WorkingDirectory = Core.basePath + @"\bin";
            p.StartInfo.Arguments = "-itsoffset -" + Core.Cache_Settings["thumbnail_screenshot_media_time"].ToString() + "  -i \"" + path + "\" -vcodec mjpeg -vframes 1 -an -f rawvideo -s " + Core.Cache_Settings["thumbnail_width"] + "x" + Core.Cache_Settings["thumbnail_height"] + " -y \"" + thumbnail_path + "\"";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.Start();
            processes.Add(p);
            try
            {

                while (!p.WaitForExit(int.Parse(Core.Cache_Settings["thumbnail_thread_ttl"])) && !ShutdownOverride)
                {
                    Thread.Sleep(20);
                }
                p.Kill();
            }
            catch (Exception ex) { }
            processes.Remove(p);
        }
        public static void ThumbnailProcessor__youtube(string path, string thumbnail_path)
        {
            try
            {
                // Get the ID
                string vid = File.ReadAllText(path);
                // Download thumbnail from YouTube
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://img.youtube.com/vi/" + vid + "/1.jpg");
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                if (resp.ContentLength == 0) return;
                // Resize and draw the thumbnail
                Bitmap img = (Bitmap)Bitmap.FromStream(resp.GetResponseStream());
                Bitmap thumbnail = new Bitmap(int.Parse(Core.Cache_Settings["thumbnail_width"]), int.Parse(Core.Cache_Settings["thumbnail_height"]));
                Graphics g = Graphics.FromImage(thumbnail);
                g.FillRectangle(new SolidBrush(Color.DarkGray), 0, 0, thumbnail.Width, thumbnail.Height);
                double fitToThumbnailRatio = (img.Width > img.Height ? (double)thumbnail.Width / (double)img.Width : (double)thumbnail.Height / (double)img.Height);
                Rectangle drawArea = new Rectangle();
                drawArea.Width = (int)(fitToThumbnailRatio * (double)img.Width);
                drawArea.Height = (int)(fitToThumbnailRatio * (double)img.Height);
                drawArea.X = (int)(((double)thumbnail.Width - (double)drawArea.Width) / 2);
                drawArea.Y = (int)(((double)thumbnail.Height - (double)drawArea.Height) / 2);
                g.DrawImage(img, drawArea);
                g.Dispose();
                thumbnail.Save(thumbnail_path, System.Drawing.Imaging.ImageFormat.Png);
                thumbnail.Dispose();
                img.Dispose();
                img = null;
                thumbnail = null;
                g = null;
                resp.Close();
                resp = null;
                req = null;
            }
            catch
            {
            }
        }
        public static void ThumbnailProcessor__image(string path, string thumbnail_path)
        {
            try
            {
                Bitmap img = new Bitmap(path);
                Bitmap thumbnail = new Bitmap(int.Parse(Core.Cache_Settings["thumbnail_width"]), int.Parse(Core.Cache_Settings["thumbnail_height"]));
                Graphics g = Graphics.FromImage(thumbnail);
                g.FillRectangle(new SolidBrush(Color.DarkGray), 0, 0, thumbnail.Width, thumbnail.Height);
                double fitToThumbnailRatio = (img.Width > img.Height ? (double)thumbnail.Width / (double)img.Width : (double)thumbnail.Height / (double)img.Height);
                Rectangle drawArea = new Rectangle();
                drawArea.Width = (int)(fitToThumbnailRatio * (double)img.Width);
                drawArea.Height = (int)(fitToThumbnailRatio * (double)img.Height);
                drawArea.X = (int)(((double)thumbnail.Width - (double)drawArea.Width) / 2);
                drawArea.Y = (int)(((double)thumbnail.Height - (double)drawArea.Height) / 2);
                g.DrawImage(img, drawArea);
                g.Dispose();
                thumbnail.Save(thumbnail_path, System.Drawing.Imaging.ImageFormat.Png);
                thumbnail.Dispose();
                img.Dispose();
                img = null;
                thumbnail = null;
                g = null;
            }
            catch
            {
            }
        }
    }
    /// <summary>
    /// Used, currently, to retrieve a film synopsis by using multiple source providers.
    /// </summary>
    public class FilmInformation
    {
        /// <summary>
        /// Intelligently retrieves the synopsis of films through comparative analysis of multiple possible results using various providers
        /// (currently IMDB and Rotten Tomatoes).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FilmSynopsis(string name, Connector Connector)
        {
            string result;
            // First try IMDB
            result = FetchSynopsis_IMDB(name);
            if (result.Length > 0) return result;
            // Try rotten tomatoes
            result = FetchSynopsis_RottenTomatoes(name, Connector);
            if (result.Length > 0) return result;
            // No synopsis found ;_;
            return string.Empty;
        }

        #region "Methods - Synopsis Providers"
        /// <summary>
        /// Fetches a film synopsis for a film specified by the name parameter using an IMDB plot.list database file.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Connector"></param>
        /// <returns></returns>
        public static string FetchSynopsis_IMDB(string name)
        {
            if (_IMDB_Downloading) return string.Empty;
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "/Cache/imdb.plot.list";
                if (!File.Exists(path))                                         // DB file does not exist ;_;
                {
                    IMDB_DownloadDatabaseFile();                                // Attempt to download the database file
                    return string.Empty;
                }
                name = name.ToLower();                                          // All comparisons ignore case - prepare the film title
                StreamReader reader = new StreamReader(path);                   // Provides an efficient buffered method of reading the text file
                string line;                                                    // Used to store the current line being read
                StringWriter sw;                                                // Used for reading film synopsis
                List<string> matches = new List<string>();                      // Used to store the matches (by title)
                List<string> matches_descriptions = new List<string>();         // Used to store the synopsis of matches array - both will be populated sequentially hence equal index links a pair

                // Get all of the possibilities and their film descriptions - we'll use Levenshtein Distance algorithm to calculate the best match (not perfect due to the format of film titles on IMDB)
                // -- this could be improved using a regex to find the specific titles ignoring the film year etc - although e.g. sunshine is the film title of about ten films...so this would still fail
                //    unless we compared the cost/difference amount and returned the lowest or none if all are equal
                while (reader.Peek() >= 0)
                {
                    line = reader.ReadLine(); // Store the current line
                    if (line.Length > 4 && line[0] == 'M' && line[1] == 'V' && line[2] == ':' && line[3] == ' ') // Movie title?
                        if (line.Substring(4).ToLower().Contains(name)) // See if the film title matches the film title requested - again not perfect
                        { // Add the match and grab the synopsis
                            matches.Add(line.Substring(4));
                            // Get the film description - always a blank line after a movie title header
                            reader.ReadLine();
                            sw = new StringWriter();
                            while (true) // Go through each line until it's not a synopsis line
                            {
                                line = reader.ReadLine();
                                if (line.Length > 4 && line[0] == 'P' && line[1] == 'L' && line[2] == ':' && line[3] == ' ')
                                    sw.WriteLine(line.Substring(4));
                                else break;
                            }
                            sw.Flush(); // Write any remaining unwritten lines to the buffer
                            matches_descriptions.Add(sw.ToString()); // Add the description/synopsis
                            sw.Dispose();
                            sw = null;
                        }
                }

                // Calculate the most likely line by comparing each match against each other using the Levenshtein distance algorithm, which
                // compares the amount of changes required to transform a phrase into the (in this scenario) specified film title -- the least
                // amount of changes (logically) will most likely be our film
                int[] bestResult = new int[] { 100000, -1 }; // Changes required, index of item
                if (matches.Count == 1) bestResult[1] = 0; // Set to first item
                else
                {
                    int distance; // Stores the distance of the current match being compared
                    for (int x = 0; x < matches.Count; x++) // Go through each match in two arrays for comparison
                        for (int y = 0; y < matches.Count; y++)
                            if (x != y && matches[x].Length < matches[y].Length)    // Check the current index is not equal (we dont want to compare the same element) and
                            // and x must be less than y (due to the way the algorithm is calculated)
                            {
                                distance = EditDistance(matches[x], matches[y]);    // Get the distance between the two titles
                                if (distance < bestResult[0])                       // If distance is less than the current best result found, set the current element as the best result
                                {
                                    bestResult[1] = x;
                                    bestResult[0] = distance;
                                }
                            }
                }
                if (bestResult[1] == -1) return string.Empty; // No match found ;_;
                else return matches_descriptions[bestResult[1]];
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Fetches a film synopsis for a film specified by the name parameter using the Rotten Tomatoes API; this does make
        /// an external request, hence use IMDB if possible!
        /// </summary>
        /// <param name="name">Name/title of the film.</param>
        /// <param name="Connector"></param>
        /// <returns></returns>
        public static string FetchSynopsis_RottenTomatoes(string name, Connector Connector)
        {
            if (!Core.Cache_Settings.ContainsKey("rotten_tomatoes_api_key") || Core.Cache_Settings["rotten_tomatoes_api_key"].Length == 0) return string.Empty;
            try
            {
                System.Threading.Thread.Sleep(100); // To ensure we dont surpass the API calls per a second allowed
                string url = "http://api.rottentomatoes.com/api/public/v1.0/movies.json?q=" + HttpUtility.UrlEncode(name) + "&page_limit=3&page=1&apikey=" + Core.Cache_Settings["rotten_tomatoes_api_key"];
                name = StripName(name);
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.UserAgent = "Uber Media";
                WebResponse resp = req.GetResponse();
                StreamReader stream = new StreamReader(resp.GetResponseStream());
                string data = stream.ReadToEnd();
                stream.Close();
                resp.Close();
                Core.Log_External_Request("Film data", url, Connector);
                JsonArray arr = (JsonArray)((JsonObject)JsonConvert.Import(data))["movies"];
                int highest_index = -1;
                bool valid;
                string t, t2;
                for (int i = 0; i < arr.Count; i++)
                {
                    t = StripName(((JsonObject)arr[i])["title"].ToString());
                    if (t == name) highest_index = i;
                    else
                    {
                        valid = true;
                        if (highest_index == -1) t2 = name;
                        else t2 = ((JsonObject)arr[highest_index])["title"].ToString();
                        foreach (string word in t2.Split(' ')) if (word.Length > 1 && !t2.Contains(t)) valid = false;
                        if (valid) highest_index = i;
                    }
                }
                if (highest_index == -1) return string.Empty;
                else
                {
                    string syn = ((JsonObject)arr[highest_index])["synopsis"].ToString();
                    if (syn.Length > 0) return syn;
                    else return ((JsonObject)arr[highest_index])["critics_consensus"].ToString();
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        #region "Methods - Misc (also contains a const)"
        /// <summary>
        /// Levenshtein Distance algorithm
        /// By Stephen Toub (Microsoft):
        /// http://blogs.msdn.com/b/toub/archive/2006/05/05/590814.aspx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int EditDistance<T>(IEnumerable<T> x, IEnumerable<T> y)
            where T : IEquatable<T>
        {
            // Validate parameters
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");

            // Convert the parameters into IList instances
            // in order to obtain indexing capabilities
            IList<T> first = x as IList<T> ?? new List<T>(x);
            IList<T> second = y as IList<T> ?? new List<T>(y);

            // Get the length of both.  If either is 0, return
            // the length of the other, since that number of insertions
            // would be required.
            int n = first.Count, m = second.Count;
            if (n == 0) return m;
            if (m == 0) return n;

            // Rather than maintain an entire matrix (which would require O(n*m) space),
            // just store the current row and the next row, each of which has a length m+1,
            // so just O(m) space. Initialize the current row.
            int curRow = 0, nextRow = 1;
            int[][] rows = new int[][] { new int[m + 1], new int[m + 1] };
            for (int j = 0; j <= m; ++j) rows[curRow][j] = j;

            // For each virtual row (since we only have physical storage for two)
            for (int i = 1; i <= n; ++i)
            {
                // Fill in the values in the row
                rows[nextRow][0] = i;
                for (int j = 1; j <= m; ++j)
                {
                    int dist1 = rows[curRow][j] + 1;
                    int dist2 = rows[nextRow][j - 1] + 1;
                    int dist3 = rows[curRow][j - 1] +
                        (first[i - 1].Equals(second[j - 1]) ? 0 : 1);

                    rows[nextRow][j] = Math.Min(dist1, Math.Min(dist2, dist3));
                }

                // Swap the current and next rows
                if (curRow == 0)
                {
                    curRow = 1;
                    nextRow = 0;
                }
                else
                {
                    curRow = 0;
                    nextRow = 1;
                }
            }

            // Return the computed edit distance
            return rows[curRow][m];
        }
        /// <summary>
        /// The allowed characters for use with the Rotten Tomatoes API queries - security and match reasons
        /// </summary>
        const string allowed_chars = "1234567890abcdefghijklmnopqrstuvwxyzàâäæçéèêëîïôœùûü ";
        /// <summary>
        /// Strips down a file name for simularity comparison.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        static string StripName(string text)
        {
            text = text.ToLower(); // Make it lower-case
            string t = "";
            char c;
            for (int i = 0; i < text.Length; i++)
                if (allowed_chars.Contains(text[i].ToString())) t += text[i];
            return t;
        }
        #endregion

        #region "IMDB Database Downloader"
        public static bool _IMDB_Downloading = false;
        public static Thread Thread_IMDB_Database_Download = null;
        const string _IMDB_DownloadDatabaseFile_URL = @"http://ftp.sunet.se/pub/tv+movies/imdb/plot.list.gz";
        const int _IMDB_DownloadDatabaseFile_Buffer = 4096;
        public static void IMDB_DownloadDatabaseFile()
        {
            // Create a separate thread responsible for downloading the database file
            Thread t = new Thread(new ParameterizedThreadStart(_IMDB_DownloadDatabaseFile));
            t.Start();
            Thread_IMDB_Database_Download = t;
        }
        private static void _IMDB_DownloadDatabaseFile(object o)
        {
            try
            {
                _IMDB_Downloading = true;
                // Check the cache directory exists
                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Cache"))
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/Cache");
                // Download the database file from IMDB
                WebRequest req = WebRequest.Create(_IMDB_DownloadDatabaseFile_URL);
                WebResponse resp = req.GetResponse();
                Stream resp_stream = resp.GetResponseStream();
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Cache/imdb.plot.list.gz"))
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "/Cache/imdb.plot.list.gz");
                FileStream file = File.OpenWrite(AppDomain.CurrentDomain.BaseDirectory + "/Cache/imdb.plot.list.gz");

                byte[] buffer = new byte[_IMDB_DownloadDatabaseFile_Buffer];
                int bytes;
                double totalBytes = 0;
                do
                {
                    bytes = resp_stream.Read(buffer, 0, _IMDB_DownloadDatabaseFile_Buffer);
                    file.Write(buffer, 0, bytes);
                    file.Flush();
                    totalBytes += bytes;
                    System.Diagnostics.Debug.WriteLine("Downloading IMDB... (" + (Math.Round(totalBytes / (double)resp.ContentLength, 2) * 100) + "%) @ " + DateTime.Now.ToString("dd/MM/yyyy - HH:mm:ss:ff - " + totalBytes + " of " + resp.ContentLength));
                }
                while (bytes > 0);

                file.Close();
                file.Dispose();
                file = null;
                System.Diagnostics.Debug.WriteLine("Extracting...");
                // Reopen the file stream for reading
                file = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "/Cache/imdb.plot.list.gz", FileMode.Open);
                // Open a decompression stream
                GZipInputStream gis = new GZipInputStream(file);

                FileStream file_decom = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "/Cache/imdb.plot.list", FileMode.Create);
                buffer = new byte[_IMDB_DownloadDatabaseFile_Buffer];
                StreamUtils.Copy(gis, file_decom, buffer);
                file_decom.Flush();
                file_decom.Close();
                file_decom.Dispose();
                gis.Close();
                gis.Dispose();
                // Delete gz file
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "/Cache/imdb.plot.list.gz");
                _IMDB_Downloading = false;
            }
            catch(Exception ex)
            {
                _IMDB_Downloading = false;
                System.Diagnostics.Debug.WriteLine("IMDB Download Failure!\n" + ex.Message + "\n\nStack:\n" + ex.StackTrace);
            }
        }
        #endregion
    }
}
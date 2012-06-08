/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /UberMedia/Indexer.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 *  Responsible for running in the background and indexing each drive/folder
 *  added by the user; the files in the drives are virtually indexed in the database
 *  if their extension matches the type of file to be indexed in the folder (specified
 *  by the user). The indexer may also invoke the thumbnail generation service to generate
 *  thumbnails of media.
 */
using System;
using System.Collections.Generic;
using System.Web;
using UberLib.Connector;
using System.Threading;
using System.IO;

namespace UberMedia
{
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
                    string parentFolder = GetParentVITEMID(tia.pfolderid, directory.Remove(0, tia.base_path.Length), ref Cache, tia.Connector);
                    tia.Connector.Query_Execute("INSERT INTO virtual_items (pfolderid, type_uid, title, phy_path, parent, date_added) VALUES('" + tia.pfolderid + "', '100', '" + Utils.Escape(Path.GetFileName(directory)) + "', '" + Utils.Escape(directory.Remove(0, tia.base_path.Length)) + "', " + (parentFolder != null ? "'" + Utils.Escape(parentFolder) + "'" : "NULL") + ", NOW());");
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
                    string parentFolder = GetParentVITEMID(tia.pfolderid, file.Remove(0, tia.base_path.Length), ref Cache, tia.Connector);
                    string vitemid = tia.Connector.Query_Scalar("INSERT INTO virtual_items (pfolderid, type_uid, title, description, phy_path, parent, date_added) VALUES('" + tia.pfolderid + "', '" + Utils.Escape(ExtensionsMap[ext]) + "', '" + Utils.Escape(title) + "', '" + Utils.Escape(desc) + "', '" + Utils.Escape(file.Remove(0, tia.base_path.Length)) + "', " + (parentFolder != null ? "'" + Utils.Escape(parentFolder) + "'" : "NULL") + ", NOW()); SELECT LAST_INSERT_ID();").ToString();
                    if (ThumbnailExts.ContainsKey(ext)) ThumbnailGeneratorService.AddToQueue(file, vitemid, ThumbnailExts[ext]);
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
            if (lsep == rsep) return null; // Item is at the top
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
}
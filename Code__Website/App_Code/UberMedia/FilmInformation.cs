using System;
using System.Collections.Generic;
using System.Web;
using UberLib.Connector;
using System.IO;
using System.Net;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using System.Threading;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Core;

namespace UberMedia
{
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
            catch (Exception ex)
            {
                _IMDB_Downloading = false;
                System.Diagnostics.Debug.WriteLine("IMDB Download Failure!\n" + ex.Message + "\n\nStack:\n" + ex.StackTrace);
            }
        }
        #endregion
    }
}
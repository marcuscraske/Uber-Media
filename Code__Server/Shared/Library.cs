/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /Shared/Library.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 * A library of shared methods.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace UberMediaServer
{
    public static class Library
    {
        /// <summary>
        /// Fetches data from a URL and returns the data.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string fetchData(string url)
        {
            try
            {
                WebClient wc = new WebClient();
                return System.Text.Encoding.UTF8.GetString(wc.DownloadData(url));
            }
            catch(Exception ex)
            {
                throw new LibraryConnectionFailure("Failed to fetch data from '" + url + "'!", ex);
            }
        }
    }
}

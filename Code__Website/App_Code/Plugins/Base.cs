﻿ ﻿/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber CMS
 * File:                    /App_Code/Plugins/Base.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 * The base class for a plugin; this is used for enum referencing and to display the
 * methods available for developers.
 */
using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using UberLib.Connector;

namespace UberCMS.Plugins
{
    /// <summary>
    /// An example class for plugins (this should also be used for referencing enum types); you shouldn't copy the State enum though.
    /// 
    /// Rules/pro-tips:
    /// - handleRequest
    ///     - If "CONTENT" in pageElements is left null/undefined, a 404 will occur.
    /// - /Cache should be used for temp content such as uploaded zip archives; this directory will automatically be wiped every-time the appicaton starts.
    /// - Your plugin SHOULD have the enable, disable and uninstall methods...regardless if they're used or not (simply return null if you don't want to use them).
    /// - Any file with the extension .file will have the extension ".file" removed when installed via contentInstall; this is because JS files are not
    /// allowed in App_Code.
    /// </summary>
    public static class Base
    {
        public enum State
        {
            Uninstalled = 0,
            Disabled = 1,
            Enabled = 2
        }
        /// <summary>
        /// Invoked to enable the plugin - any SQL, content installation etc should occur here.
        /// </summary>
        /// <returns></returns>
        public static string enable(string pluginid, Connector conn)
        {
            return "Base plugin cannot be enabled";
        }
        /// <summary>
        /// Invoked to disable the plugin - the opposite of the enable method, hence uninstall any SQL etc.
        /// </summary>
        /// <returns></returns>
        public static string disable(string pluginid, Connector conn)
        {
            return "Base plugin cannot be disabled";
        }
        /// <summary>
        /// Invoked to uninstall the plugin.
        /// </summary>
        /// <param name="pluginid"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static string uninstall(string pluginid, Connector conn)
        {
            return "Base plugin cannot be uninstalled";
        }
        /// <summary>
        /// Invoked when the CMS stops.
        /// </summary>
        /// <returns></returns>
        public static string cmsStart(string pluginid, Connector conn)
        {
            return "Base plugin cannot be started";
        }
        /// <summary>
        /// Invoked when the CMS starts.
        /// </summary>
        /// <returns></returns>
        public static string cmsStop(string pluginid, Connector conn)
        {
            return "Base plugin cannot be stopped...nananananna BATMAN!";
        }
        /// <summary>
        /// Invoked when a request is mapped to this plugin.
        /// </summary>
        /// <param name="pageElements"></param>
        public static void handleRequest(string pluginid, Connector conn, ref Misc.PageElements pageElements, HttpRequest request, HttpResponse response, ref string baseTemplateParent)
        {
        }
        /// <summary>
        /// Invoked when a request fails because a suitable plugin cannot be found.
        /// </summary>
        /// <param name="pageElements"></param>
        public static void handleRequestNotFound(string pluginid, Connector conn, ref Misc.PageElements pageElements, HttpRequest request, HttpResponse response, ref string baseTemplateParent)
        {
        }
        /// <summary>
        /// Invoked when ever a new request starts, before a plugin's handler is invoked.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="pageElements"></param>
        /// <param name="request"></param>
        public static void requestStart(string pluginid, Connector conn, ref Misc.PageElements pageElements, HttpRequest request, HttpResponse response, ref string baseTemplateParent)
        {
        }
        /// <summary>
        /// Invoked when-ever a new request has been processed, right after a plugin's handler has been invoked.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="pageElements"></param>
        /// <param name="request"></param>
        public static void requestEnd(string pluginid, Connector conn, ref Misc.PageElements pageElements, HttpRequest request, HttpResponse response, ref string baseTemplateParent)
        {
        }
        /// <summary>
        /// Invoked when a CMS error occurs.
        /// </summary>
        /// <param name="ex"></param>
        public static void handleError(string pluginid, Exception ex)
        {
        }
        /// <summary>
        /// Invoked periodically if the plugin has a cycle_interval defined.
        /// </summary>
        public static void handleCycle(string pluginid, Connector conn)
        {
        }
    }
}

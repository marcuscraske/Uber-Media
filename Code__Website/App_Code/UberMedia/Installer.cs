/*
 * License:     Creative Commons Attribution-ShareAlike 3.0 unported
 * File:        App_Code/Installer.cs
 * Author(s):   limpygnome
 * 
 * Contains functions for the installation and upgrade process of the web application.
 * 
 * Improvements/bugs:
 *          -   none.
 */

using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using UberLib.Connector;
using System.Xml;

namespace UberMedia
{
    public class Installer
    {
        /// <summary>
        /// Returns an error message; a successful installation will return string.Empty, as well as start-up the Core.
        /// </summary>
        /// <returns></returns>
        public static string Install(Connector Connector)
        {
            // Install SQL
            string error = SQLFile(AppDomain.CurrentDomain.BaseDirectory + "/Install/Install_" + Core.Cache_Settings["db_type"] + ".sql", Connector);
            if (error.Length > 0) return "Install.sql - " + error;
            // Install HTML templates
            error = HtmlTemplatesInstall(AppDomain.CurrentDomain.BaseDirectory + "/Install/Templates", Connector);
            if (error.Length > 0) return "HTML templates - " + error;
            return null;
        }
        /// <summary>
        /// Executes an SQL script; this also removes comments.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string SQLFile(string path, Connector Connector)
        {
            try
            {
                if (!File.Exists(path))
                    throw new Exception("SQL script '" + path + "' could not be found!");
                else
                {
                    StringWriter statements = new StringWriter();
                    // Build the new list of statements to be executed by stripping out any comments
                    string data = File.ReadAllText(path).Replace("\r", "");
                    int commentIndex;
                    foreach (string line in data.Split('\n'))
                    {
                        commentIndex = line.IndexOf("--");
                        if (commentIndex == -1)
                            statements.WriteLine(line);
                        else if (commentIndex < line.Length)
                            statements.WriteLine(line.Substring(0, commentIndex));
                    }
                    // Execute the statements
                    Connector.Query_Execute(statements.ToString());
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                return ex.Message + " - " + (ex.InnerException != null ? ex.InnerException.Message : "unknown reason");
            }
        }
        /// <summary>
        /// Synchronizes the HTML templates folder with the database.
        /// </summary>
        /// <param name="base_directory"></param>
        /// <param name="Connector"></param>
        /// <returns></returns>
        public static string HtmlTemplatesInstall(string base_directory, Connector connector)
        {
            // Attempt to delete any pre-existing templates
            try
            {
                connector.Query_Execute("DELETE FROM html_templates");
            }
            catch(Exception ex)
            {
                return "Failed to delete templates - " + ex.Message;
            }
            // Insert each template
            XmlDocument doc;
            string key = null;
            foreach (string file in Directory.GetFiles(base_directory, "*.xml", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    key = Path.GetFileName(file);
                    key = key.Remove(key.Length - 4, 4);
                    doc = new XmlDocument();
                    doc.Load(file);
                    connector.Query_Execute("INSERT INTO `html_templates` (`hkey`, `html`, `description`) VALUES('" + Utils.Escape(key) + "', '" + Utils.Escape(doc["template"]["html"].InnerText) + "', '" + Utils.Escape(doc["template"]["description"].InnerText) + "');");
                }
                catch (Exception ex)
                {
                    return "Failed to install template - " + (key ?? "unknown key") + " - " + ex.Message;
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// Dumps all of the HTML templates from the database to file.
        /// </summary>
        /// <param name="base_directory"></param>
        /// <param name="Connector"></param>
        /// <returns></returns>
        public static string HtmlTemplatesDump(string base_directory, Connector Connector)
        {
            if (!Directory.Exists(base_directory))
                return "Directory for dumping HTML templates does not exist!";
            else
            {
                // Delete all pre-existing templates
                foreach (string file in Directory.GetFiles(base_directory, "*", SearchOption.TopDirectoryOnly))
                    File.Delete(file);
                // Go through each template and write to file
                XmlDocument doc;
                try
                {
                    foreach (ResultRow temp in Connector.Query_Read("SELECT * FROM html_templates ORDER BY hkey ASC"))
                    {
                        doc = new XmlDocument();
                        XmlWriter xw = XmlWriter.Create(base_directory + "/" + temp["hkey"] + ".xml");
                        // Write document header
                        xw.WriteStartDocument();
                        xw.WriteStartElement("template");
                        // Write description
                        xw.WriteStartElement("description");
                        xw.WriteCData(temp["description"]);
                        xw.WriteEndElement();
                        // Write content
                        xw.WriteStartElement("html");
                        xw.WriteCData(temp["html"]);
                        xw.WriteEndElement();
                        // Write document footer
                        xw.WriteEndElement();
                        xw.WriteEndDocument();
                        xw.Flush();
                        xw.Close();
                        xw = null;
                    }
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
                return string.Empty;
            }
        }
    }
}
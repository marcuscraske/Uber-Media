/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /Install/Installer.aspx
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 * Responsible for the initial setup of the media library.
 */
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UberLib.Connector;
using UberLib.Connector.Connectors;
using System.IO;
using System.Xml;

public partial class Installer : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Determine the stage of the installation
        string content = "";
        string title = "Undefined";
        switch (Request.Form["page"] ?? Request.QueryString["1"])
        {
            // User decides which database connector to use for storing data
            default:
                title = "Choose Database Type...";
                content = File.ReadAllText(Server.MapPath("/Install/Template_dbselect.html")).Replace("%URL%", ResolveUrl(""));
                C1.Attributes["class"] = "S";
                break;
            // User decides database settings
            case "database":
                title = "Database Settings";
                switch (Request.Form["db_type"])
                {
                    // No db type specified
                    default: Response.Redirect(ResolveUrl("/installer")); break;
                    // MySQL
                    case "mysql":
                        database_MySQL(ref content);
                        break;
                }
                C1.Attributes["class"] = "S";
                break;
            // User decides install settings - currently not available, but added for the future
            case "install":
                title = "Installation";
                C2.Attributes["class"] = "S";
                // Create the connector
                UberMedia.Core.CacheSettings_Reload(false);
                Connector conn = UberMedia.Core.Connector_Create(false);
                // Install the database
                string error = UberMedia.Installer.Install(conn);
                if (error != null)
                    content = "<p>Failed to install library:</p><p>" + error + "</p><p><a href=\"" + ResolveUrl("") + "/installer/install\">Try again?</a></p>";
                else
                {
                    // Disconnect the connector
                    conn.Disconnect();
                    // Restart the core
                    UberMedia.Core.Core_Start();
                    // Redirect to the finished page
                    Response.Redirect(ResolveUrl("/installer/finish"));
                }
                break;
            // All of the installation settings are written and the website is installed (once validated)
            case "finish":
                title = "Finished";
                C3.Attributes["class"] = "S";
                content = File.ReadAllText(Server.MapPath("/Install/Template_finished.html")).Replace("%URL%", ResolveUrl(""));
                break;
        }
        // Set page title and content
        Title += " - " + title;
        AREA_CONTENT.InnerHtml = "<h2>" + title + "</h2>" + content;
    }
    void database_MySQL(ref string content)
    {
        string connError = null;
        string mysqlHost = Request.Form["host"] ?? "127.0.0.1";
        string mysqlPort = Request.Form["port"] ?? "3306";
        string mysqlDatabase = Request.Form["database"] ?? "ubermedia";
        string mysqlUsername = Request.Form["username"] ?? "root";
        string mysqlPassword = Request.Form["password"];
        // Check if any settings have been posted, if so test and save them
        if (mysqlHost != null && mysqlPort != null && mysqlDatabase != null && mysqlUsername != null && mysqlPassword != null)
        {
            // Test the connection
            try
            {
                MySQL mysql = new MySQL();
                mysql.Settings_Host = mysqlHost;
                mysql.Settings_Port = int.Parse(mysqlPort);
                mysql.Settings_Database = mysqlDatabase;
                mysql.Settings_User = mysqlUsername;
                mysql.Settings_Pass = mysqlPassword;
                mysql.Connect();
                mysql.Disconnect();
            }
            catch (FormatException)
            {
                connError = "Invalid port!";
            }
            catch (ConnectionFailureException ex)
            {
                connError = "Failed to connect! - " + (ex.InnerException != null ? ex.InnerException.Message : "unknown reason");
            }
            catch (Exception ex)
            {
                connError = "Unknown error occurred: " + ex.Message;
            }
            // Save the settings if valid
            if (connError == null)
            {
                XmlWriter xw = XmlWriter.Create(AppDomain.CurrentDomain.BaseDirectory + "/Config.xml");
                xw.WriteStartDocument();
                xw.WriteStartElement("settings");

                xw.WriteStartElement("db_type");
                xw.WriteCData("mysql");
                xw.WriteEndElement();

                xw.WriteStartElement("db_host");
                xw.WriteCData(mysqlHost);
                xw.WriteEndElement();

                xw.WriteStartElement("db_port");
                xw.WriteCData(mysqlPort);
                xw.WriteEndElement();

                xw.WriteStartElement("db_database");
                xw.WriteCData(mysqlDatabase);
                xw.WriteEndElement();

                xw.WriteStartElement("db_user");
                xw.WriteCData(mysqlUsername);
                xw.WriteEndElement();

                xw.WriteStartElement("db_pass");
                xw.WriteCData(mysqlPassword);
                xw.WriteEndElement();

                xw.WriteEndElement();
                xw.WriteEndDocument();
                xw.Flush();
                xw.Close();
                // Move onto the install stage
                Response.Redirect(ResolveUrl("/installer/install"));
            }
        }

        content = File.ReadAllText(Server.MapPath("/Install/Template_mysql.html"))
            .Replace("%HOST%", mysqlHost ?? string.Empty)
            .Replace("%PORT%", mysqlPort ?? string.Empty)
            .Replace("%DATABASE%", mysqlDatabase ?? string.Empty)
            .Replace("%USERNAME%", mysqlUsername ?? string.Empty)
            .Replace("%PASSWORD%", mysqlPassword ?? string.Empty)
            .Replace("%ERROR_STYLE%", connError != null ? "display: block; visibility: visible;" : string.Empty)
            .Replace("%ERROR_MESSAGE%", connError ?? string.Empty)
            .Replace("%URL%", ResolveUrl(""));
    }
}
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
        if (!File.Exists(Server.MapPath("/Config.xml")))
        {
            string error = "";
            // Database settings
            // -- Check if the user has specified settings
            if (Request.Form["db_host"] != null && Request.Form["db_port"] != null && Request.Form["db_database"] != null && Request.Form["db_user"] != null && Request.Form["db_pass"] != null)
            {
                string db_host = Request.Form["db_host"];
                string db_port = Request.Form["db_port"];
                string db_database = Request.Form["db_database"];
                string db_user = Request.Form["db_user"];
                string db_pass = Request.Form["db_pass"];
                // Validate input
                if (!IsValidPort(db_port))
                    error = "Invalid port!";
                else if (Uri.CheckHostName(db_host) == UriHostNameType.Unknown)
                    error = "Invalid host!";
                else if (db_database.Length < 1 || db_database.Length > 122) // 122 - based on http://dev.mysql.com/doc/refman/5.0/en/mysql-cluster-limitations-database-objects.html
                    error = "Invalid database!";
                else if (db_user.Length < 1 || db_user.Length > 16) // 16 - based on http://dev.mysql.com/doc/refman/4.1/en/user-names.html
                    error = "Invalid username!";
                else
                {
                    int previousMajor = 0;
                    int previousMinor = 0;
                    int previousBuild = 0;
                    // Attempt to establish a connection to the database
                    MySQL Connector = new MySQL();
                    try
                    {
                        Connector.Settings_Host = db_host;
                        Connector.Settings_Port = int.Parse(db_port);
                        Connector.Settings_User = db_user;
                        Connector.Settings_Pass = db_pass;
                        Connector.Settings_Database = db_database;
                        Connector.Connect();
                        // Check if a previous installation exists
                        try
                        {
                            Result data = Connector.Query_Read("SELECT value FROM settings WHERE keyid='major' OR keyid='minor' OR keyid='build' ORDER BY keyid ASC");
                            if (data.Rows.Count == 3)
                            {
                                previousBuild = int.Parse(data[0]["value"]);
                                previousMajor = int.Parse(data[1]["value"]);
                                previousMinor = int.Parse(data[2]["value"]);
                            }
                            else
                                throw new Exception("unable to determine the version of the previous installation!");
                        }
                        catch
                        {
                        }
                    }
                    catch(Exception ex)
                    {
                        error = "Unable to establish a connection with the database server using the provided information - " + ex.Message + "!";
                    }
                    if(error.Length == 0)
                    {
                        // Write the new configuration to file
                        XmlWriter xw = XmlWriter.Create(AppDomain.CurrentDomain.BaseDirectory + "/Config.xml");
                        xw.WriteStartDocument();
                        xw.WriteStartElement("settings");

                        xw.WriteStartElement("db_host");
                        xw.WriteCData(db_host);
                        xw.WriteEndElement();

                        xw.WriteStartElement("db_port");
                        xw.WriteCData(db_port);
                        xw.WriteEndElement();

                        xw.WriteStartElement("db_database");
                        xw.WriteCData(db_database);
                        xw.WriteEndElement();

                        xw.WriteStartElement("db_user");
                        xw.WriteCData(db_user);
                        xw.WriteEndElement();

                        xw.WriteStartElement("db_pass");
                        xw.WriteCData(db_pass);
                        xw.WriteEndElement();

                        xw.WriteEndElement();
                        xw.WriteEndDocument();
                        xw.Flush();
                        xw.Close();
                        xw = null;
                        // Install the database
                        UberMedia.Installer.Install(previousMajor, previousMinor, previousBuild, Connector);
                        // Redirect to the finished page - no custom install review/settings page yet
                        Response.Redirect(ResolveUrl("/installer"));
                    }
                }
            }
            // -- Build content
            AREA_CONTENT.InnerHtml =
@"
<h2>Config</h2>
<div class=""ERROR"" style=""" + (error.Length != 0 ? "display: block; visibility: visible;" : "") + @""">" + HttpUtility.HtmlEncode(error) + @"</div>
<form method=""post"" action=""" + ResolveUrl("/installer") + @""">
<table>
    <tr>
        <th colspan=""2"">Database Connectivity</th>
    </tr>
    <tr>
        <td>Host:</td>
        <td><input type=""text"" name=""db_host"" value=""%DB_HOST%"" /></td>
    </tr>
    <tr>
        <td>Port:</td>
        <td><input type=""text"" name=""db_port"" value=""%DB_PORT%"" /></td>
    </tr>
    <tr>
        <td>Database:</td>
        <td><input type=""text"" name=""db_database"" value=""%DB_DATABASE%"" /></td>
    </tr>
    <tr>
        <td>User:</td>
        <td><input type=""text"" name=""db_user"" value=""%DB_USER%"" /></td>
    </tr>
    <tr>
        <td>Pass:</td>
        <td><input type=""password"" name=""db_pass"" /></td>
    </tr>
    <tr>
        <th colspan=""2"">Finalize</th>
    </tr>
    <tr style=""text-align: right;"">
        <td colspan=""2""><input type=""submit"" value=""Install"" /></td>
    </tr>
</table>
</form>
"
            .Replace("%DB_HOST%", Request.Form["db_host"] ?? "")
            .Replace("%DB_PORT%", Request.Form["db_port"] ?? "")
            .Replace("%DB_DATABASE%", Request.Form["db_database"] ?? "")
            .Replace("%DB_USER%", Request.Form["db_user"] ?? "");

            C1.Attributes["class"] += " S";
        }
        else if (Request.QueryString["p"] == "install")
        {
            // Database is configured and database installed, provide additional options
        }
        else
        {
            // Display the finished page
            AREA_CONTENT.InnerHtml = @"
<h2>Finish</h2>
<p>Uber Media's central node is now ready to index your folders and delegate to media computers!</p><p>You will first need to <a href=""" + ResolveUrl("/admin/folders") + @""">add a folder</a>...</p>
";
        }
    }
    bool IsValidPort(string value)
    {
        try
        {
            int i = int.Parse(value);
            return i >= 1 && i <= 49151;
        }
        catch { return false; }
    }
}
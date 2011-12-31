/*
 * Creative Commons Attribution-ShareAlike 3.0 unported
 * Default.aspx.cs
 * 
 * This page is responsible for nearly the entire site.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UberLib.Connector;
using System.IO;
using System.Xml;

public partial class _Default : System.Web.UI.Page
{
    #region "Enums"
    public enum States
    {
        Playing = 1,
        Paused = 3,
        Stopped = 5,
        Error = 7,
        Loading = 9,
        WaitingForInput = 11,
        Idle = 13,
    }
    #endregion

    #region "Constants"
    const int PHYSICAL_FOLDER_TITLE_MIN = 1;
    const int PHYSICAL_FOLDER_TITLE_MAX = 30;
    const int PHYSICAL_FOLDER_PATH_MIN = 3;
    const int PHYSICAL_FOLDER_PATH_MAX = 248;
    const int TAG_TITLE_MIN = 1;
    const int TAG_TITLE_MAX = 35;
    const int TERMINAL_KEY_MIN = 3;
    const int TERMINAL_KEY_MAX = 20;
    const int TERMINAL_TITLE_MIN = 1;
    const int TERMINAL_TITLE_MAX = 28;
    const int PAGE_REQUESTS_ITEMSPERPAGE = 5;
    #endregion

    #region "Variables"
    /// <summary>
    /// Used to interface with the database.
    /// </summary>
    Connector Connector;
    /// <summary>
    /// This stores elements by name, which replaces <!--name--> in the template with the value.
    /// </summary>
    public Dictionary<string, string> PageElements = new Dictionary<string, string>();
    #endregion

    #region "Controller"
    protected void Page_Load(object sender, EventArgs e)
    {
        if (UberMedia.Core.State == UberMedia.Core.CoreState.NotInstalled)
        {
            Response.Redirect(ResolveUrl("/installer"));
            return;
        }
        else if (UberMedia.Core.State != UberMedia.Core.CoreState.Running)
        {
            Response.Write("<h1>Core Failure</h1>");
            Response.Write("<p>Your media collection cannot be accessed because the core has failed to start, current state: " + UberMedia.Core.State.ToString() + "</p>");
            Response.Write("<p>Common issues could be database connectivity, corrupt configuration or/and bad settings!</p>");
            Response.End();
        }
#if DEBUG
        UberMedia.Core.HtmlTemplates_Reload();
#endif
        // Create connector
        Connector = UberMedia.Core.Connector_Create(false);
        // Set some default page elements
        PageElements["URL"] = ResolveUrl("");
        // Invoke method corresponding to requested page
        try
        {
            this.GetType().InvokeMember("Page__" + Request.QueryString["page"], System.Reflection.BindingFlags.InvokeMethod, null, this, new object[] { });
        }
        catch (MissingMethodException ex)
        {
            this.GetType().InvokeMember("Page__404", System.Reflection.BindingFlags.InvokeMethod, null, this, new object[] { });
        }
        // Set media computers
        string mediacomputers = "";
        foreach (ResultRow mediacomputer in Connector.Query_Read("SELECT title, terminalid FROM terminals ORDER BY title ASC"))
        {
            if (Session["mediacomputer"] == null) Session["mediacomputer"] = mediacomputer["terminalid"];
            mediacomputers += "<option value=\"" + mediacomputer["terminalid"] + "\"" + (Session["mediacomputer"] != null && (string)Session["mediacomputer"] == mediacomputer["terminalid"] ? " selected" : "") + ">" + HttpUtility.HtmlEncode(mediacomputer["title"]) + "</option>";
        }
        PageElements["MEDIACOMPUTERS"] = mediacomputers;
        PageElements["MEDIACOMPUTER"] = (string)(Session["mediacomputer"] ?? "");
        // Load page template
        string template = UberMedia.Core.Cache_HtmlTemplates["basepage"];
        // Output page
        Response.Write(ReplaceElements(template, 0, 3));
        // Dispose connector
        Connector.Disconnect();
        Connector = null;
    }
    #endregion

    #region "Methods - Pages"
#if DEBUG
    public void Page__debug()
    {
        UberMedia.Installer.HtmlTemplatesDump(AppDomain.CurrentDomain.BaseDirectory + "/Install/" + UberMedia.Core.versionMajor + "." + UberMedia.Core.versionMinor + "." + UberMedia.Core.versionBuild + "/Templates", Connector);
        PageElements["CONTENT_RIGHT"] = "Dumped HTML templates from database to:<br />" + AppDomain.CurrentDomain.BaseDirectory + "/Install/" + UberMedia.Core.versionMajor + "." + UberMedia.Core.versionMinor + "." + UberMedia.Core.versionBuild + "/Templates";
    }
#endif
    public void Page__404()
    {
        PageElements["CONTENT_RIGHT"] = UberMedia.Core.Cache_HtmlTemplates["404"];
    }
    public void Page__home()
    {
        // Build list of folders/drives for the sidebar
        string folders = "";
        foreach (ResultRow folder in Connector.Query_Read("SELECT title, pfolderid FROM physical_folders ORDER BY title ASC")) folders += "<a href=\"%URL%/browse/" + folder["pfolderid"] + "\"><img src=\"%URL%/Content/Images/folder.png\" alt=\"Folder\" />" + folder["title"] + "</a>";
        // Build list of new items
        string i_new = "";
        foreach (ResultRow file in Connector.Query_Read("SELECT vi.*, it.thumbnail FROM virtual_items AS vi LEFT OUTER JOIN item_types AS it ON it.uid=vi.type_uid WHERE vi.type_uid != '100' ORDER BY vi.date_added DESC LIMIT 6")) i_new += UberMedia.Core.Cache_HtmlTemplates["browse_file"].Replace("%TITLE%", file["title"]).Replace("%TITLE_S%", Title_Split(file["title"], 15)).Replace("%URL%", ResolveUrl("/item/" + file["vitemid"])).Replace("%THUMBNAIL%", file["thumbnail"].Equals("1") ? ResolveUrl("/Content/Thumbnails/" + file["vitemid"] + ".png") : ResolveUrl("/Content/Images/thumbnail.png"));
        // Build list of random items
        string i_rand = "";
        foreach (ResultRow file in Connector.Query_Read("SELECT vi.*, it.thumbnail FROM virtual_items AS vi LEFT OUTER JOIN item_types AS it ON it.uid=vi.type_uid WHERE vi.type_uid != '100' ORDER BY RAND() DESC LIMIT 6")) i_rand += UberMedia.Core.Cache_HtmlTemplates["browse_file"].Replace("%TITLE%", file["title"]).Replace("%TITLE_S%", Title_Split(file["title"], 15)).Replace("%URL%", ResolveUrl("/item/" + file["vitemid"])).Replace("%THUMBNAIL%", file["thumbnail"].Equals("1") ? ResolveUrl("/Content/Thumbnails/" + file["vitemid"] + ".png") : ResolveUrl("/Content/Images/thumbnail.png"));
        PageElements["CONTENT_LEFT"] = UberMedia.Core.Cache_HtmlTemplates["home_sidebar"].Replace("%FOLDERS%", folders.Length == 0 ? "None." : folders).Replace("%URL%", ResolveUrl(""));
        PageElements["CONTENT_RIGHT"] = UberMedia.Core.Cache_HtmlTemplates["home"].Replace("%NEWEST%", i_new).Replace("%RANDOM%", i_rand);
        SelectNavItem("HOME");
    }
    public void Page__browse()
    {
        // URL struct:
        // /browse/<null> - all files
        // /browse/<drive> - specific drive
        // /browse/<drive>/<folder id> - specific folder
        // Ordering struct:
        // title, rating, views, date_added

        // Prep to list items by gathering any possible filtering/sorting attribs
        int page = Request.QueryString["p"] != null && IsNumeric(Request.QueryString["p"]) ? int.Parse(Request.QueryString["p"]) : 1;
        int items_per_page = 15;
        string sort = Request.QueryString["sort"] != null && Request.QueryString["sort"].Length > 0 ? Request.QueryString["sort"] == "title" ? "vi.title" : Request.QueryString["sort"] == "ratings" ? "vi.cache_rating" : Request.QueryString["sort"] == "views" ? "vi.views" : Request.QueryString["sort"] == "date_added" ? "vi.date_added" : "vi.title" : "vi.title";
        bool sort_asc = Request.QueryString["sd"] == null || !Request.QueryString["sd"].Equals("desc");
        string qtag = Request.QueryString["tag"] != null && IsNumeric(Request.QueryString["tag"]) ? Request.QueryString["tag"] : "";
        // Build current URL
        string current_url = ResolveUrl("/browse/" + (Request.QueryString["1"] != null ? Request.QueryString["1"] + (Request.QueryString["2"] != null ? "/" + Request.QueryString["2"] : "") : "") + "?");
        string current_params = "&sort=" + (Request.QueryString["sort"] ?? "") + "&sd=" + (sort_asc ? "asc" : "desc");
        string current_tag = "&tag=" + qtag;
        // Build content area
        string content = "<h2>Tags</h2>";
        // Build tags
        Result tags = Connector.Query_Read("SELECT t.tagid, t.title FROM tags AS t, tag_items AS ti, virtual_items AS vi WHERE ti.tagid=t.tagid" + (Request.QueryString["1"] != null ? " AND ti.vitemid=vi.vitemid AND vi.pfolderid='" + Utils.Escape(Request.QueryString["1"]) + "'" + (Request.QueryString["2"] != null ? " AND (vi.vitemid='" + Utils.Escape(Request.QueryString["2"]) + "' OR vi.parent='" + Utils.Escape(Request.QueryString["2"]) + "')" : " AND vi.parent='0' ") : "") + " GROUP BY ti.tagid");
        if (tags.Rows.Count == 0) content += "No items are tagged.";
        else
        {
            foreach (ResultRow tag in tags)
                content += "<a href=\"" + current_url + current_params + "&tag=" + tag["tagid"] + "\" class=\"TAG\">" + tag["title"] + "</a>";
        }
        // If viewing an actual folder, grab the desc etc - also checks the folder exists
        if (Request.QueryString["1"] != null || Request.QueryString["2"] != null)
        {
            content += "<h2>Info</h2>";
            if (Request.QueryString["2"] != null) // Display sub-folder information - may contain synopsis if e.g. a TV show
            {
                Result data = Connector.Query_Read("SELECT vi.description FROM virtual_items AS vi WHERE vi.vitemid='" + Utils.Escape(Request.QueryString["2"]) + "'");
                if (data.Rows.Count == 0)
                {
                    Page__404();
                    return;
                }
                content += data[0]["description"].Length > 0 ? data[0]["description"] : "(no description)";
            }
            else // Build info pane due to being a physical folder
            {
                Result data = Connector.Query_Read("SELECT pfolderid, (SELECT COUNT('') FROM virtual_items WHERE pfolderid='" + Utils.Escape(Request.QueryString["1"]) + "' AND type_uid='100') AS c_folders, (SELECT COUNT('') FROM virtual_items WHERE pfolderid='" + Utils.Escape(Request.QueryString["1"]) + "' AND type_uid != '100') AS c_files FROM physical_folders WHERE pfolderid='" + Utils.Escape(Request.QueryString["1"]) + "';");
                if (data.Rows.Count == 0)
                {
                    Page__404();
                    return;
                }
                content += "This main folder has " + data[0]["c_files"] + " files and " + data[0]["c_folders"] + " folders indexed.";
            }
        }
        // Check if the user has specified an action
        if (Request.QueryString["action"] != null)
        {
            switch (Request.QueryString["action"])
            {
                case "queue_all":
                    if (Session["mediacomputer"] != null) // Ensure a media computer is selected
                    {
                        // Grab all of the items
                        Result items = Connector.Query_Read("SELECT vi.vitemid FROM virtual_items AS vi " + (qtag.Length != 0 ? "LEFT OUTER JOIN tag_items AS ti ON ti.vitemid=vi.vitemid " : "") + "LEFT OUTER JOIN item_types AS it ON it.uid=vi.type_uid WHERE vi.type_uid != '100'" + (qtag.Length != 0 ? " AND ti.tagid='" + Utils.Escape(qtag) + "'" : "") + (Request.QueryString["1"] != null ? " AND vi.pfolderid='" + Utils.Escape(Request.QueryString["1"]) + "'" + (Request.QueryString["2"] != null ? " AND vi.parent='" + Utils.Escape(Request.QueryString["2"]) + "'" : " AND vi.parent='0'") : "") + " ORDER BY " + sort + " " + (sort_asc ? "ASC" : "DESC"));
                        // Generate massive statement to add them to the terminal buffer
                        StringWriter statement = new StringWriter();
                        foreach (ResultRow item in items)
                            statement.Write(terminalBufferEntry("media", (string)Session["mediacomputer"], item["vitemid"], true));
                        // Execute statement
                        Connector.Query_Execute(statement.ToString());
                    }
                    // Redirect the user back
                    Response.Redirect(ResolveUrl(current_url + current_params + current_tag));
                    break;
            }
        }
        // Query files and folders
        Result folders = Connector.Query_Read("SELECT vi.* FROM virtual_items AS vi " + (qtag.Length != 0 ? "LEFT OUTER JOIN tag_items AS ti ON ti.vitemid=vi.vitemid " : "") + "WHERE vi.type_uid = '100'" + (qtag.Length != 0 ? " AND ti.tagid='" + Utils.Escape(qtag) + "'" : "") + (Request.QueryString["1"] != null ? " AND vi.pfolderid='" + Utils.Escape(Request.QueryString["1"]) + "'" + (Request.QueryString["2"] != null ? " AND vi.parent='" + Utils.Escape(Request.QueryString["2"]) + "'" : " AND vi.parent='0'") : "") + " ORDER BY " + sort + " " + (sort_asc ? "ASC" : "DESC"));
        Result files = Connector.Query_Read("SELECT vi.*, it.thumbnail FROM virtual_items AS vi " + (qtag.Length != 0 ? "LEFT OUTER JOIN tag_items AS ti ON ti.vitemid=vi.vitemid " : "") + "LEFT OUTER JOIN item_types AS it ON it.uid=vi.type_uid WHERE vi.type_uid != '100'" + (qtag.Length != 0 ? " AND ti.tagid='" + Utils.Escape(qtag) + "'" : "") + (Request.QueryString["1"] != null ? " AND vi.pfolderid='" + Utils.Escape(Request.QueryString["1"]) + "'" + (Request.QueryString["2"] != null ? " AND vi.parent='" + Utils.Escape(Request.QueryString["2"]) + "'" : " AND vi.parent='0'") : "") + " ORDER BY " + sort + " " + (sort_asc ? "ASC" : "DESC") + " LIMIT " + ((items_per_page * page) - items_per_page) + ", " + items_per_page.ToString());
        // List files
        content += "<h2>Files " + Browse_CreateNavigationBar(Request.QueryString["1"] ?? "", Request.QueryString["2"] ?? "") + "</h2>";
        if (files.Rows.Count != 0) foreach (ResultRow file in files) content += UberMedia.Core.Cache_HtmlTemplates["browse_file"].Replace("%TITLE%", file["title"]).Replace("%TITLE_S%", Title_Split(file["title"], 15)).Replace("%URL%", ResolveUrl("/item/" + file["vitemid"])).Replace("%THUMBNAIL%", file["thumbnail"].Equals("1") ? ResolveUrl("/Content/Thumbnails/" + file["vitemid"] + ".png") : ResolveUrl("/Content/Images/thumbnail.png"));
        else content += "<p>No items - check the sidebar for sub-folders on the left!</p>";
        // Build sidebar
        string sidebar = "<h2>Main Folders</h2>";
        // -- Build a list of all the main drives
        sidebar += UberMedia.Core.Cache_HtmlTemplates["browse_side_folder"].Replace("%URL%", ResolveUrl("")).Replace("%CLASS%", "").Replace("%IURL%", ResolveUrl("/browse")).Replace("%TITLE%", "All").Replace("%ICON%", ResolveUrl("/Content/images/folders.png"));
        foreach (ResultRow drive in Connector.Query_Read("SELECT pfolderid, title FROM physical_folders ORDER BY title ASC"))
            sidebar += UberMedia.Core.Cache_HtmlTemplates["browse_side_folder"]
                .Replace("%IURL%", ResolveUrl("/browse/" + drive["pfolderid"] + "?" + current_params))
                .Replace("%CLASS%", drive["pfolderid"].Equals(Request.QueryString["1"]) ? "selected" : "")
                .Replace("%TITLE%", drive["title"])
                .Replace("%ICON%", ResolveUrl("/Content/Images/folder.png"));
        // -- Build a list of options for the current items
        sidebar += "<h2>Options</h2>";
        sidebar += "<a href=\"" + current_url + "&action=queue_all\"><img src=\"<!--URL-->/Content/Images/play_queue.png\" alt=\"Queue All Items\" title=\"Queue All Items\" />Queue All Items</a>";
        // -- Display the sub-folders for this folder
        sidebar += "<h2>Sub-folders</h2>";
        if (folders.Rows.Count == 0) sidebar += "None.";
        else
            foreach(ResultRow folder in folders)
                sidebar += UberMedia.Core.Cache_HtmlTemplates["browse_side_folder"]
                    .Replace("%CLASS%", "")
                    .Replace("%IURL%", ResolveUrl("/browse/" + folder["pfolderid"] + "/" + folder["vitemid"] + "?" + current_params))
                    .Replace("%TITLE%", folder["title"].Length > 22 ? folder["title"].Substring(0, 22) + "..." : folder["title"])
                    .Replace("%ICON%", ResolveUrl("/Content/Images/folder.png"));
        // Attach footer
        content += UberMedia.Core.Cache_HtmlTemplates["browse_footer"]
            .Replace("%PAGE%", page.ToString())
            .Replace("%BUTTONS%",
                                    (page > 1 ? UberMedia.Core.Cache_HtmlTemplates["browse_footer_previous"].Replace("%URL%", current_url + current_params + current_tag  + "&p=" + (page - 1)) : "") +
                                    (page <= int.MaxValue ? UberMedia.Core.Cache_HtmlTemplates["browse_footer_next"].Replace("%URL%", current_url + current_params + current_tag + "&p=" + (page + 1)) : "")
                    );
        // Finalize page
        PageElements["CONTENT_LEFT"] = sidebar;
        PageElements["CONTENT_RIGHT"] = content;
        if (!sort_asc && sort == "vi.date_added") SelectNavItem("NEWEST");
        else if (!sort_asc && sort == "vi.views") SelectNavItem("POPULAR");
        else SelectNavItem("BROWSE");
    }
    public void Page__newest()
    {
        Response.Redirect(ResolveUrl("/browse?sort=date_added&sd=desc"));
    }
    public void Page__popular()
    {
        Response.Redirect(ResolveUrl("/browse?sort=views&sd=desc"));
    }
    public void Page__item()
    {
        string vitemid = Request.QueryString["1"] ?? "";
        if (vitemid.Length == 0)
        {
            Page__404();
            return;
        }
        Result data = Connector.Query_Read("SELECT vi.*, CONCAT(pf.physicalpath, vi.phy_path) AS path, it.thumbnail, it.title AS type FROM (virtual_items AS vi, physical_folders AS pf) LEFT OUTER JOIN item_types AS it ON it.uid=vi.type_uid WHERE vi.vitemid='" + Utils.Escape(vitemid) + "' AND vi.type_uid != '100' AND pf.pfolderid=vi.pfolderid;");
        if (data.Rows.Count == 0)
        {
            Page__404();
            return;
        }
        string content = "<h2>Viewing " + data[0]["title"] + " - " + Browse_CreateNavigationBar(data[0]["pfolderid"], data[0]["parent"]) + "</h2>";
        switch (Request.QueryString["2"])
        {
            case "modify":
                if (Request.Form["title"] != null && Request.Form["description"] != null && Request.Form["type_uid"] != null)
                {
                    string title = Request.Form["title"];
                    string description = Request.Form["description"];
                    string type_uid = Request.Form["type_uid"];
                    // Validate
                    if (title.Length < 1 || title.Length > 30)
                        ThrowError("Title must be 1 to 30 characters in length!");
                    else if (description.Length > 4000)
                        ThrowError("Description cannot exceed 4000 characters in length!");
                    else if (!IsNumeric(type_uid) || Connector.Query_Count("SELECT COUNT('') FROM item_types WHERE uid='" + Utils.Escape(type_uid) + "'") != 1)
                        ThrowError("Type UID does not exist!");
                    else
                    {
                        // Update
                        Connector.Query_Execute("UPDATE virtual_items SET title='" + Utils.Escape(title) + "', description='" + Utils.Escape(description) + "', type_uid='" + Utils.Escape(type_uid) + "' WHERE vitemid='" + Utils.Escape(data[0]["vitemid"]) + "';");
                        Response.Redirect(ResolveUrl("/item/" + data[0]["vitemid"]));
                    }
                }
                // Build list of types for the select element
                string types = "";
                foreach (ResultRow type in Connector.Query_Read("SELECT title, uid FROM item_types WHERE system='0' ORDER BY title ASC"))
                    types += "<option value=\"" + HttpUtility.HtmlEncode(type["uid"]) + "\"" + ((Request.Form["type_uid"] != null && type["uid"].Equals(Request.Form["type_uid"]) || (type["uid"].Equals(data[0]["type_uid"]))) ? " selected=\"selected\"" : "") + ">" + HttpUtility.HtmlEncode(type["title"]) +"</option>";
                // Output content
                content += UberMedia.Core.Cache_HtmlTemplates["item_modify"]
                    .Replace("%VITEMID%", data[0]["vitemid"])
                    .Replace("%TITLE%", HttpUtility.HtmlEncode(Request.Form["title"] ?? data[0]["title"]))
                    .Replace("%TYPES%", types)
                    .Replace("%DESCRIPTION%", HttpUtility.HtmlEncode(Request.Form["description"] ?? data[0]["description"]));
                break;
            case "rebuild":
                if (Request.Form["confirm"] != null)
                {
                    UberMedia.ThumbnailGeneratorService.AddToQueue(data[0]["path"], AppDomain.CurrentDomain.BaseDirectory + "/Content/Thumbnails/" + data[0]["vitemid"] + ".png");
                    Response.Redirect(ResolveUrl("/item/" + vitemid));
                }
                else
                    content += UberMedia.Core.Cache_HtmlTemplates["confirm"]
                        .Replace("%ACTION_TITLE%", "Confirm Thumbnail Rebuild")
                        .Replace("%ACTION_URL%", "<!--URL-->/item/" + data[0]["vitemid"] + "/rebuild")
                        .Replace("%ACTION_DESC%", "Are you sure you want to rebuild the thumbnail of this item?")
                        .Replace("%ACTION_BACK%", "<!--URL-->/item/" + data[0]["vitemid"]);
                break;
            case "delete":
                if (Request.Form["confirm"] != null)
                {
                    try
                    {
                        try
                        {
                            // Delete thumbnail
                            if (File.Exists(Server.MapPath("/Content/Thumbnails/" + data[0]["vitemid"] + ".png")))
                                File.Delete(Server.MapPath("/Content/Thumbnails/" + data[0]["vitemid"] + ".png"));
                        }
                        catch(Exception ex)
                        { throw new Exception("Could not delete thumbnail: " + ex.Message); }
                        // Delete file
                        File.Delete(data[0]["path"]);
                        // Delete from virtual file-system aand buffer
                        Connector.Query_Execute("DELETE FROM virtual_items WHERE vitemid='" + Utils.Escape(data[0]["vitemid"]) + "'; DELETE FROM terminal_buffer WHERE command='media' AND arguments='" + Utils.Escape(data[0]["vitemid"]) + "';");
                        Response.Redirect(ResolveUrl("/browse"));
                    }
                    catch(Exception ex)
                    {
                        content += "<h2>Deletion Error</h2><p>Could not delete file, the following error occurred:</p><p>" + HttpUtility.HtmlEncode(ex.Message) + "</p>";
                    }
                }
                else
                    content += UberMedia.Core.Cache_HtmlTemplates["confirm"]
                        .Replace("%ACTION_TITLE%", "Confirm Deletion")
                        .Replace("%ACTION_URL%", "<!--URL-->/item/" + data[0]["vitemid"] + "/delete")
                        .Replace("%ACTION_DESC%", "Are you sure you want to delete this item?")
                        .Replace("%ACTION_BACK%", "<!--URL-->/item/" + data[0]["vitemid"]);
                break;
            case "reset_views":
                if(Request.Form["confirm"] != null)
                {
                    Connector.Query_Execute("UPDATE virtual_items SET views='0' WHERE vitemid='" + Utils.Escape(data[0]["vitemid"]) + "'");
                    Response.Redirect(ResolveUrl("/item/" + data[0]["vitemid"]));
                }
                else
                    content += UberMedia.Core.Cache_HtmlTemplates["confirm"]
                        .Replace("%ACTION_TITLE%", "Confirm Reset Views")
                        .Replace("%ACTION_URL%", "<!--URL-->/item/" + data[0]["vitemid"] + "/reset_views")
                        .Replace("%ACTION_DESC%", "Are you sure you want to reset the views of this item?")
                        .Replace("%ACTION_BACK%", "<!--URL-->/item/" + data[0]["vitemid"]);
                break;
            case "reset_ratings":
                if (Request.Form["confirm"] != null)
                {
                    Connector.Query_Execute("DELETE FROM vi_ratings WHERE vitemid='" + Utils.Escape(data[0]["vitemid"]) + "'; UPDATE virtual_items SET cache_rating='0' WHERE vitemid='" + Utils.Escape(data[0]["vitemid"]) + "';");
                    Response.Redirect(ResolveUrl("/item/" + data[0]["vitemid"]));
                }
                else
                    content += UberMedia.Core.Cache_HtmlTemplates["confirm"]
                        .Replace("%ACTION_TITLE%", "Confirm Reset Ratings")
                        .Replace("%ACTION_URL%", "<!--URL-->/item/" + data[0]["vitemid"] + "/reset_ratings")
                        .Replace("%ACTION_DESC%", "Are you sure you want to reset the ratings of this item?")
                        .Replace("%ACTION_BACK%", "<!--URL-->/item/" + data[0]["vitemid"]);
                break;
            case "play_now":
                if(Session["mediacomputer"] != null) terminalBufferEntry("media", (string)Session["mediacomputer"], data[0]["vitemid"], false, true, Connector);
                Response.Redirect(ResolveUrl("/item/" + data[0]["vitemid"]));
                break;
            case "add_to_queue":
                if(Session["mediacomputer"] != null) terminalBufferEntry("media", (string)Session["mediacomputer"], data[0]["vitemid"], true, false, Connector);
                Response.Redirect(ResolveUrl("/item/" + data[0]["vitemid"]));
                break;
            case "remove_tag":
                string t = Request.QueryString["t"];
                if (t != null && IsNumeric(t))
                    Connector.Query_Execute("DELETE FROM tag_items WHERE tagid='" + Utils.Escape(t) + "' AND vitemid='" + Utils.Escape(data[0]["vitemid"]) + "';");
                Response.Redirect(ResolveUrl("/item/" + data[0]["vitemid"]));
                break;
            default:
                // Check if the user has tried to attach a tag
                if (Request.Form["tag"] != null)
                {
                    string tag = Request.Form["tag"];
                    if (tag.Length > 0 && IsNumeric(tag) && Connector.Query_Count("SELECT COUNT('') FROM tags AS t WHERE NOT EXISTS (SELECT tagid FROM tag_items WHERE tagid=t.tagid AND vitemid='" + Utils.Escape(data[0]["vitemid"]) + "') AND t.tagid='" + Utils.Escape(Request.Form["tag"]) + "'") == 1)
                        Connector.Query_Execute("INSERT INTO tag_items (tagid, vitemid) VALUES('" + Utils.Escape(Request.Form["tag"]) + "', '" + Utils.Escape(data[0]["vitemid"]) + "')");
                }
                // Build page content
                content += UberMedia.Core.Cache_HtmlTemplates["item_page"]
                    .Replace("%THUMBNAIL%", data[0]["thumbnail"].Equals("1") ? ResolveUrl("/Content/Thumbnails/" + data[0]["vitemid"] + ".png") : ResolveUrl("/Content/Images/thumbnail.png"))
                    .Replace("%TITLE%", HttpUtility.HtmlEncode(data[0]["title"]))
                    .Replace("%DESCRIPTION%", data[0]["description"].Length > 0 ? HttpUtility.HtmlEncode(data[0]["description"]) : "(no description)")
                    .Replace("%VIEWS%", data[0]["views"]).Replace("%DATE_ADDED%", data[0]["date_added"])
                    .Replace("%RATING%", data[0]["cache_rating"]).Replace("%TYPE%", data[0]["type"])
                    .Replace("%VITEMID%", data[0]["vitemid"]);
                // Add tags
                string cTags = "";
                string optionsTags = "";
                // Buikd the current tags assigned to the item
                Result tags = Connector.Query_Read("SELECT t.title, t.tagid FROM tags AS t, tag_items AS ti WHERE ti.vitemid='" + Utils.Escape(vitemid) + "' AND t.tagid=ti.tagid");
                if (tags.Rows.Count == 0) cTags = "None.";
                else
                    foreach (ResultRow tag in tags)
                        cTags += UberMedia.Core.Cache_HtmlTemplates["item_tag"]
                            .Replace("%TAGID%", tag["tagid"])
                            .Replace("%VITEMID%", data[0]["vitemid"])
                            .Replace("%TITLE%", HttpUtility.HtmlEncode(tag["title"]));
                // Build a list of all the tags
                foreach (ResultRow tag in Connector.Query_Read("SELECT * FROM tags ORDER BY title ASC"))
                    optionsTags += "<option value=\"" + HttpUtility.HtmlEncode(tag["tagid"]) + "\">" + HttpUtility.HtmlEncode(tag["title"]) + "</option>";
                content = content.Replace("%TAGS%", cTags).Replace("%OPTIONS_TAGS%", optionsTags);
                break;
        }
        SelectNavItem("BROWSE");
        // Build content area
        PageElements["CONTENT_RIGHT"] = content;
        // Build options area
        PageElements["CONTENT_LEFT"] = UberMedia.Core.Cache_HtmlTemplates["item_sidebar"].Replace("%URL%", ResolveUrl("")).Replace("%VITEMID%", vitemid);
    }
    public void Page__search()
    {
        string query = Request.QueryString["q"] != null ? Utils.Escape(Request.QueryString["q"].Replace("%", "")) : "";
        Response.Write("<h2>Search</h2>");
        if (query.Length == 0)
            Response.Write("Enter your search query on the top-right!");
        else
        {
            Response.Write("Results for '" + query + "':<div class=\"clear\"></div>");
            Result results = Connector.Query_Read("SELECT vi.*, it.thumbnail FROM virtual_items AS vi LEFT OUTER JOIN item_types AS it ON it.uid=vi.type_uid WHERE vi.title LIKE '%" + query + "%' ORDER BY FIELD(vi.type_uid, '100') DESC, vi.title ASC LIMIT 100");
            if (results.Rows.Count == 0) Response.Write("No items were found matching your criteria...");
            else
                foreach (ResultRow file in results) Response.Write(UberMedia.Core.Cache_HtmlTemplates["browse_file"].Replace("%TITLE%", file["title"]).Replace("%TITLE_S%", Title_Split(file["title"], 15)).Replace("%URL%", file["type_uid"].Equals("100") ? ResolveUrl("/browse/" + file["pfolderid"] + "/" + file["vitemid"]) : ResolveUrl("/item/" + file["vitemid"])).Replace("%THUMBNAIL%", file["type_uid"].Equals("100") ? ResolveUrl("/Content/Images/thumbnail_folder.png") : file["thumbnail"].Equals("1") ? ResolveUrl("/Content/Thumbnails/" + file["vitemid"] + ".png") : ResolveUrl("/Content/Images/thumbnail.png")));
        }
        Response.End();
    }
    // Authentication system
    public void Page__admin()
    {
        string subpg = Request.QueryString["1"] != null ? Request.QueryString["1"] : "home";
        string content = "";

        switch (subpg)
        {
            case "home":
                // Info
                content += "<h2>Info</h2>From here you can control your media library by restricting access, configuring settings and running tasks; below you can view the mechanical status of Uber Media.";
                // Status
                content += "<h2>Status - Indexing Service</h2>";
                if (UberMedia.Indexer.ThreadPool.Count > 0)
                {
                    Result drives = Connector.Query_Read("SELECT pfolderid, title FROM physical_folders ORDER BY pfolderid ASC");
                    int i = 0;
                    foreach (KeyValuePair<string, System.Threading.Thread> thread in UberMedia.Indexer.ThreadPool)
                    {
                        i++;
                        content += "<div>Thread " + i.ToString() + " [Folder " + thread.Key + " - " + DriveTitle(drives, thread.Key) + "]: " + thread.Value.ThreadState.ToString() + " - " + UberMedia.Indexer.GetStatus(thread.Key) + "</div>";
                    }
                }
                else content += "No threads are actively indexing any folders.";
                content += "<h2>Status - Thumbnail Service</h2>";
                if (UberMedia.ThumbnailGeneratorService.Threads.Count > 0)
                {
                    int i = 0;
                    foreach (System.Threading.Thread th in UberMedia.ThumbnailGeneratorService.Threads)
                    {
                        i++;
                        content += "<div>Thread " + i.ToString() + " - " + th.ThreadState.ToString() + "</div>";
                    }
                }
                else content += "No threads have been pooled by the thumbnail service.";
                content += "<br /><br />Items queued for processing: " + UberMedia.ThumbnailGeneratorService.Queue.Count.ToString() + "<br />Next item: " + (UberMedia.ThumbnailGeneratorService.Queue.Count > 0 ? UberMedia.ThumbnailGeneratorService.Queue[0][0] : "none.") + "<br />Delegator status: " + UberMedia.ThumbnailGeneratorService.Status;
                content += "<h2>Status - External Requests - Last 5</h2>";
                Result ext_req = Connector.Query_Read("SELECT * FROM external_requests ORDER BY datetime DESC LIMIT 5");
                if (ext_req.Rows.Count != 0)
                    foreach (ResultRow req in ext_req) content += req["reason"] + " - " + req["url"];
                else content += "No external requests have been made.";
                break;
            case "rebuild_all":
                if (Request.Form["confirm"] != null)
                {
                    CheckNotDoublePost();
                    // Shutdown indexers
                    UberMedia.Indexer.TerminateThreadPool();
                    // Shutdown thumbnail service
                    UberMedia.ThumbnailGeneratorService.Delegator_Stop();
                    // Clear queue
                    UberMedia.ThumbnailGeneratorService.Queue.Clear();
                    // Delete database data
                    UberMedia.Core.GlobalConnector.Query_Execute("DELETE FROM virtual_items; ALTER TABLE virtual_items AUTO_INCREMENT = 0;");
                    // Delete thumbnails
                    if (System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Content/Thumbnails")) System.IO.Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + "/Content/Thumbnails", true);
                    // Recreate thumbnails folder
                    System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/Content/Thumbnails");
                    // Start thumbnail service
                    UberMedia.ThumbnailGeneratorService.Delegator_Start();
                    // Start indexers
                    RunIndexers();
                    Response.Redirect(ResolveUrl("/admin"));
                }
                else content = UberMedia.Core.Cache_HtmlTemplates["confirm"]
                    .Replace("%ACTION_TITLE%", "Clear &amp; Rebuild Library")
                    .Replace("%ACTION_DESC%", "All data, tagging, virtual-changes and other related items will be wiped and reindexed.")
                    .Replace("%ACTION_URL%", "<!--URL-->/admin/rebuild_all")
                    .Replace("%ACTION_BACK%", "<!--URL-->/admin");
                break;
            case "rebuild_thumbnails":
                if (Request.Form["confirm"] != null)
                {
                    CheckNotDoublePost();
                    // Shutdown indexing
                    UberMedia.Indexer.TerminateThreadPool();
                    // Shutdown thumbnail service
                    UberMedia.ThumbnailGeneratorService.Delegator_Stop();
                    // Wipe queue
                    lock (UberMedia.ThumbnailGeneratorService.Queue)
                        UberMedia.ThumbnailGeneratorService.Queue.Clear();
                    // Wipe thumbnails folder
                    if (System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Content/Thumbnails")) System.IO.Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + "/Content/Thumbnails", true);
                    // Recreate thumbnails folder
                    System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/Content/Thumbnails");
                    // Start thumbnail service
                    UberMedia.ThumbnailGeneratorService.Delegator_Start();
                    // Build map of physical folder id's to paths for requeueing
                    Dictionary<string, string> DrivePaths = new Dictionary<string, string>();
                    foreach (ResultRow drive in Connector.Query_Read("SELECT pfolderid, physicalpath FROM physical_folders ORDER BY pfolderid ASC"))
                        DrivePaths.Add(drive["pfolderid"], drive["physicalpath"]);
                    // Queue items
                    foreach (ResultRow item in Connector.Query_Read("SELECT vitemid, pfolderid, phy_path FROM virtual_items WHERE type_uid != '100' ORDER BY vitemid ASC"))
                        UberMedia.ThumbnailGeneratorService.AddToQueue(DrivePaths[item["pfolderid"]] + item["phy_path"], AppDomain.CurrentDomain.BaseDirectory + "/Content/Thumbnails/" + item["vitemid"] + ".png");
                    // Redirect user
                    Response.Redirect("/admin");
                }
                else content = UberMedia.Core.Cache_HtmlTemplates["confirm"]
                    .Replace("%ACTION_TITLE%", "Rebuild All Thumbnails")
                    .Replace("%ACTION_DESC%", "All thumbnails will be deleted and rescheduled for processing.")
                    .Replace("%ACTION_URL%", "<!--URL-->/admin/rebuild_thumbnails")
                    .Replace("%ACTION_BACK%", "<!--URL-->/admin");
                break;
            case "run_indexer":
                if (Request.Form["confirm"] != null)
                {
                    CheckNotDoublePost();
                    RunIndexers();
                    Response.Redirect(ResolveUrl("/admin"));
                }
                else content = UberMedia.Core.Cache_HtmlTemplates["confirm"]
                    .Replace("%ACTION_TITLE%", "Run Indexer")
                    .Replace("%ACTION_DESC%", "This will run the indexers manually to find new content and delete missing media; drives already being indexed (right now) will be unaffected.")
                    .Replace("%ACTION_URL%", "<!--URL-->/admin/run_indexer")
                    .Replace("%ACTION_BACK%", "<!--URL-->/admin");
                break;
            case "stop_indexer":
                if (Request.Form["confirm"] != null)
                {
                    CheckNotDoublePost();
                    UberMedia.Indexer.TerminateThreadPool();
                    Response.Redirect(ResolveUrl("/admin"));
                }
                else content = UberMedia.Core.Cache_HtmlTemplates["confirm"]
                    .Replace("%ACTION_TITLE%", "Stop Indexer")
                    .Replace("%ACTION_DESC%", "This will force the indexer offline (if any threads of the indexer are executing).")
                    .Replace("%ACTION_URL%", "<!--URL-->/admin/stop_indexer")
                    .Replace("%ACTION_BACK%", "<!--URL-->/admin");
                break;
            case "start_thumbnails":
                if (Request.Form["confirm"] != null)
                {
                    CheckNotDoublePost();
                    UberMedia.ThumbnailGeneratorService.Delegator_Start();
                    Response.Redirect(ResolveUrl("/admin"));
                }
                else content = UberMedia.Core.Cache_HtmlTemplates["confirm"]
                    .Replace("%ACTION_TITLE%", "Start Thumbnail Service")
                    .Replace("%ACTION_DESC%", "This will enable the thumbnail service to continue processing items in the thumbnail queue.")
                    .Replace("%ACTION_URL%", "<!--URL-->/admin/start_thumbnails")
                    .Replace("%ACTION_BACK%", "<!--URL-->/admin");
                break;
            case "stop_thumbnails":
                if (Request.Form["confirm"] != null)
                {
                    CheckNotDoublePost();
                    UberMedia.ThumbnailGeneratorService.Delegator_Stop();
                    Response.Redirect(ResolveUrl("/admin"));
                }
                else content = UberMedia.Core.Cache_HtmlTemplates["confirm"]
                    .Replace("%ACTION_TITLE%", "Stop Thumbnail Service")
                    .Replace("%ACTION_DESC%", "This will disable the thumbnail service from processing items in the thumbnail queue.")
                    .Replace("%ACTION_URL%", "<!--URL-->/admin/stop_thumbnails")
                    .Replace("%ACTION_BACK%", "<!--URL-->/admin");
                break;
            case "folders":
                // Check if the user has requested to add a new folder
                if (Request.Form["title"] != null && Request.Form["path"] != null)
                {
                    string title = Request.Form["title"];
                    string path = Request.Form["path"];
                    bool synopsis = Request.Form["synopsis"] != null;
                    // Validate
                    if (title.Length < PHYSICAL_FOLDER_TITLE_MIN || title.Length > PHYSICAL_FOLDER_TITLE_MAX)
                        ThrowError("Title must be " + PHYSICAL_FOLDER_TITLE_MIN + " to " + PHYSICAL_FOLDER_TITLE_MAX + " characters in length!");
                    else if (path.Length < PHYSICAL_FOLDER_PATH_MIN || path.Length > PHYSICAL_FOLDER_PATH_MAX)
                        ThrowError("Invalid path!");
                    else if (!Directory.Exists(path))
                        ThrowError("Path '" + path + "' does not exist or it is currently inaccessible!");
                    else
                    {
                        // Add folder to db
                        string pfolderid = Connector.Query_Scalar("INSERT INTO physical_folders (title, physicalpath, allow_web_synopsis) VALUES('" + Utils.Escape(title) + "', '" + Utils.Escape(path) +"', '" + (synopsis ? "1" : "0") + "'); SELECT LAST_INSERT_ID();").ToString();
                        Response.Redirect(ResolveUrl("/admin/folder/" + pfolderid));
                    }
                }
                // Build list of folders
                string currentFolders = "";
                foreach (ResultRow folder in Connector.Query_Read("SELECT * FROM physical_folders ORDER BY title ASC"))
                    currentFolders += UberMedia.Core.Cache_HtmlTemplates["admin_folders_item"]
                        .Replace("%PFOLDERID%", folder["pfolderid"])
                        .Replace("%TITLE%", HttpUtility.HtmlEncode(folder["title"]))
                        .Replace("%PATH%", HttpUtility.HtmlEncode(folder["physicalpath"]))
                        .Replace("%SYNOPSIS%", folder["allow_web_synopsis"]);
                // Build page content
                content = UberMedia.Core.Cache_HtmlTemplates["admin_folders"]
                    .Replace("%CURRENT_FOLDERS%", currentFolders)
                    .Replace("%TITLE%", HttpUtility.HtmlEncode(Request.Form["title"]) ?? "")             // Add new folder form
                    .Replace("%PATH%", HttpUtility.HtmlEncode(Request.Form["path"]) ?? "")
                    .Replace("%SYNOPSIS%", Request.Form["synopsis"] != null ? " checked" : "");
                break;
            case "folder":
                // Grab the folder identifier and validate it
                string folderid = Request.QueryString["2"];
                if (folderid == null || !IsNumeric(folderid))
                {
                    Page__404();
                    return;
                }
                // Grab the folders info
                Result data = Connector.Query_Read("SELECT (SELECT COUNT('') FROM virtual_items WHERE pfolderid=p.pfolderid AND type_uid='100') AS total_folders, IFNULL(SUM(vi.views), 0) AS total_views, COUNT(vi.vitemid) AS total_items, p.* FROM physical_folders AS p LEFT OUTER JOIN virtual_items AS vi ON vi.pfolderid=p.pfolderid WHERE vi.type_uid != '100' AND  p.pfolderid='" + Utils.Escape(folderid) + "'");
                // Redirect if no data is returned
                if (data.Rows.Count != 1)
                {
                    Page__404();
                    return;
                }
                // Build the content
                switch (Request.QueryString["3"])
                {
                    case "index":
                        UberMedia.Indexer.IndexDrive(data[0]["pfolderid"], data[0]["physicalpath"], data[0]["allow_web_synopsis"].Equals("1"));
                        Response.Redirect(ResolveUrl("/admin/folder/" + data[0]["pfolderid"]));
                        break;
                    case "remove":
                        if (Request.Form["confirm"] != null)
                        {
                            // Shutdown the indexer (if it exists)
                            UberMedia.Indexer.TerminateIndexer(data[0]["pfolderid"]);
                            // Remove thumbnails
                            foreach(ResultRow item in Connector.Query_Read("SELECT vitemid FROM virtual_items WHERE pfolderid='" + Utils.Escape(data[0]["pfolderid"]) + "'"))
                                try
                                {
                                    if (File.Exists(Server.MapPath("/Content/Thumbnails/" + item["vitemid"] + ".png")))
                                        File.Delete(Server.MapPath("/Content/Thumbnails/" + item["vitemid"] + ".png"));
                                }
                                catch { }
                            // Remove items and the folder its self
                            Connector.Query_Execute("DELETE FROM virtual_items WHERE pfolderid='" + Utils.Escape(data[0]["pfolderid"]) + "'; DELETE FROM physical_folders WHERE pfolderid='" + Utils.Escape(data[0]["pfolderid"]) + "';");
                            Response.Redirect(ResolveUrl("/admin/folders"));
                        }
                        else
                            content = UberMedia.Core.Cache_HtmlTemplates["confirm"]
                                .Replace("%ACTION_TITLE%", "Remove Folder")
                                .Replace("%ACTION_DESC%", "Are you sure you want to remove this folder? Note: this will not delete the physical folder!")
                                .Replace("%ACTION_URL%", "<!--URL-->/admin/folder/" + data[0]["pfolderid"] + "/remove")
                                .Replace("%ACTION_BACK%", "<!--URL-->/admin/folder/" + data[0]["pfolderid"]);
                        break;
                    case "add_type":
                        string type = Request.Form["type"];
                        if (type != null && IsNumeric(type) && Connector.Query_Count("SELECT ((SELECT COUNT('') FROM item_types WHERE system='0' AND typeid='" + Utils.Escape(type) + "') + (SELECT COUNT('') FROM physical_folder_types WHERE typeid='" + Utils.Escape(type) + "' AND pfolderid='" + Utils.Escape(data[0]["pfolderid"]) + "'))") == 1)
                            Connector.Query_Execute("INSERT INTO physical_folder_types (pfolderid, typeid) VALUES('" + Utils.Escape(data[0]["pfolderid"]) + "', '" + Utils.Escape(type) + "');");
                        Response.Redirect(ResolveUrl("/admin/folder/" + data[0]["pfolderid"]));
                        break;
                    case "remove_type":
                        string t = Request.QueryString["t"];
                        if(t != null && IsNumeric(t))
                            Connector.Query_Execute("DELETE FROM physical_folder_types WHERE typeid='" + Utils.Escape(t) + "' AND pfolderid='" + Utils.Escape(data[0]["pfolderid"]) + "';");
                        Response.Redirect(ResolveUrl("/admin/folder/" + data[0]["pfolderid"]));
                        break;
                    default:
                        // Check if the user has tried to modify the folder title or/and path
                        if (Request.Form["title"] != null && Request.Form["path"] != null)
                        {
                            string title = Request.Form["title"];
                            string path = Request.Form["path"];
                            bool web_synopsis = Request.Form["synopsis"] != null;
                            if (title.Length < PHYSICAL_FOLDER_TITLE_MIN || title.Length > PHYSICAL_FOLDER_TITLE_MAX)
                                ThrowError("Title must be " + PHYSICAL_FOLDER_TITLE_MIN + " to " + PHYSICAL_FOLDER_TITLE_MAX + " characters in length!");
                            else if (path.Length < PHYSICAL_FOLDER_PATH_MIN || path.Length > PHYSICAL_FOLDER_PATH_MAX)
                                ThrowError("Invalid path!");
                            else if (!Directory.Exists(path))
                                ThrowError("Path does not exist!");
                            else
                            {
                                Connector.Query_Execute("UPDATE physical_folders SET physicalpath='" + Utils.Escape(path) + "', title='" + Utils.Escape(title) + "', allow_web_synopsis='" + (web_synopsis ? "1" : "0") + "' WHERE pfolderid='" + Utils.Escape(data[0]["pfolderid"]) + "'");
                                Response.Redirect(ResolveUrl("/admin/folder/" + data[0]["pfolderid"]));
                            }
                        }
                        // Build list of item types available to add
                        string typesAdd = "";
                        foreach (ResultRow it in Connector.Query_Read("SELECT typeid, title FROM item_types WHERE system='0' ORDER BY title ASC"))
                            typesAdd += "<option value=\"" + Utils.Escape(it["typeid"]) + "\">" + HttpUtility.HtmlEncode(it["title"]) + "</option>";
                        // Build list of pre-existing types assigned to the folder
                        string typesRemove = "";
                        foreach (ResultRow it in Connector.Query_Read("SELECT pft.typeid, it.title FROM physical_folder_types AS pft LEFT OUTER JOIN item_types AS it ON it.typeid=pft.typeid WHERE pft.pfolderid='" + Utils.Escape(data[0]["pfolderid"]) + "' ORDER BY it.title ASC;"))
                            typesRemove += UberMedia.Core.Cache_HtmlTemplates["admin_folder_type"]
                                .Replace("%TYPEID%", it["typeid"])
                                .Replace("%TITLE%", HttpUtility.HtmlEncode(it["title"]));
                        if (typesRemove.Length == 0) typesRemove = "None.";
                        // Build content
                        content = UberMedia.Core.Cache_HtmlTemplates["admin_folder"]
                            .Replace("%TITLE%", HttpUtility.HtmlEncode(data[0]["title"]))                               // Modify
                            .Replace("%PATH%", HttpUtility.HtmlEncode(data[0]["physicalpath"]))
                            .Replace("%SYNOPSIS%", data[0]["allow_web_synopsis"].Equals("1") ? "checked" : "")
                            .Replace("%TOTAL_ITEMS%", data[0]["total_items"])                                           // Stats
                            .Replace("%TOTAL_FOLDERS%", data[0]["total_folders"])
                            .Replace("%TOTAL_VIEWS%", data[0]["total_views"])
                            .Replace("%TYPES%", typesAdd)                                                               // Types
                            .Replace("%TYPES_REMOVE%", typesRemove)
                            .Replace("%PFOLDERID%", data[0]["pfolderid"]);                                              // Misc
                        break;
                }
                break;
            case "requests":
                // Check if the user has requested to delete all of the external request log entries
                if (Request.QueryString["clear"] != null)
                {
                    Connector.Query_Execute("DELETE FROM external_requests;");
                    Response.Redirect(ResolveUrl("/admin/requests"));
                }
                // Build content
                int page = Request.QueryString["p"] != null && IsNumeric(Request.QueryString["p"]) ? int.Parse(Request.QueryString["p"]) : 1;
                if (page < 1) page = 1;

                string reqs = "";
                foreach (ResultRow req in Connector.Query_Read("SELECT * FROM external_requests ORDER BY datetime DESC LIMIT " + ((PAGE_REQUESTS_ITEMSPERPAGE * page) - PAGE_REQUESTS_ITEMSPERPAGE) + ", " + PAGE_REQUESTS_ITEMSPERPAGE))
                    reqs += UberMedia.Core.Cache_HtmlTemplates["admin_requests_item"]
                        .Replace("%REASON%", HttpUtility.HtmlEncode(req["reason"]))
                        .Replace("%DATETIME%", req["datetime"])
                        .Replace("%URL%", HttpUtility.HtmlEncode(req["url"]));
                content = UberMedia.Core.Cache_HtmlTemplates["admin_requests"]
                    .Replace("%ITEMS%", reqs.Length > 0 ? reqs : "None.")
                    .Replace("%P_N%", (page < int.MaxValue ? page + 1 : page).ToString())
                    .Replace("%P_P%", (page > 1 ? page - 1 : page).ToString())
                    .Replace("%PAGE%", page.ToString());
                break;
            case "settings":
                // Check if the user has requested to update a setting
				if(Request.Form.Count != 0)
				{
                    string queryUpdate = "";
                    foreach (string key in Request.Form.AllKeys)
                        if (key.StartsWith("setting_"))
                            queryUpdate += "UPDATE settings SET value='" + Utils.Escape(Request.Form[key]) + "' WHERE keyid='" + Utils.Escape(key.Remove(0, 8)) + "'; ";
                    if (queryUpdate.Length > 0)
                        Connector.Query_Execute(queryUpdate);
				}
                // Build content
                content += UberMedia.Core.Cache_HtmlTemplates["admin_settings_header"];
                string lastcat = "";
                foreach (ResultRow setting in Connector.Query_Read("SELECT * FROM settings ORDER BY category ASC, keyid ASC"))
                {
                    if (!lastcat.Equals(setting["category"]))
                    {
                        lastcat = setting["category"];
                        content += UberMedia.Core.Cache_HtmlTemplates["admin_settings_category"]
                            .Replace("%CATEGORY%", HttpUtility.HtmlEncode(setting["category"]));
                    }
                    content += UberMedia.Core.Cache_HtmlTemplates["admin_settings_item"]
                        .Replace("%KEYID%", HttpUtility.HtmlEncode(setting["keyid"]))
                        .Replace("%VALUE%", HttpUtility.HtmlEncode(setting["value"]));
                }
                content += UberMedia.Core.Cache_HtmlTemplates["admin_settings_footer"];
                break;
            case "terminals":
                switch (Request.QueryString["2"])
                {
                    case null:
                        // Check if the user has requested to add a terminal/key
                        if (Request.Form["key"] != null && Request.Form["title"] != null)
                        {
                            string key = Request.Form["key"];
                            string title = Request.Form["title"];
                            if (key.Length < TERMINAL_KEY_MIN || key.Length > TERMINAL_KEY_MAX)
                                ThrowError("Key must be " + TERMINAL_KEY_MIN + " to " + TERMINAL_KEY_MAX + " characters in length!");
                            else if (title.Length < TERMINAL_TITLE_MIN || title.Length > TERMINAL_TITLE_MAX)
                                ThrowError("Title must be " + TERMINAL_TITLE_MIN + " to " + TERMINAL_TITLE_MAX + " characters in length!");
                            else
                            {
                                bool error = false;
                                for (int i = 0; i < key.Length; i++)
                                {
                                    if (!(key[i] >= 'A' && key[i] <= 'Z') && !(key[i] >= 'a' && key[i] <= 'z') && !(key[i] >= '0' && key[i] <= '9'))
                                    {
                                        ThrowError("Invalid key - must be alpha-numeric (alphabet and number characters - no spaces etc!");
                                        error = true;
                                    }
                                }
                                if(!error)
                                    Connector.Query_Execute("INSERT INTO terminals (title, tkey) VALUES('" + Utils.Escape(title) + "', '" + Utils.Escape(key) + "');");
                            }
                        }
                        // Build list of terminals for removal and config generation
                        string terminals = "";
                        string genTerminals = "";
                        foreach (ResultRow terminal in Connector.Query_Read("SELECT * FROM terminals ORDER BY title ASC"))
                        {
                            terminals += UberMedia.Core.Cache_HtmlTemplates["admin_terminals_item"]
                                .Replace("%TERMINALID%", terminal["terminalid"])
                                .Replace("%TITLE%", HttpUtility.HtmlEncode(terminal["title"]))
                                .Replace("%KEY%", HttpUtility.HtmlEncode(terminal["tkey"]))
                                .Replace("%UPDATED%", terminal["status_updated"].Length > 0 ? terminal["status_updated"] : "(never)");
                            genTerminals = "<option value=\"" + HttpUtility.HtmlEncode(terminal["terminalid"]) + "\">" + HttpUtility.HtmlEncode(terminal["title"]) + "</option>";
                        }
                        // Build content
                        content = UberMedia.Core.Cache_HtmlTemplates["admin_terminals"]
                            .Replace("%TERMINALS%", terminals)
                            .Replace("%GEN_TERMINALS%", genTerminals)
                            .Replace("%KEY%", Request.Form["key"] ?? "")
                            .Replace("%TITLE%", Request.Form["title"] ?? "");
                        
                        break;
                    case "remove":
                        string t = Request.QueryString["t"];
                        if (t == null || !IsNumeric(t))
                        {
                            Page__404();
                            return;
                        }
                        // Grab the info of the key
                        Result tdata = Connector.Query_Read("SELECT terminalid, title, tkey FROM terminals WHERE terminalid='" + Utils.Escape(t) + "'");
                        // Check if the user has confirmed the deletion, else prompt them
                        if (Request.Form["confirm"] != null)
                        {
                            Connector.Query_Execute("DELETE FROM terminals WHERE terminalid='" + Utils.Escape(tdata[0]["terminalid"]) + "'");
                            Response.Redirect(ResolveUrl("/admin/terminals"));
                        }
                        else
                            content = UberMedia.Core.Cache_HtmlTemplates["confirm"]
                                .Replace("%ACTION_TITLE%", "Confirm Deletion of Terminal Key")
                                .Replace("%ACTION_DESC%", "Are you sure you want to delete the key '" + tdata[0]["title"] + "' (key: " + tdata[0]["tkey"] + ")?")
                                .Replace("%ACTION_URL%", "<!--URL-->/admin/terminals/remove?t=" + tdata[0]["terminalid"])
                                .Replace("%ACTION_BACK%", "<!--URL-->/admin/terminals");
                        break;
                    case "generate":
                        if (Request.QueryString["tid"] == null)
                        {
                            Page__404();
                            return;
                        }
                        // Grab the terminals information - this also ensures it exists
                        Result termdata = Connector.Query_Read("SELECT tkey FROM terminals WHERE terminalid='" + Utils.Escape(Request.QueryString["tid"]) + "'");
                        if (termdata.Rows.Count != 1)
                        {
                            Page__404();
                            return;
                        }
                        Response.ContentType = "text/xml";
                        StringWriter sw = new StringWriter();
                        XmlWriter writer = XmlWriter.Create(sw);
                        // Build configuration file
                        writer.WriteStartDocument();
                        writer.WriteStartElement("settings");

                        // -- Database settings
                        writer.WriteStartElement("database");

                        writer.WriteStartElement("type");
                        writer.WriteCData("mysql");
                        writer.WriteEndElement();

                        writer.WriteStartElement("host");
                        writer.WriteCData(UberMedia.Core.Cache_Settings["db_host"]);
                        writer.WriteEndElement();

                        writer.WriteStartElement("port");
                        writer.WriteCData(UberMedia.Core.Cache_Settings["db_port"]);
                        writer.WriteEndElement();

                        writer.WriteStartElement("user");
                        writer.WriteCData(UberMedia.Core.Cache_Settings["db_user"]);
                        writer.WriteEndElement();

                        writer.WriteStartElement("pass");
                        writer.WriteCData(UberMedia.Core.Cache_Settings["db_pass"]);
                        writer.WriteEndElement();

                        writer.WriteStartElement("db");
                        writer.WriteCData(UberMedia.Core.Cache_Settings["db_database"]);
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                        // -- Terminal settings
                        writer.WriteStartElement("terminal");

                        writer.WriteStartElement("key");
                        writer.WriteCData(termdata[0]["tkey"]);
                        writer.WriteEndElement();

                        writer.WriteEndElement();

                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                        // Output config
                        writer.Flush();
                        writer.Close();
                        sw.Flush();
                        Response.Write(sw.ToString());
                        // Disposal and terminate response
                        sw.Dispose();
                        Connector.Disconnect();
                        Response.End();
                        break;
                    default:
                        Page__404();
                        return;
                }
                
                break;
			case "tags":
                // Check what the user has requested
                switch (Request.QueryString["2"])
                {
                    case null:
                        // Check if the user has requested to add a tag
                        if (Request.Form["title"] != null)
                        {
                            string title = Request.Form["title"];
                            if (title.Length < TAG_TITLE_MIN || title.Length > TAG_TITLE_MAX)
                                ThrowError("Title must be " + TAG_TITLE_MIN + " to " + TAG_TITLE_MAX + " characters in length!");
                            else if (Connector.Query_Count("SELECT COUNT('') FROM tags WHERE title LIKE '%" + Utils.Escape(title) + "%'") != 0)
                                ThrowError("A tag with the same title already exists!");
                            else
                                Connector.Query_Execute("INSERT INTO tags (title) VALUES('" + Utils.Escape(title) + "');");
                        }
                        // Build tags
                        string tags = "";
                        foreach (ResultRow tag in Connector.Query_Read("SELECT * FROM tags ORDER BY title ASC"))
                            tags += UberMedia.Core.Cache_HtmlTemplates["admin_tags_item"]
                                .Replace("%TAGID%", tag["tagid"])
                                .Replace("%TITLE%", HttpUtility.HtmlEncode(tag["title"]));
                        // Build content
                        content = UberMedia.Core.Cache_HtmlTemplates["admin_tags"]
                            .Replace("%TAGS%", tags);
                        break;
                    case "remove":
                        string tagid = Request.QueryString["t"];
                        if (tagid != null && IsNumeric(tagid))
                        {
                            Result t = Connector.Query_Read("SELECT tagid, title FROM tags WHERE tagid='" + Utils.Escape(tagid) + "'");
                            if (t.Rows.Count != 1)
                            {
                                Page__404();
                                return;
                            }
                            if (Request.Form["confirm"] != null)
                            {
                                Connector.Query_Execute("DELETE FROM tags WHERE tagid='" + Utils.Escape(tagid) + "'");
                                Response.Redirect(ResolveUrl("/admin/tags"));
                            }
                            else
                                content = UberMedia.Core.Cache_HtmlTemplates["confirm"]
                                    .Replace("%ACTION_TITLE%", "Confirm Tag Deletion")
                                    .Replace("%ACTION_DESC%", "Are you sure you want to remvoe tag '" + t[0]["title"] + "'?")
                                    .Replace("%ACTION_URL%", "<!--URL-->/admin/tags/remove?t=" + t[0]["tagid"])
                                    .Replace("%ACTION_BACK%", "<!--URL-->/admin/tags");
                        }
                        break;
                    default:
                        Page__404();
                        return;
                }
				break;
            default:
                Page__404();
                return;
        }
        PageElements["CONTENT_LEFT"] = UberMedia.Core.Cache_HtmlTemplates["admin_sidebar"];
        PageElements["CONTENT_RIGHT"] = content;
        SelectNavItem("CONTROL");
    }
    public void Page__login()
    {
        PageElements["CONTENT_RIGHT"] = UberMedia.FilmInformation.FilmSynopsis("aliens", Connector);
    }
    public void Page__logout()
    {
    }
    public void Page__myaccount()
    {
    }
    public void Page__folder()
    {
    }
    public void Page__modify_item()
    {
    }
    public void Page__change()
    {
        // Validate the input
        if (Request.QueryString["c"] == null || !IsNumeric(Request.QueryString["c"]) || Connector.Query_Count("SELECT COUNT('') FROM terminals WHERE terminalid='" + Utils.Escape(Request.QueryString["c"]) + "'") != 1)
            Response.Write("Invalid media-computer!");
        else
            // Store the setting in a session variable
            Session["mediacomputer"] = Request.QueryString["c"];
        Response.End();
    }
    public void Page__control()
    {
        if (Request.QueryString["cmd"] != null & Request.QueryString["mc"] != null)
        {
            string mc = Request.QueryString["mc"]; // Media computer/terminalid
            switch (Request.QueryString["cmd"])
            {
                case "status":
                    // For retrieving the status of the terminal (general information)
                    Response.ContentType = "text/xml";
                    /*
                     * <d>
                     *      <p>secs</p>     Position in seconds
                     *      <d>dur</d>      Duration in seconds
                     *      <v>vol</v>      Volume from 0.0 to 1.0
                     *      <m>mute</m>     Indicates if muted - 1 or 0
                     *      <s>state</s>    State
                     *      <pv>val</pv>    Unique value to indicate changes (count of items:last cid)
                     * </d>
                     * 
                     * 
                     * 
                     */
                    Result data = Connector.Query_Read("SELECT t.*, CONCAT(CONCAT((SELECT COUNT('') FROM terminal_buffer WHERE terminalid=t.terminalid AND command='media' AND queue='1'), ':'),(SELECT cid FROM terminal_buffer WHERE terminalid=t.terminalid AND command='media' AND queue='1' ORDER BY cid DESC LIMIT 1)) AS val FROM terminals AS t WHERE t.terminalid='" + Utils.Escape(mc) + "' AND t.status_updated >= DATE_SUB(NOW(), INTERVAL 1 MINUTE);");
                    if (data.Rows.Count != 1) break;
                    ResultRow s = data[0];
                    Response.Write("<d>");
                    Response.Write("    <p>" + s["status_position"] + "</p>");
                    Response.Write("    <d>" + s["status_duration"] + "</d>");
                    Response.Write("    <v>" + s["status_volume"] + "</v>");
                    Response.Write("    <m>" + s["status_volume_muted"] + "</m>");
                    Response.Write("    <s>" + s["status_state"] + "</s>");
                    Response.Write("    <pv>" + s["status_vitemid"] + ":" + (s["val"].Length > 0 ? s["val"] : "0") + "</pv>");
                    Response.Write("</d>");
                    break;
                case "playlist":
                    // For retrieving the playlist of items (current item & upcoming items)
                    /*
                     * <d>
                     *      <c>                 Current item playing:
                     *          <v>vitemid</v>  Vitemid.
                     *          <t>title</t>    Title.
                     *      </c>
                     *      <u>                 List of upcoming items:
                     *          <i>
                     *              <c>cid</c>      cid - terminal buffer command identifier
                     *              <v>vitemid</v>  Vitemid
                     *              <t>title</t>    Title
                     *          </i>
                     *          ...                 Multiple items using i-tag
                     *      </u>
                     * </d>
                     */
                    Response.ContentType = "text/xml";
                    Result currItem = Connector.Query_Read("SELECT vi.vitemid, vi.title FROM (virtual_items AS vi, terminals AS t) WHERE vi.vitemid=t.status_vitemid AND t.terminalid='" + Utils.Escape(mc) + "' AND t.status_updated >= DATE_SUB(NOW(), INTERVAL 1 MINUTE)");
                    Result upcomingItems = Connector.Query_Read("SELECT tb.cid, vi.vitemid, vi.title FROM (virtual_items AS vi, terminal_buffer AS tb) WHERE vi.vitemid=tb.arguments AND tb.command='media' AND tb.queue='1' ORDER BY tb.cid ASC;");
                    Response.Write("<d>");
                    Response.Write("    <c>");
                    if (currItem.Rows.Count == 1)
                    {
                        Response.Write("        <v>" + currItem[0]["vitemid"] + "</v>");
                        Response.Write("        <t>" + HttpUtility.HtmlEncode(currItem[0]["title"]) + "</t>");
                    }
                    else
                    {
                        Response.Write("        <v></v>");
                        Response.Write("        <t></t>");
                    }
                    Response.Write("    </c>");
                    Response.Write("    <u>");
                    foreach (ResultRow item in upcomingItems)
                    {
                        Response.Write("        <i>");
                        Response.Write("            <c>" + item["cid"] + "</c>");
                        Response.Write("            <v>" + item["vitemid"] + "</v>");
                        Response.Write("            <t>" + HttpUtility.HtmlEncode(item["title"]) + "</t>");
                        Response.Write("        </i>");
                    }
                    Response.Write("    </u>");
                    Response.Write("</d>");
                    break;
                case "remove":
                    // Removes an item from the playlist - if cid is empty, use skip command
                    if (Request.QueryString["cid"] == null || Request.QueryString["cid"].Length == 0)
                        terminalBufferEntry("next", mc, "", false, true, Connector);
                    else
                        Connector.Query_Execute("DELETE FROM terminal_buffer WHERE command='media' AND cid='" + Utils.Escape(Request.QueryString["cid"]) + "'");
                    break;
                case "c_pi":
                    // Previous item
                    terminalBufferEntry("previous", mc, "", false, true, Connector);
                    break;
                case "c_sb":
                    // Skip backwards
                    terminalBufferEntry("skip_backward", mc, "", false, true, Connector);
                    break;
                case "c_s":
                    // Stop
                    terminalBufferEntry("stop", mc, "", false, true, Connector);
                    break;
                case "c_p":
                    // Toggles play
                    terminalBufferEntry("play_toggle", mc, "", false, true, Connector);
                    break;
                case "c_sf":
                    // Skip forward
                    terminalBufferEntry("skip_forward", mc, "", false, true, Connector);
                    break;
                case "c_ni":
                    // Next item
                    terminalBufferEntry("next", mc, "", false, true, Connector);
                    break;
                case "c_vol":
                    // Changes the value of the volume; check for v querystring
                    if (Request.QueryString["v"] == null) return;
                    double v2 = -1;
                    if (!double.TryParse(Request.QueryString["v"], out v2) || !(v2 >= 0 && v2 <= 1))
                        return;
                    terminalBufferEntry("volume", mc, Request.QueryString["v"], false, true, Connector);
                    break;
                case "c_mute":
                    // Toggles muting the volume
                    terminalBufferEntry("mute_toggle", mc, "", false, true, Connector);
                    break;
                case "c_pos":
                    // Changes the position of the current media
                    if (Request.QueryString["v"] == null) return;
                    double v = -1;
                    if (!double.TryParse(Request.QueryString["v"], out v) || v < 0)
                        return;
                    terminalBufferEntry("position", mc, Request.QueryString["v"], false, true, Connector);
                    break;
                case "remove_all":
                    Connector.Query_Execute("DELETE FROM terminal_buffer WHERE terminalid='" + Utils.Escape(mc) + "' AND command='media' AND queue='1';");
                    break;
            }
            if (Request.QueryString["manual"] != null)
                Response.Redirect(ResolveUrl("/control"));
            else
            {
                Connector.Disconnect();
                Connector = null;
                Response.End();
            }
        }
        else if (Session["mediacomputer"] == null)
        {
            // Display a message prompting the user to install a terminal
            PageElements["CONTENT_RIGHT"] = UberMedia.Core.Cache_HtmlTemplates["controls_error"];
        }
        else
        {
            // Create controls page
            PageElements["HEADER"] = "<link rel=\"Stylesheet\" type=\"text/css\" href=\"<!--URL-->/Content/CSS/Controls.css\" />";
            string right = UberMedia.Core.Cache_HtmlTemplates["controls_content"];
            string mediacomputer = (string)Session["mediacomputer"];
            PageElements["PLAYING"] = controls_PrintCurrentItem(mediacomputer);
            PageElements["UPCOMING"] = controls_PrintUpcoming(mediacomputer);
            PageElements["CONTENT_RIGHT"] = right;
            PageElements["CONTENT_LEFT"] = UberMedia.Core.Cache_HtmlTemplates["controls_left"];
            PageElements["ONLOAD"] = "controlsInit('" + HttpUtility.HtmlEncode(mediacomputer) + "')";
        }
        SelectNavItem("CONTROL");
    }
    string controls_PrintCurrentItem(string terminalid)
    {
        Result data = Connector.Query_Read("SELECT vi.vitemid, vi.title FROM virtual_items AS vi, terminals AS t WHERE vi.vitemid=t.status_vitemid AND t.terminalid='" + Utils.Escape(terminalid) + "' AND t.status_updated >= DATE_SUB(NOW(), INTERVAL 1 MINUTE)");
        return data.Rows.Count == 0 ? "None." :
            UberMedia.Core.Cache_HtmlTemplates["controls_item"]
                .Replace("%VITEMID%", data[0]["vitemid"])
                .Replace("%CID%", "")
                .Replace("%TITLE%", HttpUtility.HtmlEncode(data[0]["title"]));
            ;
    }
    string controls_PrintUpcoming(string terminalid)
    {
        string data = "";
        foreach (ResultRow item in Connector.Query_Read("SELECT tb.cid, vi.title, vi.vitemid FROM terminal_buffer AS tb, virtual_items AS vi WHERE tb.terminalid='" + Utils.Escape(terminalid) + "' AND tb.command='media' AND tb.queue='1' AND vi.vitemid=tb.arguments ORDER BY tb.cid ASC"))
            data += UberMedia.Core.Cache_HtmlTemplates["controls_item"]
                .Replace("%VITEMID%", item["vitemid"])
                .Replace("%CID%", item["cid"])
                .Replace("%TITLE%", HttpUtility.HtmlEncode(item["title"]));
        return data.Length == 0 ? "None." : data;
    }
    #endregion

    #region "Methods"
    enum NavItems
    {
        Home, Browse, Newest, Popular
    }
    void SelectNavItem(string name)
    {
        PageElements["NAVI_" + name] = "selected";
    }
    bool IsNumeric(string data)
    {
        try
        {
            int.Parse(data); return true;
        }
        catch { return false; }
    }
    /// <summary>
    /// Breaks a line up by ensuring its seperated every so often.
    /// </summary>
    /// <returns></returns>
    string Title_Split(string text, int chars)
    {
        string[] words = text.Split(' ');
        string n = "";
        foreach (string s in words)
            if (s.Length > chars) n += s.Substring(0, chars) + "\n" + s.Substring(chars) + " ";
            else n += s + " ";
        return n.Remove(n.Length - 1, 1);
    }
    /// <summary>
    /// Creates a navigation bar for an item based on its pfolderid and vitemid.
    /// </summary>
    /// <param name="pfolderid"></param>
    /// <param name="vitemid"></param>
    string Browse_CreateNavigationBar(string pfolderid, string vitemid_parent)
    {
        string html = "<a href=\"" + ResolveUrl("/browse") + "\">All</a>";
        if (pfolderid.Length == 0) return html;
        else
        {
            html += " &gt; <a href=\"" + ResolveUrl("/browse/" + pfolderid) + "\">" + (Connector.Query_Scalar("SELECT title FROM physical_folders WHERE pfolderid='" + Utils.Escape(pfolderid) + "'") ?? "Failed to retrieve folder name.").ToString() + "</a>";
            if (vitemid_parent.Length == 0) return html;
        }
        int iterations = 0;
        string parent = vitemid_parent;
        Result t;
        string html2 = "";
        while (iterations < 20 || parent != "0")
        {
            t = Connector.Query_Read("SELECT parent, title, vitemid FROM virtual_items WHERE vitemid='" + Utils.Escape(parent) + "'");
            if (t.Rows.Count != 1) break;
            parent = t[0]["parent"];
            html2 = " &gt; <a href=\"" + ResolveUrl("/browse/" + pfolderid + "/" + t[0]["vitemid"]) + "\">" + t[0]["title"] + "</a>" + html2;
            iterations++;
        }
        return html + html2;
    }
    void RunIndexers()
    {
        foreach (ResultRow drive in Connector.Query_Read("SELECT pfolderid, physicalpath, allow_web_synopsis FROM physical_folders ORDER BY pfolderid ASC"))
            if (!UberMedia.Indexer.ThreadPool.ContainsKey(drive["pfolderid"])) UberMedia.Indexer.IndexDrive(drive["pfolderid"], drive["physicalpath"], drive["allow_web_synopsis"].Equals("1"));
    }
    string DriveTitle(Result drives, string pfolderid)
    {
        foreach (ResultRow r in drives) if (r["pfolderid"] == pfolderid) return r["title"];
        return "Unknown";
    }
    void CheckNotDoublePost()
    {
        if (Session["post_protect"] != null && DateTime.Now.Subtract((DateTime)Session["post_protect"]).TotalSeconds < 2)
            Response.Redirect(ResolveUrl("/admin"), true); // We specify true in-case of any overrides
        else if (Session["post_protect"] != null) Session["post_protect"] = DateTime.Now;
        else Session.Add("post_protect", DateTime.Now);
    }
    Random terminalBufferRandom = new Random(DateTime.Now.Millisecond);
    /// <summary>
    /// Enters a command into the terminal buffer to control a media computer/terminal.
    /// </summary>
    /// <param name="command">The command to be executed e.g. play.</param>
    /// <param name="terminalid">The terminalid identifier of the media computer.</param>
    /// <param name="arguments">Any required arguments.</param>
    /// <param name="queue">True = command is queued, false = command is executed immediately.</param>
    /// <param name="Connector"></param>
    bool terminalBufferEntry(string command, string terminalid, string arguments, bool queue, bool onlineProtection, Connector Connector)
    {
        // Ensure the mc/terminal is valid and responded at least a minute ago
        if (!onlineProtection || Connector.Query_Count("SELECT COUNT('') FROM terminals WHERE status_updated >= DATE_SUB(NOW(), INTERVAL 1 MINUTE) AND terminalid='" + Utils.Escape(terminalid) + "'") == 1)
        {
            Connector.Query_Execute("INSERT INTO terminal_buffer (cid, command, terminalid, arguments, queue) VALUES('" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + terminalBufferRandom.Next(0, Int32.MaxValue) + "', '" + Utils.Escape(command) + "', '" + Utils.Escape(terminalid) + "', '" + Utils.Escape(arguments) + "', '" + (queue ? "1" : "0") + "')");
            return true;
        }
        else return false;
    }
    /// <summary>
    /// Returns the query/statement used to insert the proposed terminal buffer entry; this does not provide any online protection etc!
    /// </summary>
    /// <param name="command"></param>
    /// <param name="terminalid"></param>
    /// <param name="arguments"></param>
    /// <param name="queue"></param>
    /// <param name="onlineProtection"></param>
    /// <returns></returns>
    string terminalBufferEntry(string command, string terminalid, string arguments, bool queue)
    {
        return "INSERT INTO terminal_buffer (cid, command, terminalid, arguments, queue) VALUES('" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + terminalBufferRandom.Next(0, Int32.MaxValue) + "', '" + Utils.Escape(command) + "', '" + Utils.Escape(terminalid) + "', '" + Utils.Escape(arguments) + "', '" + (queue ? "1" : "0") + "');";
    }
    /// <summary>
    /// Throws a simple error message by setting the display style to block (so the element is visible to the user) and a message.
    /// </summary>
    /// <param name="message">The message to be displayed to the user.</param>
    void ThrowError(string message)
    {
        PageElements["ERR_STYLE"] = "display: block !important; visibility: visible !important;";
        PageElements["ERR_MSG"] = HttpUtility.HtmlEncode(message);
    }
    string ReplaceElements(string text, int treeNumber, int treeMax)
    {
        if (treeNumber > treeMax) return text;
        foreach (KeyValuePair<string, string> elm in PageElements)
            text = text.Replace("<!--" + elm.Key + "-->", elm.Value.Contains("<!--") ? ReplaceElements(elm.Value, treeNumber + 1, treeMax) : elm.Value);
        return text;
    }
    #endregion
}
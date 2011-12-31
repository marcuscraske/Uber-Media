<%@ Application Language="C#" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e) 
    {
        UberMedia.Core.Core_Start();
    }
    
    void Application_End(object sender, EventArgs e) 
    {
        UberMedia.Core.Core_Stop();
    }
    void Application_Error(object sender, EventArgs e) 
    { 
        // Code that runs when an unhandled error occurs

    }
    void Application_BeginRequest(object sender, EventArgs e)
    {
        // This code rewrites the URL
        string path = System.Web.HttpContext.Current.Request.Path;
        if (path.ToLower().StartsWith("/content") || path.ToLower().StartsWith("/backend")) return;
        else if (path.ToLower().StartsWith("/installer"))
        {
            RewriteNewPath(Request.ApplicationPath + "/Install/Installer.aspx", Request.ApplicationPath + "/Install/Installer.aspx");
            System.Web.HttpContext.Current.RewritePath(Request.ApplicationPath + "/Install/Installer.aspx");
            return;
        }
        else if (path.ToLower().StartsWith("/api")) // To be implemented perhaps
        {
            RewriteNewPath(Request.ApplicationPath + "/Backend/API.aspx", Request.ApplicationPath + "/Backend/API.aspx?page=unknown");
            System.Web.HttpContext.Current.RewritePath(Request.ApplicationPath + "/Backend/API.aspx");
            return;
        }
        else if (path.ToLower().StartsWith("/archive")) System.Web.HttpContext.Current.RewritePath(Request.ApplicationPath + "/Default.aspx?page=404");
        else RewriteNewPath(Request.ApplicationPath + "Default.aspx", Request.ApplicationPath + "Default.aspx?page=home");
    }
    void RewriteNewPath(string newpath, string newpath_default)
    {
        string[] items = System.Web.HttpContext.Current.Request.Path.Split('/');
        if (items.Length < 1 || (items.Length > 1 && items[0].Length == 0 && items[1].Length == 0) || System.Web.HttpContext.Current.Request.Path.ToLower().Equals("/default.aspx"))
            System.Web.HttpContext.Current.RewritePath(newpath_default, true);
        else
        {
            // Add each dir as a query-string
            int ai = 0;
            for (int i = 0; i < items.LongLength; i++)
            {
                if (items[i] != "")
                {
                    if (ai == 0) newpath += "?page=" + items[i] + "&";
                    else newpath += ai.ToString() + "=" + items[i] + "&";
                    ai++;
                }
            }
            // Add each pre-existing query-string
            if (Request.QueryString.Count != 0)
            {
                for (int q = 0; q < Request.QueryString.Count; q++)
                    newpath += Request.QueryString.Keys[q] + "=" + Request.QueryString[q] + "&";
            }
            // Remove newpath '&' char
            newpath = newpath.Remove(newpath.Length - 1, 1);
            // Rewrite URL to new one
            System.Web.HttpContext.Current.RewritePath(newpath, true);
        }
    }
</script>

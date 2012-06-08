/*
 * UBERMEAT FOSS
 * ****************************************************************************************
 * License:                 Creative Commons Attribution-ShareAlike 3.0 unported
 *                          http://creativecommons.org/licenses/by-sa/3.0/
 * 
 * Project:                 Uber Media
 * File:                    /Complete.cs
 * Author(s):               limpygnome						limpygnome@gmail.com
 * To-do/bugs:              none
 * 
 * Informs the user the first-time configuration of the launcher is complete; this form
 * also provides a list of possible IPs to use to access the web-server.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;

namespace WebsiteLauncher
{
    public partial class Complete : Form
    {
        #region "Variables"
        private int webPort;
        #endregion

        #region "Methods - Constructors"
        public Complete(int webPort)
        {
            this.webPort = webPort;
            InitializeComponent();
        }
        #endregion

        #region "Methods - Overrides"
        // Based on http://www.codeproject.com/Articles/20379/Disabling-Close-Button-on-Forms
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }
        #endregion

        #region "Methods - Events"
        private void buttContinue_MouseClick(object sender, MouseEventArgs e)
        {
            Application.Restart();
        }
        private void Complete_Load(object sender, EventArgs e)
        {
            // Add the machine-name address
            lbAddresses.Items.Add("http://" + Environment.MachineName + ":" + webPort + "/");
            // Add all the addresses on the network interfaces
            foreach (NetworkInterface i in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                // Only use this network interface if it's operational
                if(i.OperationalStatus == OperationalStatus.Dormant || i.OperationalStatus == OperationalStatus.Up)
                    foreach (UnicastIPAddressInformation address in i.GetIPProperties().UnicastAddresses)
                        lbAddresses.Items.Add("http://" + address.Address.ToString() + ":" + webPort + "/");
        }
        private void lbAddresses_SelectedIndexChanged(object sender, EventArgs e)
        { // An item has been selected -> copy to clipboard
            if (lbAddresses.Text != null && lbAddresses.Text.Length > 0)
                Clipboard.SetText(lbAddresses.Text);
        }
        #endregion
    }
}
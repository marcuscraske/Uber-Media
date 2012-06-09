namespace UberMediaServer
{
    partial class ConfigGenerator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtTerminalTitle = new System.Windows.Forms.TextBox();
            this.txtWebsite = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtWindowsPassword = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.buttContinue = new UberLib.Controls.Button();
            this.header1 = new UberLib.Controls.Header();
            this.header3 = new UberLib.Controls.Header();
            this.buttExit = new UberLib.Controls.Button();
            this.cbAutoLogon = new UberLib.Controls.Checkbox();
            this.cbAutoStartup = new UberLib.Controls.Checkbox();
            this.cbDesktopShortcut = new UberLib.Controls.Checkbox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtWindowsDomain = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtWindowsUsername = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(12, 163);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(119, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "A name for this terminal:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(9, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(122, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Website of media library:";
            // 
            // txtTerminalTitle
            // 
            this.txtTerminalTitle.Location = new System.Drawing.Point(148, 160);
            this.txtTerminalTitle.Name = "txtTerminalTitle";
            this.txtTerminalTitle.Size = new System.Drawing.Size(202, 20);
            this.txtTerminalTitle.TabIndex = 3;
            this.txtTerminalTitle.Text = "Media Computer";
            // 
            // txtWebsite
            // 
            this.txtWebsite.Location = new System.Drawing.Point(12, 59);
            this.txtWebsite.Name = "txtWebsite";
            this.txtWebsite.Size = new System.Drawing.Size(397, 20);
            this.txtWebsite.TabIndex = 2;
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(153)))), ((int)(((byte)(0)))));
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.ForeColor = System.Drawing.Color.White;
            this.textBox1.Location = new System.Drawing.Point(12, 374);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(400, 18);
            this.textBox1.TabIndex = 8;
            this.textBox1.Text = "This will cause the above user to be automatically logged-on when Windows starts." +
                "";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(12, 352);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Password:";
            // 
            // txtWindowsPassword
            // 
            this.txtWindowsPassword.Location = new System.Drawing.Point(162, 349);
            this.txtWindowsPassword.Name = "txtWindowsPassword";
            this.txtWindowsPassword.PasswordChar = '*';
            this.txtWindowsPassword.Size = new System.Drawing.Size(247, 20);
            this.txtWindowsPassword.TabIndex = 10;
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(153)))), ((int)(((byte)(0)))));
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.ForeColor = System.Drawing.Color.White;
            this.textBox2.Location = new System.Drawing.Point(12, 85);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(397, 69);
            this.textBox2.TabIndex = 9;
            this.textBox2.Text = "The website of your media library is the site used to control  media computers an" +
                "d manage your media library, e.g.:\r\nhttp://192.168.10/\r\n\r\nThis should be the bas" +
                "e-path of the site!";
            // 
            // buttContinue
            // 
            this.buttContinue.BackColor = System.Drawing.Color.Transparent;
            this.buttContinue.ButtonText = "Save";
            this.buttContinue.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttContinue.Location = new System.Drawing.Point(314, 438);
            this.buttContinue.Name = "buttContinue";
            this.buttContinue.Size = new System.Drawing.Size(98, 27);
            this.buttContinue.Style_Border_Colour = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.buttContinue.Style_Border_Size = 1F;
            this.buttContinue.Style_Normal_Background = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.buttContinue.Style_Normal_Background_1 = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
            this.buttContinue.Style_Normal_Background_2 = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(153)))), ((int)(((byte)(0)))));
            this.buttContinue.Style_Normal_Background_Image = null;
            this.buttContinue.Style_Normal_Text_Colour = System.Drawing.Color.White;
            this.buttContinue.Style_OnClick_Background = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.buttContinue.Style_OnClick_Background_1 = System.Drawing.Color.LawnGreen;
            this.buttContinue.Style_OnClick_Background_2 = System.Drawing.Color.LimeGreen;
            this.buttContinue.Style_OnClick_Background_Image = null;
            this.buttContinue.Style_OnClick_Text_Colour = System.Drawing.Color.White;
            this.buttContinue.Style_OnHover_Background = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.buttContinue.Style_OnHover_Background_1 = System.Drawing.Color.LightGray;
            this.buttContinue.Style_OnHover_Background_2 = System.Drawing.Color.White;
            this.buttContinue.Style_OnHover_Background_Image = null;
            this.buttContinue.Style_OnHover_Text_Colour = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
            this.buttContinue.Style_Text_Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttContinue.Style_Text_Position_Horizontal = UberLib.Controls.Button.TextTypeX.Centre;
            this.buttContinue.Style_Text_Position_Vertical = UberLib.Controls.Button.TextTypeY.Centre;
            this.buttContinue.TabIndex = 16;
            this.buttContinue.MouseClick += new System.Windows.Forms.MouseEventHandler(this.buttContinue_MouseClick);
            // 
            // header1
            // 
            this.header1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.header1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.header1.ForeColor = System.Drawing.Color.Black;
            this.header1.HeaderColour = System.Drawing.Color.White;
            this.header1.HeaderIcon = null;
            this.header1.HeaderText = "Media Library Connection";
            this.header1.Location = new System.Drawing.Point(1, 1);
            this.header1.Margin = new System.Windows.Forms.Padding(4);
            this.header1.Name = "header1";
            this.header1.Size = new System.Drawing.Size(422, 28);
            this.header1.TabIndex = 17;
            // 
            // header3
            // 
            this.header3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.header3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.header3.ForeColor = System.Drawing.Color.Black;
            this.header3.HeaderColour = System.Drawing.Color.White;
            this.header3.HeaderIcon = null;
            this.header3.HeaderText = "Windows Automatic Logon";
            this.header3.Location = new System.Drawing.Point(1, 226);
            this.header3.Margin = new System.Windows.Forms.Padding(4);
            this.header3.Name = "header3";
            this.header3.Size = new System.Drawing.Size(422, 28);
            this.header3.TabIndex = 19;
            // 
            // buttExit
            // 
            this.buttExit.BackColor = System.Drawing.Color.Transparent;
            this.buttExit.ButtonText = "Exit";
            this.buttExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttExit.Location = new System.Drawing.Point(12, 438);
            this.buttExit.Name = "buttExit";
            this.buttExit.Size = new System.Drawing.Size(98, 27);
            this.buttExit.Style_Border_Colour = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.buttExit.Style_Border_Size = 1F;
            this.buttExit.Style_Normal_Background = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.buttExit.Style_Normal_Background_1 = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
            this.buttExit.Style_Normal_Background_2 = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(153)))), ((int)(((byte)(0)))));
            this.buttExit.Style_Normal_Background_Image = null;
            this.buttExit.Style_Normal_Text_Colour = System.Drawing.Color.White;
            this.buttExit.Style_OnClick_Background = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.buttExit.Style_OnClick_Background_1 = System.Drawing.Color.LawnGreen;
            this.buttExit.Style_OnClick_Background_2 = System.Drawing.Color.LimeGreen;
            this.buttExit.Style_OnClick_Background_Image = null;
            this.buttExit.Style_OnClick_Text_Colour = System.Drawing.Color.White;
            this.buttExit.Style_OnHover_Background = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            this.buttExit.Style_OnHover_Background_1 = System.Drawing.Color.LightGray;
            this.buttExit.Style_OnHover_Background_2 = System.Drawing.Color.White;
            this.buttExit.Style_OnHover_Background_Image = null;
            this.buttExit.Style_OnHover_Text_Colour = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(102)))), ((int)(((byte)(0)))));
            this.buttExit.Style_Text_Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttExit.Style_Text_Position_Horizontal = UberLib.Controls.Button.TextTypeX.Centre;
            this.buttExit.Style_Text_Position_Vertical = UberLib.Controls.Button.TextTypeY.Centre;
            this.buttExit.TabIndex = 20;
            this.buttExit.MouseClick += new System.Windows.Forms.MouseEventHandler(this.buttExit_MouseClick);
            // 
            // cbAutoLogon
            // 
            this.cbAutoLogon.BackColor = System.Drawing.Color.Transparent;
            this.cbAutoLogon.BackgroundChecked = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.cbAutoLogon.BackgroundUnchecked = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.cbAutoLogon.Checked = false;
            this.cbAutoLogon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbAutoLogon.HeaderText = "Automatically logon the following Windows user";
            this.cbAutoLogon.HeaderTextColour = System.Drawing.Color.White;
            this.cbAutoLogon.HeaderTextFont = new System.Drawing.Font("Arial", 10F);
            this.cbAutoLogon.ImageChecked = global::UberMediaServer.Properties.Resources.tick;
            this.cbAutoLogon.ImageUnchecked = global::UberMediaServer.Properties.Resources.cross;
            this.cbAutoLogon.Location = new System.Drawing.Point(15, 264);
            this.cbAutoLogon.Name = "cbAutoLogon";
            this.cbAutoLogon.Size = new System.Drawing.Size(397, 27);
            this.cbAutoLogon.TabIndex = 22;
            // 
            // cbAutoStartup
            // 
            this.cbAutoStartup.BackColor = System.Drawing.Color.Transparent;
            this.cbAutoStartup.BackgroundChecked = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.cbAutoStartup.BackgroundUnchecked = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.cbAutoStartup.Checked = false;
            this.cbAutoStartup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbAutoStartup.HeaderText = "Automatically start Uber Media on Windows logon";
            this.cbAutoStartup.HeaderTextColour = System.Drawing.Color.White;
            this.cbAutoStartup.HeaderTextFont = new System.Drawing.Font("Arial", 10F);
            this.cbAutoStartup.ImageChecked = global::UberMediaServer.Properties.Resources.tick;
            this.cbAutoStartup.ImageUnchecked = global::UberMediaServer.Properties.Resources.cross;
            this.cbAutoStartup.Location = new System.Drawing.Point(15, 190);
            this.cbAutoStartup.Name = "cbAutoStartup";
            this.cbAutoStartup.Size = new System.Drawing.Size(397, 27);
            this.cbAutoStartup.TabIndex = 21;
            // 
            // cbDesktopShortcut
            // 
            this.cbDesktopShortcut.BackColor = System.Drawing.Color.Transparent;
            this.cbDesktopShortcut.BackgroundChecked = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.cbDesktopShortcut.BackgroundUnchecked = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.cbDesktopShortcut.Checked = true;
            this.cbDesktopShortcut.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbDesktopShortcut.HeaderText = "Create desktop shortcut";
            this.cbDesktopShortcut.HeaderTextColour = System.Drawing.Color.White;
            this.cbDesktopShortcut.HeaderTextFont = new System.Drawing.Font("Arial", 10F);
            this.cbDesktopShortcut.ImageChecked = global::UberMediaServer.Properties.Resources.tick;
            this.cbDesktopShortcut.ImageUnchecked = global::UberMediaServer.Properties.Resources.cross;
            this.cbDesktopShortcut.Location = new System.Drawing.Point(15, 394);
            this.cbDesktopShortcut.Name = "cbDesktopShortcut";
            this.cbDesktopShortcut.Size = new System.Drawing.Size(397, 27);
            this.cbDesktopShortcut.TabIndex = 23;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(12, 300);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 24;
            this.label1.Text = "Domain:";
            // 
            // txtWindowsDomain
            // 
            this.txtWindowsDomain.Location = new System.Drawing.Point(162, 297);
            this.txtWindowsDomain.Name = "txtWindowsDomain";
            this.txtWindowsDomain.Size = new System.Drawing.Size(247, 20);
            this.txtWindowsDomain.TabIndex = 25;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(12, 326);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 26;
            this.label4.Text = "Username:";
            // 
            // txtWindowsUsername
            // 
            this.txtWindowsUsername.Location = new System.Drawing.Point(162, 323);
            this.txtWindowsUsername.Name = "txtWindowsUsername";
            this.txtWindowsUsername.Size = new System.Drawing.Size(247, 20);
            this.txtWindowsUsername.TabIndex = 27;
            // 
            // ConfigGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(153)))), ((int)(((byte)(0)))));
            this.ClientSize = new System.Drawing.Size(422, 476);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtWindowsUsername);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtWindowsDomain);
            this.Controls.Add(this.cbDesktopShortcut);
            this.Controls.Add(this.cbAutoLogon);
            this.Controls.Add(this.cbAutoStartup);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtWindowsPassword);
            this.Controls.Add(this.txtTerminalTitle);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.buttExit);
            this.Controls.Add(this.header3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtWebsite);
            this.Controls.Add(this.header1);
            this.Controls.Add(this.buttContinue);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigGenerator";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Uber Media - Server Configurator";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ConfigGenerator_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtTerminalTitle;
        private System.Windows.Forms.TextBox txtWebsite;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtWindowsPassword;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private UberLib.Controls.Button buttContinue;
        private UberLib.Controls.Header header1;
        private UberLib.Controls.Header header3;
        private UberLib.Controls.Button buttExit;
        private UberLib.Controls.Checkbox cbAutoStartup;
        private UberLib.Controls.Checkbox cbAutoLogon;
        private UberLib.Controls.Checkbox cbDesktopShortcut;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtWindowsDomain;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtWindowsUsername;
    }
}
namespace WebsiteLauncher
{
    partial class Settings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.headerSettings = new UberLib.Controls.Header();
            this.headerPorts = new UberLib.Controls.Header();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPortWeb = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPortDatabase = new System.Windows.Forms.TextBox();
            this.buttExit = new UberLib.Controls.Button();
            this.buttContinue = new UberLib.Controls.Button();
            this.labelDesc = new System.Windows.Forms.TextBox();
            this.cbHideWindows = new UberLib.Controls.Checkbox();
            this.cbLaunchDatabase = new UberLib.Controls.Checkbox();
            this.cbLaunchWeb = new UberLib.Controls.Checkbox();
            this.cbShortcut = new UberLib.Controls.Checkbox();
            this.SuspendLayout();
            // 
            // headerSettings
            // 
            this.headerSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.headerSettings.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headerSettings.ForeColor = System.Drawing.Color.Black;
            this.headerSettings.HeaderColour = System.Drawing.Color.White;
            this.headerSettings.HeaderIcon = null;
            this.headerSettings.HeaderText = "Settings";
            this.headerSettings.Location = new System.Drawing.Point(1, 0);
            this.headerSettings.Margin = new System.Windows.Forms.Padding(4);
            this.headerSettings.Name = "headerSettings";
            this.headerSettings.Size = new System.Drawing.Size(425, 28);
            this.headerSettings.TabIndex = 0;
            // 
            // headerPorts
            // 
            this.headerPorts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.headerPorts.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headerPorts.ForeColor = System.Drawing.Color.Black;
            this.headerPorts.HeaderColour = System.Drawing.Color.White;
            this.headerPorts.HeaderIcon = null;
            this.headerPorts.HeaderText = "Ports";
            this.headerPorts.Location = new System.Drawing.Point(1, 170);
            this.headerPorts.Margin = new System.Windows.Forms.Padding(4);
            this.headerPorts.Name = "headerPorts";
            this.headerPorts.Size = new System.Drawing.Size(425, 28);
            this.headerPorts.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(9, 213);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 16);
            this.label2.TabIndex = 26;
            this.label2.Text = "Web:";
            // 
            // txtPortWeb
            // 
            this.txtPortWeb.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPortWeb.Location = new System.Drawing.Point(86, 207);
            this.txtPortWeb.MaxLength = 5;
            this.txtPortWeb.Name = "txtPortWeb";
            this.txtPortWeb.Size = new System.Drawing.Size(97, 26);
            this.txtPortWeb.TabIndex = 25;
            this.txtPortWeb.Text = "80";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(9, 245);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 16);
            this.label1.TabIndex = 28;
            this.label1.Text = "Database:";
            // 
            // txtPortDatabase
            // 
            this.txtPortDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPortDatabase.Location = new System.Drawing.Point(86, 239);
            this.txtPortDatabase.MaxLength = 5;
            this.txtPortDatabase.Name = "txtPortDatabase";
            this.txtPortDatabase.Size = new System.Drawing.Size(97, 26);
            this.txtPortDatabase.TabIndex = 27;
            this.txtPortDatabase.Text = "3306";
            // 
            // buttExit
            // 
            this.buttExit.BackColor = System.Drawing.Color.Transparent;
            this.buttExit.ButtonText = "Cancel";
            this.buttExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttExit.Location = new System.Drawing.Point(12, 311);
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
            this.buttExit.TabIndex = 30;
            this.buttExit.MouseClick += new System.Windows.Forms.MouseEventHandler(this.buttExit_MouseClick);
            // 
            // buttContinue
            // 
            this.buttContinue.BackColor = System.Drawing.Color.Transparent;
            this.buttContinue.ButtonText = "Save";
            this.buttContinue.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttContinue.Location = new System.Drawing.Point(311, 311);
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
            this.buttContinue.TabIndex = 29;
            this.buttContinue.MouseClick += new System.Windows.Forms.MouseEventHandler(this.buttContinue_MouseClick);
            // 
            // labelDesc
            // 
            this.labelDesc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(153)))), ((int)(((byte)(0)))));
            this.labelDesc.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.labelDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDesc.ForeColor = System.Drawing.Color.White;
            this.labelDesc.Location = new System.Drawing.Point(12, 284);
            this.labelDesc.Multiline = true;
            this.labelDesc.Name = "labelDesc";
            this.labelDesc.ReadOnly = true;
            this.labelDesc.Size = new System.Drawing.Size(397, 20);
            this.labelDesc.TabIndex = 31;
            this.labelDesc.Text = "If you are not sure what any of the above means, click \"Continue\"...";
            // 
            // cbHideWindows
            // 
            this.cbHideWindows.BackColor = System.Drawing.Color.Transparent;
            this.cbHideWindows.BackgroundChecked = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.cbHideWindows.BackgroundUnchecked = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.cbHideWindows.Checked = true;
            this.cbHideWindows.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbHideWindows.HeaderText = "Hide windows";
            this.cbHideWindows.HeaderTextColour = System.Drawing.Color.White;
            this.cbHideWindows.HeaderTextFont = new System.Drawing.Font("Arial", 10F);
            this.cbHideWindows.ImageChecked = global::WebsiteLauncher.Properties.Resources.tick;
            this.cbHideWindows.ImageUnchecked = global::WebsiteLauncher.Properties.Resources.cross;
            this.cbHideWindows.Location = new System.Drawing.Point(12, 101);
            this.cbHideWindows.Name = "cbHideWindows";
            this.cbHideWindows.Size = new System.Drawing.Size(397, 27);
            this.cbHideWindows.TabIndex = 32;
            // 
            // cbLaunchDatabase
            // 
            this.cbLaunchDatabase.BackColor = System.Drawing.Color.Transparent;
            this.cbLaunchDatabase.BackgroundChecked = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.cbLaunchDatabase.BackgroundUnchecked = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.cbLaunchDatabase.Checked = true;
            this.cbLaunchDatabase.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbLaunchDatabase.HeaderText = "Launch database server";
            this.cbLaunchDatabase.HeaderTextColour = System.Drawing.Color.White;
            this.cbLaunchDatabase.HeaderTextFont = new System.Drawing.Font("Arial", 10F);
            this.cbLaunchDatabase.ImageChecked = global::WebsiteLauncher.Properties.Resources.tick;
            this.cbLaunchDatabase.ImageUnchecked = global::WebsiteLauncher.Properties.Resources.cross;
            this.cbLaunchDatabase.Location = new System.Drawing.Point(12, 68);
            this.cbLaunchDatabase.Name = "cbLaunchDatabase";
            this.cbLaunchDatabase.Size = new System.Drawing.Size(397, 27);
            this.cbLaunchDatabase.TabIndex = 23;
            // 
            // cbLaunchWeb
            // 
            this.cbLaunchWeb.BackColor = System.Drawing.Color.Transparent;
            this.cbLaunchWeb.BackgroundChecked = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.cbLaunchWeb.BackgroundUnchecked = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.cbLaunchWeb.Checked = true;
            this.cbLaunchWeb.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbLaunchWeb.HeaderText = "Launch web server";
            this.cbLaunchWeb.HeaderTextColour = System.Drawing.Color.White;
            this.cbLaunchWeb.HeaderTextFont = new System.Drawing.Font("Arial", 10F);
            this.cbLaunchWeb.ImageChecked = global::WebsiteLauncher.Properties.Resources.tick;
            this.cbLaunchWeb.ImageUnchecked = global::WebsiteLauncher.Properties.Resources.cross;
            this.cbLaunchWeb.Location = new System.Drawing.Point(12, 35);
            this.cbLaunchWeb.Name = "cbLaunchWeb";
            this.cbLaunchWeb.Size = new System.Drawing.Size(397, 27);
            this.cbLaunchWeb.TabIndex = 22;
            // 
            // cbShortcut
            // 
            this.cbShortcut.BackColor = System.Drawing.Color.Transparent;
            this.cbShortcut.BackgroundChecked = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.cbShortcut.BackgroundUnchecked = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.cbShortcut.Checked = true;
            this.cbShortcut.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbShortcut.HeaderText = "Create desktop shortcut to website";
            this.cbShortcut.HeaderTextColour = System.Drawing.Color.White;
            this.cbShortcut.HeaderTextFont = new System.Drawing.Font("Arial", 10F);
            this.cbShortcut.ImageChecked = global::WebsiteLauncher.Properties.Resources.tick;
            this.cbShortcut.ImageUnchecked = global::WebsiteLauncher.Properties.Resources.cross;
            this.cbShortcut.Location = new System.Drawing.Point(12, 134);
            this.cbShortcut.Name = "cbShortcut";
            this.cbShortcut.Size = new System.Drawing.Size(397, 27);
            this.cbShortcut.TabIndex = 33;
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(153)))), ((int)(((byte)(0)))));
            this.ClientSize = new System.Drawing.Size(421, 350);
            this.Controls.Add(this.cbShortcut);
            this.Controls.Add(this.cbHideWindows);
            this.Controls.Add(this.labelDesc);
            this.Controls.Add(this.buttExit);
            this.Controls.Add(this.buttContinue);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPortDatabase);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPortWeb);
            this.Controls.Add(this.headerPorts);
            this.Controls.Add(this.cbLaunchDatabase);
            this.Controls.Add(this.cbLaunchWeb);
            this.Controls.Add(this.headerSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Uber Media - Website Launcher - Configuration";
            this.Load += new System.EventHandler(this.Settings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private UberLib.Controls.Header headerSettings;
        private UberLib.Controls.Checkbox cbLaunchWeb;
        private UberLib.Controls.Checkbox cbLaunchDatabase;
        private UberLib.Controls.Header headerPorts;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPortWeb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPortDatabase;
        private UberLib.Controls.Button buttExit;
        private UberLib.Controls.Button buttContinue;
        private System.Windows.Forms.TextBox labelDesc;
        private UberLib.Controls.Checkbox cbHideWindows;
        private UberLib.Controls.Checkbox cbShortcut;
    }
}


namespace WebsiteLauncher
{
    partial class Complete
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Complete));
            this.headerSettings = new UberLib.Controls.Header();
            this.labelInfo = new System.Windows.Forms.TextBox();
            this.lbAddresses = new System.Windows.Forms.ListBox();
            this.buttExit = new UberLib.Controls.Button();
            this.SuspendLayout();
            // 
            // headerSettings
            // 
            this.headerSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(0)))));
            this.headerSettings.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headerSettings.ForeColor = System.Drawing.Color.Black;
            this.headerSettings.HeaderColour = System.Drawing.Color.White;
            this.headerSettings.HeaderIcon = null;
            this.headerSettings.HeaderText = "Complete";
            this.headerSettings.Location = new System.Drawing.Point(0, 0);
            this.headerSettings.Margin = new System.Windows.Forms.Padding(4);
            this.headerSettings.Name = "headerSettings";
            this.headerSettings.Size = new System.Drawing.Size(425, 28);
            this.headerSettings.TabIndex = 1;
            // 
            // labelInfo
            // 
            this.labelInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(153)))), ((int)(((byte)(0)))));
            this.labelInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelInfo.ForeColor = System.Drawing.Color.White;
            this.labelInfo.Location = new System.Drawing.Point(12, 35);
            this.labelInfo.Multiline = true;
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.ReadOnly = true;
            this.labelInfo.Size = new System.Drawing.Size(397, 128);
            this.labelInfo.TabIndex = 32;
            this.labelInfo.Text = resources.GetString("labelInfo.Text");
            // 
            // lbAddresses
            // 
            this.lbAddresses.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbAddresses.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbAddresses.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(153)))), ((int)(((byte)(0)))));
            this.lbAddresses.FormattingEnabled = true;
            this.lbAddresses.ItemHeight = 20;
            this.lbAddresses.Location = new System.Drawing.Point(12, 169);
            this.lbAddresses.Name = "lbAddresses";
            this.lbAddresses.Size = new System.Drawing.Size(397, 160);
            this.lbAddresses.TabIndex = 33;
            this.lbAddresses.SelectedIndexChanged += new System.EventHandler(this.lbAddresses_SelectedIndexChanged);
            // 
            // buttExit
            // 
            this.buttExit.BackColor = System.Drawing.Color.Transparent;
            this.buttExit.ButtonText = "Restart";
            this.buttExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttExit.Location = new System.Drawing.Point(311, 335);
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
            this.buttExit.TabIndex = 34;
            this.buttExit.MouseClick += new System.Windows.Forms.MouseEventHandler(this.buttContinue_MouseClick);
            // 
            // Complete
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(153)))), ((int)(((byte)(0)))));
            this.ClientSize = new System.Drawing.Size(424, 371);
            this.Controls.Add(this.buttExit);
            this.Controls.Add(this.lbAddresses);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.headerSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Complete";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Uber Media - Website Launcher - Complete";
            this.Load += new System.EventHandler(this.Complete_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private UberLib.Controls.Header headerSettings;
        private System.Windows.Forms.TextBox labelInfo;
        private System.Windows.Forms.ListBox lbAddresses;
        private UberLib.Controls.Button buttExit;
    }
}
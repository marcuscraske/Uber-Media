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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dbDatabase = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.dbPass = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dbUser = new System.Windows.Forms.TextBox();
            this.dbPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.startupWebserver = new System.Windows.Forms.CheckBox();
            this.startupServer = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.startupLogon = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.startupLogonPassword = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.startupDatabase = new System.Windows.Forms.CheckBox();
            this.dbHost = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.startupWebserverPort = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.terminalKey = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.terminalTitle = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dbHost);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.dbDatabase);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.dbPass);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.dbUser);
            this.groupBox1.Controls.Add(this.dbPort);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(358, 154);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Database";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 126);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Database:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Pass:";
            // 
            // dbDatabase
            // 
            this.dbDatabase.Location = new System.Drawing.Point(142, 123);
            this.dbDatabase.Name = "dbDatabase";
            this.dbDatabase.Size = new System.Drawing.Size(202, 20);
            this.dbDatabase.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "User:";
            // 
            // dbPass
            // 
            this.dbPass.Location = new System.Drawing.Point(142, 97);
            this.dbPass.Name = "dbPass";
            this.dbPass.PasswordChar = '*';
            this.dbPass.Size = new System.Drawing.Size(202, 20);
            this.dbPass.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port:";
            // 
            // dbUser
            // 
            this.dbUser.Location = new System.Drawing.Point(142, 71);
            this.dbUser.Name = "dbUser";
            this.dbUser.Size = new System.Drawing.Size(202, 20);
            this.dbUser.TabIndex = 3;
            this.dbUser.Text = "root";
            // 
            // dbPort
            // 
            this.dbPort.Location = new System.Drawing.Point(142, 45);
            this.dbPort.Name = "dbPort";
            this.dbPort.Size = new System.Drawing.Size(102, 20);
            this.dbPort.TabIndex = 2;
            this.dbPort.Text = "3306";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Host:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.startupDatabase);
            this.groupBox2.Controls.Add(this.startupWebserverPort);
            this.groupBox2.Controls.Add(this.startupWebserver);
            this.groupBox2.Controls.Add(this.startupServer);
            this.groupBox2.Location = new System.Drawing.Point(12, 172);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(358, 120);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // startupWebserver
            // 
            this.startupWebserver.AutoSize = true;
            this.startupWebserver.Location = new System.Drawing.Point(9, 42);
            this.startupWebserver.Name = "startupWebserver";
            this.startupWebserver.Size = new System.Drawing.Size(237, 17);
            this.startupWebserver.TabIndex = 7;
            this.startupWebserver.Text = "Automatically startup the portable web server";
            this.startupWebserver.UseVisualStyleBackColor = true;
            // 
            // startupServer
            // 
            this.startupServer.AutoSize = true;
            this.startupServer.Location = new System.Drawing.Point(9, 19);
            this.startupServer.Name = "startupServer";
            this.startupServer.Size = new System.Drawing.Size(257, 17);
            this.startupServer.TabIndex = 6;
            this.startupServer.Text = "Automatically startup Uber Media server on logon";
            this.startupServer.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Controls.Add(this.startupLogon);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.startupLogonPassword);
            this.groupBox3.Location = new System.Drawing.Point(12, 298);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(358, 103);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Automatic Logon";
            // 
            // startupLogon
            // 
            this.startupLogon.AutoSize = true;
            this.startupLogon.Location = new System.Drawing.Point(279, 45);
            this.startupLogon.Name = "startupLogon";
            this.startupLogon.Size = new System.Drawing.Size(65, 17);
            this.startupLogon.TabIndex = 11;
            this.startupLogon.Text = "Enabled";
            this.startupLogon.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Password:";
            // 
            // startupLogonPassword
            // 
            this.startupLogonPassword.Location = new System.Drawing.Point(142, 19);
            this.startupLogonPassword.Name = "startupLogonPassword";
            this.startupLogonPassword.PasswordChar = '*';
            this.startupLogonPassword.Size = new System.Drawing.Size(202, 20);
            this.startupLogonPassword.TabIndex = 10;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(297, 525);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 15;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 525);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 14;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(9, 68);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(335, 32);
            this.textBox1.TabIndex = 8;
            this.textBox1.Text = "Enter the password for your current Windows account and it will automatically log" +
                "on when your computer starrts-up.";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // startupDatabase
            // 
            this.startupDatabase.AutoSize = true;
            this.startupDatabase.Location = new System.Drawing.Point(9, 65);
            this.startupDatabase.Name = "startupDatabase";
            this.startupDatabase.Size = new System.Drawing.Size(188, 17);
            this.startupDatabase.TabIndex = 8;
            this.startupDatabase.Text = "Automatically startup the database";
            this.startupDatabase.UseVisualStyleBackColor = true;
            // 
            // dbHost
            // 
            this.dbHost.FormattingEnabled = true;
            this.dbHost.Items.AddRange(new object[] {
            "127.0.0.1"});
            this.dbHost.Location = new System.Drawing.Point(142, 18);
            this.dbHost.Name = "dbHost";
            this.dbHost.Size = new System.Drawing.Size(202, 21);
            this.dbHost.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 91);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(86, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Web server port:";
            // 
            // startupWebserverPort
            // 
            this.startupWebserverPort.Location = new System.Drawing.Point(142, 88);
            this.startupWebserverPort.Name = "startupWebserverPort";
            this.startupWebserverPort.Size = new System.Drawing.Size(102, 20);
            this.startupWebserverPort.TabIndex = 9;
            this.startupWebserverPort.Text = "80";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Controls.Add(this.terminalTitle);
            this.groupBox4.Controls.Add(this.textBox2);
            this.groupBox4.Controls.Add(this.terminalKey);
            this.groupBox4.Location = new System.Drawing.Point(12, 407);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(360, 112);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Terminal Key";
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Control;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Location = new System.Drawing.Point(9, 45);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(345, 32);
            this.textBox2.TabIndex = 10;
            this.textBox2.Text = "Your terminal key should be a unique identifier for this computer; it will be aut" +
                "omatically added to the media website if automatic joining is allowed.";
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // terminalKey
            // 
            this.terminalKey.Location = new System.Drawing.Point(142, 19);
            this.terminalKey.Name = "terminalKey";
            this.terminalKey.Size = new System.Drawing.Size(212, 20);
            this.terminalKey.TabIndex = 12;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(28, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Key:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 86);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(30, 13);
            this.label9.TabIndex = 10;
            this.label9.Text = "Title:";
            // 
            // terminalTitle
            // 
            this.terminalTitle.Location = new System.Drawing.Point(142, 83);
            this.terminalTitle.Name = "terminalTitle";
            this.terminalTitle.Size = new System.Drawing.Size(212, 20);
            this.terminalTitle.TabIndex = 13;
            this.terminalTitle.Text = "My New Media Computer";
            // 
            // ConfigGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 558);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigGenerator";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Uber Media - Server Configurator";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigGenerator_FormClosing);
            this.Load += new System.EventHandler(this.ConfigGenerator_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox dbDatabase;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox dbPass;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox dbUser;
        private System.Windows.Forms.TextBox dbPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox startupWebserver;
        private System.Windows.Forms.CheckBox startupServer;
        private System.Windows.Forms.CheckBox startupLogon;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox startupLogonPassword;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox startupDatabase;
        private System.Windows.Forms.ComboBox dbHost;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox startupWebserverPort;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox terminalTitle;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox terminalKey;
    }
}
namespace AGLatency
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            lblPrimaryProgress = new System.Windows.Forms.Label();
            lblSecondaryProgress = new System.Windows.Forms.Label();
            txtSecondaryFolder = new System.Windows.Forms.TextBox();
            lblPrimary = new System.Windows.Forms.Label();
            lblSecondary = new System.Windows.Forms.Label();
            btnBrowseSecondary = new System.Windows.Forms.Button();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tSQLScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            logToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            userManualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            videoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            lbVersion = new System.Windows.Forms.Label();
            btnBrowsePrimary = new System.Windows.Forms.Button();
            txtPrimaryFolder = new System.Windows.Forms.TextBox();
            btnStart = new System.Windows.Forms.Button();
            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            chkBox_UseLogScout = new System.Windows.Forms.CheckBox();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            tsslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            tsslElapsed = new System.Windows.Forms.ToolStripStatusLabel();
            tsslEvents = new System.Windows.Forms.ToolStripStatusLabel();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lblPrimaryProgress
            // 
            lblPrimaryProgress.AutoSize = true;
            lblPrimaryProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblPrimaryProgress.Location = new System.Drawing.Point(58, 142);
            lblPrimaryProgress.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblPrimaryProgress.Name = "lblPrimaryProgress";
            lblPrimaryProgress.Size = new System.Drawing.Size(60, 25);
            lblPrimaryProgress.TabIndex = 4;
            lblPrimaryProgress.Text = "        ";
            // 
            // lblSecondaryProgress
            // 
            lblSecondaryProgress.AutoSize = true;
            lblSecondaryProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblSecondaryProgress.Location = new System.Drawing.Point(58, 298);
            lblSecondaryProgress.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblSecondaryProgress.Name = "lblSecondaryProgress";
            lblSecondaryProgress.Size = new System.Drawing.Size(54, 25);
            lblSecondaryProgress.TabIndex = 5;
            lblSecondaryProgress.Text = "       ";
            // 
            // txtSecondaryFolder
            // 
            txtSecondaryFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txtSecondaryFolder.Location = new System.Drawing.Point(58, 247);
            txtSecondaryFolder.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            txtSecondaryFolder.Name = "txtSecondaryFolder";
            txtSecondaryFolder.Size = new System.Drawing.Size(842, 30);
            txtSecondaryFolder.TabIndex = 6;
            // 
            // lblPrimary
            // 
            lblPrimary.AutoSize = true;
            lblPrimary.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblPrimary.Location = new System.Drawing.Point(58, 55);
            lblPrimary.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblPrimary.Name = "lblPrimary";
            lblPrimary.Size = new System.Drawing.Size(91, 25);
            lblPrimary.TabIndex = 7;
            lblPrimary.Text = "Primary:";
            // 
            // lblSecondary
            // 
            lblSecondary.AutoSize = true;
            lblSecondary.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblSecondary.Location = new System.Drawing.Point(58, 211);
            lblSecondary.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lblSecondary.Name = "lblSecondary";
            lblSecondary.Size = new System.Drawing.Size(121, 25);
            lblSecondary.TabIndex = 7;
            lblSecondary.Text = "Secondary:";
            // 
            // btnBrowseSecondary
            // 
            btnBrowseSecondary.Location = new System.Drawing.Point(921, 244);
            btnBrowseSecondary.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            btnBrowseSecondary.Name = "btnBrowseSecondary";
            btnBrowseSecondary.Size = new System.Drawing.Size(99, 47);
            btnBrowseSecondary.TabIndex = 8;
            btnBrowseSecondary.Text = "Browse";
            btnBrowseSecondary.UseVisualStyleBackColor = true;
            btnBrowseSecondary.Click += btnBrowseSecondary_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            menuStrip1.Size = new System.Drawing.Size(1145, 33);
            menuStrip1.TabIndex = 9;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.ItemClicked += menuStrip1_ItemClicked;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tSQLScriptToolStripMenuItem, logToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(54, 29);
            fileToolStripMenuItem.Text = "File";
            // 
            // tSQLScriptToolStripMenuItem
            // 
            tSQLScriptToolStripMenuItem.Name = "tSQLScriptToolStripMenuItem";
            tSQLScriptToolStripMenuItem.Size = new System.Drawing.Size(205, 34);
            tSQLScriptToolStripMenuItem.Text = "TSQL Script";
            tSQLScriptToolStripMenuItem.Click += tSQLScriptToolStripMenuItem_Click;
            // 
            // logToolStripMenuItem
            // 
            logToolStripMenuItem.Name = "logToolStripMenuItem";
            logToolStripMenuItem.Size = new System.Drawing.Size(205, 34);
            logToolStripMenuItem.Text = "Log";
            logToolStripMenuItem.Click += logToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { userManualToolStripMenuItem, videoToolStripMenuItem, aboutToolStripMenuItem1 });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new System.Drawing.Size(65, 29);
            helpToolStripMenuItem.Text = "Help";
            // 
            // userManualToolStripMenuItem
            // 
            userManualToolStripMenuItem.Name = "userManualToolStripMenuItem";
            userManualToolStripMenuItem.Size = new System.Drawing.Size(220, 34);
            userManualToolStripMenuItem.Text = "User Manual";
            userManualToolStripMenuItem.Click += userManualToolStripMenuItem_Click;
            // 
            // videoToolStripMenuItem
            // 
            videoToolStripMenuItem.Name = "videoToolStripMenuItem";
            videoToolStripMenuItem.Size = new System.Drawing.Size(220, 34);
            videoToolStripMenuItem.Text = "HowTo Video";
            videoToolStripMenuItem.Click += videoToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem1
            // 
            aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            aboutToolStripMenuItem1.Size = new System.Drawing.Size(220, 34);
            aboutToolStripMenuItem1.Text = "About";
            aboutToolStripMenuItem1.Click += aboutToolStripMenuItem_Click;
            // 
            // lbVersion
            // 
            lbVersion.AutoSize = true;
            lbVersion.ForeColor = System.Drawing.SystemColors.HotTrack;
            lbVersion.Location = new System.Drawing.Point(789, 9);
            lbVersion.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            lbVersion.Name = "lbVersion";
            lbVersion.Size = new System.Drawing.Size(85, 25);
            lbVersion.TabIndex = 10;
            lbVersion.Text = "lbVersion";
            lbVersion.Click += lbVersion_Click;
            // 
            // btnBrowsePrimary
            // 
            btnBrowsePrimary.Location = new System.Drawing.Point(921, 94);
            btnBrowsePrimary.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            btnBrowsePrimary.Name = "btnBrowsePrimary";
            btnBrowsePrimary.Size = new System.Drawing.Size(99, 47);
            btnBrowsePrimary.TabIndex = 8;
            btnBrowsePrimary.Text = "Browse";
            btnBrowsePrimary.UseVisualStyleBackColor = true;
            btnBrowsePrimary.Click += btnBrowsePrimary_Click;
            // 
            // txtPrimaryFolder
            // 
            txtPrimaryFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txtPrimaryFolder.Location = new System.Drawing.Point(58, 95);
            txtPrimaryFolder.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            txtPrimaryFolder.Name = "txtPrimaryFolder";
            txtPrimaryFolder.Size = new System.Drawing.Size(842, 30);
            txtPrimaryFolder.TabIndex = 0;
            // 
            // btnStart
            // 
            btnStart.Image = Properties.Resources.green2;
            btnStart.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btnStart.Location = new System.Drawing.Point(448, 400);
            btnStart.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            btnStart.Name = "btnStart";
            btnStart.Size = new System.Drawing.Size(168, 92);
            btnStart.TabIndex = 1;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // chkBox_UseLogScout
            // 
            chkBox_UseLogScout.AutoSize = true;
            chkBox_UseLogScout.Checked = true;
            chkBox_UseLogScout.CheckState = System.Windows.Forms.CheckState.Checked;
            chkBox_UseLogScout.Location = new System.Drawing.Point(758, 463);
            chkBox_UseLogScout.Name = "chkBox_UseLogScout";
            chkBox_UseLogScout.Size = new System.Drawing.Size(262, 29);
            chkBox_UseLogScout.TabIndex = 11;
            chkBox_UseLogScout.Text = "Files are from SQL LogScout (DataMovement)";
            chkBox_UseLogScout.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsslStatus, tsslElapsed, tsslEvents });
            statusStrip1.Location = new System.Drawing.Point(0, 498);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1145, 25);
            statusStrip1.TabIndex = 12;
            // 
            // tsslStatus
            // 
            tsslStatus.Name = "tsslStatus";
            tsslStatus.Size = new System.Drawing.Size(59, 20);
            tsslStatus.Text = "Ready";
            // 
            // tsslElapsed
            // 
            tsslElapsed.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            tsslElapsed.Name = "tsslElapsed";
            tsslElapsed.Size = new System.Drawing.Size(4, 20);
            // 
            // tsslEvents
            // 
            tsslEvents.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            tsslEvents.Name = "tsslEvents";
            tsslEvents.Size = new System.Drawing.Size(4, 20);
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1145, 523);
            Controls.Add(statusStrip1);
            Controls.Add(chkBox_UseLogScout);
            Controls.Add(lbVersion);
            Controls.Add(btnBrowseSecondary);
            Controls.Add(btnBrowsePrimary);
            Controls.Add(lblSecondary);
            Controls.Add(lblPrimary);
            Controls.Add(txtSecondaryFolder);
            Controls.Add(lblSecondaryProgress);
            Controls.Add(lblPrimaryProgress);
            Controls.Add(btnStart);
            Controls.Add(txtPrimaryFolder);
            Controls.Add(menuStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            Name = "Form1";
            Text = "AGLatency - AlwaysON AG Log Block Movement Latency Report Tool";
            FormClosing += Form1_FormClosing;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblPrimaryProgress;
        private System.Windows.Forms.Label lblSecondaryProgress;
        private System.Windows.Forms.TextBox txtSecondaryFolder;
        private System.Windows.Forms.Label lblPrimary;
        private System.Windows.Forms.Label lblSecondary;
        private System.Windows.Forms.Button btnBrowseSecondary;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tSQLScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logToolStripMenuItem;
        private System.Windows.Forms.Label lbVersion;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem userManualToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem videoToolStripMenuItem;
        private System.Windows.Forms.Button btnBrowsePrimary;
        private System.Windows.Forms.TextBox txtPrimaryFolder;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.CheckBox chkBox_UseLogScout;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsslStatus;
        private System.Windows.Forms.ToolStripStatusLabel tsslElapsed;
        private System.Windows.Forms.ToolStripStatusLabel tsslEvents;
    }
}

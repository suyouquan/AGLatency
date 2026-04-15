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
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            textBox2 = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            button3 = new System.Windows.Forms.Button();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tSQLScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            logToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            userManualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            videoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            lbVersion = new System.Windows.Forms.Label();
            button2 = new System.Windows.Forms.Button();
            textBox1 = new System.Windows.Forms.TextBox();
            button1 = new System.Windows.Forms.Button();
            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            chkBox_UseLogScout = new System.Windows.Forms.CheckBox();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label1.Location = new System.Drawing.Point(58, 142);
            label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(60, 25);
            label1.TabIndex = 4;
            label1.Text = "        ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label2.Location = new System.Drawing.Point(58, 298);
            label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(54, 25);
            label2.TabIndex = 5;
            label2.Text = "       ";
            // 
            // textBox2
            // 
            textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            textBox2.Location = new System.Drawing.Point(58, 247);
            textBox2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(842, 30);
            textBox2.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label3.Location = new System.Drawing.Point(58, 55);
            label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(91, 25);
            label3.TabIndex = 7;
            label3.Text = "Primary:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label4.Location = new System.Drawing.Point(58, 211);
            label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(121, 25);
            label4.TabIndex = 7;
            label4.Text = "Secondary:";
            // 
            // button3
            // 
            button3.Location = new System.Drawing.Point(921, 244);
            button3.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(99, 47);
            button3.TabIndex = 8;
            button3.Text = "Browse";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
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
            // button2
            // 
            button2.Location = new System.Drawing.Point(921, 94);
            button2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(99, 47);
            button2.TabIndex = 8;
            button2.Text = "Browse";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // textBox1
            // 
            textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            textBox1.Location = new System.Drawing.Point(58, 95);
            textBox1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(842, 30);
            textBox1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Image = Properties.Resources.green2;
            button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            button1.Location = new System.Drawing.Point(448, 400);
            button1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(168, 92);
            button1.TabIndex = 1;
            button1.Text = "Start";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
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
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1145, 523);
            Controls.Add(chkBox_UseLogScout);
            Controls.Add(lbVersion);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(textBox2);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(menuStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            Name = "Form1";
            Text = "AGLatency - AlwaysON AG Log Block Movement Latency Report Tool";
            FormClosing += Form1_FormClosing;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tSQLScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logToolStripMenuItem;
        private System.Windows.Forms.Label lbVersion;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem userManualToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem videoToolStripMenuItem;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.CheckBox chkBox_UseLogScout;
    }
}


namespace AGLatency
{
    partial class FormXMLFiles
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
            btnSecondaryXML = new System.Windows.Forms.Button();
            label6 = new System.Windows.Forms.Label();
            txtBxSecondaryXMLFile = new System.Windows.Forms.TextBox();
            btnPrimaryXML = new System.Windows.Forms.Button();
            label5 = new System.Windows.Forms.Label();
            txtBxPrimaryXMLFile = new System.Windows.Forms.TextBox();
            btnDone = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // btnSecondaryXML
            // 
            btnSecondaryXML.Location = new System.Drawing.Point(702, 203);
            btnSecondaryXML.Margin = new System.Windows.Forms.Padding(2);
            btnSecondaryXML.Name = "btnSecondaryXML";
            btnSecondaryXML.Size = new System.Drawing.Size(79, 38);
            btnSecondaryXML.TabIndex = 22;
            btnSecondaryXML.Text = "Browse";
            btnSecondaryXML.UseVisualStyleBackColor = true;
            btnSecondaryXML.Click += btnSecondaryXML_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label6.Location = new System.Drawing.Point(11, 177);
            label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(160, 20);
            label6.TabIndex = 21;
            label6.Text = "Secondary XML File:";
            label6.Click += label6_Click;
            // 
            // txtBxSecondaryXMLFile
            // 
            txtBxSecondaryXMLFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txtBxSecondaryXMLFile.Location = new System.Drawing.Point(11, 206);
            txtBxSecondaryXMLFile.Margin = new System.Windows.Forms.Padding(2);
            txtBxSecondaryXMLFile.Name = "txtBxSecondaryXMLFile";
            txtBxSecondaryXMLFile.Size = new System.Drawing.Size(674, 27);
            txtBxSecondaryXMLFile.TabIndex = 20;
            txtBxSecondaryXMLFile.TextChanged += txtBxSecondaryXMLFile_TextChanged;
            // 
            // btnPrimaryXML
            // 
            btnPrimaryXML.Location = new System.Drawing.Point(702, 137);
            btnPrimaryXML.Margin = new System.Windows.Forms.Padding(2);
            btnPrimaryXML.Name = "btnPrimaryXML";
            btnPrimaryXML.Size = new System.Drawing.Size(79, 38);
            btnPrimaryXML.TabIndex = 19;
            btnPrimaryXML.Text = "Browse";
            btnPrimaryXML.UseVisualStyleBackColor = true;
            btnPrimaryXML.Click += btnPrimaryXML_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label5.Location = new System.Drawing.Point(11, 113);
            label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(142, 20);
            label5.TabIndex = 18;
            label5.Text = "Primary XML File:";
            label5.Click += label5_Click;
            // 
            // txtBxPrimaryXMLFile
            // 
            txtBxPrimaryXMLFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txtBxPrimaryXMLFile.Location = new System.Drawing.Point(11, 141);
            txtBxPrimaryXMLFile.Margin = new System.Windows.Forms.Padding(2);
            txtBxPrimaryXMLFile.Name = "txtBxPrimaryXMLFile";
            txtBxPrimaryXMLFile.Size = new System.Drawing.Size(674, 27);
            txtBxPrimaryXMLFile.TabIndex = 17;
            txtBxPrimaryXMLFile.TextChanged += txtBxPrimaryXMLFile_TextChanged;
            // 
            // btnDone
            // 
            btnDone.Location = new System.Drawing.Point(257, 251);
            btnDone.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            btnDone.Name = "btnDone";
            btnDone.Size = new System.Drawing.Size(203, 102);
            btnDone.TabIndex = 23;
            btnDone.Text = "Done";
            btnDone.UseVisualStyleBackColor = true;
            btnDone.Click += btnDone_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            label1.Location = new System.Drawing.Point(11, 28);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(550, 32);
            label1.TabIndex = 24;
            label1.Text = "Cannot locate Primary and Secondary xml files";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            label3.Location = new System.Drawing.Point(12, 60);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(335, 32);
            label3.TabIndex = 26;
            label3.Text = "please select them manually";
            // 
            // FormXMLFiles
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 376);
            Controls.Add(label3);
            Controls.Add(label1);
            Controls.Add(btnDone);
            Controls.Add(btnSecondaryXML);
            Controls.Add(label6);
            Controls.Add(txtBxSecondaryXMLFile);
            Controls.Add(btnPrimaryXML);
            Controls.Add(label5);
            Controls.Add(txtBxPrimaryXMLFile);
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "FormXMLFiles";
            Text = "Get XML Files";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSecondaryXML;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtBxSecondaryXMLFile;
        private System.Windows.Forms.Button btnPrimaryXML;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtBxPrimaryXMLFile;
        private System.Windows.Forms.Button btnDone;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
    }
}
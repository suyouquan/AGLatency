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
            this.btnSecondaryXML = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtBxSecondaryXMLFile = new System.Windows.Forms.TextBox();
            this.btnPrimaryXML = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.txtBxPrimaryXMLFile = new System.Windows.Forms.TextBox();
            this.btnDone = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSecondaryXML
            // 
            this.btnSecondaryXML.Location = new System.Drawing.Point(706, 246);
            this.btnSecondaryXML.Margin = new System.Windows.Forms.Padding(2);
            this.btnSecondaryXML.Name = "btnSecondaryXML";
            this.btnSecondaryXML.Size = new System.Drawing.Size(79, 30);
            this.btnSecondaryXML.TabIndex = 22;
            this.btnSecondaryXML.Text = "Browse";
            this.btnSecondaryXML.UseVisualStyleBackColor = true;
            this.btnSecondaryXML.Click += new System.EventHandler(this.btnSecondaryXML_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(15, 226);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(160, 20);
            this.label6.TabIndex = 21;
            this.label6.Text = "Secondary XMLFile:";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // txtBxSecondaryXMLFile
            // 
            this.txtBxSecondaryXMLFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBxSecondaryXMLFile.Location = new System.Drawing.Point(15, 249);
            this.txtBxSecondaryXMLFile.Margin = new System.Windows.Forms.Padding(2);
            this.txtBxSecondaryXMLFile.Name = "txtBxSecondaryXMLFile";
            this.txtBxSecondaryXMLFile.Size = new System.Drawing.Size(674, 27);
            this.txtBxSecondaryXMLFile.TabIndex = 20;
            this.txtBxSecondaryXMLFile.TextChanged += new System.EventHandler(this.txtBxSecondaryXMLFile_TextChanged);
            // 
            // btnPrimaryXML
            // 
            this.btnPrimaryXML.Location = new System.Drawing.Point(706, 194);
            this.btnPrimaryXML.Margin = new System.Windows.Forms.Padding(2);
            this.btnPrimaryXML.Name = "btnPrimaryXML";
            this.btnPrimaryXML.Size = new System.Drawing.Size(79, 30);
            this.btnPrimaryXML.TabIndex = 19;
            this.btnPrimaryXML.Text = "Browse";
            this.btnPrimaryXML.UseVisualStyleBackColor = true;
            this.btnPrimaryXML.Click += new System.EventHandler(this.btnPrimaryXML_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(15, 174);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(142, 20);
            this.label5.TabIndex = 18;
            this.label5.Text = "Paimray XMLFile:";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // txtBxPrimaryXMLFile
            // 
            this.txtBxPrimaryXMLFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBxPrimaryXMLFile.Location = new System.Drawing.Point(15, 197);
            this.txtBxPrimaryXMLFile.Margin = new System.Windows.Forms.Padding(2);
            this.txtBxPrimaryXMLFile.Name = "txtBxPrimaryXMLFile";
            this.txtBxPrimaryXMLFile.Size = new System.Drawing.Size(674, 27);
            this.txtBxPrimaryXMLFile.TabIndex = 17;
            this.txtBxPrimaryXMLFile.TextChanged += new System.EventHandler(this.txtBxPrimaryXMLFile_TextChanged);
            // 
            // btnDone
            // 
            this.btnDone.Location = new System.Drawing.Point(266, 315);
            this.btnDone.Name = "btnDone";
            this.btnDone.Size = new System.Drawing.Size(203, 82);
            this.btnDone.TabIndex = 23;
            this.btnDone.Text = "Done";
            this.btnDone.UseVisualStyleBackColor = true;
            this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // FormXMLFiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnDone);
            this.Controls.Add(this.btnSecondaryXML);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtBxSecondaryXMLFile);
            this.Controls.Add(this.btnPrimaryXML);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtBxPrimaryXMLFile);
            this.Name = "FormXMLFiles";
            this.Text = "Get XML Files";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSecondaryXML;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtBxSecondaryXMLFile;
        private System.Windows.Forms.Button btnPrimaryXML;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtBxPrimaryXMLFile;
        private System.Windows.Forms.Button btnDone;
    }
}
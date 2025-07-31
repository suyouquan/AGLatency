using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AGLatency
{
    public partial class FormXMLFiles : Form
    {
        
        public FormXMLFiles()
        {
            InitializeComponent();
        }


        private string selectFile(string startFolder = "")
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            // Set filter to allow only XML files
            openFileDialog1.Filter = "XML files (*.xml)|*.xml";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "Select AG Topology XML file";

            if (string.IsNullOrEmpty(startFolder))
                openFileDialog1.InitialDirectory = System.Environment.SpecialFolder.MyComputer.ToString();
            else
            {
                if (Directory.Exists(startFolder)) openFileDialog1.InitialDirectory = startFolder;
                else openFileDialog1.InitialDirectory = System.Environment.SpecialFolder.MyComputer.ToString();
            }
            // Prevent navigating above the initial directory
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;
            openFileDialog1.Multiselect = false; // Allow only single file selection

            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                // the code here will be executed if the user presses Open in
                // the dialog.
                return Path.GetFileName(openFileDialog1.FileName);
            }
            return "";
        }

        private bool getXMLFileFromDialog(string sFolderName, string sFileName, out string selectedFile)
        {
            string folder = sFolderName;
            string file = Path.Combine(folder, sFileName);
            string fileName = sFileName;

            if (!Utility.isValidfolder(folder))
            {
                MessageBox.Show("Folder [" + folder + "] doesn't exist! \r\n Please select a valid folder first");
                selectedFile = "";
                return false;
            }


            if (Utility.isValidFile(file))
                fileName = selectFile(folder);
            else
            {
                if (Utility.isValidfolder(folder))
                    fileName = selectFile(folder);
                else fileName = selectFile();
            }

            if (!String.IsNullOrEmpty(fileName))
            {
                selectedFile = fileName;
                return true;
            }
            else
            {
                selectedFile = "";
                return false;

            }
        }
  

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void txtBxSecondaryXMLFile_TextChanged(object sender, EventArgs e)
        {
          
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void txtBxPrimaryXMLFile_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnPrimaryXML_Click(object sender, EventArgs e)
        {
            if (getXMLFileFromDialog(Controller.primaryFolder, "", out string selectedFile))
            {
                txtBxPrimaryXMLFile.Text = selectedFile;
                Controller.primaryXmlFile = selectedFile;
            }
        }

        private void btnSecondaryXML_Click(object sender, EventArgs e)
        {
            if (getXMLFileFromDialog(Controller.secondaryFolder, "", out string selectedFile))
            {
                txtBxSecondaryXMLFile.Text = selectedFile;
                Controller.secondaryXmlFile = selectedFile;
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            if (File.Exists(Path.Combine(Controller.primaryFolder, Controller.primaryXmlFile)) )
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select valid XML file for Primary Node");
            }
        }
    }
}

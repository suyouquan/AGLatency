using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;

using Microsoft.SqlServer.XEvent;
using Microsoft.SqlServer.XEvent.Linq;


namespace AGLatency
{
    public partial class Form1 : Form
    {
        static Control mylable1;
        static Control mylable2;
        static Control mylable3;
        Thread updateTD;
        public Form1()
        {
            InitializeComponent();

            Init();

            try
            {
                UpdateService myweb = new UpdateService(VersionUpdate);
                  updateTD = new Thread(myweb.VersionUpdate);

                updateTD.Start();
            }
            catch(Exception e)
            {
                Logger.LogException(e, Thread.CurrentThread);
            }

        }

        public void VersionUpdate(string versionStr)
        {
            //  MessageBox.Show(versionStr);
            if (this.InvokeRequired) lbVersion.BeginInvoke((MethodInvoker)delegate
            {
                lbVersion.Text = versionStr;
            });
        }


        public void Init()
        {
            lbVersion.Text = "";

            mylable1 = label1;
            mylable2 = label2;

            DoStop(false);

            // this.textBox1.Text = @"C:\AGLatency\data\PerfMon_AUSYDSQLC31N4\PerfMon\AlwaysOn_DataMove_Tracing_0_131751865495060000.xel";
            this.textBox1.Text = @"C:\AGLatency\data\PerfMon_AUSYDSQLC31N4\PerfMon\";
            this.textBox2.Text = @"C:\AGLatency\data\PerfMon_AUMELSQLR31N1\PerfMon";

            //  this.textBox1.Text = @"E:\xevent\Primary";
            //  this.textBox2.Text = @"E:\xevent\Third";

            //this.textBox1.Text = @"C:\AGLatency\data\AGXevent\Primary";
            //this.textBox2.Text = @"C:\AGLatency\data\AGXevent\SyncSecondary";


            this.textBox1.Text = @"C:\AGLatency\data\AGxevent2_slowLink\primary";
            this.textBox2.Text = @"C:\AGLatency\data\AGxevent2_slowLink\slow_secondary_sync";

                this.textBox1.Text = @"C:\AGLatency\data\AGXevent_linkSpeedChange\primary";
            this.textBox2.Text = @"C:\AGLatency\data\AGXevent_linkSpeedChange\async_slow_secondary";

        }

        //static readonly object _updateProgressLock = new object();
        //public static UInt64 total = 0;
        //public void UpdateProgress5(UInt64 rows)
        //{
        //    lock (_updateProgressLock)
        //    {
        //        total = total + rows;

        //        if (this.InvokeRequired) Form1.mylable.BeginInvoke((MethodInvoker)delegate
        //        {

        //                mylable.Text =  " Rows processed:"+total;



        //        });
        //    }
        //}




        static readonly object _updateProgressLock1 = new object();

        public void UpdateProgress1(string msg)
        {
            lock (_updateProgressLock1)
            {


                if (this.InvokeRequired) Form1.mylable1.BeginInvoke((MethodInvoker)delegate
                {

                    mylable1.Text = msg;



                });
            }
        }

        public void Done(string msg)
        {
           
                if (this.InvokeRequired) Form1.mylable1.BeginInvoke((MethodInvoker)delegate
                {

                    DoStop(false);



                });
             
        }

        static readonly object _updateProgressLock2 = new object();

        public void UpdateProgress2(string msg)
        {
            lock (_updateProgressLock2)
            {


                if (this.InvokeRequired) Form1.mylable1.BeginInvoke((MethodInvoker)delegate
                {

                    mylable2.Text = msg;



                });
            }
        }
        /*
         * 
         * */
        bool notStarted = true;
        bool isAbort = false;
        Thread mythread = null;
        Thread mythread2 = null;
        Thread td1 = null;
        Thread td2 = null;

        XELoader xel = null;

        XELoader xel2 = null;
        Latency.LogBlockLocalHarden primary_log_flush = null;
        Latency.LogBlockLocalHarden secondary_log_flush = null;
        Latency.NetworkLatency primary_secondary = null;
        Latency.LogCapturePrimary logCapturePrimary = null;
        Latency.SyncReceiveNetLatency syncReceiveNetLatency = null;
        Latency.DBFlowControl dbFlowControl = null;
        public void WaitUntilDone()
        {
            
            mythread2.Join();
            UpdateProgress2("Done.");
            mythread.Join();

            XELoader.CleanUp();

            if (isAbort)
            {
                isAbort = false;
                return;
            }

            UInt64 reads = xel.GetReads() + xel2.GetReads();
            UInt64 cnt = XELoader.GetAllCount();
            Logger.LogMessage("All Done, Total Reads:" + reads + " Total Committed:" + cnt);
            UpdateProgress1("All Done, Total Reads:" + reads + " Total Committed:" + cnt);
            UpdateProgress2("Done.Creating report...");
            CreateReport();
            //Now kick off network latency

            UpdateProgress2("Done.Report created.");

            string outputPath = PageTemplate.HtmlPageOutput.reportOutputFolder;

            string url = outputPath + "/report.html";// Path.Combine(outputPath, "data");

            UpdateProgress2("Done.Report created:" + url);
            Logger.LogMessage("Done.Report created." + url);

            Done("");

            System.Diagnostics.Process.Start(outputPath);
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = url;
            prc.Start();
            prc.Close();

            
        }

        public void CreateReport()
        {
            UpdateProgress2("Creating primary_log_flush page");
            Logger.LogMessage("Creating primary_log_flush page");

            primary_log_flush.CreatePages();
            UpdateProgress2("Creating secondary_log_flush page");
            Logger.LogMessage("Creating secondary_log_flush page");
            secondary_log_flush.CreatePages();
            UpdateProgress2("Creating primary_secondary page");
            Logger.LogMessage("Creating primary_secondary page");
            primary_secondary.CreatePages();
            UpdateProgress2("Creating logCapturePrimary page");
            Logger.LogMessage("Creating logCapturePrimary page");
            logCapturePrimary.CreatePages();

            UpdateProgress2("Creating syncReceiveNetLatency page");
            Logger.LogMessage("Creating syncReceiveNetLatency page");
            syncReceiveNetLatency.CreatePages();

            UpdateProgress2("Creating DBFlowControlPage page");
            Logger.LogMessage("Creating DBFlowControlPage page");
            dbFlowControl.CreatePages();


            UpdateProgress2("Creating Summary page");
            Logger.LogMessage("Creating Summary page");

            Pages.AGLatencySummaryPage sum = new Pages.AGLatencySummaryPage(Latency.NetworkLatency.replicaId, "Summary", "");
            sum.SavePageToDisk();

            //Now save other pages,this way, the summayr page will be the first page
            foreach (PageTemplate.PageObject pg in Controller.pageObjs)
            {
                pg.page.SavePageToDisk();
            }
        }
        private void Stop(bool flag)
        {
            isAbort = flag;


            if (updateTD != null)
            {
                try
                {
                    updateTD.Abort();
                    updateTD = null;
                }
                catch (Exception ex)
                {

                }
            }

            if (mythread!=null)
            {
                try
                {
                    mythread.Abort();
                    
                }
                catch(Exception ex)
                {

                }
            }
            if (mythread2 != null)
            {
                try
                {
                    mythread2.Abort();

                }
                catch (Exception ex)
                {

                }
            }

            if (td1 != null)
            {
                try
                {
                    td1.Abort();

                }
                catch (Exception ex)
                {

                }
            }

            if (td2 != null)
            {
                try
                {
                    td2.Abort();

                }
                catch (Exception ex)
                {

                }
            }


           

        }
        private void Start()
        {
            //primary_secondary.GetPerfPointData(@"C:\AGLatency\AGLatency\bin\Debug\Primary_To_Secondary_2__2018-08-02_21_47_45.791.SQLiteDB");

            //cleanup db, create db mapping
            XELoader.Reset();
            Controller.Reset();


            //delete files in the sqldb folder
            SQLiteDB.DeleteOldFile();

            string outputPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Report";


            //Set this path here, otherwise the output won't work

            PageTemplate.HtmlPageOutput.reportOutputFolder = Path.Combine(outputPath, System.DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss"));
            if (!Directory.Exists(PageTemplate.HtmlPageOutput.reportOutputFolder))
                Directory.CreateDirectory(PageTemplate.HtmlPageOutput.reportOutputFolder);

            Utility.CopyHtmlFiles(PageTemplate.HtmlPageOutput.reportOutputFolder);

            Logger.LogMessage("Output folder:" + PageTemplate.HtmlPageOutput.reportOutputFolder);

            primary_log_flush = new Latency.LogBlockLocalHarden(Replica.Primary);
            secondary_log_flush = new Latency.LogBlockLocalHarden(Replica.Secondary);

            primary_secondary = new Latency.NetworkLatency(NetworkDirection.Primary_To_Secondary);

            logCapturePrimary = new Latency.LogCapturePrimary();

            syncReceiveNetLatency = new Latency.SyncReceiveNetLatency(NetworkDirection.Secondary_To_Primary);
            dbFlowControl = new Latency.DBFlowControl();
            //Register to XELoader
            primary_log_flush.Register();
            secondary_log_flush.Register();
            primary_secondary.Register();
            logCapturePrimary.Register();
            syncReceiveNetLatency.Register();

            dbFlowControl.Register();

            //  Latency.LogBlockLocalHarden.GeneratePerfMonCSV(@"C:\AGLatency\AGLatency\bin\Debug\SQLiteDB\LocalHarden_Primary_perf.CSV"); 

            Logger.LogMessage("Start...");
            //SQLiteDB lite = new SQLiteDB();
            //lite.Init("test");
            //lite.Execute("create table highscores (name varchar(20), score int)");


            Logger.LogMessage("Primary:" + this.textBox1.Text);
            Logger.LogMessage("Secondary:" + this.textBox2.Text);

            xel = new XELoader(this.textBox1.Text, Replica.Primary, UpdateProgress1);


            mythread = new Thread(xel.Start);


            //  Thread.Sleep(1000);//wait for one second to avoid duplicate filename
            xel2 = new XELoader(this.textBox2.Text, Replica.Secondary, UpdateProgress2);



            td1 = new Thread(xel.GetTotalEventCount);
            td2 = new Thread(xel2.GetTotalEventCount);
            td1.Start();
            td2.Start();

            //start another thread otherwise it will block the main UI thread

            Thread proProcess = new Thread(PreProcessing);
            proProcess.Start();
        }

        private void DoStop(bool flag)
        {
            Stop(flag);
            notStarted = true;

            button1.Text = "   Start";
            button1.Image = Properties.Resources.green2;
            button1.ImageAlign = ContentAlignment.MiddleLeft;
            if (flag) //if aborted.
            {
                label1.Text = "";
                label2.Text = "";
            }
        }

        private bool isValidfolder(string path)
        {
            if (String.IsNullOrEmpty(path)) return false;
            if (Directory.Exists(path)) return true;
            else return false;

        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(!isValidfolder(textBox1.Text))
               
            {
               MessageBox.Show("Primary folder [" +textBox1.Text+"] doesn't exist!");
                return;

            }
            if (!isValidfolder(textBox2.Text))

            {
                MessageBox.Show("Secondary folder [" + textBox2.Text + "] doesn't exist!");
                return;

            }

            if(textBox1.Text.Trim()==textBox2.Text.Trim())
            {
                MessageBox.Show("You cannot set primary and secondary to the same folder.");
                return;


            }
            Controller.primaryFolder = textBox1.Text.Trim();
            Controller.secondaryFolder = textBox2.Text.Trim();

         

             button1.Enabled = false;

            if (notStarted)
            {
               

                button1.Text = "   Stop";
                button1.Image = Properties.Resources.red2;
                button1.ImageAlign = ContentAlignment.MiddleLeft;

                Start();
                notStarted = false;
                isAbort = false;
            }
            else
            {
                DoStop(true);
            }
 
            button1.Enabled = true;

        }

        public void PreProcessing()
        {
            td1.Join();
            td2.Join();

            if (isAbort) return;

            UInt64 totalCount = xel.eventCount + xel2.eventCount;
            Logger.LogMessage("Total events:" + totalCount);


            mythread.Start();
            mythread2 = new Thread(xel2.Start);
            mythread2.Start();

            Thread wait = new Thread(WaitUntilDone);
            wait.Start();

        }


        private string SelectFolder(string startFolder="")
        {
               FolderBrowserDialog folderBrowserDialog1=new FolderBrowserDialog();
            if (string.IsNullOrEmpty(startFolder))
                folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
            else
            {
                if(Directory.Exists(startFolder))    folderBrowserDialog1.SelectedPath = startFolder;
                else folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;

            }

            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                // the code here will be executed if the user presses Open in
                // the dialog.
                return folderBrowserDialog1.SelectedPath;
            }

            return "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = "";
            if (isValidfolder(textBox1.Text)) 
             path= SelectFolder(textBox1.Text);
            else path = SelectFolder();

            if (!String.IsNullOrEmpty(path)) this.textBox1.Text = path;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            string path = "";
            if (isValidfolder(textBox2.Text))
                path = SelectFolder(textBox2.Text);
            else
            {
                if (isValidfolder(textBox1.Text))
                    path = SelectFolder( textBox1.Text );
                else path = SelectFolder();
            }

            if (!String.IsNullOrEmpty(path)) this.textBox2.Text = path;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!notStarted)
            {
                DialogResult result = MessageBox.Show("Do you want to exit while there are still files beging processed? ", "Confirm to exit", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (result == DialogResult.OK)
                {
                    DoStop(true);
                    e.Cancel = false;  //点击OK


                }
                else
                {
                    e.Cancel = true;
                }
            }

        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Logger.LogFile)) return;
            var process = new   System.Diagnostics.Process();
            process.StartInfo.FileName = "notepad.exe";
            process.StartInfo.Arguments = Logger.LogFile;

            process.Start();
        }

        private void tSQLScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
          
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "notepad.exe";
            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var file = Path.Combine(path, "TSQL_XEvent.sql");
            process.StartInfo.Arguments = file;

            process.Start();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string msg = "AGLatency Report Tool. Version " + typeof(Program).Assembly.GetName().Version ;
            MessageBox.Show(msg, "About");
        }

        private void lbVersion_Click(object sender, EventArgs e)
        {
            string msg = "There is new version available in the web, please download it accordingly.";
            MessageBox.Show(msg, "Version Update");
        }
    }
}

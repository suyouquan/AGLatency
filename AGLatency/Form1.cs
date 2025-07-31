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
using System.Reflection;
using Microsoft.SqlServer.XEvent;
using Microsoft.SqlServer.XEvent.Linq;


namespace AGLatency
{
    public partial class Form1 : Form
    {
        static Control mylable1;
        static Control mylable2;
        static Control mylable3;
        private FormXMLFiles xmlFilesForm = new FormXMLFiles();

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
            catch (Exception e)
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

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Text = "AGLatency Report tool, Version " + version;
            lbVersion.Text = "";

            mylable1 = label1;
            mylable2 = label2;

            DoStop(false);

            // this.textBox1.Text = @"C:\AGLatency\data\PerfMon_AUSYDSQLC31N4\PerfMon\AlwaysOn_DataMove_Tracing_0_131751865495060000.xel";
           // this.textBox1.Text = @"C:\data\PerfMon_AUSYDSQLC31N4\PerfMon\";
           // this.textBox2.Text = @"C:\data\PerfMon_AUMELSQLR31N1\PerfMon";

            //  this.textBox1.Text = @"E:\xevent\Primary";
            //  this.textBox2.Text = @"E:\xevent\Third";

            //this.textBox1.Text = @"C:\data\AGXevent\Primary";
            //this.textBox2.Text = @"C:\data\AGXevent\SyncSecondary";


           // this.textBox1.Text = @"C:\data\AGxevent2_slowLink\primary";
          //  this.textBox2.Text = @"C:\data\AGxevent2_slowLink\slow_secondary_sync";
            /*
                     //   this.textBox1.Text = @"C:\data\AGXevent_linkSpeedChange\primary";
                     // this.textBox2.Text = @"C:\data\AGXevent_linkSpeedChange\async_slow_secondary";
                     */

            //  this.textBox1.Text = @"E:\xevent\9.7 AGLatency\CSNP00099B5A";
            //  this.textBox2.Text=@"E:\xevent\9.7 AGLatency\CSNP00099B59";
          //  this.textBox1.Text = @"C:\data\report testing\primary";
           // this.textBox2.Text = @"C:\data\report testing\sync secondary\";
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
        Latency.TranRemoteCommit tranRemoteCommit = null;
        Latency.TranProcessingTime tranProcessing = null;

        Latency.EventProcessingTemplate hadr_log_block_send_complete = null;
        Latency.EventProcessingTemplate hadr_db_commit_mgr_harden = null;
        Latency.EventProcessingTemplate recovery_unit_harden_log_timestamps = null;
        Latency.EventProcessingTemplate log_flush_complete = null;
        Latency.EventProcessingTemplate hadr_log_block_compression = null;
        Latency.EventProcessingTemplate hadr_log_block_decompression = null;
        Latency.EventProcessingTemplate hadr_receive_harden_lsn_message = null;
        Latency.EventProcessingTemplate hadr_transport_receive_log_block_message = null;
        Latency.EventProcessingTemplate hadr_database_flow_control_action = null;

        Latency.EventProcessingTemplate log_flush_complete_secondary = null;

        Latency.EventProcessingTemplate hadr_lsn_send_complete = null;
        Latency.EventProcessingTemplate hadr_lsn_send_complete2 = null;
        

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
            /*

            UpdateProgress2("Creating Transaction Processing page");
            Logger.LogMessage("Creating Transaction Processing page");
            tranProcessing.CreatePages();

            UpdateProgress2("Creating primary_log_flush page");
            Logger.LogMessage("Creating primary_log_flush page");
            primary_log_flush.CreatePages();

            UpdateProgress2("Creating Transaction Remote Commit page");
            Logger.LogMessage("Creating Transaction Remote Commit page");
            tranRemoteCommit.CreatePages();



            UpdateProgress2("Creating secondary_log_flush page");
            Logger.LogMessage("Creating secondary_log_flush page");
            secondary_log_flush.CreatePages();
            */
            /*
             * Don't get network latency becuase it is not accurate
            UpdateProgress2("Creating primary_secondary page");
            Logger.LogMessage("Creating primary_secondary page");
            primary_secondary.CreatePages();
            */

            /*
            UpdateProgress2("Creating logCapturePrimary page");
            Logger.LogMessage("Creating logCapturePrimary page");
            logCapturePrimary.CreatePages();
            */
            /*
             * * Don't get network latency becuase it is not accurate
            UpdateProgress2("Creating syncReceiveNetLatency page");
            Logger.LogMessage("Creating syncReceiveNetLatency page");
            syncReceiveNetLatency.CreatePages();

   
            UpdateProgress2("Creating DBFlowControlPage page");
            Logger.LogMessage("Creating DBFlowControlPage page");
            dbFlowControl.CreatePages();
 */
            /*************************/
            UpdateProgress2("Creating hadr_log_block_send_complete page");
            Logger.LogMessage("Creating hadr_log_block_send_complete page");
            var list = hadr_log_block_send_complete.GetPerfPointData();
            Pages.ProcessingTimePageTemplate sendPage = new Pages.ProcessingTimePageTemplate
                (list, "Send", "Primary Statistics", "Primary-Send",11);
            sendPage.GetData();
            sendPage.page.pageDescription = Annotation.GetExplain(LatencyEvent.Primary_Send);
            PageTemplate.PageObject pageObj = new PageTemplate.PageObject("SEND", sendPage, PageTemplate.PageObjState.SaveToDiskOnly,11);
            Controller.pageObjs.Add(pageObj);

            /*************************/


            /*************************/
            UpdateProgress2("Creating hadr_db_commit_mgr_harden page");
            Logger.LogMessage("Creating hadr_db_commit_mgr_harden page");
            var list2 = hadr_db_commit_mgr_harden.GetPerfPointData();
            Pages.ProcessingTimePageTemplate mgrPage = new Pages.ProcessingTimePageTemplate
                (list2, "Remote Harden", "Primary Statistics", "Primary-RemoteHarden",13);
            mgrPage.GetData();

            mgrPage.page.pageDescription = Annotation.GetExplain(LatencyEvent.Primary_RemoteHarden);
            mgrPage.page.pageSummary = "<br>Here is the xevent of <b>hadr_db_commit_mgr_harden:</b><br><br><img src='../images/hadr_db_commit_mgr_harden.png' height='300'/>";


            PageTemplate.PageObject pageObj2 = new PageTemplate.PageObject("RemoteHarden", mgrPage, PageTemplate.PageObjState.SaveToDiskOnly,13);
            Controller.pageObjs.Add(pageObj2);


            //Time to get uniqueu database IDs for latter use

           // Controller.databaseIds = hadr_db_commit_mgr_harden.GetDatabaseIDs(hadr_db_commit_mgr_harden.eventLatency.eventDB.SQLiteDBFile);

            /*************************/


            /*************************/
            UpdateProgress2("Creating log_flush_complete page");
            Logger.LogMessage("Creating log_flush_complete page");
            log_flush_complete.preprocessingQueries = new List<string>();
            if(Controller.primaryInfo!=null && Controller.primaryInfo.database_id!=null 
                && Controller.primaryInfo.database_id.Count>0)
            //if (Controller.databaseIds != null && Controller.databaseIds.Count > 0)
            {
                string dbstr = " (" + String.Join(", ", Controller.primaryInfo.database_id.ToArray()) + ")";
                string exclude_NonAG_db = "DELETE FROM log_flush_complete WHERE database_id NOT IN " + dbstr;
         
                log_flush_complete.preprocessingQueries.Add(exclude_NonAG_db);


            }

                //for log flush, need to *1000=microseconds
                string multiply1000 = "UPDATE log_flush_complete SET duration=duration*1000";
                log_flush_complete.preprocessingQueries.Add(multiply1000);

            var list4 = log_flush_complete.GetPerfPointData();
            Pages.ProcessingTimePageTemplate flushPage = new Pages.ProcessingTimePageTemplate
                (list4, "Local Flush", "Primary Statistics", "Primary-LocalFlush",14);
            flushPage.GetData();

            flushPage.page.pageDescription = Annotation.GetExplain(LatencyEvent.Primary_LocalFlush);

            PageTemplate.PageObject pageObj4 = new PageTemplate.PageObject("LocalFlush", flushPage, PageTemplate.PageObjState.SaveToDiskOnly,14);
            Controller.pageObjs.Add(pageObj4);

            /*************************/




            /*************************/
            UpdateProgress2("Creating recovery_unit_harden_log_timestamps page");
            Logger.LogMessage("Creating recovery_unit_harden_log_timestamps page");

            if (Controller.primaryInfo != null && Controller.primaryInfo.database_id != null
             && Controller.primaryInfo.database_id.Count > 0)
            //if (Controller.databaseIds != null && Controller.databaseIds.Count > 0)
            {
                string dbstr = " (" + String.Join(", ", Controller.primaryInfo.database_id.ToArray()) + ")";
                string exclude_NonAG_db = "DELETE FROM recovery_unit_harden_log_timestamps WHERE database_id NOT IN " + dbstr;
                recovery_unit_harden_log_timestamps.preprocessingQueries = new List<string>();
                recovery_unit_harden_log_timestamps.preprocessingQueries.Add(exclude_NonAG_db);
            }

            var list3 = recovery_unit_harden_log_timestamps.GetPerfPointData();
            Pages.ProcessingTimePageTemplate commitPage = new Pages.ProcessingTimePageTemplate
                (list3, "Commit", "Primary Statistics", "Primary-Commit",15);
            commitPage.GetData();

            
            commitPage.page.pageDescription =  Annotation.GetExplain(LatencyEvent.Primary_Commit);
            commitPage.page.pageSummary = "<br>Here is the xevent of <b>recovery_unit_harden_log_timestamps:</b><br><br><img src='../images/recovery_unit_harden_log_timestamps.png' height='300'/>";

            PageTemplate.PageObject pageObj3 = new PageTemplate.PageObject("Commit", commitPage, PageTemplate.PageObjState.SaveToDiskOnly,15);
            Controller.pageObjs.Add(pageObj3);

            /*************************/




            /*************************/
            UpdateProgress2("Creating hadr_log_block_compression page");
            Logger.LogMessage("Creating hadr_log_block_compression page");
            if (Controller.primaryInfo != null && Controller.primaryInfo.database_id != null
             && Controller.primaryInfo.database_id.Count > 0)
                //if (Controller.databaseIds != null && Controller.databaseIds.Count > 0)
            {
                string dbstr = " (" + String.Join(", ", Controller.primaryInfo.database_id.ToArray()) + ")";
                string exclude_NonAG_db = "DELETE FROM hadr_log_block_compression WHERE database_id NOT IN " + dbstr;
                hadr_log_block_compression.preprocessingQueries = new List<string>();
                hadr_log_block_compression.preprocessingQueries.Add(exclude_NonAG_db);
            }

            var list5 = hadr_log_block_compression.GetPerfPointData();
            Pages.ProcessingTimePageTemplate compressionPage = new Pages.ProcessingTimePageTemplate
                (list5, "Compression", "Primary Statistics", "Primary-Compression",10);
            compressionPage.GetData();

            compressionPage.page.pageDescription = Annotation.GetExplain(LatencyEvent.Primary_Compression);

            PageTemplate.PageObject pageObj5 = new PageTemplate.PageObject("primaryCompression", compressionPage, PageTemplate.PageObjState.SaveToDiskOnly,10);
            Controller.pageObjs.Add(pageObj5);

            /*************************/

            /*************************/
            UpdateProgress2("Creating hadr_receive_harden_lsn_message page");
            Logger.LogMessage("Creating hadr_receive_harden_lsn_message page");



            var list7 = hadr_receive_harden_lsn_message.GetPerfPointData();
            Pages.ProcessingTimePageTemplate recPage = new Pages.ProcessingTimePageTemplate
                (list7, "Receive", "Primary Statistics", "Primary-Receive",12);
            recPage.GetData();

            recPage.page.pageDescription = Annotation.GetExplain(LatencyEvent.Primary_Receive);
            PageTemplate.PageObject pageObj7 = new PageTemplate.PageObject("PrimaryReceive", recPage, PageTemplate.PageObjState.SaveToDiskOnly,12);
            Controller.pageObjs.Add(pageObj7);

            /*************************/

            /*************************/
            UpdateProgress2("Creating hadr_database_flow_control_action page");
            Logger.LogMessage("Creating hadr_database_flow_control_action page");

            hadr_database_flow_control_action.preprocessingQueries.Add("DELETE from hadr_database_flow_control_action where control_action='Set'");

            var list74 = hadr_database_flow_control_action.GetPerfPointData();
            Pages.ProcessingTimePageTemplate flowPage = new Pages.ProcessingTimePageTemplate
                (list74, "Flow Control", "Primary Statistics", "Primary-FlowControl", 19);//last section, so set it to 19
            flowPage.GetData();


            flowPage.page.pageDescription = Annotation.GetExplain(LatencyEvent.Primary_FlowControl);
            PageTemplate.PageObject pageObj74 = new PageTemplate.PageObject("PrimaryFlowControl", flowPage, PageTemplate.PageObjState.SaveToDiskOnly, 19);
            Controller.pageObjs.Add(pageObj74);

            /*************************/





            /*************************/
            UpdateProgress2("Creating hadr_log_block_decompression page");
            Logger.LogMessage("Creating hadr_log_block_decompression page");

            if (Controller.secondaryInfo != null && Controller.secondaryInfo.database_id != null
             && Controller.secondaryInfo.database_id.Count > 0)

            //if (Controller.databaseIds != null && Controller.databaseIds.Count > 0)
            {
                string dbstr = " (" + String.Join(", ", Controller.secondaryInfo.database_id.ToArray()) + ")";
                string exclude_NonAG_db = "DELETE FROM hadr_log_block_decompression WHERE database_id NOT IN " + dbstr;
                hadr_log_block_decompression.preprocessingQueries = new List<string>();
                hadr_log_block_decompression.preprocessingQueries.Add(exclude_NonAG_db);
            }

            var list6 = hadr_log_block_decompression.GetPerfPointData();
            Pages.ProcessingTimePageTemplate decompressionPage = new Pages.ProcessingTimePageTemplate
                (list6, "Decompression", "Secondary Statistics", "Secondary-Decompression",22);
            decompressionPage.GetData();

            decompressionPage.page.pageDescription = Annotation.GetExplain(LatencyEvent.Secondary_Decompression);

            PageTemplate.PageObject pageObj6 = new PageTemplate.PageObject("SecondaryDescompression", decompressionPage, PageTemplate.PageObjState.SaveToDiskOnly,22);
            Controller.pageObjs.Add(pageObj6);

            /*************************/

          


            /*************************/
            UpdateProgress2("Creating hadr_transport_receive_log_block_message page");
            Logger.LogMessage("Creating hadr_transport_receive_log_block_message page");

           

            var list8 = hadr_transport_receive_log_block_message.GetPerfPointData();
            Pages.ProcessingTimePageTemplate secReceivePage = new Pages.ProcessingTimePageTemplate
                (list8, "Receive", "Secondary Statistics", "Secondary-Receive",21);
            secReceivePage.GetData();

            secReceivePage.page.pageDescription = Annotation.GetExplain(LatencyEvent.Secondary_Receive);
            PageTemplate.PageObject pageObj8 = new PageTemplate.PageObject("SecondaryReceive", secReceivePage, PageTemplate.PageObjState.SaveToDiskOnly,21);
            Controller.pageObjs.Add(pageObj8);

            /*************************/



            /*************************/
            UpdateProgress2("Creating secondary log_flush_complete page");
            Logger.LogMessage("Creating secondary log_flush_complete page");
           log_flush_complete_secondary.preprocessingQueries = new List<string>();
            if (Controller.secondaryInfo != null && Controller.secondaryInfo.database_id != null
        && Controller.secondaryInfo.database_id.Count > 0)

            {
                string dbstr = " (" + String.Join(", ", Controller.secondaryInfo.database_id.ToArray()) + ")";
                string exclude_NonAG_db = "DELETE FROM log_flush_complete WHERE database_id NOT IN " + dbstr;
             
                log_flush_complete_secondary.preprocessingQueries.Add(exclude_NonAG_db);

                //for log flush, need to *1000=microseconds
               
            }
         multiply1000 = "UPDATE log_flush_complete SET duration=duration*1000";
                log_flush_complete_secondary.preprocessingQueries.Add(multiply1000);

            var list9 = log_flush_complete_secondary.GetPerfPointData();
            Pages.ProcessingTimePageTemplate flushSecPage = new Pages.ProcessingTimePageTemplate
                (list9, "Local Flush", "Secondary Statistics", "Secondary-LocalFlush",23);
            flushSecPage.GetData();

            flushSecPage.page.pageDescription = Annotation.GetExplain(LatencyEvent.Secondary_LocalFlush);  
            PageTemplate.PageObject pageObj9 = new PageTemplate.PageObject("LocalFlushSecondary", flushSecPage, PageTemplate.PageObjState.SaveToDiskOnly,23);
            Controller.pageObjs.Add(pageObj9);

            /*************************/



            /*************************/
            UpdateProgress2("Creating hadr_lsn_send_complete page");
            Logger.LogMessage("Creating hadr_lsn_send_complete page");



            var list10 = hadr_lsn_send_complete.GetPerfPointData();
            Pages.ProcessingTimePageTemplate lsnSendPage = new Pages.ProcessingTimePageTemplate
                (list10, "Send", "Secondary Statistics", "Secondary-Send",24);
            lsnSendPage.GetData();

            lsnSendPage.page.pageDescription = Annotation.GetExplain(LatencyEvent.Secondary_Send);
            PageTemplate.PageObject pageObj10 = new PageTemplate.PageObject("SecondarySend", lsnSendPage, PageTemplate.PageObjState.SaveToDiskOnly,24);
            Controller.pageObjs.Add(pageObj10);

            /*************************/


            /*************************/
            UpdateProgress2("Creating hadr_lsn_send_complete2 page");
            Logger.LogMessage("Creating hadr_lsn_send_complete2 page");



            var list11 = hadr_lsn_send_complete2.GetPerfPointData();
            Pages.ProcessingTimePageTemplate lsnSendPage2 = new Pages.ProcessingTimePageTemplate
                (list11, "Processing", "Secondary Statistics", "Secondary-Processing",25);
            lsnSendPage2.GetData();
            lsnSendPage2.page.pageDescription = Annotation.GetExplain(LatencyEvent.Secondary_Processing);
            lsnSendPage2.page.pageSummary = "<br>Here is the xevent of <b>hadr_lsn_send_complete:</b><br><br><img src='../images/hadr_lsn_send_complete.png' height='300'/>";

            PageTemplate.PageObject pageObj11 = new PageTemplate.PageObject("SecondaryProcessing", lsnSendPage2, PageTemplate.PageObjState.SaveToDiskOnly,25);
            Controller.pageObjs.Add(pageObj11);

            /*************************/



            UpdateProgress2("Creating Summary page");
            Logger.LogMessage("Creating Summary page");

            Pages.AGLatencySummaryPage sum = new Pages.AGLatencySummaryPage(Latency.NetworkLatency.replicaId, "Summary", "");
            sum.SavePageToDisk();


            Controller.pageObjs = Controller.pageObjs.OrderBy(p => p.outputOrder).ToList();
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

            if (mythread != null)
            {
                try
                {
                    mythread.Abort();

                }
                catch (Exception ex)
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


            /*
            primary_log_flush = new Latency.LogBlockLocalHarden(Replica.Primary);
            secondary_log_flush = new Latency.LogBlockLocalHarden(Replica.Secondary);

            //Ignore it now
            //primary_secondary = new Latency.NetworkLatency(NetworkDirection.Primary_To_Secondary);

            logCapturePrimary = new Latency.LogCapturePrimary();

            //Ignore it now
            //syncReceiveNetLatency = new Latency.SyncReceiveNetLatency(NetworkDirection.Secondary_To_Primary);
            */
           // dbFlowControl = new Latency.DBFlowControl();
            /*
            tranRemoteCommit = new Latency.TranRemoteCommit();
            tranProcessing = new Latency.TranProcessingTime();
            //Register to XELoader
            primary_log_flush.Register();
            secondary_log_flush.Register();

            //Ignore it now
            //primary_secondary.Register();
            //syncReceiveNetLatency.Register();

            logCapturePrimary.Register();

            tranRemoteCommit.Register();
            tranProcessing.Register();

            dbFlowControl.Register();
   */         
            //  Latency.LogBlockLocalHarden.GeneratePerfMonCSV(@"C:\AGLatency\AGLatency\bin\Debug\SQLiteDB\LocalHarden_Primary_perf.CSV"); 
            bool IsPrimary = true;

            hadr_log_block_send_complete
                = new Latency.EventProcessingTemplate(IsPrimary, "total_processing_time",
                EventMetaData.xEvent.hadr_log_block_send_complete);


            hadr_db_commit_mgr_harden
                = new Latency.EventProcessingTemplate(IsPrimary, "time_to_commit",
                EventMetaData.xEvent.hadr_db_commit_mgr_harden);


            recovery_unit_harden_log_timestamps
                = new Latency.EventProcessingTemplate(IsPrimary, "processing_time",
                EventMetaData.xEvent.recovery_unit_harden_log_timestamps);

            log_flush_complete
                = new Latency.EventProcessingTemplate(IsPrimary, "duration",
                EventMetaData.xEvent.log_flush_complete);

            log_flush_complete_secondary
              = new Latency.EventProcessingTemplate(!IsPrimary, "duration",
              EventMetaData.xEvent.log_flush_complete);


            hadr_log_block_compression
             = new Latency.EventProcessingTemplate(IsPrimary, "processing_time",
             EventMetaData.xEvent.hadr_log_block_compression);


         

            hadr_receive_harden_lsn_message
               = new Latency.EventProcessingTemplate(IsPrimary, "processing_time",
               EventMetaData.xEvent.hadr_receive_harden_lsn_message,2);

            hadr_database_flow_control_action
               = new Latency.EventProcessingTemplate(IsPrimary, "duration",
               EventMetaData.xEvent.hadr_database_flow_control_action);

             


            hadr_log_block_decompression
             = new Latency.EventProcessingTemplate(!IsPrimary, "processing_time",
             EventMetaData.xEvent.hadr_log_block_decompression);

            hadr_transport_receive_log_block_message
                  = new Latency.EventProcessingTemplate(!IsPrimary, "processing_time",
               EventMetaData.xEvent.hadr_transport_receive_log_block_message, 2);


            hadr_lsn_send_complete  
                  = new Latency.EventProcessingTemplate(!IsPrimary, "total_sending_time",
               EventMetaData.xEvent.hadr_lsn_send_complete);


            hadr_lsn_send_complete2
              = new Latency.EventProcessingTemplate(!IsPrimary, "total_processing_time",
           EventMetaData.xEvent.hadr_lsn_send_complete);


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


        private void button1_Click(object sender, EventArgs e)
        {
            if (!Utility.isValidfolder(textBox1.Text))

            {
                MessageBox.Show("Primary folder [" + textBox1.Text + "] doesn't exist!");
                return;

            }
            if (!Utility.isValidfolder(textBox2.Text))

            {
                MessageBox.Show("Secondary folder [" + textBox2.Text + "] doesn't exist!");
                return;

            }

            if (textBox1.Text.Trim() == textBox2.Text.Trim())
            {
                MessageBox.Show("You cannot set primary and secondary to the same folder.");
                return;


            }

          
            Controller.primaryFolder = textBox1.Text.Trim();
            Controller.secondaryFolder = textBox2.Text.Trim();
            
            if (String.IsNullOrEmpty(Controller.primaryXmlFile))
            {
                xmlFilesForm.ShowDialog();
            }
            //Controller.primaryXmlFile = txtBxPrimaryXMLFile.Text.Trim();
            //Controller.secondaryXmlFile = txtBxSecondaryXMLFile.Text.Trim();

            //check to see if primary.xml and secondary.xml exists or not:

            if (!File.Exists(Path.Combine(Controller.primaryFolder,Controller.primaryXmlFile)))
            {
                MessageBox.Show("primary.xml not found in ["+Controller.primaryFolder+"]");
                return;

            }

         

            //Now time to load AGinfo
            Controller.primaryInfo = null;
            Controller.secondaryInfo = null;
            Logger.LogMessage("Parsing primary.xml and secondary.xml...");
            Controller.primaryInfo=AGInfo.LoadAGInfo(Path.Combine(Controller.primaryFolder, Controller.primaryXmlFile));
            Controller.secondaryInfo = AGInfo.LoadAGInfo(Path.Combine(Controller.secondaryFolder, Controller.secondaryXmlFile));



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

        private string selectFile (string startFolder="")
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

        private string SelectFolder(string startFolder = "")
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (string.IsNullOrEmpty(startFolder))
                folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
            else
            {
                if (Directory.Exists(startFolder)) folderBrowserDialog1.SelectedPath = startFolder;
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

        private bool TryFindTopologyXml(string folderPath, out string fileName)
        {
            fileName = null;
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                return false;

            // Search for "primary.xml" and "*GetAGTopology.xml" in the folder (not recursive)
            var files = Directory.GetFiles(folderPath, "primary.xml", SearchOption.TopDirectoryOnly)
                .Concat(Directory.GetFiles(folderPath, "*GetAGTopology.xml", SearchOption.TopDirectoryOnly))
                .Concat(Directory.GetFiles(folderPath, "secondary.xml", SearchOption.TopDirectoryOnly))
                .ToList();

            if (files.Count == 1)
            {
                fileName = Path.GetFileName(files[0]);
                return true;
            }
            // If more than one or none found, return false
            return false;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            string path = "";
            if (Utility.isValidfolder(textBox1.Text))
                path = SelectFolder(textBox1.Text);
            else path = SelectFolder();

            if (!String.IsNullOrEmpty(path))
            {
                string fileName = "";
                this.textBox1.Text = path;
                if (TryFindTopologyXml(path, out fileName))
                {
                    Controller.primaryXmlFile = fileName;
                } else
                {
                    Controller.primaryXmlFile = string.Empty; 
                }    
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {

            string path = "";
            if (Utility.isValidfolder(textBox2.Text))
                path = SelectFolder(textBox2.Text);
            else
            {
                if (Utility.isValidfolder(textBox1.Text))
                    path = SelectFolder(textBox1.Text);
                else path = SelectFolder();
            }

            if (!String.IsNullOrEmpty(path))
            {
                string fileName = "";
                this.textBox2.Text = path;
                if (TryFindTopologyXml(path, out fileName))
                {
                    Controller.secondaryXmlFile = fileName;
                } else
                {
                    Controller.secondaryXmlFile = string.Empty;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!notStarted)
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
            var process = new System.Diagnostics.Process();
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
            string msg = "AGLatency Report Tool. Version " + typeof(Program).Assembly.GetName().Version + "\nhttps://github.com/suyouquan/AGLatency";
            MessageBox.Show(msg, "About");
        }

        private void lbVersion_Click(object sender, EventArgs e)
        {
            string msg = "There is new version available in the web, please download it accordingly.\nhttps://github.com/suyouquan/AGLatency";
            MessageBox.Show(msg, "Version Update");
        }

        

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            string msg = "AGLatency Report Tool. Version " + typeof(Program).Assembly.GetName().Version + "\nhttps://github.com/suyouquan/AGLatency";
            MessageBox.Show(msg, "About");
        }

        private void userManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var file = Path.Combine(path, "AGLatencyTool_UserManual_V2.pdf");
                System.Diagnostics.Process.Start(file);
            }
            catch(Exception ex )
            {
                Logger.LogException(ex, Thread.CurrentThread);
            }
        }

        private void videoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var file = Path.Combine(path, "AGLatency_HowTo.mp4");
                System.Diagnostics.Process.Start(file);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, Thread.CurrentThread);
            }
        }
    }
}

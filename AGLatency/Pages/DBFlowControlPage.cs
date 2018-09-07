using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGLatency.Pages
{
   public class DBFlowControlPage : PageTemplate.PageDataCommon
    {
         
        public PageTemplate.HtmlPageOutput page = null;
        public string replica;
        string title;
        public string dbReplica;
        List<Latency.FlowControl_Sec> list = null;
        //can have multile instance with diffferent menuTitle.
        public DBFlowControlPage(string replicaStr, List<Latency.FlowControl_Sec> l,string menuTitle,string groupTitle,string DBReplica)
        {
            title = menuTitle;
            this.replica  = replicaStr;
            string db = DBReplica.Substring(0, 8);
            dbReplica = DBReplica;
            this.page = new PageTemplate.HtmlPageOutput("DB ("+db+"...)", groupTitle );
            InitHtmlPage();
            list = l;
          }

        //this function will be called by SQLDumpData class. you implement your logic in it, say, call PopulateItems to fill items.
        public override bool GetData()
        {
            try
            {
                PopulateItems();
            }
            catch (Exception ex)
            {
                Logger.LogMessage(this.page.MenuTitle + " Execute:" + ex.Message);
            }
            return true;
        }

        public string GetChart()
        {

            Dictionary<string, string> summary = new Dictionary<string, string>();
            Int64 total = 0;
            Int64 duration=0;

            foreach (Latency.FlowControl_Sec lb in list)
            {
                total += lb.Occurence;
                duration += lb.Sum_Duration;
            }
            summary.Add("Total Flow Control Occurence", total.ToString());
            summary.Add("Avg Flow Control Duration", ((1.0*duration)/total ).ToString("F") +" ms");
            string sumHtml = OutputProcessor.ConvertDictionaryToHTMLTable(summary);

            Controller.latencySummaryDict.Add("DB Flow Control (db="+ dbReplica.Substring(0, 8)+")", (int)(duration / total));

            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            /*Date.UTC(1970, 10, 25,10,20,55), 1*/
            List<string> avg_Duration = new List<string>();
            foreach (Latency.FlowControl_Sec lb in list)
            {
                string str = "Date.UTC("
                      + lb.EventTimeStamp.Year.ToString() + "," + lb.EventTimeStamp.Month.ToString() + ","
                      + lb.EventTimeStamp.Day.ToString() + "," + lb.EventTimeStamp.Hour.ToString()
                      + "," + lb.EventTimeStamp.Minute.ToString()
                      + "," + lb.EventTimeStamp.Second.ToString()
                      + ")," + lb.Avg_Duration;
                avg_Duration.Add(str);
            }
            dict.Add("Avg Duration", avg_Duration);
            Controller.AddChartData("FlowControl_Avg_Duration", avg_Duration);

           Dictionary<string, List<string>> dict2 = new Dictionary<string, List<string>>();
            List<string> occurences = new List<string>();
            foreach (Latency.FlowControl_Sec lb in list)
            {
                string str = "Date.UTC("
                      + lb.EventTimeStamp.Year.ToString() + "," + lb.EventTimeStamp.Month.ToString() + ","
                      + lb.EventTimeStamp.Day.ToString() + "," + lb.EventTimeStamp.Hour.ToString()
                      + "," + lb.EventTimeStamp.Minute.ToString()
                      + "," + lb.EventTimeStamp.Second.ToString()
                      + ")," + lb.Occurence;
                occurences.Add(str);
            }
            dict2.Add("Occurence", occurences);


            string chartHtml = Output.HighCharts.GetChartHtml("Primary - Database Flow Control Avg Duration Per Second", title,
                "Time", "Duration (ms)", dict, "#4da6ff");

            string chartHtml2 = Output.HighCharts.GetChartHtml("Database Flow Control Occurence Sum Per Second", title,
             "Time", "Occurences", dict2, "#408000");

            return sumHtml+"<br>"+ chartHtml + "<br>"+chartHtml2;
        }


        //Must implement this so your page will be in the report.
        public override bool SavePageToDisk()
        {

         
            //if (page.GetCount() == 0)
            //{
            //    Logger.LogMessage(page.MenuTitle + ":page data row count is 0, won't add it to report.");
            //}
            //else
                this.page.SavePageToDisk();
            return true;
        }

        //control how your page looks like.
        public void InitHtmlPage()
        {

            // this.page.Group = "LocalHarden";
            this.page.pageTitle = "Primary-Database Flow Control <br> Replica:" + replica + "<br>Database:" + dbReplica;


            this.page.pageDescription = "This report shows the database flow control information occured to replica (" + replica + ")";
                 

            


            //   this.page.PagingCount = -1;


        }


        public void PopulateItems()
        {
            /*
            Scheduler.GetSchedulers();
       
            string sum = Scheduler.GetTableSum(Scheduler.items);
        

            PageTemplate.DataTable dt = new PageTemplate.DataTable(Scheduler.items);
            dt.tableSummary = sum;
            if (Scheduler.items.Count < 5) dt.columnFilterEnabled = false;
            this.page.AddDataTable(dt);

            this.page.pageContent = dt.GetHtmlStr();

            this.page.pageTitle = "sys_dm_os_schedulers" + " (" + Scheduler.items.Count + ")";
            this.page.MenuTitle = this.page.MenuTitle + " (" + Scheduler.items.Count + ")";

    */

//            this.page.pageDescription =
                
            PageTemplate.DataTable dt = new PageTemplate.DataTable(list);
            dt.tableDescription= GetChart();

            //  dt.tableSummary = sum;
            if (list.Count < 5) dt.columnFilterEnabled = false;
            this.page.AddDataTable(dt);

            this.page.pageContent = dt.GetHtmlStr();

           


        }





    }
}

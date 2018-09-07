using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGLatency.Pages
{
   public class NetworkLatencyPage : PageTemplate.PageDataCommon
    {
         
        public PageTemplate.HtmlPageOutput page = null;
        public string replica;
        string title;
        
        List<Latency.LogBlockNetLatency_Sec> list = null;
        //can have multile instance with diffferent menuTitle.
        public NetworkLatencyPage(string replicaStr, List<Latency.LogBlockNetLatency_Sec> l,string menuTitle,string groupTitle )
        {
            title = menuTitle;
            this.replica  = replicaStr;
            string grpTitle = "Network";
            string mTitle = "Primary->Secondary";
            this.page = new PageTemplate.HtmlPageOutput(mTitle, grpTitle );
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

            foreach (Latency.LogBlockNetLatency_Sec lb in list)
            {
                total += lb.LogBlocks;
                duration += lb.Sum_Latency;
            }
            summary.Add("Total Log Blocks Processed", total.ToString());
            summary.Add("Log Blocks Avg Latency", ((1.0*duration) / total).ToString("F") +" ms");
            string sumHtml = OutputProcessor.ConvertDictionaryToHTMLTable(summary);

            Controller.latencySummaryDict.Add("Primary->Secondary Net Latency", (int)(duration / total));

                Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            /*Date.UTC(1970, 10, 25,10,20,55), 1*/
            List<string> Avg_Latency = new List<string>();
            foreach (Latency.LogBlockNetLatency_Sec lb in list)
            {
                string str = "Date.UTC("
                      + lb.EventTimeStamp.Year.ToString() + "," + lb.EventTimeStamp.Month.ToString() + ","
                      + lb.EventTimeStamp.Day.ToString() + "," + lb.EventTimeStamp.Hour.ToString()
                      + "," + lb.EventTimeStamp.Minute.ToString()
                      + "," + lb.EventTimeStamp.Second.ToString()
                      + ")," + lb.Avg_Latency;
                Avg_Latency.Add(str);
            }
            dict.Add("Avg_Latency", Avg_Latency);
            Controller.AddChartData("Primary->Secondary", Avg_Latency);

            Dictionary<string, List<string>> dict2 = new Dictionary<string, List<string>>();
            List<string> logBlocks = new List<string>();
            foreach (Latency.LogBlockNetLatency_Sec lb in list)
            {
                string str = "Date.UTC("
                      + lb.EventTimeStamp.Year.ToString() + "," + lb.EventTimeStamp.Month.ToString() + ","
                      + lb.EventTimeStamp.Day.ToString() + "," + lb.EventTimeStamp.Hour.ToString()
                      + "," + lb.EventTimeStamp.Minute.ToString()
                      + "," + lb.EventTimeStamp.Second.ToString()
                      + ")," + lb.LogBlocks;
                logBlocks.Add(str);
            }
            dict2.Add("LogBlocks", logBlocks);


            string chartHtml = Output.HighCharts.GetChartHtml("Log Block Network Avg Latency Per Second", title,
                "Time", "Latency (ms)", dict, "#800000");

            string chartHtml2 = Output.HighCharts.GetChartHtml(" Log Blocks Sum Per Second", title,
             "Time", "Log Blocks", dict2, "#408000");

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
            this.page.pageTitle = "Log Blocks Network Latency<br>Primary -> Replica " + replica;


            this.page.pageDescription = "Log block network latency includes below duration"
                + "<br>1. The time of sending log blocks to the UCS component on primary."
                + "<br>2. The time of transfering the log blocks over the wire."
                + "<br>3. The time of log blocks arriving to the UCS component on secondary."
                + "<br>4. The time of receiving the log block from UCS component on secondary.";





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

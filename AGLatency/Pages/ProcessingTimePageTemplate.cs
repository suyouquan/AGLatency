using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGLatency.Pages
{
   public class ProcessingTimePageTemplate: PageTemplate.PageDataCommon
    {
         
        public PageTemplate.HtmlPageOutput page = null;
         
        string title;
        
        List<Latency.EventRecord_Sec> list = null;
        string chartName = "";
        //can have multile instance with diffferent menuTitle.
        public ProcessingTimePageTemplate(  List<Latency.EventRecord_Sec> l,string menuTitle,string groupTitle,string chart)
        {
            title = menuTitle;
           
            this.page = new PageTemplate.HtmlPageOutput(menuTitle, groupTitle );
            InitHtmlPage();
            list = l;
            chartName = chart;
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
            Int64 total_processingTime=0;
            Int64 max_processingTime = 0;

            foreach (Latency.EventRecord_Sec lb in list)
            {
                total += lb.Count;
                total_processingTime += lb.Sum_ProcessingTime;
                if (max_processingTime < lb.Max_ProcessingTime) max_processingTime = lb.Max_ProcessingTime;
            }
            summary.Add("Events Processed", total.ToString());
            summary.Add("Avg Processing Time", ((total_processingTime) / total)  +" microseconds");
            summary.Add("Max Processing Time",  max_processingTime + " microseconds");
            string sumHtml = OutputProcessor.ConvertDictionaryToHTMLTable(summary);

            
                Controller.latencySummaryDict.Add(chartName+"->Avg Processing Time", (int)((total_processingTime) / total));
            
            
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            /*Date.UTC(1970, 10, 25,10,20,55), 1*/
            List<string> Avg_Duration = new List<string>();
            foreach (Latency.EventRecord_Sec lb in list)
            {
                string str = "Date.UTC("
                      + lb.EventTimeStamp.Year.ToString() + "," + lb.EventTimeStamp.Month.ToString() + ","
                      + lb.EventTimeStamp.Day.ToString() + "," + lb.EventTimeStamp.Hour.ToString()
                      + "," + lb.EventTimeStamp.Minute.ToString()
                      + "," + lb.EventTimeStamp.Second.ToString()
                      + ")," + lb.Avg_ProcessingTime;
                Avg_Duration.Add(str);
            }
            dict.Add(chartName + "->Avg_ProcessingTime", Avg_Duration);

           
                Controller.AddChartData(chartName + "->Avg Processing time", Avg_Duration);
             
            Dictionary<string, List<string>> dict2 = new Dictionary<string, List<string>>();
            List<string> trans = new List<string>();
            foreach (Latency.EventRecord_Sec lb in list)
            {
                string str = "Date.UTC("
                      + lb.EventTimeStamp.Year.ToString() + "," + lb.EventTimeStamp.Month.ToString() + ","
                      + lb.EventTimeStamp.Day.ToString() + "," + lb.EventTimeStamp.Hour.ToString()
                      + "," + lb.EventTimeStamp.Minute.ToString()
                      + "," + lb.EventTimeStamp.Second.ToString()
                      + ")," + lb.Count;
                trans.Add(str);
            }
            dict2.Add(chartName + "->Count/Sec", trans);


            string chartHtml = Output.HighCharts.GetChartHtml(chartName + "->Avg ProcessingTime Per Second", title +" (Microseconds)",
                "Time", "Duration (Microseconds)", dict, "#005c99");

            string chartHtml2 = Output.HighCharts.GetChartHtml(chartName + "->Count Per Second", title,
             "Time", "Count", dict2, "#408000");

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
            this.page.pageTitle = title +" Statistics" ;


           // this.page.pageDescription = "The report data is from xevent recovery_unit_harden_log_timestamps. This xevent will fire for every transaction. In this event there is processing_time field which is used to track the duration of the commit of a transaction. This includes the wait of Local commit time or Remote commit delay.";



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

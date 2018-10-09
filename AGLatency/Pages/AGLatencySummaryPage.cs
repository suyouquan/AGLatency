using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGLatency.Pages
{
   public class AGLatencySummaryPage : PageTemplate.PageDataCommon
    {
         
        public PageTemplate.HtmlPageOutput page = null;
        public string replica;
        string title;
        public string dbReplica;
        List<Latency.FlowControl_Sec> list = null;
        //can have multile instance with diffferent menuTitle.
        public AGLatencySummaryPage(string replicaId,string menuTitle,string groupTitle)
        {
            title = menuTitle;
            replica = replicaId;
            this.page = new PageTemplate.HtmlPageOutput("Summary", "AG Latency" );
            InitHtmlPage();
            
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


            Dictionary<string, int> good = Controller.latencySummaryDict;
            
            //Ignore the sort

            //good = good.OrderByDescending(p => p.Value).ToDictionary(x=>x.Key,x=>x.Value);

            string dictChart = OutputProcessor.ConvertDictionaryToHTMLTable(good);

           // string barChartHtmlraw = OutputProcessor.CreateBarHtmlForDictionary(good, "barChartWaitType", 900, 300, null, false, "false");
           // string barChartHtml = OutputProcessor.CreateBarHtmlForDictionary(good, "barChartWaitType", 900, 300, null, true, "false");

            //  return dictChart+ "<br>"+ barChartHtml;

            string barJs = Output.HighCharts.GetBarHtml("SummaryBar", "Avg Latency Summary", "", "", "Time (ms)", good);

            string chartHtml = Output.HighCharts.GetChartHtml("Latency charts", "",
              "Time", "Latency (ms)", Controller.chartsData, "RED",600);

            string AGLatencyImg = "<br><br><hr><span style='color:goldenrod; font-size:150%;'><i class='fa fa-lightbulb-o'></i></span>&nbsp;<b>Explanation</b><br>Below figure shows how the latency is caculated. <br><img src='../images/AGLatency.png' height='600'/>";

            return barJs+   "<br>" + chartHtml+AGLatencyImg;
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
            this.page.pageTitle = "AG Latency Summary";

            string chart = GetChart();
            this.page.pageDescription = "This is the summary of the latency information between primary and replica "+replica+".";
            this.page.pageDescription += "<br><br>  Primary folder : " + Controller.primaryFolder.Replace("\\","/");
            this.page.pageDescription += "<br>Secondary folder : " + Controller.secondaryFolder.Replace("\\", "/"); ;
            this.page.pageDescription += "<br><br>"+chart;

            


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
          //  dt.tableDescription= GetChart();

            //  dt.tableSummary = sum;
            if (list.Count < 5) dt.columnFilterEnabled = false;
            this.page.AddDataTable(dt);

            this.page.pageContent = dt.GetHtmlStr();

           


        }





    }
}

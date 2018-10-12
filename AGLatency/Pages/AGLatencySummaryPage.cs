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


            Dictionary<string, int> good =new Dictionary<string, int>() ;
            Dictionary<string, List<string>> curlve = new Dictionary<string, List<string>>();

            var sum = Controller.latencySummaryDict_new.OrderBy(p => p.Key);
            int i = 1;
            foreach (var p in sum)
            {
                good.Add(i.ToString()+" "+p.Value.Key, p.Value.Value);
                i++;
            }

            var charts = Controller.chartsData_new.OrderBy(p => p.Key);
              i = 1;
            foreach(var p in charts)
            {
                curlve.Add(i.ToString()+" "+p.Value.Key, p.Value.Value);
                i++;
            }
            
            //Ignore the sort

            //good = good.OrderByDescending(p => p.Value).ToDictionary(x=>x.Key,x=>x.Value);

            string dictChart = OutputProcessor.ConvertDictionaryToHTMLTable(good);

           // string barChartHtmlraw = OutputProcessor.CreateBarHtmlForDictionary(good, "barChartWaitType", 900, 300, null, false, "false");
           // string barChartHtml = OutputProcessor.CreateBarHtmlForDictionary(good, "barChartWaitType", 900, 300, null, true, "false");

            //  return dictChart+ "<br>"+ barChartHtml;

            string barJs = Output.HighCharts.GetBarHtml("SummaryBar", "Avg Latency Summary", "", "", "Time (Microsecond)", good);

            string chartHtml = Output.HighCharts.GetChartHtml("Latency charts-Avg Processing Time", "",
              "Time", "Latency (Microsecond)", curlve, "RED",600);

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
            this.page.pageDescription = "This is the summary of the latency information between replicas.";
            this.page.pageDescription += "<br><br>  Primary folder : " + Controller.primaryFolder.Replace("\\","/");
            this.page.pageDescription += "<br>Secondary folder : " + Controller.secondaryFolder.Replace("\\", "/"); ;
         

            if (Controller.primaryInfo != null && Controller.primaryInfo.nodes != null && Controller.primaryInfo.nodes.Count > 0)
            {

                string AGInfo= OutputProcessor.Convert2HTMLTable<ReplicaNode>(Controller.primaryInfo.nodes, true, false);

                //  dt.tableDescription= GetChart();
                this.page.pageDescription +="<br><br>"+ AGInfo;
            }

            this.page.pageDescription += "<br><br>" + chart;
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

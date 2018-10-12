using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGLatency
{
   public static class Controller
    {
      public  static List<PageTemplate.PageObject> pageObjs = new List<PageTemplate.PageObject>();
       public static Dictionary<string, int> latencySummaryDict = new Dictionary<string, int>();
        public static Dictionary<string, List<string>> chartsData = new Dictionary<string, List<string>>();
        public static string primaryFolder = "";
        public static string secondaryFolder = "";

        public static List<Int32> databaseIds = new List<int>();

        public static int id = 1;

        public static Dictionary<int,  KeyValuePair<string, int>> latencySummaryDict_new = new Dictionary<int, KeyValuePair<string, int>>();
        public static Dictionary<int, KeyValuePair<string, List<string>> > chartsData_new = new Dictionary<int, KeyValuePair<string, List<string>>>();

        public static AGInfo primaryInfo;
        public static AGInfo secondaryInfo;

        public static void AddChartData(string name,List<string> data)
        {
            chartsData.Add(id.ToString()+" "+name, data);
            id++;
        }

        public static void AddChartData_new(int order,string name, List<string> data)
        {
            chartsData_new.Add(order, new KeyValuePair<string, List<string>>(name, data));
            
        
        }

        public static void AddChartDataSummary_new(int order, string name, int value)
        {
            latencySummaryDict_new.Add(order, new KeyValuePair<string, int>( name, value));

            
        }

        public static void Reset()
        {
            id = 1;
            chartsData.Clear();
            pageObjs.Clear();
            latencySummaryDict.Clear();
            latencySummaryDict_new.Clear();
            chartsData_new.Clear();
        }

    }
}

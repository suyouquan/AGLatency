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
        public static void AddChartData(string name,List<string> data)
        {
            chartsData.Add(id.ToString()+" "+name, data);
            id++;
        }

        public static void Reset()
        {
            id = 1;
            chartsData.Clear();
            pageObjs.Clear();
            latencySummaryDict.Clear();

        }

    }
}

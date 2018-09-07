using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace SQLDumpViewer.PageTemplate
{

    
    /// <summary>
    /// template for output. 
    /// </summary>
    public class HtmlPageOutput 
    {
        private static int idx = -1;//use to distinguish file name. some menu may have the same name (won't happen though)

        public string pageTitle = "";
        public string pageDescription = "";
        public string pageContent = "";
        public string pageSummary = "";

       
        public string MenuTitle;//The title in the left menu pane
        public string Group;//Which group the page belongs to
      

        private List<DataTable> dtables = new List<DataTable>();
        public HtmlPageOutput(string menu, string grp )
        {
          //  this.PageId = "Page_" + System.Guid.NewGuid().ToString();
            this.MenuTitle = menu;
            this.Group = grp;
        
          
        }
        
        public int GetCount()
        {
            int cnt = 0;
            foreach (DataTable dt in dtables)
            {
                cnt=cnt+dt.rows.Count();
            }
            return cnt;
        }

        public void AddDataTable(DataTable dt)
        {
            this.dtables.Add(dt);
        }
        public static void AddMenuEntry(string menuTitle, string group, string shortFileName)
        {

            try
            {

                string reportsFolder = SQLDumpData.outputPath + "/reports";// Path.Combine(outputPath, "data");

                string menuFile = Path.Combine(reportsFolder, "menuItems.js");


                //menuItems.push({menuTitle:"Summary",group:"Diagnosis",src:"DiagnosisSummary.html"});
                string menu = "\n\rmenuItems.push({menuTitle: \"" + menuTitle
                    + "\",group: \"" + group
                    + "\",src: \"" + shortFileName + "\"});";

                File.AppendAllText(menuFile, menu);

            }
            catch (Exception ex)
            {
                SQLHelper.LogMessage("[ERROR]AddMenuEntry:" + ex.Message);
            }

        }


        public static void CreateHtmlFile(string fileNameWithoutExtension)
        {

            try
            {

                string reportsFolder = SQLDumpData.outputPath + "/reports";// Path.Combine(outputPath, "data");

                string src = Path.Combine(reportsFolder, "reportTemplate.html");
                string targetHtmlFile = Path.Combine(reportsFolder, fileNameWithoutExtension + ".html");

                string content = File.ReadAllText(src,Encoding.UTF8);


                content = content.Replace("reportTemplate.js", fileNameWithoutExtension + ".js");
                File.WriteAllText(targetHtmlFile, content,Encoding.UTF8);



            }
            catch (Exception ex)
            {
                SQLHelper.LogMessage("[ERROR]CreateHtmlFile:" + ex.Message);
            }

        }
        //only allow digit and letter in a string, otherwise replace it with "_"
        public static string GetElegantName(string s)
        {
            StringBuilder name = new StringBuilder();
            foreach (char c in s)
            {
                if (Char.IsDigit(c) || Char.IsLetter(c)) name.Append(c);
                else name.Append("_");
            }

            idx++;
            return name.ToString() + "_" + idx.ToString();
        }

       

        public string GetJSONData()
        {
            StringBuilder s = new StringBuilder();

            var page = this;

            s.AppendLine("var pageData=");
            s.AppendLine("{");

           
                s.AppendLine("pageTitle:" + "\"" + page.pageTitle + "\",");
           

            s.AppendLine("pageDescription:" + "\"" + page.pageDescription + "\",");
            s.AppendLine("pageContent:" + "\"" + page.pageContent + "\",");
            s.AppendLine("pageSummary:" + "\"" + page.pageSummary + "\",");

            s.AppendLine("dtables:[");

            foreach (DataTable dt in dtables)
            {
                string dtJSON = dt.GetJSONData();
                s.AppendLine(dtJSON + ",");
            }

            s.AppendLine("]}"); //end of SQL server request info

            return s.ToString();

        }

       

        public void SavePageToDisk()
        {
            var page = this;

            try
            {

                string reportsFolder = SQLDumpData.outputPath + "/reports";// Path.Combine(outputPath, "data");

                string name = GetElegantName(page.MenuTitle);


                string JSONfileName = Path.Combine(reportsFolder, name + ".js");

                //create the html report file
                CreateHtmlFile(name);
                string menu = page.MenuTitle;
             

                AddMenuEntry(menu, page.Group, name + ".html");
                string JSONcontent = GetJSONData();
                File.WriteAllText(JSONfileName, JSONcontent, Encoding.UTF8);

            }
            catch (Exception ex)
            {
                SQLHelper.LogMessage("[ERROR]SavePageToDisk:[" + page.pageTitle + "] " + ex.Message);
            }

        }



    }
}

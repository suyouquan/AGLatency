using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Reflection;

namespace SQLDumpViewer.PageTemplate
{
    /// <summary>
    /// Class to instance an wrapper for datatable object in tableData_V3.js
    /// </summary>
    public class DataTable
    {
        private static int idx = -1;
        public string tableId = "";

        //Property for the table grid
        public string tableDescription = "";//title of the grid table in the page
        public string tableTitle = "";//title of the grid table in the page
        public string tableSummary = "";//title of the grid table in the page
        public string bgColorExpand = "lightGrey"; //background color when expanded.

        public int pageLength = 20;//How many items displayed in a page. -1 to display all rows without paging
        public bool sortEnabled = true;
        public bool filterEnabled = true;
        public bool columnFilterEnabled = true;

        //control how vertical displays
        public bool expandEnabled = true;

        //control the vertical display. if true, won't split. otherwise display 16 rows in one grid and others will display in horizontal 	
        //use in bug report
        public bool splitDisabled = false;


        private string verticalExcludedColumns = "[]";
        private string verticalBigColumns = "[]";
        private string verticalLongColumns = "[]";
        private string hiddenColumns = "[]";



        public UInt32 tag = 0;//use for some special format in table-data.js. At this moment only summary page use it.
        public bool limitTableWidth = false;

        //the data list which will feed the javascript datatable object
        public IEnumerable<object> rows = null;


        public DataTable(IEnumerable<object> items)
        {
            this.tableId = "table_" + System.Guid.NewGuid().ToString().Replace("-", "_");
            rows = items;

        }

        public string GetHtmlStr()
        {
            //<div id='tableDiv1'></div><script>tableDataModule.ShowTable('tableDiv1','table_53668da8_ebd3_4b7c_a3b3_f53584480f41')>/script>
            string result = "";

            idx++;
            string div= "tableDiv"+idx.ToString();
            result = "<div id='" + div + "'></div><script>tableDataModule.ShowTable('" + div + "','" + this.tableId + "')</script>";


            return result;

        }



        public void GetSpecialColumns()
        {
            if (rows.Count() == 0) return;

            Type myType = rows.FirstOrDefault().GetType();


            List<Int32> hiddenCols = new List<Int32>();
            List<Int32> longCols = new List<Int32>();
            List<Int32> bigCols = new List<Int32>();
            List<Int32> excludedCols = new List<Int32>();

            FieldInfo[] fieldNames = myType.GetFields();


            int idx = 0;
            for (var i = 0; i < fieldNames.Length; i++)
            {
                FieldInfo f = fieldNames[i];
                if (f.Name.StartsWith("_")) continue;
                //if should be executed, don't increase idx, since in JSON file the excluded column is not there.
                if (OutputProcessor.OutputExclude(f)) continue;

                if (OutputProcessor.HasAttribute(f, ColumnAttr.Hidden)) hiddenCols.Add(idx);
                if (OutputProcessor.HasAttribute(f, ColumnAttr.Long)) longCols.Add(idx);
                if (OutputProcessor.HasAttribute(f, ColumnAttr.VerticalBig)) bigCols.Add(idx);
                if (OutputProcessor.HasAttribute(f, ColumnAttr.VerticalExclude)) excludedCols.Add(idx);



                idx++;

            }

            this.hiddenColumns = "[" + String.Join(",", hiddenCols) + "]";
            this.verticalBigColumns = "[" + String.Join(",", bigCols) + "]";
            this.verticalLongColumns = "[" + String.Join(",", longCols) + "]";
            this.verticalExcludedColumns = "[" + String.Join(",", excludedCols) + "]";

        }



        public string GetJSONData()
        {
            StringBuilder s = new StringBuilder();


            var dt = this;

            s.AppendLine("{");
            s.AppendLine("tableId:" + "\"" + dt.tableId + "\",");


            s.AppendLine("tableTitle:" + "\"" + dt.tableTitle + "\",");
            s.AppendLine("tableDescription:" + "\"" + dt.tableDescription + "\",");
            s.AppendLine("tableSummary:" + "\"" + dt.tableSummary + "\",");


            s.AppendLine("limitTableWidth:" + dt.limitTableWidth.ToString().ToLower() + " ,");

            s.AppendLine("tag:" + "\"" + dt.tag + "\",");
            s.AppendLine("bgColorExpand:" + "\"" + dt.bgColorExpand + "\",");

            s.AppendLine("pageLength:" + dt.pageLength + ",");
            s.AppendLine("expandEnabled:" + dt.expandEnabled.ToString().ToLower() + ",");
            s.AppendLine("sortEnabled:" + dt.sortEnabled.ToString().ToLower() + ",");
            s.AppendLine("filterEnabled:" + dt.filterEnabled.ToString().ToLower() + ",");
            s.AppendLine("columnFilterEnabled:" + dt.columnFilterEnabled.ToString().ToLower() + ",");

            s.AppendLine("splitDisabled:" + dt.splitDisabled.ToString().ToLower() + ",");



            //set special columsn
            GetSpecialColumns();

            s.AppendLine("verticalExcludeColumns:" + dt.verticalExcludedColumns + ","); //vertical view, don't show this inputbuffer 

            s.AppendLine("verticalBigColumns:" + dt.verticalBigColumns + ","); //show it as big TD in vertical view
            s.AppendLine("verticalLongColumns:" + dt.verticalLongColumns + ","); //show it as long row in vertical view

            s.AppendLine("columnDefs:" + "[{targets:" + dt.hiddenColumns + ",visible:false}],"); //don't show the inputbufferHtml and callstack in table view




            string rowsContent = OutputProcessor.GenerateDataTableHtmlStr(rows);
            s.AppendLine(rowsContent);


            s.AppendLine("}"); //end of SQL server request info




            return s.ToString();
        }
















    }



    /*
     
     
        dtables:[
        {
        tableId:"table_53668da8_ebd3_4b7c_a3b3_f53584480f41",
        tableDescription:"This is desc section",
        tableTitle:"<span style='color:green;font-weight:bold;font-size:130%;margin-left:6px;margin-right:10px;'><i class='fa fa-check' aria-hidden='true'></i></span><span style='margin-right:30px;font-weight:bold'>9</span><span style='background-color:yellow;font-weight:bold;font-size:110%;margin-left:6px;margin-right:10px;'>\u26A0</span><span style='margin-right:30px;font-weight:bold'>10</span><span style='color:green;font-weight:bold;font-size:130%;margin-left:6px;margin-right:10px;'><i class='fa fa-info-circle'></i></span><span style='margin-right:30px;font-weight:bold'>1</span>",
        tableSummary:"summary of the table",

        tag:"1",
        bgColorExpand:"lightGrey",
        limitTableWidth:false ,
        pageLength:-1,
        expandEnabled:true,
        sortEnabled:true,
        filterEnabled:false,
        columnFilterEnabled:false,
        splitDisabled:false,
        expandedExtraAsHtml:false,
        verticalExcludeColumns:[0,1],
        verticalBigColumns:[],
        verticalLongColumns:[],
        columnDefs:[{targets:[2],visible:false}],
     */

}

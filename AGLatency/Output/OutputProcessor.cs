using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;


 

namespace AGLatency
{

    public class OutputExclude : System.Attribute
    {
        private readonly bool _value;

        public OutputExclude(bool value)
        {
            _value = value;
        }

        public bool Value
        {
            get { return _value; }
        }

    }

    [Flags]
    public enum ColumnAttr
    {
        //When datatable row expands, it shows as long text
        Long = 1,
        //when dtatable row expands, shows as vertical big column
        VerticalBig = 2,
        //won't show in the expanded child row
        VerticalExclude = 4,
        //this column will be hidden in the datatable row (horizantal view)
        Hidden = 8,

        //should keep html, won't call JavaScriptStringEncode on it
        NoJSEncode = 16
    }

    public class ColumnDefs : System.Attribute
    {
        private readonly ColumnAttr _value;

        public ColumnDefs(ColumnAttr value)
        {
            _value = value;
        }

        public ColumnAttr Value
        {
            get { return _value; }
        }

    }



    /// <summary>
    /// static class to generate JSON output and text output
    /// </summary>
    public static class OutputProcessor
    {
        /// <summary>
        /// support List,Dictionrary output to Json format 
        /// Process<T> will report data ilke below
        /// header:["colum1","colum2",...],
        /// data:[
        /// ["c1","c2",...],
        /// ["c12","c22",...],
        /// ]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        //public static void Process<T>(IEnumerable<T> collection) where T : class
        /*
          public static string Process<T>(IEnumerable<T> collection) where T : class
          {
              if (collection == null) return "";

              if (collection.Count<T>() == 0) return "";
              //get the names of fields
              StringBuilder result = new StringBuilder ();
              result.Append("header:[");
              FieldInfo[] fieldNames = typeof(T).GetFields();// collection.First<T>().GetType().GetFields();

              foreach (FieldInfo f in fieldNames) { if (f.Name.StartsWith("_")) continue; else result.Append("\"" + f.Name + "\","); }
              result.Append("],\ndata:[\n");

              // List<string> _FailedReadFields = new List<string>();
              dynamic _FailedReadFields=null;

              foreach (T element in collection)
              {
                  StringBuilder rowStr = new StringBuilder();
                  rowStr.Append("[");
                  FieldInfo[] fields = element.GetType().GetFields();

                  //check to see if field "FailedReadFields" exists or not
                  for (var i = 0; i < fields.Length; i++)
                  {
                      if (fields[i].Name == "_FailedReadFields")
                      {
                          try { _FailedReadFields = fields[i].GetValue(element); }
                          catch (Exception ex) { _FailedReadFields = null; }
                          break;
                      }
                  }
                  foreach (var f in fields)
                  {
                      if (f.Name.StartsWith("_")) continue;

                       string columDataStr="";
                      try
                      {

                          if (_FailedReadFields!=null)
                          {
                              try
                              {
                                  //If FailedReadFields contains this field, it means it has error when reading it from dump, so ignore it, just output ""
                                  if (!_FailedReadFields.Contains(f.Name))  
                                      columDataStr = f.GetValue(element).ToString();
                              }
                              catch (Exception) { columDataStr = ""; }
                          }
                         // Console.Write(f.Name + ":" + f.GetValue(element) + " ");
                      }
                      catch (Exception ex)
                      { 
                          columDataStr="";
                          SQLHelper.LogMessage("OutputProcessor:Process:"+ex.Message);
                      }

                      //add some interesting padding so it look good during debugging. every column should at least x charaters, otherwise padding with space
                      int len = columDataStr.Length;
                      int COLUMNLEN=20;
                      string paddingSpace ="";
                      if (len<COLUMNLEN) paddingSpace= new string(' ', COLUMNLEN - len);

                      rowStr.Append("\"" + columDataStr + "\","+paddingSpace);
                  } //for each field
                  rowStr.Append("],\n");

                  result.Append(rowStr);

                  //Console.WriteLine("");

              } //foreach 
              result.Append("\n]");
              return result.ToString();

          }
          */
        /// <summary>
        /// Compiled regular expression for performance.
        /// </summary>
        static Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

        /// <summary>
        /// Remove HTML from string with compiled Regex.
        /// </summary>
        public static string StripTagsRegexCompiled(string source)
        {
            return _htmlRegex.Replace(source, string.Empty);
        }

        /// <summary>
        /// Remove HTML tags from string using char array.
        /// </summary>


        /// <summary>
        /// output dictionary to Json
        /// </summary>
        /// <param name="dict"></param>
        public static bool ProcessDictionaryToFile(Dictionary<string, string> dict, System.IO.StreamWriter sw)
        {
            StringBuilder forEachReading = new StringBuilder();
            //sb.Append("header:[");
            sw.Write("header:[");
            forEachReading.Append("//[");
            foreach (string key in dict.Keys)
            {
                string s = "\"" + HttpUtility.JavaScriptStringEncode(key) + "\",";

                int w = StripTagsRegexCompiled(dict[key]).Length;
                w = Math.Min(w, 200); //200 chars, 200*8 in js,1600px
                string s2 = "{\"title\":\"" + HttpUtility.JavaScriptStringEncode(key) + "\",\"chars\":\"" + w + "\"},";
                sw.Write(s2);
                forEachReading.Append(s);
            }
            sw.Write("],\n");
            forEachReading.Append("]");
            sw.Write(forEachReading.ToString());
            sw.Write("\ndata:[\n[");

            foreach (KeyValuePair<string, string> pr in dict)
            {
                sw.Write("  \"" + HttpUtility.JavaScriptStringEncode(pr.Value) + "\",");
            }
            sw.Write("{}]],");//an empty object {} there for JS parsing
            sw.Flush();

            return true;
        }


        public static string ConvertDictionaryToHTMLTable(Dictionary<int, string> dict)
        {
            Dictionary<string, string> di = new Dictionary<string, string>();
            foreach (var item in dict)
                di.Add(item.Key.ToString(), item.Value);

            return ConvertDictionaryToHTMLTable(di);
        }

        public static string ConvertDictionaryToHTMLTable(Dictionary<string, int> dict)
        {
            Dictionary<string, string> di = new Dictionary<string, string>();
            foreach (var item in dict)
                di.Add(item.Key, item.Value.ToString());

            return ConvertDictionaryToHTMLTable(di);
        }
        /// <summary>
        /// output dictionary to HTML table string
        /// </summary>
        /// <param name="dict"></param>
        public static string ConvertDictionaryToHTMLTable(Dictionary<string, string> dict, bool shouldEncode = true)
        {

            if (dict.Keys.Count == 0) return "";


            StringBuilder tbl = new StringBuilder();
            tbl.Append("<table style='border:solid 1px grey; font-size:14px;'  rules='all'>");
            int i = 0;

            foreach (KeyValuePair<string, string> pr in dict)
            {
                if (i % 2 == 0)

                    tbl.Append("<tr style='background-color:#FAFDFA;'><td style='padding:4px !important;'>"
                        + ((shouldEncode == true) ? HttpUtility.JavaScriptStringEncode(pr.Key) : pr.Key)
                      + "</td><td style='color:DarkBlue;padding-left:12px !important;padding-right:12px !important;'>"
                      + ((shouldEncode == true) ? HttpUtility.JavaScriptStringEncode(pr.Value) : pr.Value) + "</td></tr>");

                else
                    tbl.Append("<tr style='background-color:white;'><td style='padding:4px !important;'>"
                      + ((shouldEncode == true) ? HttpUtility.JavaScriptStringEncode(pr.Key) : pr.Key)
                      + "</td><td style='color:DarkBlue;padding-left:12px !important;padding-right:12px !important;'>"
                      + ((shouldEncode == true) ? HttpUtility.JavaScriptStringEncode(pr.Value) : pr.Value) + "</td></tr>");


                i++;
            }

            tbl.Append("</table>");
            return tbl.ToString();
        }


        /// <summary>
        /// output dictionary to HTML table string, with 4 columns
        /// </summary>
        /// <param name="dict"></param>
        public static string ConvertDictionaryToHTMLTable_4Columns(Dictionary<string, string> dict)
        {

            if (dict.Keys.Count == 0) return "";


            StringBuilder tbl = new StringBuilder();
            tbl.Append("<table style='border:solid 1px grey; font-size:14px;white-space:nowrap;'  rules='all'>");
            int i = 0;

            //< tr style = 'background-color:#FAFDFA;' >

            foreach (KeyValuePair<string, string> pr in dict)
            {
                if (i % 2 == 1)
                {
                    tbl.Append("<td style='padding:4px !important;'>" + HttpUtility.JavaScriptStringEncode(pr.Key)
                      + "</td><td style='color:DarkBlue;padding-left:12px !important;padding-right:12px !important;'>" + HttpUtility.JavaScriptStringEncode(pr.Value) + "</td>");
                    tbl.Append("</tr>");
                }
                else
                {

                    //add color to even row
                    if (i % 4 == 0) tbl.Append("<tr style = 'background-color:#FAFDFA;'>");
                    else tbl.Append("<tr>");
                    tbl.Append("<td style='padding:4px !important;'>" + HttpUtility.JavaScriptStringEncode(pr.Key)
                      + "</td><td style='color:DarkBlue;padding-left:12px !important;padding-right:12px !important;'>" + HttpUtility.JavaScriptStringEncode(pr.Value) + "</td>");
                }



                i++;
            }
            if (dict.Count % 2 != 0) tbl.Append("</tr>");
            tbl.Append("</table>");
            return tbl.ToString();
        }


        /// <summary>
        /// output dictionary to HTML table string, with 4 columns
        /// </summary>
        /// <param name="dict"></param>
        public static string ConvertDictionaryToHTMLTable_OneRow(Dictionary<string, string> dict, int fontSize = 13)
        {

            if (dict.Keys.Count == 0) return "";


            StringBuilder tbl = new StringBuilder();
            tbl.Append("<table style='border:solid 1px grey; font-size:" + fontSize + "px;white-space:nowrap;'  rules='all'>");
            tbl.Append("<tr style = 'background-color:#FAFDFA;'>");
            int i = 0;

            //< tr style = 'background-color:#FAFDFA;' >

            foreach (KeyValuePair<string, string> pr in dict)
            {
                if (i % 2 == 1)
                {
                    tbl.Append("<td style='padding:4px !important;'>" + HttpUtility.JavaScriptStringEncode(pr.Key)
                      + "</td><td style='background-color:white;color:DarkBlue;padding-left:12px !important;padding-right:12px !important;'>"
                      + HttpUtility.JavaScriptStringEncode(pr.Value) + "</td>");

                }
                else
                {

                    //add color to even cell

                    tbl.Append("<td style='padding:4px !important;'>" + HttpUtility.JavaScriptStringEncode(pr.Key)
                      + "</td><td style='color:DarkBlue;padding-left:12px !important;padding-right:12px !important;'>"
                      + HttpUtility.JavaScriptStringEncode(pr.Value) + "</td>");
                }



                i++;
            }
            tbl.Append("</tr>");
            tbl.Append("</table>");
            return tbl.ToString();
        }

        public static string GetFieldForHtmlTable<T>(T element, FieldInfo f) where T : class
        {
            string columDataStr = "";
            string name = "";
            try
            {
                name = f.Name;

                if (f.FieldType == typeof(System.DateTime))
                {
                    var t = (System.DateTime)f.GetValue(element);
                    if (t != null)
                        columDataStr = t.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    else columDataStr = "";
                }
                else
                {

                    var obj = f.GetValue(element);
                    if (obj != null) columDataStr = f.GetValue(element).ToString();
                    else columDataStr = "";
                }

            }
            catch (Exception ex)
            {
                columDataStr = "";
                Logger.LogMessage("OutputProcessor:fileds:" + name + ", " + ex.Message);
            }

            return columDataStr;
        }
        /// <summary>
        /// convert a list of class objects to HTML table, ignore nested data of each class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static string Convert2HTMLTable<T>(IEnumerable<T> collection, bool needHeader, bool keepHtmlChar) where T : class
        {
            if ((collection == null) || collection.Count<T>() == 0)

            {
                //return empty  
                return "";

            }

            //get the names of fields
            StringBuilder result = new StringBuilder();
            result.Append("<table rules='all' style='border:solid 1px lightgray;padding:4px !important;font-family:Segoe UI;'>");


            if (needHeader)
            {
                result.Append("<tr  style='background-color:#FAFDFA;'>");
                FieldInfo[] fieldNames = typeof(T).GetFields();// collection.First<T>().GetType().GetFields();

                //foreach (FieldInfo f in fieldNames)
                for (int i = 0; i < fieldNames.Length; i++)
                {
                    FieldInfo f = fieldNames[i];
                    if (f.Name.StartsWith("_")) continue;
                    //check to see if output is enable or not.
                    if (OutputExclude(f)) continue;
                    else
                    {

                        string columnObj = "<th>" + f.Name + "</th>";

                        // result_forEasyReading.Append("\"" + f.Name + "\",  " + paddingSpace);
                        result.Append(columnObj);
                    }


                }
                result.Append("</tr>");
            }


            dynamic _FailedReadFields = null;

            int tmp_row = 0;
            foreach (T element in collection)
            {
                tmp_row++;
                StringBuilder rowStr = new StringBuilder();
                if (tmp_row % 2 == 0)
                    rowStr.Append("<tr style='background-color:#FAFDFA;'>");
                else rowStr.Append("<tr>");
                FieldInfo[] fields = element.GetType().GetFields();

                //check to see if field "FailedReadFields" exists or not
                for (var i = 0; i < fields.Length; i++)
                {
                    if (fields[i].Name == "_FailedReadFields")
                    {
                        try { _FailedReadFields = fields[i].GetValue(element); }
                        catch (Exception ex) { _FailedReadFields = null; }

                    }

                }
                for (int i = 0; i < fields.Length; i++)
                //foreach (var f in fields)
                {
                    var f = fields[i];
                    if (f.Name.StartsWith("_")) continue;
                    //check to see if output is enable or not.
                    if (OutputExclude(f)) continue;
                    string columDataStr = "";


                    if (_FailedReadFields != null)
                    {

                        //If FailedReadFields contains this field, it means it has error when reading it from dump, so ignore it, just output ""
                        if (!_FailedReadFields.Contains(f.Name))
                        {
                            columDataStr = GetFieldForHtmlTable(element, f);
                        }


                    }
                    // Console.Write(f.Name + ":" + f.GetValue(element) + " ");

                    else
                        columDataStr = GetFieldForHtmlTable(element, f);

                    //if Remark this, will this break the SQLDumpData.js???
                    if (!keepHtmlChar) columDataStr = HttpUtility.JavaScriptStringEncode(columDataStr);



                    // rowStr.Append("\"" + (columDataStr) + "\",  " + paddingSpace);
                    rowStr.Append("<td style='padding:4px !important;border:solid 1px lightgray;'>" + (columDataStr) + "</td>");
                } //for each field


                rowStr.Append("</tr>");

                result.Append(rowStr);


            } //foreach 
            result.Append("</table>");
            return result.ToString();

        }



        /// <summary>
        /// write the List to filestream
        /// Json format:
        /// {
        /// id:"sys_dm_os_nodes",
        /// "TableDescription":" this is output of sys.dm_exec_nodes",
        /// menu:[menu_title:"sys.dm_exec_nodes",group:"SQLOS",icon:""]
        /// header:["col1","col2"],
        /// layout:"vertical" or "horizontal" or "both" or "container",
        /// expandEnabled:true or fase,
        /// data:[
        /// ["1","2",{"callstack":callstackID,...}] //it will has object as last column for JS to render nested row when i click the row
        /// ]
        /// summary:"you have 4 nodes"
        /// }
        /// callstacks=
        /// {
        /// 0:"xxx",
        /// 1:"222"
        /// }
        /// This function only write header and data to fs stream, becuase it it has no idea about others like summary etc.
        /// 
        /// 
        /// header: [{
        //“title”:”DumpFile”,
        //“chars”:”120px”
        //},
        //{
        //“title”:”CommandLine”,
        //“width”:”200px”
        //},…]


        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>

        public static string ProcessingWithNestedData<T>(IEnumerable<T> collection) where T : class
        {
            if ((collection == null) || collection.Count<T>() == 0)

            {
                //return empty table
                return "header:[],data:[],";

            }

            List<int> columnMaxWidths = GetColumnMaxWidth(collection);

            //get the names of fields
            StringBuilder result = new StringBuilder();
            StringBuilder result_forEasyReading = new StringBuilder();
            result.Append("header:\n[");
            result_forEasyReading.Append("\n//[");
            FieldInfo[] fieldNames = typeof(T).GetFields();// collection.First<T>().GetType().GetFields();

            //foreach (FieldInfo f in fieldNames)
            for (int i = 0; i < fieldNames.Length; i++)
            {
                FieldInfo f = fieldNames[i];
                if (f.Name.StartsWith("_")) continue;
                //check to see if output is enable or not.
                if (OutputExclude(f)) continue;
                else
                {

                    //add some interesting padding so it look good during debugging. every column should at least x charaters, otherwise padding with space
                    int len = f.Name.Length + 2; //+2 because the js table will have sort sign
                    //"title":"DumpFile","chars":"20"

                    int COLUMNLEN = columnMaxWidths[i];
                    string paddingSpace = "";
                    if (len < COLUMNLEN) paddingSpace = new string(' ', COLUMNLEN - len);
                    //if name length is greather than that, need to adjus the list so that the row can match with column width
                    else columnMaxWidths[i] = len;//two quotes,one comma,and one space

                    string columnObj = "{\"title\":\"" + f.Name + "\",\"chars\":\"" + (columnMaxWidths[i]) + "\"},";

                    result_forEasyReading.Append("\"" + f.Name + "\",  " + paddingSpace);
                    result.Append(columnObj);
                }


            }
            result.Append("],");
            result_forEasyReading.Append("]");
            result.Append(result_forEasyReading.ToString());
            result.Append("\ndata:[\n");

            // List<string> _FailedReadFields = new List<string>();
            dynamic _FailedReadFields = null;
            dynamic _NestedData = null;

            foreach (T element in collection)
            {
                StringBuilder rowStr = new StringBuilder();
                rowStr.Append("  [");
                FieldInfo[] fields = element.GetType().GetFields();

                //check to see if field "FailedReadFields" exists or not
                for (var i = 0; i < fields.Length; i++)
                {
                    if (fields[i].Name == "_FailedReadFields")
                    {
                        try { _FailedReadFields = fields[i].GetValue(element); }
                        catch (Exception ex) { _FailedReadFields = null; }

                    }
                    if (fields[i].Name == "_NestedData")
                    {
                        try { _NestedData = fields[i].GetValue(element); }
                        catch (Exception ex) { _NestedData = null; }

                    }
                }
                for (int i = 0; i < fields.Length; i++)
                //foreach (var f in fields)
                {
                    var f = fields[i];
                    if (f.Name.StartsWith("_")) continue;
                    //check to see if output is enable or not.
                    if (OutputExclude(f)) continue;

                    string columDataStr = "";
                    try
                    {

                        if (_FailedReadFields != null)
                        {
                            try
                            {
                                //If FailedReadFields contains this field, it means it has error when reading it from dump, so ignore it, just output ""
                                if (!_FailedReadFields.Contains(f.Name))
                                {
                                    if (f.FieldType == typeof(System.DateTime))
                                    {
                                        var t = (System.DateTime)f.GetValue(element);
                                        if (t != null)
                                            columDataStr = t.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                        else columDataStr = "";
                                    }
                                    else
                                    {
                                        var obj = f.GetValue(element);
                                        if (obj != null) columDataStr = f.GetValue(element).ToString();
                                        else columDataStr = "";
                                    }
                                }
                            }
                            catch (Exception) { columDataStr = ""; }
                        }
                        // Console.Write(f.Name + ":" + f.GetValue(element) + " ");
                    }
                    catch (Exception ex)
                    {
                        columDataStr = "";
                        Logger.LogMessage("OutputProcessor:Process:" + ex.Message);
                    }

                    //add some interesting padding so it look good during debugging. every column should at least x charaters, otherwise padding with space
                    columDataStr = HttpUtility.JavaScriptStringEncode(columDataStr);
                    int len = columDataStr.Length;
                    int COLUMNLEN = columnMaxWidths[i];
                    string paddingSpace = "";
                    if (len < COLUMNLEN) paddingSpace = new string(' ', COLUMNLEN - len);

                    rowStr.Append("\"" + (columDataStr) + "\",  " + paddingSpace);
                } //for each field

                //now add nested data
                if (_NestedData != null)
                {
                    rowStr.Append("{");
                    foreach (KeyValuePair<string, string> pr in _NestedData)
                    {
                        rowStr.Append("\"" + pr.Key + "\":" + "\"" + HttpUtility.JavaScriptStringEncode(pr.Value) + "\",");
                    }

                    rowStr.Append("},");
                }
                rowStr.Append("],\n");

                result.Append(rowStr);

                //Console.WriteLine("");

            } //foreach 
            result.Append("\n],");
            return result.ToString();

        }

        public static bool OutputExclude(FieldInfo fi)
        {

            OutputExclude[] attrs =
               fi.GetCustomAttributes(typeof(OutputExclude),
                                       false) as OutputExclude[];
            if (attrs.Length > 0)
            {
                var result = attrs[0].Value;
                if (result == true) return true;
            }

            return false;

        }

        public static bool HasAttribute(FieldInfo fi, ColumnAttr attr)
        {

            ColumnDefs[] attrs =
               fi.GetCustomAttributes(typeof(ColumnDefs), false) as ColumnDefs[];
            if (attrs.Length > 0)
            {
                var result = attrs[0].Value;
                if ((result & attr) == attr) return true;
            }

            return false;

        }
        public static String GetFieldStr<T>(T element, FieldInfo f) where T : class
        {
            string columDataStr = "";
            string name = "";
            try
            {
                name = f.Name;
                if (f.FieldType == typeof(System.DateTime))
                {
                    var t = (System.DateTime)f.GetValue(element);
                    if (t != null)
                        columDataStr = t.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    else columDataStr = "";
                }
                else
                {
                    var obj = f.GetValue(element);
                    if (obj != null)
                    {
                        columDataStr = f.GetValue(element).ToString();
                        if (!HasAttribute(f, ColumnAttr.NoJSEncode))
                        {
                            columDataStr = HttpUtility.JavaScriptStringEncode(columDataStr);
                        }
                    }
                    else columDataStr = "";
                }
            }
            catch (Exception ex)
            {
                columDataStr = ""; Logger.LogMessage("OutputProcessor:fileds:" + name + ", " + ex.Message);
            }

            return columDataStr;
        }
        public static string GenerateDataTableHtmlStr<T>(IEnumerable<T> collection) where T : class
        {
            if ((collection == null) || collection.Count<T>() == 0)

            {
                //return empty table
                return "header:[],data:[],";

            }
            //    SQLHelper.LogMessage("GenerateDataTableHtmlStr -- " + typeof(T).ToString() + " (" + collection.Count() + ") --started.");

            List<int> columnMaxWidths = GetColumnMaxWidth(collection);

            //get the names of fields
            StringBuilder result = new StringBuilder();
            StringBuilder result_forEasyReading = new StringBuilder();
            result.Append("header:\n[");
            result_forEasyReading.Append("\n//[");
            FieldInfo[] fieldNames = typeof(T).GetFields();// collection.First<T>().GetType().GetFields();

            //foreach (FieldInfo f in fieldNames)
            for (int i = 0; i < fieldNames.Length; i++)
            {
                FieldInfo f = fieldNames[i];
                if (f.Name.StartsWith("_")) continue;

                //check to see if output is enable or not.
                if (OutputExclude(f)) continue;
                else
                {

                    //add some interesting padding so it look good during debugging. every column should at least x charaters, otherwise padding with space

                    // int len = f.Name.Length + 2; //+2 because the js table will have sort sign
                    //don't add teh extra 2 chars, cause in summary page it looks odd.
                    int len = f.Name.Length;
                    //"title":"DumpFile","chars":"20"

                    int COLUMNLEN = columnMaxWidths[i];
                    string paddingSpace = "";
                    if (len < COLUMNLEN) paddingSpace = new string(' ', COLUMNLEN - len);
                    //if name length is greather than that, need to adjus the list so that the row can match with column width
                    else columnMaxWidths[i] = len;//two quotes,one comma,and one space

                    string columnObj = "{\"title\":\"" + f.Name + "\",\"chars\":\"" + (columnMaxWidths[i]) + "\"},";

                    result_forEasyReading.Append("\"" + f.Name + "\",  " + paddingSpace);
                    result.Append(columnObj);
                }


            }
            result.Append("],");
            result_forEasyReading.Append("]");
            result.Append(result_forEasyReading.ToString());
            result.Append("\ndata:[\n");

            // List<string> _FailedReadFields = new List<string>();
            dynamic _FailedReadFields = null;


            foreach (T element in collection)
            {
                StringBuilder rowStr = new StringBuilder();
                rowStr.Append("  [");
                FieldInfo[] fields = element.GetType().GetFields();

                //check to see if field "FailedReadFields" exists or not
                for (var i = 0; i < fields.Length; i++)
                {
                    if (fields[i].Name == "_FailedReadFields")
                    {
                        try { _FailedReadFields = fields[i].GetValue(element); }
                        catch (Exception ex) { _FailedReadFields = null; }

                    }

                }
                for (int i = 0; i < fields.Length; i++)
                //foreach (var f in fields)
                {
                    var f = fields[i];
                    if (f.Name.StartsWith("_")) continue;
                    if (OutputExclude(f)) continue;

                    string columDataStr = "";

                    if (_FailedReadFields != null)
                    {
                        //If FailedReadFields contains this field, it means it has error when reading it from dump, so ignore it, just output ""
                        if (!_FailedReadFields.Contains(f.Name))
                        {
                            columDataStr = GetFieldStr(element, f);
                        }

                    }
                    else //if no FailedReadFileds defined in class
                    {
                        columDataStr = GetFieldStr(element, f);
                    }
                    // Console.Write(f.Name + ":" + f.GetValue(element) + " ");


                    //add some interesting padding so it look good during debugging. every column should at least x charaters, otherwise padding with space
                    //  columDataStr = HttpUtility.JavaScriptStringEncode(columDataStr);

                    int len = columDataStr.Length;
                    int COLUMNLEN = columnMaxWidths[i];
                    string paddingSpace = "";
                    if (len < COLUMNLEN) paddingSpace = new string(' ', COLUMNLEN - len);

                    rowStr.Append("\"" + (columDataStr) + "\",  " + paddingSpace);
                } //for each field


                rowStr.Append("],\n");

                result.Append(rowStr);

                //Console.WriteLine("");

            } //foreach 
            result.Append("\n],");

            //   SQLHelper.LogMessage("GenerateDataTableHtmlStr --"+  typeof(T).ToString()+" --done.");
            return result.ToString();

        }

        public static string GenerateDataTableHtmlStr(IEnumerable<object> collection)  
        {
            if ((collection == null) || collection.Count() == 0)

            {
                //return empty table
                return "header:[],data:[],";

            }
            //    SQLHelper.LogMessage("GenerateDataTableHtmlStr -- " + typeof(T).ToString() + " (" + collection.Count() + ") --started.");

            List<int> columnMaxWidths = GetColumnMaxWidth(collection);

            //get the names of fields
            StringBuilder result = new StringBuilder();
            StringBuilder result_forEasyReading = new StringBuilder();
            result.Append("header:\n[");
            result_forEasyReading.Append("\n//[");
            Type myType = collection.FirstOrDefault().GetType();
            FieldInfo[] fieldNames = myType.GetFields();// collection.First<T>().GetType().GetFields();

            //foreach (FieldInfo f in fieldNames)
            for (int i = 0; i < fieldNames.Length; i++)
            {
                FieldInfo f = fieldNames[i];
                if (f.Name.StartsWith("_")) continue;

                //check to see if output is enable or not.
                if (OutputExclude(f)) continue;
                else
                {

                    //add some interesting padding so it look good during debugging. every column should at least x charaters, otherwise padding with space

                    // int len = f.Name.Length + 2; //+2 because the js table will have sort sign
                    //don't add teh extra 2 chars, cause in summary page it looks odd.
                    int len = f.Name.Length;
                    //"title":"DumpFile","chars":"20"

                    int COLUMNLEN = columnMaxWidths[i];
                    string paddingSpace = "";
                    if (len < COLUMNLEN) paddingSpace = new string(' ', COLUMNLEN - len);
                    //if name length is greather than that, need to adjus the list so that the row can match with column width
                    else columnMaxWidths[i] = len;//two quotes,one comma,and one space

                    string columnObj = "{\"title\":\"" + f.Name + "\",\"chars\":\"" + (columnMaxWidths[i]) + "\"},";

                    result_forEasyReading.Append("\"" + f.Name + "\",  " + paddingSpace);
                    result.Append(columnObj);
                }


            }
            result.Append("],");
            result_forEasyReading.Append("]");
            result.Append(result_forEasyReading.ToString());
            result.Append("\ndata:[\n");

            // List<string> _FailedReadFields = new List<string>();
            dynamic _FailedReadFields = null;


            foreach (var element in collection)
            {
                StringBuilder rowStr = new StringBuilder();
                rowStr.Append("  [");
                FieldInfo[] fields = element.GetType().GetFields();

                //check to see if field "FailedReadFields" exists or not
                for (var i = 0; i < fields.Length; i++)
                {
                    if (fields[i].Name == "_FailedReadFields")
                    {
                        try { _FailedReadFields = fields[i].GetValue(element); }
                        catch (Exception ex) { _FailedReadFields = null; }

                    }

                }
                for (int i = 0; i < fields.Length; i++)
                //foreach (var f in fields)
                {
                    var f = fields[i];
                    if (f.Name.StartsWith("_")) continue;
                    if (OutputExclude(f)) continue;

                    string columDataStr = "";

                    if (_FailedReadFields != null)
                    {
                        //If FailedReadFields contains this field, it means it has error when reading it from dump, so ignore it, just output ""
                        if (!_FailedReadFields.Contains(f.Name))
                        {
                            columDataStr = GetFieldStr(element, f);
                        }

                    }
                    else //if no FailedReadFileds defined in class
                    {
                        columDataStr = GetFieldStr(element, f);
                    }
                    // Console.Write(f.Name + ":" + f.GetValue(element) + " ");


                    //add some interesting padding so it look good during debugging. every column should at least x charaters, otherwise padding with space
                    //  columDataStr = HttpUtility.JavaScriptStringEncode(columDataStr);

                    int len = columDataStr.Length;
                    int COLUMNLEN = columnMaxWidths[i];
                    string paddingSpace = "";
                    if (len < COLUMNLEN) paddingSpace = new string(' ', COLUMNLEN - len);

                    rowStr.Append("\"" + (columDataStr) + "\",  " + paddingSpace);
                } //for each field


                rowStr.Append("],\n");

                result.Append(rowStr);

                //Console.WriteLine("");

            } //foreach 
            result.Append("\n],");

            //   SQLHelper.LogMessage("GenerateDataTableHtmlStr --"+  typeof(T).ToString()+" --done.");
            return result.ToString();

        }

        /// <summary>
        /// caculate column max width, so that the JSON output file is more reabable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static List<int> GetColumnMaxWidth<T>(IEnumerable<T> collection) where T : class
        {

            // SQLHelper.LogMessage("GetColumnMaxWidth --" + typeof(T).ToString() + " started.");
            //the caller knows the collection is not empty. 
            var ele = collection.First();
            int colNums = ele.GetType().GetFields().Length;


            List<int> columnMaxWidths = new List<int>(new int[colNums]);

            foreach (T element in collection)
            {
                FieldInfo[] fields = element.GetType().GetFields();

                for (var i = 0; i < fields.Length; i++)
                {
                    try
                    {
                        int len = 0;
                        if (fields[i].FieldType == typeof(System.DateTime))
                        {
                            len = 23; //ToString("yyyy-MM-dd HH:mm:ss.fff");
                        }
                        else
                        {
                            //bugfix:need to remove html tags
                            var obj = fields[i].GetValue(element);
                            if (obj != null)
                            {
                                string raws = fields[i].GetValue(element).ToString();
                                string after = StripTagsRegexCompiled(raws);

                                len = after.Length;
                            }
                        }
                        //max width, but not longer than 100 chars
                        columnMaxWidths[i] = Math.Min(100, Math.Max(columnMaxWidths[i], len));
                    }
                    catch (Exception e)
                    {
                        Logger.LogMessage("[ERROR]:GetColumnMaxWidth" + e.Message);
                        //eat the exception. column width is not critcal thing
                    }
                }

            }
            //   SQLHelper.LogMessage("GetColumnMaxWidth --" + typeof(T).ToString() + " done.");
            return columnMaxWidths;


        }

        public static Dictionary<string, int> GetColumn2IDMap<T>() where T : class
        {

            Dictionary<string, int> columnsMap = new Dictionary<string, int>();
            FieldInfo[] fieldNames = typeof(T).GetFields();



            int idx = 0;
            for (var i = 0; i < fieldNames.Length; i++)
            {
                FieldInfo f = fieldNames[i];
                if (f.Name.StartsWith("_")) continue;
                columnsMap.Add(f.Name, idx);
                idx++;

            }

            return columnsMap;

        }


        public static string CreatePieHtmlForDictionary(Dictionary<string, UInt64> data, string containerName, int width, int height, string customColorStr = null, bool shouldEncode = true, string showLegend = "true")
        {
            if (data == null || data.Count == 0) return "";

            string pieChartHtml = @"<div id='" + containerName + @"' style='z-index:999;'></div>"
                + @"<script>
                var container = document.getElementById('" + containerName + @"');                
                var canvas = document.createElement('canvas'); 
                var ctx = canvas.getContext('2d');
                canvas.width=" + width + @";
                canvas.height=" + height + @";
                container.appendChild(canvas);
                var pieChart = new Chart(ctx,{
                  type: 'pie',
                  data: {
                    labels:[labelStr],
                    borderColor: 'red',
                    datasets: [
                      {
                        data: [dataStr],
                        borderColor: 'lightblue',
                        borderWidth: '1',
                        hoverBorderColor: 'red',
                        backgroundColor: [colorStr],
                        hoverBackgroundColor: [colorStr]
                      }]
                  },
                  options: {
                    responsive:false,
	                legend: { position:'right',display:" + showLegend + @"  }
                  }
                });
                </script>";





            string labelStr = "";
            string dataStr = "";
            string colorStr = "";

            int i = 0;
            colorEnumIdx = 0;
            foreach (KeyValuePair<string, UInt64> pr in data)
            {
                if (i == 0)
                {
                    labelStr = String.Format("'{0}'", pr.Key);
                    dataStr = String.Format("{0}", pr.Value);
                    colorStr = String.Format("'{0}'", GetColor());
                }
                else
                {
                    labelStr += String.Format(",'{0}'", pr.Key);
                    dataStr += String.Format(",{0}", pr.Value);
                    colorStr += String.Format(",'{0}'", GetColor());
                }

                i++;
            }

            if (customColorStr != null)
            {
                if (shouldEncode) pieChartHtml = HttpUtility.JavaScriptStringEncode(pieChartHtml.Replace("labelStr", labelStr).Replace("dataStr", dataStr).Replace("colorStr", customColorStr));
                else pieChartHtml = pieChartHtml.Replace("labelStr", labelStr).Replace("dataStr", dataStr).Replace("colorStr", customColorStr);
            }
            else
            {
                if (shouldEncode)
                    pieChartHtml = HttpUtility.JavaScriptStringEncode(pieChartHtml.Replace("labelStr", labelStr).Replace("dataStr", dataStr).Replace("colorStr", colorStr));
                else pieChartHtml = pieChartHtml.Replace("labelStr", labelStr).Replace("dataStr", dataStr).Replace("colorStr", colorStr);
            }

            return pieChartHtml;

        }


        public static string CreateBarHtmlForDictionary(Dictionary<string, Int32> data, string containerName, int width, int height, string customColorStr = null, bool shouldEncode = true, string showLegend = "true")
        {
            if (data == null || data.Count == 0) return "";

            string chartHtml = @"<div id='" + containerName + @"' style='z-index:999;'></div>"
                + @"<script>
                var container = document.getElementById('" + containerName + @"');                
                var canvas = document.createElement('canvas'); 
                var ctx = canvas.getContext('2d');
                canvas.width=" + width + @";
                canvas.height=" + height + @";
                container.appendChild(canvas);
                var barChart = new Chart(ctx,{
                  type: 'bar',
                  data: {
                    labels:[labelStr],
                    borderColor: 'red',
                    datasets: [
                      {
                        data: [dataStr],
                        borderColor: 'lightblue',
                        borderWidth: '1',
                        hoverBorderColor: 'red',
                        backgroundColor: [colorStr],
                        hoverBackgroundColor: [colorStr]
                      }]
                  },
                  options: {
                    responsive:false,
	                legend: { position:'right',display:" + showLegend + @"  },
                     maintainAspectRatio: false,
   
                        scales: {
                          yAxes: [{
                            ticks: {
                              beginAtZero: true,
                            }
                          }]
                        },
                    plugins: {
                          datalabels: {
                            anchor: 'end',
                            align: 'top',
                            formatter: Math.round,
                            font: {
                              weight: 'bold'
                            }
                          }
                        }

                  }
                });
                </script>";





            string labelStr = "";
            string dataStr = "";
            string colorStr = "";

            int i = 0;
            colorEnumIdx = 0;
            foreach (KeyValuePair<string, Int32> pr in data)
            {
                if (i == 0)
                {
                    labelStr = String.Format("'{0}'", pr.Key);
                    dataStr = String.Format("{0}", pr.Value);
                    colorStr = String.Format("'{0}'", GetColor());
                }
                else
                {
                    labelStr += String.Format(",'{0}'", pr.Key);
                    dataStr += String.Format(",{0}", pr.Value);
                    colorStr += String.Format(",'{0}'", GetColor());
                }

                i++;
            }

            if (customColorStr != null)
            {
                if (shouldEncode) chartHtml = HttpUtility.JavaScriptStringEncode(chartHtml.Replace("labelStr", labelStr).Replace("dataStr", dataStr).Replace("colorStr", customColorStr));
                else chartHtml = chartHtml.Replace("labelStr", labelStr).Replace("dataStr", dataStr).Replace("colorStr", customColorStr);
            }
            else
            {
                if (shouldEncode)
                    chartHtml = HttpUtility.JavaScriptStringEncode(chartHtml.Replace("labelStr", labelStr).Replace("dataStr", dataStr).Replace("colorStr", colorStr));
                else chartHtml = chartHtml.Replace("labelStr", labelStr).Replace("dataStr", dataStr).Replace("colorStr", colorStr);
            }

            return chartHtml;

        }




        //draw 1 lines with data1  
        public static string CreateOneLineChartHtmlForDictionary
            (List<string> label, List<UInt64> data1, string label1, string containerName, int width, int height, string bkColorStr, string colorStr)
        {
            if (data1 == null || data1.Count == 0) return "";

            if (label.Count != data1.Count) return "";

            string lineChartHtml = @"<div id='" + containerName + @"' style='z-index:999;'></div>"
                + @"<script>
                var container = document.getElementById('" + containerName + @"');                
          
            var canvas = document.createElement('canvas');
            var ctx = canvas.getContext('2d');
            canvas.width = 1500;
            canvas.height = 400;
            container.appendChild(canvas);
            var pieChart = new Chart(ctx,
          {
            type: 'line',
            data:
            {
              labels: [labelStr],
              borderColor: 'rgba(255,255,0,1)',
  		      datasets:[
               {
                 label: '" + label1 + @"',    
                 backgroundColor: '" + bkColorStr + @"',
                 borderColor: '" + colorStr + @"',
		         lineTension: 0,
                 data: [data1Str]
               },
	
			  
              ]
             },
           options:
           {
            responsive: false,
	        legend: { position: 'top',display:true  },
			elements: { line: { tension: 0   } },
 		    animation:
              {duration: 0  },
           }
        });
        </script>";





            string labelStr = "";
            string data1Str = "";


            int i = 0;

            for (i = 0; i < label.Count; i++)
            {
                if (i == 0)
                {
                    labelStr = String.Format("'{0}'", label[i]);
                    data1Str = String.Format("{0}", data1[i]);


                }
                else
                {
                    labelStr += String.Format(",'{0}'", label[i]);
                    data1Str += String.Format(",{0}", data1[i]);

                }


            }

            lineChartHtml = lineChartHtml.Replace("labelStr", labelStr).Replace("data1Str", data1Str);
            return lineChartHtml;

        }

        //draw 2 lines with data1 and data2
        public static string CreateLineChartHtmlForDictionary
            (List<string> label, List<UInt64> data1, string label1, List<UInt64> data2, string label2, string containerName, int width, int height)
        {
            if (data1 == null || data1.Count == 0) return "";
            if (data2 == null || data2.Count == 0) return "";
            if (label.Count != data1.Count || label.Count != data2.Count || data1.Count != data2.Count) return "";

            string lineChartHtml = @"<div id='" + containerName + @"' style='z-index:999;'></div>"
                + @"<script>
                var container = document.getElementById('" + containerName + @"');                
          
            var canvas = document.createElement('canvas');
            var ctx = canvas.getContext('2d');
            canvas.width = 1500;
            canvas.height = 400;
            container.appendChild(canvas);
            var pieChart = new Chart(ctx,
          {
            type: 'line',
            data:
            {
              labels: [labelStr],
              borderColor: 'rgba(255,0,0,0.1)',
  		      datasets:[
               {
                 label: '" + label1 + @"',    
                 backgroundColor: 'rgba(255,0,0,0.2)',
                 borderColor: 'rgba(255,0,0,0.5)',
		         lineTension: 0,
                 data: [data1Str]
               },
	
			  {
                label: '" + label2 + @"',
                backgroundColor: 'rgba(0,0,255,0.1)',
		        borderColor: 'rgba(0,0,255,0.5)',
				lineTension: 0,
				data: [data2Str]
               }
              ]
             },
           options:
           {
            responsive: false,
	        legend: { position: 'top',display:true  },
			elements: { line: { tension: 0   } },
 		    animation:
              {duration: 0  },
           }
        });
        </script>";





            string labelStr = "";
            string data1Str = "";
            string data2Str = "";

            int i = 0;

            for (i = 0; i < label.Count; i++)
            {
                if (i == 0)
                {
                    labelStr = String.Format("'{0}'", label[i]);
                    data1Str = String.Format("{0}", data1[i]);
                    data2Str = String.Format("{0}", data2[i]);

                }
                else
                {
                    labelStr += String.Format(",'{0}'", label[i]);
                    data1Str += String.Format(",{0}", data1[i]);
                    data2Str += String.Format(",{0}", data2[i]);
                }


            }

            lineChartHtml = lineChartHtml.Replace("labelStr", labelStr).Replace("data1Str", data1Str).Replace("data2Str", data2Str);
            return lineChartHtml;

        }


        public enum ColorEnum
        {




            brown = 0,
            blue,
            green,
            cyan,
            purple,

            yellow,

            orange,
            olive,
            darkblue



        };
        private static int colorEnumIdx = 0;
        //http://tool.c7sky.com/webcolor/
        private static string[] colorArray = new string[]
        {
            //"#66CCFF","#CCFFCC","#CCFF99","#CCCCFF","#FFFFCC",
            "#99CCFF","#99CC66","#FFCCCC","#FF9966","#FFFFCC","#CCFFCC","#99CC99","#66CCCC","#CCCCFF"
            ,"#BBCCFF","#AACCFF","#99CCFF","#88CCFF","#77CCFF","#66CCFF","#55CCFF","#44CCFF","#44BBFF","#44AAFF","#4499FF","#4488FF","#4477FF"
        };


        public static string GetColor()
        {

            // if (colorEnumIdx >= Enum.GetNames(typeof(ColorEnum)).Length) colorEnumIdx = 0;

            //string colorStr = ((ColorEnum)colorEnumIdx).ToString();

            if (colorEnumIdx >= colorArray.Length) colorEnumIdx = 0;
            string colorStr = colorArray[colorEnumIdx];
            colorEnumIdx++;

            return colorStr;


        }


    }
}

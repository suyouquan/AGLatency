using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.SqlServer.XEvent;
using Microsoft.SqlServer.XEvent.Linq;
using System.Data.SQLite;
using System.IO;

 
using System.Windows.Forms.DataVisualization.Charting;
 


namespace AGLatency.Latency
{

    public class LogBlockFlush_Sec //every second, how many log blocks processed and their metrics
    {
        public List<string> _FailedReadFields = new List<string>(); //first field so that GetFields will return it fast.
        public DateTime EventTimeStamp;
        [OutputExclude(true)]
        public Int64 secondDistance;//use to idenity how many seconds passed since the first record
        public Int32 database_id;
        public float Avg_Duration;
        [OutputExclude(true)]//don't want to show sum, since multiple log blocks flush concurrently? is it possible?
        public Int64 Sum_Duration;
        public Int64 Max_Duration;
        public Int64 Min_Duration;
        public Int64 LogBlocks;//how many logblocks in this second
        public Int64 Sum_write_size;
        //Don't show below, bring confuse
        [OutputExclude(true)]
        public float Avg_write_size;
        [OutputExclude(true)]
        public Int64 Max_write_size;
        [OutputExclude(true)]
        public Int64 Min_write_size;
    }

    /*
     According to http://rusanu.com/2012/01/17/what-is-an-lsn-log-sequence-number/
     log block id is from LSN , while LSN cross database could be the same although its chance is very very less
      (multiple databaase has the same LSN-->so they have same block id for xevent)
      the chance is very less, so i ignore the database during processing 

         */
    public  class LogBlockLocalHarden
    {
        public Replica server ;
        public     EventLatency eventLatency = null;
        public string chartHtml = "";
        public LogBlockLocalHarden(Replica location)
        {
            server = location;
        }
        /*
         * public static string getTimeDeltaUpdateSQL =
     "update log_flush_complete set TimeDelta="
     + "(select log_flush_complete.EventTimeStamp - log_flush_start.EventTimeStamp "
     + "   from log_flush_start where log_flush_start.log_block_id=log_flush_complete.log_block_id)"
     + " WHERE EXISTS (select * from log_flush_start where log_flush_start.log_block_id = log_flush_complete.log_block_id)";

     SELECT log_flush_start.EventTimeStamp,  log_flush_start.log_block_id,log_flush_complete.database_id,log_flush_complete.file_id,log_flush_complete.duration,  (log_flush_complete.EventTimeStamp-log_flush_start.EventTimeStamp) AS TimeDelta
FROM log_flush_start
INNER JOIN log_flush_complete
ON log_flush_start.log_block_id=log_flush_complete.log_block_id

         */
        //Multiple database, need to take care of that. 
        //but I assume the log block is unique across the database (Can I assume this way?)
        public   void Register()
        {

            if (server == Replica.Primary)
            {
                eventLatency = new EventLatency("LocalHarden_Primary");

                //   eventLatency.primaryEvents.
                //   Add(new EventWithMode(EventMetaData.xEvent.log_flush_start));

                //In log_flush_complete, there is "durataion" column, use that as the flush delta time (start to complete)
                eventLatency.primaryEvents.
                    Add(new EventWithMode(EventMetaData.xEvent.log_flush_complete));


                //Add it to xeloader
                XELoader.AddEventLatency(eventLatency);
            }

            else

            {
                eventLatency = new EventLatency("LocalHarden_Secondary");


                eventLatency.secondaryEvents.
                    Add(new EventWithMode(EventMetaData.xEvent.log_flush_complete));


                //Add it to xeloader
                XELoader.AddEventLatency(eventLatency);
            }
        }
        public void CreatePages()
        {
            var dict = GetPerfPointData(eventLatency.eventDB.SQLiteDBFile);

            foreach (KeyValuePair<int, List<Latency.LogBlockFlush_Sec>> kv in dict)
            {
                Pages.LogBlockLocalHarden page = 
                    new Pages.LogBlockLocalHarden(server,kv.Key, kv.Value, server.ToString() +" (db=" + kv.Key+")","Log Harden");

                page.GetData();
                // page.SavePageToDisk();

                PageTemplate.PageObject pageObj = new PageTemplate.PageObject("logblockHarden ", page, PageTemplate.PageObjState.SaveToDiskOnly);

                Controller.pageObjs.Add(pageObj);

            }
        }
        public   Dictionary<int,List<LogBlockFlush_Sec>> GetPerfPointData(string sqliteDBFile)
        {
            Dictionary<int, List<LogBlockFlush_Sec>> dict = new Dictionary<int, List<LogBlockFlush_Sec>>();


            string dbfile = sqliteDBFile;// @"C:\AGLatency\AGLatency\bin\Debug\SQLiteDB\LocalHarden_Primary_2__2018-07-27_22_06_38.175.SQLiteDB";
            SQLiteDB db = new SQLiteDB();
            db.Open(dbfile);

            String databaseNum = "SELECT DISTINCT database_id FROM log_flush_complete WHERE database_id>4";
            SQLiteDataReader dbidDR =
            db.ExecuteReader(databaseNum);

            List<int> databases = new List<int>();

            while (dbidDR.Read())
            {
                databases.Add(dbidDR.GetInt16(0));
            }

            databases.Sort();


            foreach (int id in databases)
            {
                dict.Add(id, new List<LogBlockFlush_Sec>());
            }
                
            //    String select = "SELECT   (EventTimeStamp/10000000) as EventTimeStamp,database_id, AVG(duration) as Avg_Duration,SUM(duration) as Sum_Duration, COUNT(*) as Flushes,SUM(write_size) as Sum_write_size from log_flush_complete group by  EventTimeStamp/10000000,database_id ORDER BY EventTimeStamp / 10000000,database_id";
            String select = "SELECT   (EventTimeStamp/10000000) as EventTimeStamp,database_id, COUNT(*) as LogBlocks, AVG(duration) as Avg_Duration,SUM(duration) as Sum_Duration, max(duration) as Max_duration, min(duration) as Min_duration, AVG(write_size) as Avg_write_size, SUM(write_size) as Sum_write_size, max(write_size) as Max_write_size,min(write_size) as Min_write_size from log_flush_complete WHERE database_id>4 group by  EventTimeStamp/10000000,database_id ORDER BY EventTimeStamp / 10000000,database_id";

            SQLiteDataReader dr =
            db.ExecuteReader(select);

            if (dr == null) return dict;

          
   
            bool isFirst = true;
       
            Int64 firstTimeStamp=0;
            while (dr.Read())
            {

                LogBlockFlush_Sec pfp = new LogBlockFlush_Sec();
                Int64 EventTimeStamp = dr.GetInt64(0)+1;//Add one more second , say, 1.220 should be map to 2.00 

                pfp. EventTimeStamp = new DateTime(EventTimeStamp * 10000000);
                pfp.database_id = dr.GetInt32(1);
                pfp.LogBlocks = dr.GetInt64(2);
                pfp.Avg_Duration = Math.Max(0, dr.GetFloat(3));
                pfp.Sum_Duration = Math.Max(0, dr.GetInt64(4));
                pfp.Max_Duration   = Math.Max(0, dr.GetInt64(5));
                pfp.Min_Duration = Math.Max(0, dr.GetInt64(6));

                pfp.Avg_write_size = dr.GetFloat(7);
                pfp.Sum_write_size = dr.GetInt64(8);
                pfp.Max_write_size = dr.GetInt64(9);
                pfp.Min_write_size = dr.GetInt64(10);


                if (isFirst)
                {
                    firstTimeStamp = EventTimeStamp;
                
                    isFirst = false;
                }
 
           

                pfp.secondDistance =   EventTimeStamp - firstTimeStamp;

                dict[pfp.database_id].Add(pfp);


            }



            db.CloseConnection();

            return dict;
        }
        public   void GeneratePerfMonCSV(string CSVfile)
        {

            string dbfile = @"C:\AGLatency\AGLatency\bin\Debug\SQLiteDB\LocalHarden_Primary_2__2018-07-27_22_06_38.175.SQLiteDB";
            SQLiteDB db = new SQLiteDB();
            db.Open(dbfile);

            String databaseNum = "SELECT DISTINCT database_id FROM log_flush_complete";
            SQLiteDataReader dbidDR =
            db.ExecuteReader(databaseNum);

            List<int> databases = new List<int>();

            while (dbidDR.Read())
            {
                databases.Add(dbidDR.GetInt16(0));
            }

            databases.Sort();
            //generate performance monitor CSV format file
            String select = "SELECT   (EventTimeStamp/10000000) as EventTimeStamp,database_id, AVG(duration) as Avg_Duration,SUM(duration) as Sum_Duration, COUNT(*) as LogBlocks, max(duration) as Max_duration, min(duration) as Min_duration, max( from log_flush_complete group by  EventTimeStamp/10000000,database_id ORDER BY EventTimeStamp / 10000000,database_id";


            SQLiteDataReader dr =
            db.ExecuteReader(select);

            if (dr == null) return;

            //Now time to save the resultset to file
            /*
             "(PDH-CSV 4.0) (UTC Standard Time)(0)","\\Primary\Database(1)\% LocalHarden Time","\\Primary\Database(2)\% LocalHarden Time"
             */

            string Title = "\"(PDH-CSV 4.0) (UTC Standard Time)(0)\"";
            foreach (int id in databases)
            {
                Title = Title + "," + "\"\\\\Primary\\Database(dbid=" + id + ")\\% LocalHarden Time\"";
            }
            CSVfile = CSVfile +"."+ System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_")+".CSV";
            
            File.AppendAllText(CSVfile, Title+"\n");

            Dictionary<int, LogBlockFlush_Sec> pointMap = new Dictionary<int, LogBlockFlush_Sec>();
            foreach(var id in databases)
            {
                pointMap.Add(id, new LogBlockFlush_Sec());
            }
            Int64 savedSecond = 0;
            StringBuilder sb = new StringBuilder();
            int batchSize = 1000;
            int writes = 0;
            DateTime firstDateTime = new DateTime();
            bool isFirst = true;

            while (dr.Read())
            {
             

                Int64 EventTimeStamp = dr.GetInt64(0);
                Int32 database_id = dr.GetInt32(1);
                float Avg_Duration = dr.GetFloat(2);
                Int64 Sum_Duration = dr.GetInt64(3);
                Int64 Flushes = dr.GetInt64(4);

                DateTime dt = new DateTime(EventTimeStamp * 10000000);
                Int64 second = dt.Second;

                if(isFirst)
                {
                    firstDateTime = new DateTime(EventTimeStamp * 10000000);
                    isFirst = false;
                }

                TimeSpan ts = dt - firstDateTime;
                Int64 secondDistance = (Int64) ts.TotalSeconds;

                if (savedSecond != second )
                {
                    //Get avg_duration first
                    string avg_duration_str = "";
                    foreach (KeyValuePair<int, LogBlockFlush_Sec> kv in pointMap)
                    {
                        avg_duration_str += ",\""+kv.Value.Avg_Duration.ToString()+"\"";
                    }
                    avg_duration_str ="\""+ dt.ToString("MM/dd/yyyy HH:mm:ss") +"\""+avg_duration_str;

                    sb.AppendLine(avg_duration_str);

                    savedSecond = second;


                    writes++;

                    if(writes==batchSize)
                    {
                        writes = 0;
                        File.AppendAllText(CSVfile, sb.ToString()+"\n");
                        sb.Clear();
                    }

                }


                var pdd = pointMap[database_id];
                pdd.database_id = database_id;
                pdd.Avg_Duration = Avg_Duration;
                pdd.EventTimeStamp = new DateTime(dt.Ticks - (dt.Ticks % TimeSpan.TicksPerSecond), dt.Kind);
                pdd.LogBlocks = Flushes;

              

            }




        }


    }
}




/*
 
     
        public List<string> _FailedReadFields = new List<string>(); //first field so that GetFields will return it fast.
        public Variable _diag;
        public SQLThread _sqlThread;

        public ulong SessionId;//0
        public string ThreadId = "";//1
        public EDiagThreadType _ediagThreadType;
        public ReqStatus Status;//2
        [ColumnDefs( ColumnAttr.VerticalExclude)]
        public string InputBuffer = ""; //3 th
        public uint database_id; //4
        public DateTime Start_time;//5
        
        

        public ulong wait_duration_ms; //6
        public string wait_description = ""; //7

 

        public string ThreadType = "";//  8 eDiagInvalid = 0n0   eDiagMainThread = 0n1   eDiagSubThread = 0n2   eDiagProxyMainThread = 0n3
        public uint open_transaction_count; //9
        public ulong cpu_time_ms; //10
        public ulong total_elapsed_time_ms; //11

        public uint scheduler_id; //12
        public string task_address = ""; //13

        public ulong reads; //14
        public ulong writes; //15
        public ulong logical_reads; //16


        public InputBufferEventType _EventType;
        public string EventType = ""; //17

        [ColumnDefs(ColumnAttr.Hidden | ColumnAttr.Long)]
        public string InputBuffer_ = "";//18,for rpc parameter good look,4th

        

        [ColumnDefs(ColumnAttr.Hidden | ColumnAttr.VerticalBig)]
        public string CallStack = "";


        [OutputExclude(true)]
        public static List<myInputBuffer> items = new List<myInputBuffer>();
     
     */

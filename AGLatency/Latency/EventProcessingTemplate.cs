using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SqlServer.XEvent;
using Microsoft.SqlServer.XEvent.Linq;
using System.Data.SQLite;
using System.IO;

namespace AGLatency.Latency
{

    public class EventRecord_Sec //every second, how many tran processed and their metrics
    {
        public List<string> _FailedReadFields = new List<string>(); //first field so that GetFields will return it fast.
        public DateTime EventTimeStamp;
     
       
        public Int64 Avg_ProcessingTime;
        [OutputExclude(true)]//don't want to show sum, since multiple log blocks flush concurrently? is it possible?
        public Int64 Sum_ProcessingTime;
        public Int64 Max_ProcessingTime;
        public Int64 Min_ProcessingTime;
        public Int64 Count;//how many events in this second

    }

    public class EventProcessingTemplate
    {

        public Replica server;
        public EventLatency eventLatency = null;
        public string chartHtml = "";
        public string processTimeFieldName = "";
        bool isPrimary = true;
        string eventName = "";
        //A list to record those databases in AG
        public List<Int32> databaseIDs = new List<int>();
        
        public List<string> preprocessingQueries = new List<string>();
        public EventProcessingTemplate(bool isPri,string timeField, EventMetaData.xEvent evt, int mode=-1, List<string> queries=null)
        {
            isPrimary = isPri;
            processTimeFieldName = timeField;
            eventName = evt.ToString(); ;
            Register(evt,mode);
           
           if(queries!=null)  preprocessingQueries = queries;
        }

        private void PreProcessing(SQLiteDB db,List<string> queries)
        {
            if (queries == null) return;
            foreach(string sql in queries)
            {
                try
                {
                    Logger.LogMessage(db.databaseName + ":EXECUTE:" + sql);

                    db.Execute(sql);
                }
                catch(Exception ex)
                {
                    Logger.LogException(ex, Thread.CurrentThread);
                }
            }

        }

        //Multiple database, need to take care of that. 

        public void Register(EventMetaData.xEvent evt,int mode)
        {
            string tag = "_primary";
            if (!isPrimary) tag = "_secondary";
            eventLatency = new EventLatency(eventName+tag);

            if (isPrimary  )
            {
                eventLatency.primaryEvents.Add(new EventWithMode(evt,mode));           
            }

            else
            {
                eventLatency.secondaryEvents.Add(new EventWithMode(evt,mode));
            }

            XELoader.AddEventLatency(eventLatency);
        }
         public void CreatePages()
         {
            string group = "Primary Statistics";
            if (!isPrimary) group = "Secondary Statistics";

            var list = GetPerfPointData(eventLatency.eventDB.SQLiteDBFile);

            //    foreach (KeyValuePair<int, List<Latency.TranProcessingTime_Sec>> kv in dict)
            //    {
            //        Pages.TranProcessingTimePage page =
            //            new Pages.TranProcessingTimePage(server, kv.Key, kv.Value, "Commit (db=" + kv.Key + ")", group);

            //        page.GetData();
            //        // page.SavePageToDisk();

            //        PageTemplate.PageObject pageObj = new PageTemplate.PageObject("TranProcessingTime ", page, PageTemplate.PageObjState.SaveToDiskOnly);

            //        Controller.pageObjs.Add(pageObj);

            //    }
        }

        //this function is for hadr_db_commit_mgr_harden
        public List<Int32> GetDatabaseIDs(string sqliteDBFile)
        {
            List<Int32> list = new List<int>();


            string dbfile = sqliteDBFile;// @"C:\AGLatency\AGLatency\bin\Debug\SQLiteDB\LocalHarden_Primary_2__2018-07-27_22_06_38.175.SQLiteDB";
            SQLiteDB db = new SQLiteDB();
            db.Open(dbfile);

            String databaseNum = "SELECT DISTINCT database_id FROM hadr_db_commit_mgr_harden WHERE database_id>4";
            SQLiteDataReader dbidDR =
            db.ExecuteReader(databaseNum);

            

            if (dbidDR == null) return list;

            while (dbidDR.Read())
            {
                list.Add(dbidDR.GetInt16(0));
            }
            db.CloseConnection();

            list.Sort();
            return list;

        }
        public List<EventRecord_Sec> GetPerfPointData(string sqliteDBFile=null)
        {
             List<EventRecord_Sec> list = new List<EventRecord_Sec>();


            string dbfile = sqliteDBFile;// @"C:\AGLatency\AGLatency\bin\Debug\SQLiteDB\LocalHarden_Primary_2__2018-07-27_22_06_38.175.SQLiteDB";
            if (String.IsNullOrEmpty(dbfile))
                dbfile = eventLatency.eventDB.SQLiteDBFile;
            SQLiteDB db = new SQLiteDB();
            db.Open(dbfile);

            //preprocessing, like delete records, add index, etc
            PreProcessing(db,preprocessingQueries);
 
            //    String select = "SELECT   (EventTimeStamp/10000000) as EventTimeStamp,database_id, AVG(duration) as Avg_Duration,SUM(duration) as Sum_Duration, COUNT(*) as Flushes,SUM(write_size) as Sum_write_size from log_flush_complete group by  EventTimeStamp/10000000,database_id ORDER BY EventTimeStamp / 10000000,database_id";
           String select = @"SELECT   (EventTimeStamp/10000000) as EventTimeStamp,
                 COUNT(*) as count, AVG("
                 + processTimeFieldName + ") as Avg_ProcessingTime,SUM("
                 + processTimeFieldName + ") as Sum_ProcessingTime, MAX("
                 + processTimeFieldName + ") as Max_ProcessingTime, MIN("
                 + processTimeFieldName + ") as Min_ProcessingTime  FROM "
                 + eventName+"    GROUP BY EventTimeStamp/10000000 ";

            Logger.LogMessage(select);

            SQLiteDataReader dr =
            db.ExecuteReader(select);

            if (dr == null) return list;



            bool isFirst = true;

            Int64 firstTimeStamp = 0;
            while (dr.Read())
            {

                EventRecord_Sec pfp = new EventRecord_Sec();
                Int64 EventTimeStamp = dr.GetInt64(0) + 1;//Add one more second , say, 1.220 should be mapped to 2.00 

                pfp.EventTimeStamp = new DateTime(EventTimeStamp * 10000000);
            
                pfp.Count = dr.GetInt64(1);
                
                pfp.Avg_ProcessingTime = (Int64) Math.Max(0, dr.GetFloat(2) );
                pfp.Sum_ProcessingTime = Math.Max(0, dr.GetInt64(3) );
                pfp.Max_ProcessingTime = Math.Max(0, dr.GetInt64(4) );
                pfp.Min_ProcessingTime = Math.Max(0, dr.GetInt64(5) );




                if (isFirst)
                {
                    firstTimeStamp = EventTimeStamp;

                    isFirst = false;
                }



               

              list.Add(pfp);


            }



            db.CloseConnection();

            //order it
            list = list.OrderBy(p => p.EventTimeStamp).ToList();

            return list;
        }



    }
}

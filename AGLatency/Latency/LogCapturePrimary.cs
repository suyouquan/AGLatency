using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SqlServer.XEvent;
using Microsoft.SqlServer.XEvent.Linq;
using System.Data.SQLite;
using System.IO;

namespace AGLatency.Latency
{
    //Need to consider multiple replicas

    public class LogCapture_Sec //every second, how many log blocks processed and their metrics
    {
     

        public List<string> _FailedReadFields = new List<string>(); //first field so that GetFields will return it fast.
        public DateTime EventTimeStamp;
        [OutputExclude(true)]
        public Int64 secondDistance;//use to idenity how many seconds passed since the first record
        [OutputExclude(true)]
        public string availability_replica_id;//	3F1B42BD-D297-4FA5-AEEF-8CAD261125C6
        public Int64 Avg_Latency;
        [OutputExclude(true)]//don't want to show sum, since multiple log blocks flush concurrently? is it possible?
        public Int64 Sum_Latency;
        public Int64 Max_Latency;
        public Int64 Min_Latency;
        public Int64 LogBlocks;//how many logblocks in this second
    }
    public   class LogCapturePrimary
    {
        private   EventLatency eventLatency = null;

        public   void Register()
        {
            eventLatency = new EventLatency("Hadr_capture_log_block_primary");


            eventLatency.primaryEvents.
                Add(new EventWithMode(EventMetaData.xEvent.hadr_capture_log_block, 1));


            eventLatency.primaryEvents.
                Add(new EventWithMode(EventMetaData.xEvent.hadr_capture_log_block, 4));


            //Add it to xeloader
            XELoader.AddEventLatency(eventLatency);

        }

        public void CreatePages()
        {
            var dict = GetPerfPointData(eventLatency.eventDB.SQLiteDBFile);

            foreach (KeyValuePair<string, List<Latency.LogCapture_Sec>> kv in dict)
            {
                string groupTitle = "LogCapturePrimary";
                 
                Pages.LogCapturePrimaryPage page =
                    new Pages.LogCapturePrimaryPage(kv.Key, kv.Value, "Replica" + "(" + kv.Key + ")", groupTitle);

                page.GetData();
                // page.SavePageToDisk();

                PageTemplate.PageObject pageObj = new PageTemplate.PageObject("logcapture", page, PageTemplate.PageObjState.SaveToDiskOnly);

                Controller.pageObjs.Add(pageObj);


            }
        }

        public Dictionary<string, List<LogCapture_Sec>> GetPerfPointData(string sqliteDBFile)
        {
            Dictionary<string, List<LogCapture_Sec>> dict = new Dictionary<string, List<LogCapture_Sec>>();


            string dbfile = sqliteDBFile;// @"C:\AGLatency\AGLatency\bin\Debug\SQLiteDB\LocalHarden_Primary_2__2018-07-27_22_06_38.175.SQLiteDB";
            SQLiteDB db = new SQLiteDB();
            db.Open(dbfile);

            String databaseNum = "SELECT DISTINCT availability_replica_id	  FROM hadr_capture_log_block";
            SQLiteDataReader replicaDr =
            db.ExecuteReader(databaseNum);

            List<string> replicas = new List<string>();

            if (replicaDr == null) return dict;

            while (replicaDr.Read())
            {
                replicas.Add(replicaDr.GetString(0));
            }

            replicas.Sort();


            foreach (string r in replicas)
            {
                dict.Add(r, new List<LogCapture_Sec>());
            }


            string idx1 = "CREATE INDEX lb ON hadr_capture_log_block (log_block_id,availability_replica_id,mode )";
            db.Execute(idx1);

            // string idx2 = "CREATE INDEX lb2 ON log_block_pushed_to_logpool (log_block_id )";
            // db.Execute(idx2);
            string update = @"UPDATE hadr_capture_log_block set TimeDelta=
                (SELECT hadr_capture_log_block.EventTimeStamp-B.EventTimeStamp from hadr_capture_log_block AS B WHERE B.log_block_id = hadr_capture_log_block.log_block_id AND B.availability_replica_id = hadr_capture_log_block.availability_replica_id AND B.mode = 1)
                WHERE hadr_capture_log_block.mode = 4";
            //Update time delta for each log blocks
            string update2 = @"UPDATE hadr_capture_log_block set TimeDelta=
           (SELECT hadr_capture_log_block.EventTimeStamp - log_block_pushed_to_logpool.EventTimeStamp
            FROM log_block_pushed_to_logpool  
            WHERE log_block_pushed_to_logpool.log_block_id = hadr_capture_log_block.log_block_id )
            WHERE EXISTS (SELECT * FROM log_block_pushed_to_logpool  WHERE log_block_pushed_to_logpool.log_block_id = hadr_capture_log_block.log_block_id)";

            db.Execute(update);

            string select = @"
            SELECT   (EventTimeStamp/10000000) as EventTimeStamp,availability_replica_id, COUNT(*) as LogBlocks, AVG(TimeDelta) as Avg_latency,SUM(TimeDelta) as Sum_latency, max(TimeDelta) as Max_latency, min(TimeDelta) as Min_latency
            FROM hadr_capture_log_block  
            WHERE TimeDelta is not null
            GROUP BY  EventTimeStamp/10000000,availability_replica_id ORDER BY EventTimeStamp / 10000000,availability_replica_id";




            SQLiteDataReader dr =
            db.ExecuteReader(select);

            if (dr == null) return dict;



            bool isFirst = true;

            Int64 firstTimeStamp = 0;
            while (dr.Read())
            {

                LogCapture_Sec pfp = new LogCapture_Sec();
                Int64 EventTimeStamp = dr.GetInt64(0) + 1;//Add one more second , say, 1.220 should be map to 2.00 

                pfp.EventTimeStamp = new DateTime(EventTimeStamp * 10000000);
                pfp.availability_replica_id = dr.GetString(1);
                pfp.LogBlocks = dr.GetInt64(2);
                pfp.Avg_Latency = Math.Max(0, (Int64)(dr.GetFloat(3) / 10000));
                pfp.Sum_Latency = Math.Max(0, dr.GetInt64(4) / 10000);
                pfp.Max_Latency = Math.Max(0, dr.GetInt64(5) / 10000);
                pfp.Min_Latency = Math.Max(0, dr.GetInt64(6) / 10000);

              


                if (isFirst)
                {
                    firstTimeStamp = EventTimeStamp;

                    isFirst = false;
                }



                pfp.secondDistance = EventTimeStamp - firstTimeStamp;

                dict[pfp.availability_replica_id].Add(pfp);


            }



            db.CloseConnection();

            return dict;
        }











    }
}
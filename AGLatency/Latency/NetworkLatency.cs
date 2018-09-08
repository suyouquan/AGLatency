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
    public class LogBlockNetLatency_Sec //every second, how many log blocks processed and their metrics
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
    //From primary to secondary
    public class NetworkLatency
    {
        public static string replicaId = "";
        private EventLatency networkLatency = null;
        public NetworkDirection networkDir;
        //could have multiple replicas, but we just take care of replica only, because
        //we just provide   xevent trace from one replica in the UI
        //So we just read the replica from hte secondry and process that replica

        public static string getTimeDeltaUpdateSQL =
           "update log_flush_complete set TimeDelta="
           + "(select log_flush_complete.EventTimeStamp - log_flush_start.EventTimeStamp "
           + "   from log_flush_start where log_flush_start.log_block_id=log_flush_complete.log_block_id)"
           + " WHERE EXISTS (select * from log_flush_start where log_flush_start.log_block_id = log_flush_complete.log_block_id)";

        public NetworkLatency(NetworkDirection dir)
        {
            networkDir = dir;
        }
        public void Register()
        {

            if (networkDir == NetworkDirection.Primary_To_Secondary)
            {
                networkLatency = new EventLatency(networkDir.ToString());

                /*
                 mode 4:Occurs after the dequeued message reaches Replica layer and before sending to transport (UCS). Only message routing actions between mode 3 and 4
                 using 2 because some xevent doesn't have mode=4
                 */
                networkLatency.primaryEvents.
                    Add(new EventWithMode(EventMetaData.xEvent.hadr_capture_log_block, 2));

                //mode=1 or 2. 1 means Occurs when receiving new log block message from transport (UCS)
                networkLatency.secondaryEvents.
                    Add(new EventWithMode(EventMetaData.xEvent.hadr_transport_receive_log_block_message, 1));


                //Add it to xeloader
                XELoader.AddEventLatency(networkLatency);
            }


        }

        public void CreatePages()
        {
            var dict = GetPerfPointData(networkLatency.eventDB.SQLiteDBFile);

            foreach (KeyValuePair<string, List<Latency.LogBlockNetLatency_Sec>> kv in dict)
            {
                string groupTitle = "Primary Send";
                if (networkDir == NetworkDirection.Secondary_To_Primary) groupTitle = "Secondary Send";
                Pages.NetworkLatencyPage page =
                    new Pages.NetworkLatencyPage(kv.Key, kv.Value, "Replica" + "(" + kv.Key + ")", groupTitle);

                page.GetData();
                //  page.SavePageToDisk();

                PageTemplate.PageObject pageObj = new PageTemplate.PageObject("network latency", page, PageTemplate.PageObjState.SaveToDiskOnly);

                Controller.pageObjs.Add(pageObj);


            }
        }

        public Dictionary<string, List<LogBlockNetLatency_Sec>> GetPerfPointData(string sqliteDBFile)
        {
            Dictionary<string, List<LogBlockNetLatency_Sec>> dict = new Dictionary<string, List<LogBlockNetLatency_Sec>>();


            string dbfile = sqliteDBFile;// @"C:\AGLatency\AGLatency\bin\Debug\SQLiteDB\LocalHarden_Primary_2__2018-07-27_22_06_38.175.SQLiteDB";
            SQLiteDB db = new SQLiteDB();
            db.Open(dbfile);

            String databaseNum = "SELECT DISTINCT local_availability_replica_id	 FROM hadr_transport_receive_log_block_message";
            SQLiteDataReader replicaDr =
            db.ExecuteReader(databaseNum);

            List<string> replicas = new List<string>();

            if (replicaDr == null) return dict;

            while (replicaDr.Read())
            {
                replicas.Add(replicaDr.GetString(0));
            }

            replicas.Sort();
            if (replicas.Count > 1)
                Logger.LogMessage("[WARNING] Two many replicas found! num:" + replicas.Count);

            //Save this id, since SyncReceiveNetLatency will reference this id to know which  replica to use in hadr_receive_harden_lsn_message
            replicaId = replicas[0];

            foreach (string r in replicas)
            {
                dict.Add(r, new List<LogBlockNetLatency_Sec>());
            }


            string idx1 = "CREATE INDEX lb ON hadr_capture_log_block (log_block_id,availability_replica_id )";
            db.Execute(idx1);

            string idx2 = "CREATE INDEX lb2 ON hadr_transport_receive_log_block_message (log_block_id,local_availability_replica_id )";
            db.Execute(idx2);

            //Update time delta for each log blocks
            string update = @"UPDATE hadr_transport_receive_log_block_message set TimeDelta=
           (SELECT hadr_transport_receive_log_block_message.EventTimeStamp - hadr_capture_log_block.EventTimeStamp
            FROM hadr_capture_log_block  
            WHERE hadr_capture_log_block.log_block_id = hadr_transport_receive_log_block_message.log_block_id and hadr_capture_log_block. availability_replica_id=hadr_transport_receive_log_block_message.local_availability_replica_id)
            WHERE EXISTS (SELECT * FROM hadr_capture_log_block  WHERE hadr_capture_log_block.log_block_id = hadr_transport_receive_log_block_message.log_block_id and hadr_capture_log_block. availability_replica_id=hadr_transport_receive_log_block_message.local_availability_replica_id)";

            db.Execute(update);

            string select = @"
            SELECT   (EventTimeStamp/10000000) as EventTimeStamp,local_availability_replica_id, COUNT(*) as LogBlocks, AVG(TimeDelta) as Avg_latency,SUM(TimeDelta) as Sum_latency, max(TimeDelta) as Max_latency, min(TimeDelta) as Min_latency
            FROM hadr_transport_receive_log_block_message  
            WHERE TimeDelta is not null
            GROUP BY  EventTimeStamp/10000000,local_availability_replica_id ORDER BY EventTimeStamp / 10000000,local_availability_replica_id";




            SQLiteDataReader dr =
            db.ExecuteReader(select);

            if (dr == null) return dict;



            bool isFirst = true;

            Int64 firstTimeStamp = 0;
            while (dr.Read())
            {

                LogBlockNetLatency_Sec pfp = new LogBlockNetLatency_Sec();
                Int64 EventTimeStamp = dr.GetInt64(0) + 1;//Add one more second , say, 1.220 should be map to 2.00 

                pfp.EventTimeStamp = new DateTime(EventTimeStamp * 10000000);
                pfp.availability_replica_id = dr.GetString(1);
                pfp.LogBlocks = dr.GetInt64(2);
                pfp.Avg_Latency =Math.Max(0, (Int64)( dr.GetFloat(3)/10000));
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

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
    
    //For sync replica, secondary to primary
    //works only one sync second replica
    //If there is multiple sync replicas, then ?? seems we need to know the id of the secondary replica
    public class SyncReceiveNetLatency
    {
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

        public SyncReceiveNetLatency(NetworkDirection dir)
        {
            networkDir = dir;
        }
        public void Register()
        {

            if (networkDir == NetworkDirection.Secondary_To_Primary)
            {
                networkLatency = new EventLatency(networkDir.ToString());

                /*
                 mode 4:Occurs after the dequeued message reaches Replica layer and before sending to transport (UCS). Only message routing actions between mode 3 and 4
                 */
                networkLatency.primaryEvents.
                    Add(new EventWithMode(EventMetaData.xEvent.hadr_receive_harden_lsn_message, 1));

                //mode=1 or 2. 1 means Occurs when receiving new log block message from transport (UCS)
                networkLatency.secondaryEvents.
                    Add(new EventWithMode(EventMetaData.xEvent.hadr_send_harden_lsn_message, 3));


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
                Pages.SyncReceiveNetLatencyPage page =
                    new Pages.SyncReceiveNetLatencyPage(kv.Key, kv.Value, "Replica" + "(" + kv.Key + ")", groupTitle);

                page.GetData();
                //  page.SavePageToDisk();
                PageTemplate.PageObject pageObj = new PageTemplate.PageObject("sync receive latency", page, PageTemplate.PageObjState.SaveToDiskOnly);

                Controller.pageObjs.Add(pageObj);


            }
        }

        public Dictionary<string, List<LogBlockNetLatency_Sec>> GetPerfPointData(string sqliteDBFile)
        {
            Dictionary<string, List<LogBlockNetLatency_Sec>> dict = new Dictionary<string, List<LogBlockNetLatency_Sec>>();


            string dbfile = sqliteDBFile;// @"C:\AGLatency\AGLatency\bin\Debug\SQLiteDB\LocalHarden_Primary_2__2018-07-27_22_06_38.175.SQLiteDB";
            SQLiteDB db = new SQLiteDB();
            db.Open(dbfile);

            String databaseNum = "SELECT DISTINCT target_availability_replica_id	   FROM hadr_receive_harden_lsn_message";
            SQLiteDataReader replicaDr =
            db.ExecuteReader(databaseNum);

            List<string> replicas = new List<string>();
            if (null == replicaDr) return dict;

            while (replicaDr.Read())
            {
                replicas.Add(replicaDr.GetString(0));
            }

            replicas.Sort();


            foreach (string r in replicas)
            {
                dict.Add(r, new List<LogBlockNetLatency_Sec>());
            }


            string idx1 = "CREATE INDEX lb ON hadr_receive_harden_lsn_message (log_block_id,target_availability_replica_id )";
            db.Execute(idx1);

            string idx2 = "CREATE INDEX lb2 ON hadr_send_harden_lsn_message (log_block_id)";
            db.Execute(idx2);

            //record which replica to caculate. Only one is valid since we just allow two replicas
            string replicaId = NetworkLatency.replicaId;

            //Update time delta for each log blocks
            string update = @"UPDATE hadr_receive_harden_lsn_message set TimeDelta=
           (SELECT hadr_receive_harden_lsn_message.EventTimeStamp - hadr_send_harden_lsn_message.EventTimeStamp
            FROM hadr_send_harden_lsn_message  
            WHERE hadr_send_harden_lsn_message.log_block_id = hadr_receive_harden_lsn_message.log_block_id AND hadr_receive_harden_lsn_message.target_availability_replica_id='" + replicaId+"')"
            +"WHERE EXISTS (SELECT * FROM hadr_send_harden_lsn_message  WHERE hadr_send_harden_lsn_message.log_block_id = hadr_receive_harden_lsn_message.log_block_id)";

            db.Execute(update);

            string select = @"
            SELECT   (EventTimeStamp/10000000) as EventTimeStamp, target_availability_replica_id,COUNT(*) as LogBlocks, AVG(TimeDelta) as Avg_latency,SUM(TimeDelta) as Sum_latency, max(TimeDelta) as Max_latency, min(TimeDelta) as Min_latency
            FROM hadr_receive_harden_lsn_message  
            WHERE TimeDelta is not null AND  target_availability_replica_id='" + replicaId+"'"
            + "GROUP BY  EventTimeStamp/10000000,target_availability_replica_id ORDER BY EventTimeStamp / 10000000,target_availability_replica_id";




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

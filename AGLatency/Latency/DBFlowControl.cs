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

    public class FlowControl_Sec //every second, how many log blocks processed and their metrics
    {
     

        public List<string> _FailedReadFields = new List<string>(); //first field so that GetFields will return it fast.
        public DateTime EventTimeStamp;
        [OutputExclude(true)]
        public Int64 secondDistance;//use to idenity how many seconds passed since the first record
        [OutputExclude(true)]
        public string database_replica_id;//	3F1B42BD-D297-4FA5-AEEF-8CAD261125C6

        public Int64 Avg_Duration;

        public Int64 Sum_Duration;
        public Int64 Max_Duration;
        public Int64 Min_Duration;
        public Int64 Occurence;//how many times flow control occurs in this second
    }
    public   class DBFlowControl
    {
        private   EventLatency eventLatency = null;

        public   void Register()
        {
            eventLatency = new EventLatency("hadr_database_flow_control_action");


            eventLatency.primaryEvents.
                Add(new EventWithMode(EventMetaData.xEvent.hadr_database_flow_control_action));


           // eventLatency.primaryEvents.
         //       Add(new EventWithMode(EventMetaData.xEvent.hadr_capture_log_block, 4));


            //Add it to xeloader
            XELoader.AddEventLatency(eventLatency);

        }

        public void CreatePages()
        {
            var dict = GetPerfPointData(eventLatency.eventDB.SQLiteDBFile);
            if (dict == null) return;
            foreach (KeyValuePair<string, List<Latency.FlowControl_Sec>> kv in dict)
            {
                string groupTitle = "DBFlowControl";
                 
                Pages.DBFlowControlPage page =
                    new Pages.DBFlowControlPage(NetworkLatency.replicaId, kv.Value, "DB" + "(" + kv.Key + ")", groupTitle,kv.Key);

                page.GetData();

                PageTemplate.PageObject pageObj = new PageTemplate.PageObject("dbflowcontrolpage", page, PageTemplate.PageObjState.SaveToDiskOnly);

                Controller.pageObjs.Add(pageObj);
                //page.SavePageToDisk();

            }
        }

        public Dictionary<string, List<FlowControl_Sec>> GetPerfPointData(string sqliteDBFile)
        {
            Dictionary<string, List<FlowControl_Sec>> dict = new Dictionary<string, List<FlowControl_Sec>>();


            string dbfile = sqliteDBFile;// @"C:\AGLatency\AGLatency\bin\Debug\SQLiteDB\LocalHarden_Primary_2__2018-07-27_22_06_38.175.SQLiteDB";
            SQLiteDB db = new SQLiteDB();
            db.Open(dbfile);

            String databaseNum = "SELECT DISTINCT database_replica_id  FROM hadr_database_flow_control_action WHERE local_availability_replica_id='"+NetworkLatency.replicaId+"'";
            SQLiteDataReader replicaDr =
            db.ExecuteReader(databaseNum);
            if (replicaDr == null) return null;

            List<string> replicas = new List<string>();
            if (replicaDr == null) return dict;
            while (replicaDr.Read())
            {
                replicas.Add(replicaDr.GetString(0));
            }

            replicas.Sort();


            foreach (string r in replicas)
            {
                dict.Add(r, new List<FlowControl_Sec>());
            }

            
            string idx1 = "CREATE INDEX lb ON hadr_database_flow_control_action (local_availability_replica_id,control_action)";
            db.Execute(idx1);

            string select = @"
             SELECT   (EventTimeStamp/10000000) as EventTimeStamp,database_replica_id, COUNT(*) as Occurence, AVG(Duration) as Avg_latency,SUM(Duration) as Sum_latency, max(Duration) as Max_latency, min(Duration) as Min_latency
             FROM hadr_database_flow_control_action  
             WHERE local_availability_replica_id = '" + NetworkLatency.replicaId + "' AND control_action='Cleared'"

           + " GROUP BY  EventTimeStamp/10000000,database_replica_id ORDER BY EventTimeStamp / 10000000,database_replica_id";




            SQLiteDataReader dr =
            db.ExecuteReader(select);

            if (dr == null) return dict;



            bool isFirst = true;

            Int64 firstTimeStamp = 0;
            while (dr.Read())
            {

                FlowControl_Sec pfp = new FlowControl_Sec();
                Int64 EventTimeStamp = dr.GetInt64(0) + 1;//Add one more second , say, 1.220 should be map to 2.00 

                pfp.EventTimeStamp = new DateTime(EventTimeStamp * 10000000);
                pfp.database_replica_id = dr.GetString(1);
                pfp.Occurence = dr.GetInt64(2);

                //the duration/1000=ms
                pfp.Avg_Duration = Math.Max(0, (Int64)(dr.GetFloat(3) / 1000));
                pfp.Sum_Duration = Math.Max(0, dr.GetInt64(4) / 1000);
                pfp.Max_Duration = Math.Max(0, dr.GetInt64(5) / 1000);
                pfp.Min_Duration = Math.Max(0, dr.GetInt64(6) / 1000);

              


                if (isFirst)
                {
                    firstTimeStamp = EventTimeStamp;

                    isFirst = false;
                }



                pfp.secondDistance = EventTimeStamp - firstTimeStamp;

                dict[pfp.database_replica_id].Add(pfp);


            }



            db.CloseConnection();

            return dict;
        }











    }
}
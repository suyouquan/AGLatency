using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace AGLatency
{


    /*



select 
AGNode.group_name
,AGNode.replica_server_name
,AGNode.node_name,ReplicaState.role,ReplicaState.role_desc
,ReplicaState.is_local
,DatabaseState.database_id
,db_name(DatabaseState.database_id) as database_name
,DatabaseState.group_database_id
,DatabaseState.is_commit_participant
,DatabaseState.is_primary_replica
,DatabaseState.synchronization_state_desc
,DatabaseState.synchronization_health_desc
,ClusterState.group_id
,ReplicaState.replica_id

from sys.dm_hadr_availability_replica_cluster_nodes AGNode
join sys.dm_hadr_availability_replica_cluster_states ClusterState 
on AGNode.replica_server_name = ClusterState.replica_server_name 
join sys.dm_hadr_availability_replica_states ReplicaState  
on ReplicaState.replica_id = ClusterState.replica_id 
join sys.dm_hadr_database_replica_states DatabaseState 
on ReplicaState.replica_id=DatabaseState.replica_id
for XML  RAW, ROOT('AGInfoRoot') 


<AGInfoRoot>
<row group_name="AG2016" replica_server_name="NODE1\SQLAG" node_name="NODE1" role="1" role_desc="PRIMARY" is_local="1" database_id="6" database_name="memClerks" group_database_id="4F966D84-D3C9-4824-A539-7265B933E92F" is_commit_participant="1" is_primary_replica="1" synchronization_state_desc="SYNCHRONIZED" synchronization_health_desc="HEALTHY" group_id="216DFF44-9FAA-41D6-8758-C155576C7F84" replica_id="20944245-5A76-41AC-8CC7-985AB3B4E068" />
<row group_name="AG2016" replica_server_name="NODE1\SQLAG" node_name="NODE1" role="1" role_desc="PRIMARY" is_local="1" database_id="7" database_name="slowDB" group_database_id="F3400915-BB03-42A0-8C5E-26D35506A893" is_commit_participant="1" is_primary_replica="1" synchronization_state_desc="SYNCHRONIZED" synchronization_health_desc="HEALTHY" group_id="216DFF44-9FAA-41D6-8758-C155576C7F84" replica_id="20944245-5A76-41AC-8CC7-985AB3B4E068" />
<row group_name="AG2016" replica_server_name="NODE2\SQLAG" node_name="NODE2" role="2" role_desc="SECONDARY" is_local="0" database_id="6" database_name="memClerks" group_database_id="4F966D84-D3C9-4824-A539-7265B933E92F" is_commit_participant="1" is_primary_replica="0" synchronization_state_desc="SYNCHRONIZED" synchronization_health_desc="HEALTHY" group_id="216DFF44-9FAA-41D6-8758-C155576C7F84" replica_id="6DBAD5CA-2423-4BB1-88E7-E53A9EC00F20" />
<row group_name="AG2016" replica_server_name="NODE2\SQLAG" node_name="NODE2" role="2" role_desc="SECONDARY" is_local="0" database_id="7" database_name="slowDB" group_database_id="F3400915-BB03-42A0-8C5E-26D35506A893" is_commit_participant="1" is_primary_replica="0" synchronization_state_desc="SYNCHRONIZED" synchronization_health_desc="HEALTHY" group_id="216DFF44-9FAA-41D6-8758-C155576C7F84" replica_id="6DBAD5CA-2423-4BB1-88E7-E53A9EC00F20" />
<row group_name="AG2016" replica_server_name="NODE3" node_name="NODE3" role="2" role_desc="SECONDARY" is_local="0" database_id="6" database_name="memClerks" group_database_id="4F966D84-D3C9-4824-A539-7265B933E92F" is_commit_participant="0" is_primary_replica="0" synchronization_state_desc="SYNCHRONIZING" synchronization_health_desc="HEALTHY" group_id="216DFF44-9FAA-41D6-8758-C155576C7F84" replica_id="D66EF97C-1924-43BE-AA82-783A4D4FE4DB" />
<row group_name="AG2016" replica_server_name="NODE3" node_name="NODE3" role="2" role_desc="SECONDARY" is_local="0" database_id="7" database_name="slowDB" group_database_id="F3400915-BB03-42A0-8C5E-26D35506A893" is_commit_participant="0" is_primary_replica="0" synchronization_state_desc="SYNCHRONIZING" synchronization_health_desc="HEALTHY" group_id="216DFF44-9FAA-41D6-8758-C155576C7F84" replica_id="D66EF97C-1924-43BE-AA82-783A4D4FE4DB" />
</AGInfoRoot>
    */

    public class ReplicaNode
    {
        public List<string> _FailedReadFields = new List<string>(); //first field so that GetFields will return it fast.
        public string replica_server_name;
        public string role_desc;
        public Int32 is_commit_participant;//sync mode or async mode
        public string replica_id;


    }
    public class AGInfo
    {
        public ReplicaNode node;
        public HashSet<Int32> database_id = new HashSet<int>();
        public HashSet<string> group_database_id = new HashSet<string>();
        public HashSet<string> group_id = new HashSet<string>();
        public HashSet<string> group_name = new HashSet<string>();
        public Dictionary<string, ReplicaNode> replicaDict = new Dictionary<string, ReplicaNode>();
        public List<ReplicaNode> nodes = new List<ReplicaNode>();





        public static AGInfo LoadAGInfo(string filename)
        {
            AGInfo ag = new AGInfo();
            try
            {


                DataSet ds = new DataSet();
                ds.ReadXml(filename);



                foreach (DataTable table in ds.Tables)
                {

                    foreach (var row in table.AsEnumerable())
                    {
                        ReplicaNode node = new ReplicaNode();

                        node = new ReplicaNode();
                        node.replica_id = row["replica_id"].ToString();
                        node.role_desc = row["role_desc"].ToString();
                        node.replica_server_name = row["replica_server_name"].ToString();
                        node.is_commit_participant = Int32.Parse(row["is_commit_participant"].ToString());

                        if (row["is_local"].ToString() == "1")
                        {
                            ag.node = node;
                        }

                        if (!ag.replicaDict.ContainsKey(node.replica_server_name))
                        {
                            ag.replicaDict.Add(node.replica_server_name, node);
                            ag.nodes.Add(node);
                        }

                        ag.database_id.Add(Int32.Parse(row["database_id"].ToString()));
                        ag.group_database_id.Add(row["group_database_id"].ToString());



                        ag.group_id.Add(row["group_id"].ToString());
                        ag.group_name.Add(row["group_name"].ToString());


                    }
                }

            }
            catch (Exception ex)
            {
                Logger.LogException(ex, Thread.CurrentThread);
            }

            return ag;
        }






    }
}
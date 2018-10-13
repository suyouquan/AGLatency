# AGLatency

AGLatency is a tool to analyze AG log block movement latency between replicas and create report accordingly.

![alt text](https://github.com/suyouquan/AGLatency/blob/master/AGLatency.png)

You can download it here:
https://github.com/suyouquan/AGLatency/releases/download/V1.0/AGLatencyV1.01.zip

Video:
https://github.com/suyouquan/AGLatency/blob/master/AGLatency.mp4


You capture log block Xevent trace from both primary and secondary replica for 5-10 minutes and then this tool will 
generate report about the latency of the log block movement.

![alt text](https://github.com/suyouquan/AGLatency/blob/master/AGLatencyReport.png)



Reference link:<br>
https://blogs.msdn.microsoft.com/psssql/2018/04/05/troubleshooting-data-movement-latency-between-synchronous-commit-always-on-availability-groups/

New in SSMS – Always On Availability Group Latency Reports
https://blogs.msdn.microsoft.com/sql_server_team/new-in-ssms-always-on-availability-group-latency-reports/

#### Steps to capture logs to feed the tool:

##### Step 1:Run below SQL statement from both primary and secondary, and save the result as primary.xml and secondary.xml accordingly.
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

##### Step 2: Run below script on both primary and secondary at the same time, for about 5-10 minutes, and then stop it.
<br>--You can change "c:\temp\" to other folder accordingly.  

CREATE EVENT SESSION [AlwaysOn_Data_Movement_Tracing] ON SERVER   
ADD EVENT sqlserver.hadr_apply_log_block,  
ADD EVENT sqlserver.hadr_capture_filestream_wait,  
ADD EVENT sqlserver.hadr_capture_log_block,  
ADD EVENT sqlserver.hadr_capture_vlfheader,  
ADD EVENT sqlserver.hadr_db_commit_mgr_harden,  
ADD EVENT sqlserver.hadr_log_block_compression,  
ADD EVENT sqlserver.hadr_log_block_decompression,  
ADD EVENT sqlserver.hadr_log_block_group_commit ,  
ADD EVENT sqlserver.hadr_log_block_send_complete,  
ADD EVENT sqlserver.hadr_lsn_send_complete,  
ADD EVENT sqlserver.hadr_receive_harden_lsn_message,  
ADD EVENT sqlserver.hadr_send_harden_lsn_message,  
ADD EVENT sqlserver.hadr_transport_flow_control_action,  
ADD EVENT sqlserver.hadr_transport_receive_log_block_message,  
ADD EVENT sqlserver.log_block_pushed_to_logpool,  
ADD EVENT sqlserver.log_flush_complete ,   
ADD EVENT sqlserver.recovery_unit_harden_log_timestamps   
ADD TARGET package0.event_file(SET filename=N'c:\temp\AlwaysOn_Data_Movement_Tracing.xel',max_file_size=(500),max_rollover_files=(4))  
WITH (MAX_MEMORY=4096 KB,EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS,MAX_DISPATCH_LATENCY=30 SECONDS,MAX_EVENT_SIZE=0 KB,  
MEMORY_PARTITION_MODE=NONE,TRACK_CAUSALITY=OFF,STARTUP_STATE=ON)    
GO   

--Now start it and keep it running for 10 minutes <br>  
ALTER EVENT SESSION [AGLatency] ON SERVER STATE=START;   

/*  
--You can stop it this way:  

ALTER EVENT SESSION [AGLatency] ON SERVER STATE=STOP;   

*/   

##### Step 3: Generate report  
Put AlwaysOn_Data_Movement_Tracing.xel and primary.xml from primary to a folder, say, c:\data\primary, and put AlwaysOn_Data_Movement_Tracing.xel from secondary and secondary.xml to another folder, say, c:\data\secondary. And then feed the tool with these two folders, click "Start" to generate the report. 

Note:
Your SQL server may need to be updated to support above xevents:<br>
https://support.microsoft.com/en-us/help/3173156/update-adds-alwayson-extended-events-and-performance-counters-in-sql-s

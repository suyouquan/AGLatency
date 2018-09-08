# AGLatency

AGLatency is a tool to analyze AG log block movement latency between replicas and create report accordingly.

![alt text](https://github.com/suyouquan/AGLatency/blob/master/AGLatency.png)


You capture log block Xevent trace from both primary and secondary replica for 5-10 minutes and then this tool will 
generate report about the latency of the log block movement.

![alt text](https://github.com/suyouquan/AGLatency/blob/master/AGLatencyReport.png)


/*
Reference link:
https://blogs.msdn.microsoft.com/psssql/2018/04/05/troubleshooting-data-movement-latency-between-synchronous-commit-always-on-availability-groups/
*/

--Note: Please run below script on both primary and secondary at the same time, for about 5-10 minutes, and then stop it.

--You can change "c:\temp\" to other folder accordingly.

CREATE EVENT SESSION [AGLatency] ON SERVER 
ADD EVENT sqlserver.hadr_capture_log_block,
ADD EVENT sqlserver.hadr_database_flow_control_action,
ADD EVENT sqlserver.hadr_receive_harden_lsn_message,
ADD EVENT sqlserver.hadr_send_harden_lsn_message,
ADD EVENT sqlserver.hadr_transport_flow_control_action,
ADD EVENT sqlserver.log_flush_complete 
ADD TARGET package0.event_file(SET filename=N'c:\temp\AGLatency',max_file_size=(200),max_rollover_files=(20))
WITH (MAX_MEMORY=4096 KB,EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS,MAX_DISPATCH_LATENCY=30 SECONDS,MAX_EVENT_SIZE=0 KB,MEMORY_PARTITION_MODE=NONE,TRACK_CAUSALITY=OFF,STARTUP_STATE=OFF)
GO




--Now start it and keep it running for 10 minutes
ALTER EVENT SESSION [AGLatency] ON SERVER STATE=START; 

/*
--You can stop it this way:
ALTER EVENT SESSION [AGLatency] ON SERVER STATE=STOP; 

*/

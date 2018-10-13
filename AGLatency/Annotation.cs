using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AGLatency
{
    public static class Annotation
    {
        public static Dictionary<LatencyEvent, string> LatencyExplain = new Dictionary<LatencyEvent, string>
        {
            { LatencyEvent.Primary_Commit,
                @"Avg time to commit transactions on the Primary Replica. When a transactio commit completes, xevent <b>recovery_unit_harden_log_timestamps</b> will be fired and its field processing_time records how long the transaction takes to finish.<br>Primary->Commit time</b>= max(primary->local flush, primary->remote harden)." },
            {LatencyEvent.Primary_Send,"Avg time spent in sending the log block to the UCS layer in SQL. The sending time is from xevent <b>hadr_log_block_send_complete</b>. The sending time does not include the time spent in the network layer (i.e. while in transit between the primary and secondary replicas).<br><b>Note:</b>If you have asynchronous mode replica, then the sending time including the time sending log blocks to asynchronous replica, which impacts the avg value of the latency. If asynchronous replica's network is slow, then it will increase the avg time of the calculation." },

            {LatencyEvent.Primary_RemoteHarden,"Time elapsed between sending a log block to a secondary replica and getting the associated harden_lsn message back from the secondary replicas.It is recorded in xevent of <b>hadr_db_commit_mgr_harden</b>.<br>remote Harden time </b>= primary->send +  network wire+ secondary->processing + network wire+ primary->receive + other." },
            {LatencyEvent.Secondary_Processing,"Avg total_processing_time of log blocks on secondary, recorded in xevent of <b>hadr_lsn_send_complete. </b><br>Secondary->processing= Secondary:receive + decompression + local flush +  send + other. <br>" },
            {LatencyEvent.Primary_Compression,"Processing time of xevent <b>hadr_log_block_compression</b>." },
            {LatencyEvent.Primary_Receive,"Processing time statistics of xevent <b>hadr_receive_harden_lsn_message</b> (mode=2)." },
            {LatencyEvent.Primary_LocalFlush,"Processing time statistics of xevent <b>log_flush_complete</b>,the time to flush the log block to local stable media."},
            {LatencyEvent.Secondary_LocalFlush,"Processing time statistics of xevent <b>log_flush_complete</b>,the time to flush the log block to local stable media."},
            {LatencyEvent.Primary_FlowControl,"Processing time statistics of xevent <b>hadr_database_flow_control_action</b>." },
            {LatencyEvent.Secondary_Receive,"Processing time statistics of xevent <b>hadr_transport_receive_log_block_message</b> (mode=2)." },
            {LatencyEvent.Secondary_Send,"Processing time statistics of xevent of <b>hadr_lsn_send_complete</b>." },
            {LatencyEvent.Secondary_Decompression,"Processing time of xevent <b>hadr_log_block_decompression</b>." },
        };

        public static string GetExplain(LatencyEvent e)
        {
            if (LatencyExplain.ContainsKey(e)) return HttpUtility.JavaScriptStringEncode(LatencyExplain[e]);
            else return "";
        }

 


    }
}

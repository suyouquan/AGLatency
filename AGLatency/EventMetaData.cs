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

namespace AGLatency
{
    public enum Replica
        {
            Primary,
            Secondary
        }

        public enum NetworkDirection
        {
            Primary_To_Secondary,
            Secondary_To_Primary
        }
    public static class EventMetaData
    {
     
        public enum xEvent
        {
            hadr_capture_log_block,
            log_block_pushed_to_logpool,
            hadr_log_block_compression,

            log_flush_start,
            log_flush_complete,

            //primary
            hadr_log_block_send_complete,
            //secondary
            hadr_transport_receive_log_block_message,

            //sync replica
            hadr_receive_harden_lsn_message,
            hadr_send_harden_lsn_message,

            //flow control
            hadr_database_flow_control_action,

            //primary. Please be aware that below events will be fired for every transaction. so the trace could be very big.
            hadr_db_commit_mgr_harden,
            recovery_unit_harden_log_timestamps


        }



        public static int GetEventMode(PublishedEvent  e)
        {
            foreach (PublishedEventField xe_field in e.Fields)
            {
                if (xe_field.Name == "mode") return Int16.Parse(xe_field.Value.ToString());

            }

            return -1;

        }







    }
}

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


    public static class Tables
    {
        //https://www.guru99.com/sqlite-data-types.html

        public static Dictionary<string, string> insertSQLMap = new Dictionary<string, string>();

        public static string GetInsertSQL(IEventMetadata xe_event)
        {
            string sql = "INSERT INTO  ";
            string tableName = xe_event.Name;
            sql = sql + tableName + " VALUES(@EventTimeStamp,@TimeDelta,";
            //iterate through the fields
            int total = xe_event.Fields.Count;
            int cnt = 0;
            foreach (IEventFieldMetadata xe_field in xe_event.Fields)
            {
                string colName = xe_field.Name;

                sql = sql + "@" + colName;
                cnt++;
                if (cnt != total) sql = sql + ",";

            }
            sql = sql + ")";

            return sql;

        }

        public static string GetInsertSQL(PublishedEvent xe_event)
        {
            string sql = "INSERT INTO  ";
            string tableName = xe_event.Name;
            sql = sql + tableName + " VALUES(@EventTimeStamp,@TimeDelta,";
            //iterate through the fields
            int total = xe_event.Fields.Count;
            int cnt = 0;
            foreach (PublishedEventField xe_field in xe_event.Fields)
            {
                string colName = xe_field.Name;

                sql = sql + "@" + colName;
                cnt++;
                if (cnt != total) sql = sql + ",";

            }
            sql = sql + ")";

            return sql;

        }

        public static string GetTableSchema(IEventMetadata xe_event)
        {
            string sql = "CREATE TABLE ";
            string tableName = xe_event.Name;
            sql = sql + tableName + "(EventTimeStamp INTEGER,TimeDelta INTEGER Default null,";
            //iterate through the fields
            int total = xe_event.Fields.Count;
            int cnt = 0;
            foreach (IEventFieldMetadata xe_field in xe_event.Fields)
            {
                string colName = xe_field.Name;
                string type = GetSQLiteType(xe_field.Type);
                sql = sql + colName + " " + type;
                cnt++;
                if (cnt != total) sql = sql + ",";

            }
            sql = sql + ")";

            return sql;
        }

        static readonly object cmdObj = new object();
        public static SQLiteCommand PrepareInsertCmd(SQLiteConnection conn, PublishedEvent e)
        {
            SQLiteCommand cmd = new SQLiteCommand(conn);
            lock (cmdObj)
            {

             

                if (!insertSQLMap.ContainsKey(e.Name))
                {
                    string insert = Tables.GetInsertSQL(e);

                    insertSQLMap.Add(e.Name, insert);
                }
            }

            cmd.CommandText = insertSQLMap[e.Name];

            try
            {

                cmd.Parameters.Add(new SQLiteParameter("@EventTimeStamp", e.Timestamp.Ticks));
                cmd.Parameters.Add(new SQLiteParameter("@TimeDelta", null));

                foreach (PublishedEventField xe_field in e.Fields)
                {
                    string colName = xe_field.Name;
                    if (xe_field.Type == typeof(System.Guid))
                    {
                        cmd.Parameters.Add(new SQLiteParameter("@" + colName, xe_field.Value.ToString()));
                    }
                    else
                    {
                        cmd.Parameters.Add(new SQLiteParameter("@" + colName, xe_field.Value));
                    }
                }



            }



            catch (Exception ex)
            {
                cmd = null;
                Logger.LogException(ex, Thread.CurrentThread);
            }

            return cmd;
        }

        private static string GetSQLiteType(System.Type in_type)
        {
            switch (in_type.ToString())
            {
                case "System.UInt64":
                case "System.UInt32":
                case "System.UInt16":
                case "System.UInt8":
                case "System.Int64":
                case "System.Int32":
                case "System.Int16":
                case "System.Int8": return "INTEGER";

                case "System.DateTime": return "DATETIME";

                case "System.Boolean": return "BOOLEAN";

                case "Microsoft.SqlServer.XEvent.MapValue":
                case "Microsoft.SqlServer.XEvent.ActivityId":
                case "Microsoft.SqlServer.XEvent.XMLData":
                case "System.String":
                    return "TEXT";
                default:
                    return "TEXT";
            }
        }

    }
}

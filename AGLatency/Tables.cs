using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SqlServer.XEvent;
using Microsoft.SqlServer.XEvent.Linq;
using Microsoft.Data.Sqlite;
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
        public static SqliteCommand PrepareInsertCmd(SqliteConnection conn, PublishedEvent e)
        {
            SqliteCommand cmd = conn.CreateCommand();
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

                cmd.Parameters.Add(new SqliteParameter("@EventTimeStamp", e.Timestamp.Ticks));
                cmd.Parameters.Add(new SqliteParameter("@TimeDelta", DBNull.Value));

                foreach (PublishedEventField xe_field in e.Fields)
                {
                    object value = xe_field.Value ?? DBNull.Value;
                    if (xe_field.Type == typeof(System.Guid) && xe_field.Value != null)
                    {
                        value = xe_field.Value.ToString();
                    }
                    if (xe_field.Type == typeof(MapValue) && xe_field.Value != null)
                    {
                        value = xe_field.Value.ToString();
                    }

                    var p = new SqliteParameter("@" + xe_field.Name, value);
                    cmd.Parameters.Add(p);
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

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
    public class SQLiteDB
    {
        private CancellationTokenSource cts = new CancellationTokenSource();
        public string SQLiteDBFile = "";
        public string databaseName = "";
        SqliteConnection sqliteConn;
        private static uint dbid = 0;
        private Thread dataLoopThread;

        public Dictionary<string, string> tables = new Dictionary<string, string>();

        private Queue<PublishedEvent> eventsQueue = new Queue<PublishedEvent>();
        private AutoResetEvent autoEvent;
        object _lock = new object();

        public UInt64 count = 0;

        // Controls commit size per transaction when draining the queue
        public UInt64 batchSize = (UInt64)Controller.BatchSize;
        public UInt32 errorCount = 0;

        // Transaction for batch inserts
        private SqliteTransaction _sqLiteTransaction = null;

        // Prepared insert commands per event/table for reuse
        private readonly Dictionary<string, SqliteCommand> _insertCmdCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _cmdLock = new();

        static readonly object _dblock = new object();

        public void AddTable(IEventMetadata e)
        {
            lock (_dblock)
            {
                if (tables.ContainsKey(e.Name)) return;
                string tableName = e.Name;
                string tableSchema = Tables.GetTableSchema(e);

                tables.Add(tableName, tableSchema);

                Execute(tableSchema);
            }
        }

        public SqliteConnection GetConnection() => sqliteConn;

        public void Init(string dbName)
        {
            lock (_dblock)
            {
                CreateDBFile(dbName + "_" + dbid + "_");
                this.databaseName = dbName;
                dbid++;
                Open(SQLiteDBFile);

                // Important: set pragmas on fresh DB before any table creation
                Execute("PRAGMA page_size = 4096");                   // or 8192 based on storage
                Execute("PRAGMA temp_store = MEMORY");
                Execute("PRAGMA cache_size = -65536");                // ~64MB cache
                Execute("PRAGMA mmap_size = 134217728");              // 128MB
                Execute("PRAGMA locking_mode = EXCLUSIVE");
                Execute("PRAGMA synchronous = OFF");                  // durability trade-off for speed
                Execute("PRAGMA journal_mode = MEMORY");              // or WAL if you need concurrent reads
            }
            autoEvent = new AutoResetEvent(false);
            StartDataLoopThread();
        }

        public void StartDataLoopThread()
        {
            dataLoopThread = new Thread(() => DataLoop(cts.Token))
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            dataLoopThread.Start();
        }

        public Int32 GetQueueLength()
        {
            return eventsQueue.Count;
        }

        public void Push(PublishedEvent e)
        {
            bool shouldSignal = false;
            lock (_lock)
            {
                // Signal only on transition from empty to non-empty to reduce wake-ups
                shouldSignal = eventsQueue.Count == 0;
                eventsQueue.Enqueue(e);
            }
            if (shouldSignal) autoEvent.Set();
        }

        public void Signal()
        {
            autoEvent.Set();
        }

        public void CloseConnection()
        {
            sqliteConn?.Close();
        }

        public void CleanUp()
        {
            // 1. Signal cancellation
            cts.Cancel();

            // 2. Wake the thread if blocked on WaitOne()
            autoEvent.Set();

            // 3. Wait for dataLoopThread to finish (it commits its own transaction in the finally block)
            if (dataLoopThread != null && dataLoopThread.IsAlive)
            {
                Logger.LogMessage("Waiting for data loop thread to exit...");
                if (!dataLoopThread.Join(TimeSpan.FromSeconds(10)))
                {
                    Logger.LogMessage("Data loop thread did not exit within timeout.");
                }
            }

            // 4. Dispose prepared commands
            foreach (var cmd in _insertCmdCache.Values)
            {
                cmd.Dispose();
            }
            _insertCmdCache.Clear();

            // 5. Now safe to close connection (worker is done)
            CloseConnection();
        }

        public static void DeleteOldFile()
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string dbFolder = Path.Combine(path, "SQLiteDB");
            List<string> files = Utility.GetFileListFromFolder(dbFolder, new string[] { "*.*" });
            foreach (string f in files)
            {
                try { File.Delete(f); }
                catch (Exception ex) { Logger.LogException(ex, Thread.CurrentThread); }
            }
            if (!Directory.Exists(dbFolder)) Directory.CreateDirectory(dbFolder);
        }

        public void CreateDBFile(string dbName)
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var filename = dbName + System.DateTime.Now.ToString("_yyyy-MM-dd_HH_mm_ss.fff") + ".SQLiteDB";

            string logFolder = Path.Combine(path, "SQLiteDB");
            if (!Directory.Exists(logFolder)) Directory.CreateDirectory(logFolder);

            string dbFile = Path.Combine(logFolder, filename);
            SQLiteDBFile = dbFile;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (!File.Exists(dbFile))
            {
                try
                {
                    using (var connection = new SqliteConnection(string.Format("Data Source={0};Mode=ReadWriteCreate", dbFile)))
                    {
                        connection.Open();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, Thread.CurrentThread);
                }
            }
        }//create DB

        public void Open(string dbFile)
        {
            try
            {
                sqliteConn = new SqliteConnection(string.Format("Data Source={0}", dbFile));
                sqliteConn.Open();
                // Additional pragmas moved to Init to ensure order for fresh DBs
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, Thread.CurrentThread);
            }
        }

        public void Execute(string sql)
        {
            try
            {
                using var command = new SqliteCommand(sql, sqliteConn);
                command.ExecuteNonQuery();
                Logger.LogMessage("Executed:" + sql);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, Thread.CurrentThread);
            }
        }

        public SqliteDataReader ExecuteReader(string sql)
        {
            try
            {
                var command = new SqliteCommand(sql, sqliteConn);
                return command.ExecuteReader();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, Thread.CurrentThread);
            }

            return null;
        }

        // Centralize transaction lifecycle around draining batches in DataLoop
        private void CommitTran()
        {
            if (_sqLiteTransaction != null)
            {
                _sqLiteTransaction.Commit();
                _sqLiteTransaction.Dispose();
                _sqLiteTransaction = null;
            }
        }

        private void BeginTran()
        {
            if (_sqLiteTransaction == null)
                _sqLiteTransaction = sqliteConn.BeginTransaction();
        }

        // Reuse prepared insert command per event/table; update parameter values only
        private SqliteCommand GetOrCreateInsertCommand(PublishedEvent e)
        {
            lock (_cmdLock)
            {
                if (_insertCmdCache.TryGetValue(e.Name, out var cached))
                    return cached;

                var cmd = sqliteConn.CreateCommand();
                cmd.Transaction = _sqLiteTransaction;
                cmd.CommandText = Tables.GetInsertSQL(e);

                // Static parameters (always present)
                cmd.Parameters.Add(new SqliteParameter("@EventTimeStamp", e.Timestamp.Ticks));
                cmd.Parameters.Add(new SqliteParameter("@TimeDelta", DBNull.Value));

                // One parameter per field in fixed order
                foreach (PublishedEventField xe_field in e.Fields)
                {
                    object value = xe_field.Value ?? DBNull.Value;
                    if (xe_field.Type == typeof(System.Guid) && xe_field.Value != null)
                        value = xe_field.Value.ToString();
                    if (xe_field.Type == typeof(MapValue) && xe_field.Value != null)
                        value = xe_field.Value.ToString();

                    cmd.Parameters.Add(new SqliteParameter("@" + xe_field.Name, value));
                }

                _insertCmdCache[e.Name] = cmd;
                return cmd;
            }
        }

        private void UpdateInsertParameters(SqliteCommand cmd, PublishedEvent e)
        {
            // Order matches construction above: EventTimeStamp, TimeDelta, then fields
            cmd.Transaction = _sqLiteTransaction;

            int idx = 0;
            cmd.Parameters[idx++].Value = e.Timestamp.Ticks;   // @EventTimeStamp
            cmd.Parameters[idx++].Value = DBNull.Value;        // @TimeDelta

            foreach (PublishedEventField xe_field in e.Fields)
            {
                object value = xe_field.Value ?? DBNull.Value;
                if (xe_field.Type == typeof(System.Guid) && xe_field.Value != null)
                    value = xe_field.Value.ToString();
                if (xe_field.Type == typeof(MapValue) && xe_field.Value != null)
                    value = xe_field.Value.ToString();

                cmd.Parameters[idx++].Value = value;
            }
        }

        // Perform a single-row insert using prepared command reuse (no transaction mgmt here)
        public void Insert(PublishedEvent x_event)
        {
            try
            {
                var cmd = GetOrCreateInsertCommand(x_event);
                UpdateInsertParameters(cmd, x_event);
                cmd.ExecuteNonQuery();
                count++;
            }
            catch (Exception ex)
            {
                errorCount++;
                Logger.LogException(ex, Thread.CurrentThread);
            }
        }

        // Drain the queue in batches with a transaction
        public void DataLoop(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested) return;

                autoEvent.WaitOne();
                if (token.IsCancellationRequested) return;

                BeginTran();
                try
                {
                    UInt64 inTxn = 0;

                    while (true)
                    {
                        if (token.IsCancellationRequested) break;

                        PublishedEvent e = null;
                        lock (_lock)
                        {
                            if (eventsQueue.Count > 0)
                                e = eventsQueue.Dequeue();
                        }
                        if (e == null)
                            break;

                        Insert(e);
                        inTxn++;

                        if (inTxn >= batchSize)
                        {
                            CommitTran();
                            BeginTran();
                            inTxn = 0;
                        }
                    }
                }
                finally
                {
                    CommitTran();
                }
            }
        }
    }
}

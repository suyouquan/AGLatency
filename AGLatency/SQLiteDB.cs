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
    public class SQLiteDB
    {
        public string SQLiteDBFile = "";
        public string databaseName = "";
        SQLiteConnection sqliteConn;
        private static uint dbid = 0;
        private Thread dataLoopThread;
        
        public Dictionary<string, string> tables = new Dictionary<string, string>();
      

        private Queue<PublishedEvent> eventsQueue = new Queue<PublishedEvent>();
        private AutoResetEvent autoEvent;
        object _lock = new object();

        public UInt64 count = 0;

        public UInt64 batchSize = 5000;
        public UInt32 errorCount = 0;
        private uint inserted = 0;
  
        private SQLiteTransaction _sqLiteTransaction = null;

        public void AddTable(IEventMetadata e)
        {
            if (tables.ContainsKey(e.Name)) return;

            lock (_dblock)
            {
                string tableName = e.Name;
                string tableSchema = Tables.GetTableSchema(e);
                
                tables.Add(tableName, tableSchema);
                

                Execute(tableSchema);
               
            }

        }
        public SQLiteConnection GetConnection()
        {
            return sqliteConn;
        }

        static readonly object _dblock = new object();
        public void Init(string dbName)
        {
            lock (_dblock)
            {
                CreateDBFile(dbName + "_" + dbid + "_");
                this.databaseName = dbName;
                dbid++;
                Open(SQLiteDBFile);
            }
            autoEvent = new AutoResetEvent(false);
            StartDataLoopThread();
        }

  
        public void StartDataLoopThread()
        {
            dataLoopThread = new Thread(DataLoop);
            dataLoopThread.Start();
        }

        public Int32 GetQueueLength()
        {
            return eventsQueue.Count;
        }

        public void Push(PublishedEvent e)
        {
            lock (_lock)
            {
                eventsQueue.Enqueue(e);
            }
            Signal();
        }

        public void Signal()
        {
            autoEvent.Set();
        }

        public void CloseConnection()
        {
            sqliteConn.Close();

        }

        public void CleanUp()
        {
            if (_sqLiteTransaction != null)
            {
                _sqLiteTransaction.Commit();
                _sqLiteTransaction.Dispose();
                _sqLiteTransaction = null;
            }

            CloseConnection();
            if (dataLoopThread != null) dataLoopThread.Abort();
        }


        public static void DeleteOldFile()
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //string path2 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            string dbFolder = Path.Combine(path, "SQLiteDB");
            List<string> files = Utility.GetFileListFromFolder(dbFolder, new string[]{ "*.*"});
            foreach (string f in files)
            {
                try
                {
                    File.Delete(f);

                }
                catch (Exception ex)
                {
                    Logger.LogException(ex, Thread.CurrentThread);
                }
            }
            if (!Directory.Exists(dbFolder)) Directory.CreateDirectory(dbFolder);

            
        }
        public   void CreateDBFile(string dbName)
        {
           
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //string path2 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
           
            var filename = dbName+ System.DateTime.Now.ToString("_yyyy-MM-dd_HH_mm_ss.fff") + ".SQLiteDB";

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
                    SQLiteConnection.CreateFile(dbFile);
                }
                catch(Exception ex)
                {
                    Logger.LogException(ex, Thread.CurrentThread);
                }
            }
        }//create DB 


        public void Open(string dbFile)
        {
            

            try
            {
                sqliteConn = new SQLiteConnection(string.Format("Data Source={0}", dbFile));
                sqliteConn.Open();
                Execute("PRAGMA synchronous = OFF");
                Execute("PRAGMA journal_mode = MEMORY");
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
                //   string sql = "create table highscores (name varchar(20), score int)";

                SQLiteCommand command = new SQLiteCommand(sql, sqliteConn);
                command.ExecuteNonQuery();
                Logger.LogMessage("Executed:"+sql);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, Thread.CurrentThread);
            }


        }
        //execute query and return result set
        public SQLiteDataReader ExecuteReader(string sql)
        {



            try
            {
                //   string sql = "create table highscores (name varchar(20), score int)";

                SQLiteCommand command = new SQLiteCommand(sql, sqliteConn);
                return command.ExecuteReader();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, Thread.CurrentThread);
            }

            return null;
        }










        public void ProcessEvent()
        {
            PublishedEvent e = null;
            // Logger.LogMessage("ProcessEvent:" + eventsQueue.Count);
            while (eventsQueue.Count > 0)
            {
                lock (_lock)
                {
                    e = eventsQueue.Dequeue();

                }
                if (e != null) Insert(e);
            }
        }
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
            _sqLiteTransaction = sqliteConn.BeginTransaction();

        }
        public void Insert(PublishedEvent x_event)
        {
            bool ok = true;
            if (inserted == batchSize)
            {

                CommitTran();

                BeginTran();
                inserted = 0;
            }

            
            SQLiteCommand cmd = Tables.PrepareInsertCmd(sqliteConn, x_event);

           

            try
            {

                if (cmd != null) cmd.ExecuteNonQuery();
                else
                {
                    ok = false;
                    errorCount++;
                    Logger.LogMessage("[ERROR]cmd is null!");

                }

            }
            catch (Exception e)
            {
                ok = false;
                errorCount++;
                Logger.LogException(e, Thread.CurrentThread);
            }
            if(cmd!=null) cmd.Dispose();

            if (ok)
            {
                count++;
                inserted++;
            }
            // Logger.LogMessage(this.TableName + ":" + count+" processed.");

           
            
        }
        public void DataLoop()
        {
            while (true)
            {
                autoEvent.WaitOne();
                //Now data arrive, process it.
                ProcessEvent();


            }

        }











    }
}

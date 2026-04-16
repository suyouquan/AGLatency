using Microsoft.SqlServer.XEvent;
using Microsoft.SqlServer.XEvent.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace AGLatency
{
   
    class XELoader
    {
        Replica server;
        public string fileOrFolder = "";
        public UInt64 reads = 0;
        private string fileName = "";
        private int totalFile = 0;
        private int fileNum = 0;
        private int fileNum2 = 0;
        public UInt64 eventCount = 0;
        // Dictionary<string, Import> imports = new Dictionary<string, Import>();
        public static List<EventLatency> eventLatencies = new List<EventLatency>();
        private static object _dblock = new object();
        private static object _readlock = new object();
        private static object _uilock = new object();


        public delegate void outputCallBackFunction(UInt64 count);
        outputCallBackFunction fn_UpdateRowCount = null;

        public delegate void outputCallBack(string s);
        outputCallBack fn_UpdateMsg = null;


        //only one instance for all XELoader
      //  public static Dictionary<string, SQLiteDB> dbMap = new Dictionary<string, SQLiteDB>();
        public enum dbNames
        {
            //
            primary_to_secondary_network

        }
        public static void AddEventLatency(EventLatency el)
        {
            eventLatencies.Add(el);
        }
       
        public static void Reset()
        {
            foreach (EventLatency el in eventLatencies)
            {
                string dbName = el.eventDB.databaseName;
                el.eventDB.CleanUp();

                //var eventDB = new SQLiteDB();
                //eventDB.Init(dbName);
                //el.eventDB = eventDB;
            }

            XELoader.eventLatencies.Clear();

        }
       
        public XELoader(string fileFolder, Replica repl, outputCallBack fn)
        {
            this.fileOrFolder = fileFolder;
            server = repl;
            fn_UpdateMsg = fn;
        }
        public QueryableXEventData Open(string fileName)
        {
            try
            {
                return new QueryableXEventData(fileName);
            } catch (Microsoft.SqlServer.XEvent.Linq.XEventException ex)
            {
                Logger.LogException(ex, Thread.CurrentThread);
                return null;
            }
            
        }

        public void GetTotalEventCount()
        {
            try
            {


                UInt64 count = 0;

                if (File.Exists(fileOrFolder))
                {

                    fileNum2++;
                    var data = Open(fileOrFolder);
                    if (data != null)
                    {
                        count = GetCount(data);
                        //count = EstimateEventCount(data, fileOrFolder);
                        Logger.LogMessage("GetEventCount:" + fileOrFolder + "==>" + count);
                    }
                    else
                    {
                        Logger.LogMessage("GetEventCount: " + fileOrFolder + " is not a valid XEL file.");

                    }
                }

                else //if it is folder
                // we will perform estimate event count
                {
                    if (Directory.Exists(fileOrFolder))
                    {
                        var masks = new[] { "*.xel" };
                        var xelFiles = Utility.GetFileListFromFolder(fileOrFolder, masks);
 
                        totalFile = xelFiles.Count;
                        if (totalFile > 0)
                        {
                            string f = xelFiles.First();
                            var data = Open(f);
                            if (data != null)
                            {
                                // get the first file exact count
                                UInt64 k = GetCount(data);

                                // Get file size
                                var fileInfo = new FileInfo(f);
                                long fileSize = fileInfo.Length;

                                count = k;

                                if (k == 0 || fileSize == 0)
                                {
                                    Logger.LogMessage("First file had 0 events or 0 bytes; falling back to exact counting.");
                                    foreach (string file in xelFiles.Skip(1))
                                    {
                                        if (Controller.CancellationToken.IsCancellationRequested)
                                        {
                                            Logger.LogMessage("Cancellation requested by user.");
                                            return;
                                        }
                                        var nextData = Open(file);
                                        if (nextData != null)
                                        {
                                            count += GetCount(nextData);
                                            fileNum2++;
                                            lock (_uilock)
                                                fn_UpdateMsg($"File: {fileNum2}/{totalFile}, Counting {count}");
                                        }
                                    }
                                }
                                else
                                {
                                    double avgEventSize = (double)fileSize / k;

                                    foreach (string file in xelFiles.Skip(1))
                                    {
                                        if (Controller.CancellationToken.IsCancellationRequested)
                                        {
                                            Logger.LogMessage("Cancellation requested by user.");
                                            return;
                                        }
                                        fileInfo = new FileInfo(file);
                                        double estimated = fileInfo.Length / avgEventSize;
                                        k = estimated > 0 ? (ulong)Math.Min(estimated, (double)ulong.MaxValue - count) : 0;
                                        count += k;
                                        fileNum2++;
                                        lock (_uilock)
                                            fn_UpdateMsg($"File: {fileNum2}/{totalFile}, Calculating {count}");
                                        Logger.LogMessage($"GetEventCount - estimate:{file} ==> {k}");
                                    }
                                }

                                eventCount = count;
                            }
                            else
                            {
                                Logger.LogMessage($"GetEventCount:{f} is not a valid XEL file.");
                            }

                        }
                    }

                }

            }
            catch (Microsoft.SqlServer.XEvent.Linq.XEventException ex)
            {
                Logger.LogException(ex, Thread.CurrentThread);
                Logger.LogMessage($"Invalid XEL file: {fileOrFolder}. Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, Thread.CurrentThread);
                Logger.LogMessage("Error in GetTotalEventCount:" + ex.Message);
            }
        }

        public void Start()
        {
            if (File.Exists(fileOrFolder))
            {
                fileName = Path.GetFileName(fileOrFolder);
                fileNum = 1;
                totalFile = 1;
                Logger.LogMessage("Processing File:" + fileOrFolder);
                var data = Open(fileOrFolder);
                if (data == null)
                {
                    Logger.LogMessage("File is not a valid XEL file: " + fileOrFolder);
                    return;
                }
                CreateTablesFromMetadata(data);
                ProcessEvent(data);
            }

            else //if it is folder
            {
                if (Directory.Exists(fileOrFolder))
                {
                    var masks = new[] { "*.xel" };
                    var xelFiles = Utility.GetFileListFromFolder(fileOrFolder, masks);
                    totalFile = xelFiles.Count;

                    var parallelOptions = new ParallelOptions
                    {
                        CancellationToken = Controller.CancellationToken,
                        MaxDegreeOfParallelism = Controller.MaxDOP
                    };
                    try { 
                        Parallel.ForEach(xelFiles, parallelOptions, f =>
                        {
                            parallelOptions.CancellationToken.ThrowIfCancellationRequested();

                            var localFileName = Path.GetFileName(f);
                            var localFileNum = Interlocked.Increment(ref fileNum);

                            Logger.LogMessage($"Processing File: {f}");

                            var data = Open(f);
                            if (data == null)
                            {
                                Logger.LogMessage("File is not a valid XEL file: " + f);
                                return;
                            }

                            // Safe: AddTable is locked; duplicate creates are filtered inside
                            CreateTablesFromMetadata(data);

                            // Push() is locked; each eventDB has its own queue/worker
                            ProcessEvent(data);
                        });
                    }
                    catch (OperationCanceledException) 
                    {
                        Logger.LogMessage("Processing cancelled by user.");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex, Thread.CurrentThread);
                        Logger.LogMessage("Error in processing files: " + ex.Message);
                    }
                    // Sequential processing (commented out)
                    /*
                    foreach (string f in xelFiles)
                    {
                        if (Controller.CancellationToken.IsCancellationRequested)
                        {
                            Logger.LogMessage("Processing cancelled by user.");
                            return;
                        }
                        fileName = Path.GetFileName(f);
                        fileNum++;
                        Logger.LogMessage("Processing File:" + f);
                        var data = Open(f);
                        if (data == null)
                        {
                            Logger.LogMessage("File is not a valid XEL file: " + f);
                            continue;
                        }
                        CreateTablesFromMetadata(data);
                        ProcessEvent(data);

                    }*/
                }

            }


        }
        private static int GetAllQueueLength()
        {
            int cnt = 0;

            foreach (EventLatency el in eventLatencies)
            {
                if (Controller.CancellationToken.IsCancellationRequested)
                {
                    Logger.LogMessage("Cancellation requested by user.");
                    return cnt;
                }
                cnt = cnt + el.eventDB.GetQueueLength();
            }

            return cnt;

        }

        public static UInt64 GetAllCount()
        {
            UInt64 cnt = 0;
            foreach (EventLatency el in eventLatencies)
            {
                if (Controller.CancellationToken.IsCancellationRequested)
                {
                    Logger.LogMessage("Cancellation requested by user.");
                    return cnt;
                }   
                cnt = cnt + el.eventDB.count;
            }
            return cnt;

        }

        public UInt64 GetReads()
        {
            return reads;
        }

        public static void CleanUp()
        {
            Logger.LogMessage($"Clean Up {eventLatencies.Count} events ..." );

            foreach (EventLatency el in eventLatencies)
            {
                //  imp.CleanUp();
                //last chance to get them drain up their queue
                el.eventDB.Signal();

            }
            Logger.LogMessage("Finished EventLatency");

            while (true)
            {
                //wait for queue to be drain up
                foreach (EventLatency el in eventLatencies)
                {
                    Logger.LogMessage("Signal eventDB");

                    //  imp.CleanUp();
                    //last chance to get them drain up their queue
                    el.eventDB.Signal();


                }
                int cnt = GetAllQueueLength();
                if (cnt > 0) //need to wait for a while
                {
                    Logger.LogMessage("Wait for one second to wait for db queue cleanup...");
                    Thread.Sleep((1000));
                }
                else break;

            }


            // UInt64 c = GetAllCount();
            //fn_UpdateMsg("File:" + fileNum + "/" + totalFile + ", Reading " + reads.ToString() + " Committed:" + c);

            //now time to cleanup
            foreach (EventLatency el in eventLatencies)
            {

                el.eventDB.CleanUp();


            }
        }

        public UInt64 GetCount(QueryableXEventData data, int filenum=1)
        {
            foreach (PublishedEvent x_event in data)
            {
                if (Controller.CancellationToken.IsCancellationRequested)
                {
                    Logger.LogMessage("Cancellation requested by user");
                    return eventCount;
                }

                if (eventCount % 8000==0)
                {
                    lock (_uilock)
                        fn_UpdateMsg($"File: {fileNum}/{totalFile}, Calculating {eventCount}");
                }

                eventCount++;    

            }
            
            return eventCount;
        }
        public void ProcessEvent(QueryableXEventData data)
        { 

            foreach (PublishedEvent x_event in data)
            {
                if (Controller.CancellationToken.IsCancellationRequested)
                {
                    Logger.LogMessage("Processing cancelled by user.");
                    return;
                }
                string name = x_event.Name;

                // thread-safe increment and snapshot
                ulong r;
                lock (_readlock)
                {
                    reads++;
                    r = reads;
                }
                
                if (server == Replica.Primary)
                {
                    foreach (EventLatency el in eventLatencies)
                    {
                        foreach (EventWithMode em in el.primaryEvents)
                        {
                            if (em.e.ToString() == name)
                            {
                                if (em.mode == -1 || em.mode == EventMetaData.GetEventMode(x_event))
                                {
                                    el.eventDB.Push(x_event);
                                    
                                }

                            }

                        }

                    }


                }

                else if (server == Replica.Secondary)
                {
                    foreach (EventLatency el in eventLatencies)
                    {
                        foreach (EventWithMode em in el.secondaryEvents)
                        {
                            if (Controller.CancellationToken.IsCancellationRequested)
                            {
                                Logger.LogMessage("Cancellation requested by user.");
                                return;
                            }

                            if (em.e.ToString() == name)
                            {
                                if (em.mode == -1 || em.mode == EventMetaData.GetEventMode(x_event))
                                {
                                    el.eventDB.Push(x_event);
                                    
                                }

                            }

                        }

                    }

                }


                if (r % 4000 == 0)
                {
                    UInt64 i = GetAllCount();
                    int percent = eventCount ==0 ? 0: (int)(r * 100 / eventCount);
                    lock(_uilock)
                        fn_UpdateMsg($"File: {fileNum}/{totalFile}, Processing {r}/{eventCount} ({percent}%)");
                    int cnt = GetAllQueueLength();
                    if (cnt > 5000) //need to wait for a while
                    {
                        Thread.Sleep((cnt / 5000) * 100);
                    }
                }
                // if (k > 1000) break;





            }




        }



        public void updateUI(UInt64 inserted)
        {
            if (fn_UpdateRowCount != null) fn_UpdateRowCount(inserted);
        }
        //Create import instances from metadata
        public void CreateTablesFromMetadata(QueryableXEventData data)
        {


            foreach (IMetadataGeneration mgen in data.EventProvider.MetadataGenerations)
            {

                // iterate through each package to extract events
                foreach (IPackage xe_package in mgen.Packages)
                {
                    //iterate through each event
                    foreach (IEventMetadata xe_event in xe_package.Events)
                    {


                        //justfiy which database to create.
                        if (server == Replica.Primary)
                        {
                            foreach (EventLatency el in eventLatencies)
                            {
                                foreach (EventWithMode em in el.primaryEvents)
                                {
                                    if (em.e.ToString() == xe_event.Name)
                                    {
                                        el.eventDB.AddTable(xe_event);
                                    }
                                }
                            }

                        }
                        else if (server == Replica.Secondary)
                        {

                            foreach (EventLatency el in eventLatencies)
                            {
                                foreach (EventWithMode em in el.secondaryEvents)
                                {
                                    if (em.e.ToString() == xe_event.Name)
                                    {
                                        el.eventDB.AddTable(xe_event);
                                    }
                                }
                            }



                        }


                    }
                }


            }


        }//CreateTablesFromMetadata








    }
}

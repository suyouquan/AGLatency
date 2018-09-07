using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SqlServer.XEvent;
using Microsoft.SqlServer.XEvent.Linq;
using System.IO;


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

            return new QueryableXEventData(fileName);

            

        }

        public void GetTotalEventCount()
        {
            UInt64 count = 0;

            if (File.Exists(fileOrFolder))
            {

                fileNum2++;
                var data = Open(fileOrFolder);
                count = GetCount(data);
                Logger.LogMessage("GetEventCount:" + fileOrFolder + "==>" + count);
            }

            else //if it is folder
            {
                if (Directory.Exists(fileOrFolder))
                {
                    var masks = new[] { "*.xel" };
                    var xelFiles = Utility.GetFileListFromFolder(fileOrFolder, masks);
                    totalFile = xelFiles.Count;
                    foreach (string f in xelFiles)
                    {
                        fileNum2++;
                           var data = Open(f);
                        UInt64 k= GetCount(data);
                        Logger.LogMessage("GetEventCount:" + f+"==>"+k);
                        count += k;
                    }
                }

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
                    foreach (string f in xelFiles)
                    {
                        fileName = Path.GetFileName(f);
                        fileNum++;
                        Logger.LogMessage("Processing File:" + f);
                        var data = Open(f);
                        CreateTablesFromMetadata(data);
                        ProcessEvent(data);

                    }
                }

            }


        }
        private static int GetAllQueueLength()
        {
            int cnt = 0;

            foreach (EventLatency el in eventLatencies)
            {
                cnt = cnt + el.eventDB.GetQueueLength();
            }

            return cnt;

        }

        public static UInt64 GetAllCount()
        {
            UInt64 cnt = 0;
            foreach (EventLatency el in eventLatencies)
            {
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
            Logger.LogMessage("Clean Up...");

            foreach (EventLatency el in eventLatencies)
            {
                //  imp.CleanUp();
                //last chance to get them drain up their queue
                el.eventDB.Signal();

            }

            while (true)
            {
                //wait for queue to be drain up
                foreach (EventLatency el in eventLatencies)
                {

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

        public UInt64 GetCount(QueryableXEventData data)
        {
           
            foreach (PublishedEvent x_event in data)
            {
             if(eventCount % 8000==0)
                {
                    fn_UpdateMsg("File:" + fileNum2 + "/" + totalFile + ", Caculating " + eventCount);
                }

                eventCount++;    

            }

            return eventCount;
        }
        public void ProcessEvent(QueryableXEventData data)
        { 

            foreach (PublishedEvent x_event in data)
            {
                string name = x_event.Name;
                reads++;


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


                if (reads % 4000 == 0)
                {
                    //UInt64 i = GetAllCount();
                    var percent = (int)(reads * 100 / eventCount);

                    fn_UpdateMsg("File:" + fileNum + "/" + totalFile + ", Processing " + reads.ToString()+"/"+eventCount.ToString()+" ("+ percent.ToString()+"%)");
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

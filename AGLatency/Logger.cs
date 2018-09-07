using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;


namespace AGLatency
{
    public static class Logger
    {
        public static string LogFile = "";
        private static StreamWriter LoggingSW = null;
        private static bool InitDone = false;
        /// <summary>
        /// Delete older logs, only keep 30 latest logs
        /// </summary>
        private static void DeleteOldLogs(string logpath)
        {
            const int MAXLOGFILES = 30;
            DirectoryInfo info = new DirectoryInfo(logpath);
            FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
            var toDelete = files.Length - MAXLOGFILES;
            if (toDelete <= 0) return;

            foreach (FileInfo file in files)
            {
                toDelete--;
                if (toDelete < 0) break;

                File.Delete(file.FullName);

                // DO Something...
            }
        }
        private static void CreateLogFile()
        {
            try
            {
                string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                //string path2 = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                var exe = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

                var filename = Path.GetFileNameWithoutExtension(exe) + System.DateTime.Now.ToString("_yyyy-MM-dd_HH_mm_ss") + ".log";

                string logFolder = Path.Combine(path, "LOG");
                if (!Directory.Exists(logFolder)) Directory.CreateDirectory(logFolder);

                DeleteOldLogs(logFolder);

                string logfile = Path.Combine(logFolder, filename);

                if (File.Exists(logfile)) File.Delete(logfile);// ; else File.Create(logfile);

                LogFile = logfile;

                LoggingSW = new StreamWriter(logfile, false);
            }
            catch (Exception ex)
            {
                LogFile = null;
                LoggingSW = null;
                File.AppendAllText("Error.txt", ex.Message);
            }

        }

        private static void LogHeader()
        {
            LoggingSW.WriteLine(System.DateTime.Now.ToString("=========== yyyy-MM-dd:HH:mm:ss ")
                + " Logging start ===========");



            LoggingSW.WriteLine("LogFile:" + LogFile);
            var version = typeof(Program).Assembly.GetName().Version;
            LoggingSW.WriteLine("Version:" + version);
            LoggingSW.WriteLine("==========================================================");

            LoggingSW.Flush();
        }

        public static void LogMessage(string msg)
        {
            if (!InitDone)
            {
                InitDone = true;
                CreateLogFile();
                LogHeader();
            }

            string txt = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + msg;
            Console.WriteLine(txt);
            LoggingSW.WriteLine(txt);
            LoggingSW.Flush();

        }


        public static void LogException(Exception e, Thread currentThread)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format("Thread {0} : ***  Exception encountered  *** ", currentThread.ManagedThreadId));
            sb.AppendLine(String.Format("Thread {0} : Exception message    : {1}  ", currentThread.ManagedThreadId, e.Message));
            sb.AppendLine(String.Format("Thread {0} : Exception code       : {1}  ", currentThread.ManagedThreadId, e.HResult));
            sb.AppendLine(String.Format("Thread {0} : Exception stack      : {1}  ", currentThread.ManagedThreadId, e.StackTrace));

            LogMessage(sb.ToString());

        }


    }
}

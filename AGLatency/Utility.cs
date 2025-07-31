using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace AGLatency
{
    public static class Utility
    {
        private static void DirectoryCopy(
        string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, true);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static void CopyHtmlFiles(string outputPath)
        {
            string htmlPath = Path.Combine(System.IO.Path.GetDirectoryName(
             System.Reflection.Assembly.GetExecutingAssembly().Location),  "html");

            if (Directory.Exists(htmlPath))
            {
                try
                {
                    DirectoryCopy(htmlPath, outputPath, true);
                }
                catch (Exception e)
                {
                    Logger.LogMessage("[ERROR]CopyHtmlFiles:" + e.Message);
                }

                //modify report.html and change its title

                try
                {
                    //string report = Path.Combine(outputPath, "report.html");
                    //string content = File.ReadAllText(report);
                    //var title = Path.GetFileNameWithoutExtension(dumpFile);

                    //content = content.Replace("<title>SQLDumpViewer Report</title>", "<title>" + title + "</title>");
                    //File.WriteAllText(report, content);
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("[ERROR]CopyHtmlFiles:Modify title:" + ex.Message);
                }

            }

            else
            {
                Logger.LogMessage("[ERROR]" + htmlPath + " not found.");
            }

        }

        public static List<string> GetFileListFromFolder(string sourceDirectory,string[] masks)
        {
            List<string> files = new List<string>();

            try
            {
                //var mdmpFiles = Directory.EnumerateFiles(sourceDirectory, "*.mdmp", SearchOption.AllDirectories);

                DirectoryInfo di = new DirectoryInfo(sourceDirectory);
                var i = 0;

                var directory = new DirectoryInfo(sourceDirectory);
              
                var allfiles = masks.SelectMany(p => directory.EnumerateFiles(p, SearchOption.AllDirectories));

                //foreach (var dumpfile in di.EnumerateFiles("*.mdmp", SearchOption.AllDirectories))
                foreach (var f in allfiles)
                {
                    i++;
                    // if (i > 6) break;
                    files.Add(f.FullName);


                }


            }
            catch (Exception ex)
            {
                Logger.LogException(ex, Thread.CurrentThread);

            }


            return files;
        }

        public static bool isValidFile(string file)
        {
            if (String.IsNullOrEmpty(file)) return false;
            if (File.Exists(file)) return true;
            else return false;
        }

        public static bool isValidfolder(string path)
        {
            if (String.IsNullOrEmpty(path)) return false;
            if (Directory.Exists(path)) return true;
            else return false;

        }

    }
}

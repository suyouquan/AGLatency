using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace SQLDumpViewer.PageTemplate
{
   public abstract class PageDataCommon
    {
        //the inherited class must implement Execute function
        public abstract bool GetData();
        //The inherited class must implement this function to save  result to disk file.
        public abstract bool SavePageToDisk();

         
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGLatency
{
    public class EventWithMode
    {
        public EventMetaData.xEvent e;
        public int mode=-1;

        public EventWithMode(EventMetaData.xEvent evt, int md=-1)
        {
            e = evt;
            mode = md;
        }

    }
    public class EventLatency
    {
        public SQLiteDB eventDB;
        public List<EventWithMode> primaryEvents = new List<EventWithMode>();
        public List<EventWithMode> secondaryEvents = new List<EventWithMode>();

        public EventLatency(string dbname)
        {
            eventDB = new SQLiteDB();
            eventDB.Init(dbname);
        }

       

    }
}

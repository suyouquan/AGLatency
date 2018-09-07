using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGLatency.PageTemplate
{
    public enum PageObjState { SaveToDiskOnly, ExecuteAndSaveToDisk };

    public class PageObject
    {
        public string name;
        public PageTemplate.PageDataCommon page;
        public PageObjState state;
        public Int32 outputOrder = 999;
        public PageObject(string n, PageTemplate.PageDataCommon obj,int order=999)
        {
            this.name = n;
            this.page = obj;
            this.state =PageObjState.ExecuteAndSaveToDisk;
            outputOrder = order;
        }

        public PageObject(string n, PageTemplate.PageDataCommon obj, PageObjState state, int order=999)
        {
            this.name = n;
            this.page = obj;
            this.state = state;
            outputOrder = order;
        }
        
            

 

    }

}

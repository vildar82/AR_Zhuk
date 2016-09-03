using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_DataModel
{
    public  class HouseInfo
    {
        public List<FlatInfo> Sections = new List<FlatInfo>();
        public List<Section> SectionsBySize = new List<Section>();
        public ProjectInfo SpotInf { get; set; }

        
    }
}

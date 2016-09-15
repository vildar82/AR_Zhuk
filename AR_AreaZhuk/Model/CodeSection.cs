using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Model
{
    public class Code
    {
        public int CountReqFlats { get; set; }
        public string CodeStr { get; set; }
        public int CountFlats { get; set; }
        public List<FlatInfo> IdSections { get; set; }
        public int NumberSection { get; set; }
        public string SpotOwner { get; set; }
        public int Floors { get; set; }

        public Code (string code, List<FlatInfo> idSections, int countFlats, int numberSection, string spotName, int floors,int countReqFlats)
        {
            CodeStr = code;
            IdSections = idSections;            
            CountFlats = countFlats;
            NumberSection = numberSection;
            SpotOwner = spotName;
            Floors = floors;
            CountReqFlats = countReqFlats * (floors-1);
        }
    }

    public class CodeSection
    {
        public int CountFloors { get; set; }
        public List<FlatsInSection> SectionsByCountFlats = new List<FlatsInSection>();
    }

    public class FlatsInSection
    {
        public int CountFlats { get; set; }
        public List<Code> SectionsByCode = new List<Code>();
    }
}

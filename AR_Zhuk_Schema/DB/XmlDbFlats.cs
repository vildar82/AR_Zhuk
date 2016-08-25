using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_Schema.DB
{
    [Serializable]
    public class XmlDbFlats
    {
        public SelectSectionParam SelectParam { get; set; }
        public List<List<DbFlat>> DbFlats { get; set; }        
    }
}

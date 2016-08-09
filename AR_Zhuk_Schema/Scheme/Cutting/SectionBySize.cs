using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_Zhuk_Schema.Scheme.Cutting
{
    class SectionBySize
    {
        public Section Section { get; set; }
        public string Key { get; set; }
        public bool isFromPassedDict { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Percentage.New
{
    class SectionByCountFlat
    {
        private List<FlatInfo> sections;
        public int IndexSec { get; private set; }
        public bool IsDominant { get; private set; }
        public int CountFlatWoLLU { get; private set; }          
        public Dictionary<string, List<FlatInfo>> DictCodes { get; private set; }   
        public Dictionary<int, SectionByCode> SectionsByCodeIndexes { get; private set; }

        public SectionByCountFlat (int indexSec, int сountFlatsWoLLU, List<FlatInfo> sections)
        {
            IndexSec = indexSec;
            this.sections = sections;
            IsDominant = sections[0].IsDominant;
            CountFlatWoLLU = сountFlatsWoLLU;            
            // группировка секций по коду
            DictCodes = GetSectionsbyCode();
            SectionsByCodeIndexes = SectionByCode.GetSections(sections, 0);
        }  

        private Dictionary<string, List<FlatInfo>> GetSectionsbyCode ()
        {
            var res = sections.GroupBy(s => s.Code).ToDictionary(k=>k.Key, v=>v.ToList());
            return res;
        }

        public List<int> GetCountsFlatByReq (int indexReq)
        {
            List<int> res = SectionsByCodeIndexes.SelectMany(s => s.Value.GetCountFlatsByReq(indexReq)).ToList();
            return res;
        }
    }
}

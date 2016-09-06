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
        public int CountFlatWoLLU { get; private set; }  
        /// <summary>
        /// Список секций сгруппированных по кол квартир в первом требовании
        /// </summary>
        public List<SectionByCode> SectionsByCode { get; private set; }
        public double FactorDom { get; private set; }

        public SectionByCountFlat (int сountFlatsWoLLU, List<FlatInfo> sections, double factorDom)
        {
            this.sections = sections;
            CountFlatWoLLU = сountFlatsWoLLU;
            FactorDom = factorDom;
            // группировка секций по коду
            SectionsByCode = SectionByCode.GetSections(sections, 0, FactorDom);
        }

        public static List<SectionByCountFlat> GetSections (List<FlatInfo> sections, double factorDom)
        {
            var sizes = sections.GroupBy(g => g.CountFlats).
                    Select(g => new SectionByCountFlat(g.Key-1, g.ToList(), factorDom)).ToList();            
            return sizes;
        }        
    }
}

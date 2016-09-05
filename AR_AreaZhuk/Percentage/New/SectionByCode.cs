using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Percentage.New
{
    class SectionByCode
    {        
        private List<FlatInfo> sections;        
        public int IndexReq { get; private set; } 
        public int CountCodeIndexFlat { get; private set; }
        /// <summary>
        /// Процентажное кол квартир в этом индексе требования
        /// </summary>
        public double CountFlatPercentage { get; private set; }
        /// <summary>
        /// Секции по индексу требования - ключ - кол квартир процентажное (с учетом коэф этажности дома)
        /// </summary>
        public List<SectionByCode> SectionsByCodeNextReq { get; private set; }

        public SectionByCode (int indexReq, int countCodeIndexFlat, List<FlatInfo> sections, double factorDom)
        {
            IndexReq = indexReq;
            CountCodeIndexFlat = countCodeIndexFlat;
            this.sections = sections;
            // Процентажное кол квартир
            CountFlatPercentage = countCodeIndexFlat * factorDom;
            // Квартиры след индекса
            SectionsByCodeNextReq = GetSections(sections, indexReq + 1, factorDom);
        }

        public static List<SectionByCode> GetSections(List<FlatInfo> sections, int indexReq, double factorDom)
        {
            if (indexReq == sections[0].CodeArray.Length)
            {
                return null;
            }
            // группировка сецйи по первому требованию (кол квартир в первой индексе кода секций)
            List<SectionByCode> secsByCode = sections.GroupBy(g => g.CodeArray[indexReq]).OrderBy(o=>o.Key).
                Select(v => new SectionByCode(indexReq, v.Key, v.ToList(), factorDom)).ToList();
            return secsByCode;
        }
    }
}

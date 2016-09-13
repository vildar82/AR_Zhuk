using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_AreaZhuk.Percentage.New.Satisfy
{
    class SectionsByType
    {
        private double factorDom;
        public bool IsDominant { get; private set; }
        public double TotalCountFlatD { get; private set; }

        public List<SectionByCountFlat> Sections = new List<SectionByCountFlat>();        
        private List<List<int>>[] codesByReq;

        public SectionsByType(bool isDominant, double factorDom, int countRequirements)
        {
            IsDominant = isDominant;
            this.factorDom = factorDom;
            codesByReq = new List<List<int>>[countRequirements];
            for (int i = 0; i < codesByReq.Length; i++)
            {
                codesByReq[i] = new List<List<int>>();
            }
        }

        public void Add (List<SectionByCountFlat> sections)
        {                        
            this.Sections.AddRange(sections);
            foreach (var secByCountFlat in sections)
            {
                TotalCountFlatD += secByCountFlat.CountFlatWoLLU * factorDom;
                for (int i = 0; i < codesByReq.Length; i++)
                {
                    var codes = secByCountFlat.GetCountsFlatByReq(i);                    
                    codesByReq[i].Add(codes.GroupBy(g=>g).OrderBy(o=>o.Key).Select(s=>s.Key).ToList());
                }
            }            
        }

        /// <summary>
        /// Определение вариантов секций для заданной суммы квартир
        /// </summary>        
        public List<int[]> GetSectionsByReqSum (int sumFlatReq, int indexReq)
        {
            // список кол квартир по всем секциям для заданного индекса требования
            var codes = codesByReq[indexReq];
            // варианты кодов для заданной суммы
            var sucCodes = Combina.CodeCombinations(codes, sumFlatReq);
            return sucCodes;
        }        
    }
}

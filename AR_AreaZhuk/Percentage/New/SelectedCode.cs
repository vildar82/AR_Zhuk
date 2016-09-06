using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_AreaZhuk.Percentage.New
{
    class SelectedCode
    {        
        /// <summary>
        /// Индекс требования
        /// </summary>
        private int indexReq;
        /// <summary>
        /// Выбранный индекс кол.квартир (общее кол = размеру списков)
        /// </summary>
        private int selectedIndex;
        /// <summary>
        /// Следующий индекс требования
        /// </summary>
        private List<SelectedCode> codes { get; set; }  
        /// <summary>
        /// Секции по кол квартир этого индекса требования
        /// </summary>
        private List<SectionByCode> sectionsByCodeReq { get; set; } 
             
        /// <summary>
        /// Создание списка вариантов процентажных кодов всех секций по номерам требования
        /// Размер массива = кол секций в текущем варианте домов
        /// </summary>        
        public static SelectedCode[] GetSelectedCodes (List<SectionByCountFlat> sectionsByCountFlat,
            ReqCountFlat[] reqCountFlat)
        {
            SelectedCode[] selCodes = new SelectedCode[sectionsByCountFlat.Count];
            // для каждой секции опредилить индексы перебора требований начиная с первого
            for (int i = 0; i < selCodes.Length; i++)
            {                
                var sections = sectionsByCountFlat[i];
                SelectedCode selCode = new SelectedCode();
                selCode.Calc(sections.SectionsByCode, reqCountFlat, 0);
                selCodes[i] = selCode;
            }
            return selCodes;
        }

        public SectionByCode GetSelectedSections ()
        {
            SectionByCode res;            
            if (codes.Count ==0)
            {
                res = sectionsByCodeReq[selectedIndex];
            }
            else
            {                
                res = codes[selectedIndex].GetSelectedSections();
            }
            return res;                
        }

        public void ResetSelIndex ()
        {
            selectedIndex = 0;
        }

        public bool IncrementSelIndex ()
        {
            selectedIndex++;
            bool res = selectedIndex < sectionsByCodeReq.Count;
            return res;
        }

        public double GetCountFlat (int indexReq)
        {
            double countFlatRes = 0; 
            if (this.indexReq == indexReq)
            {
                countFlatRes = sectionsByCodeReq[selectedIndex].CountFlatPercentage;
            }
            else
            {                
                countFlatRes = codes[selectedIndex].GetCountFlat(indexReq);
            }
            return countFlatRes;
        }

        private void Calc (List<SectionByCode> sectionsByCode, ReqCountFlat[] reqCountFlat, int indexReq)
        {
            if (indexReq < reqCountFlat.Length)
            {
                this.indexReq = indexReq;
                // определение допустимых индексов кодов для перебора по первому требованию

                // диапазон индексов кол квартир которые удовлетворяют требованию - от 0 до требуемого кол квартир процентажного с шагом в 1 кв по коду
                var reqFlat = reqCountFlat[indexReq];
                double countFlatReq = reqFlat.Count + reqFlat.Offset;

                // Пересечение индексов в налиичии и всех индексов удовл. требованию (от 0 до тр.кол.кв.)
                sectionsByCodeReq = sectionsByCode.Where(w => w.CountFlatPercentage <= countFlatReq).ToList();

                // Расчет следующий требований для каждого кол.квартир первого требования

                codes = new List<SelectedCode>();
                for (int i = 0; i < sectionsByCodeReq.Count; i++)// заменить на foreach
                {
                    var selCode = new SelectedCode();
                    selCode.Calc(sectionsByCodeReq[i].SectionsByCodeNextReq, reqCountFlat, indexReq + 1);
                    if (selCode.codes != null)
                    {
                        codes.Add(selCode);
                    }
                }
            }
        }    
    }
}

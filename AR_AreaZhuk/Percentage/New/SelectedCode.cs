using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_AreaZhuk.Percentage.New
{
    class SelectedCode
    {
        private int selectedIndex;
        private List<SelectedCode> codes { get; set; }        
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
                var selCode = selCodes[i];
                var sections = sectionsByCountFlat[i];
                selCode.Calc(sections, reqCountFlat);
            }
            return selCodes;
        }

        private void Calc (SectionByCountFlat sections, ReqCountFlat[] reqCountFlat)
        {
            // определение допустимых индексов кодов для перебора по первому требованию

            // диапазон индексов кол квартир которые удовлетворяют требованию - от 0 до требуемого кол квартир процентажного с шагом в 1 кв по коду
            double countReq = reqCountFlat[0].Count;

            // Пересечение индексов в налиичии и всех индексов удовл. требованию (от 0 до тр.кол.кв.)
            var sectionsByCodeReq = sections.SectionsByCode.Where(w => w.CountFlatPercentage <= countReq).ToList(); // пока без допуска

            // расчет следующих кодов для каждого варианта этих кодов
            
        }
    }
}

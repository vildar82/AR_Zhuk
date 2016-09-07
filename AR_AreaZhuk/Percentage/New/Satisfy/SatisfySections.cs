using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_AreaZhuk.Percentage.New.Satisfy;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Percentage.New
{
    /// <summary>
    /// Определение подходящих секций под процентаж
    /// </summary>
    class SatisfySections
    {
        List<SectionByCountFlat> selSecsByCountFlat;
        List<Requirment> requirments;
        double factorDom;
        double totalCountFlatD;
        ReqCountFlat[] reqCountFlat;

        // обычные секции (int - индекс секции)
        SectionsByType ordinarySections;
        SectionsByType dominantSections;

        /// <summary>
        /// Список подходящих кодов секций под процентаж
        /// </summary>
        public List<string[]> SectionsCodes { get; private set; }

        public SatisfySections (List<SectionByCountFlat> selSecsByCountFlat, 
            List<Requirment> requirments, double factorDom)
        {
            this.selSecsByCountFlat = selSecsByCountFlat;
            this.requirments = requirments;
            this.factorDom = factorDom;
            ordinarySections = new SectionsByType(false, 1, requirments.Count);
            dominantSections = new SectionsByType(true, factorDom, requirments.Count);
        }

        public void Calc ()
        {
            // разделение секций на рядовые и доминанты
            DefSectionsAndCountFlatD();
            totalCountFlatD = ordinarySections.TotalCountFlatD + dominantSections.TotalCountFlatD;
            // Кол квартир на каждое требования (процентажное)
            reqCountFlat = ReqCountFlat.GetRequirementCountFlats(requirments, totalCountFlatD);

            // варианты сумм квартир рядовых и доминантных секций для каждого требования                        
            for(int r =0; r< reqCountFlat.Length; r++)
            {
                var reqFlat = reqCountFlat[r];
                // Список вариантов сумм для типов секций (рядовых и доминант)
                List<int[]> varsSumFlatBySecType = GetVarsSumSecType(reqFlat);
                
                // Получить список вариантов кодов секций для каждого сочетания сумм квартир по типам секций
                foreach (var sumFlatBySecType in varsSumFlatBySecType)
                {
                    // варианты кодов рядовых секций
                    var varSecsOrdinary = ordinarySections.GetSectionsByReqSum(sumFlatBySecType[0], r);
                    // варианты кодов доминантных секций
                    var varSecsDom = dominantSections.GetSectionsByReqSum(sumFlatBySecType[1], r);
                }
            }            
        }

        private List<int[]> GetVarsSumSecType (ReqCountFlat req)
        {
            // список вариантов сумм (рядовых + доминант)
            List<int[]> resVars = new List<int[]>();

            // Варианты сумм кв доминант (квартиры чистые, без коэф.этажности)
            var domSumVars = GetVarSumDominants(req);

            // варианты сумм квартир рядовых секций, для каждого варианта сумм квартир доминант
            foreach (int sumDom in domSumVars)
            {                
                // Формула Лени
                double sumOrdinary = req.Count - (sumDom * factorDom);
                int sumOrdinaryInt;
                // Если это целое число, то оно подходит под процентаж. Если не больше 0, то берем сумму 0 для рядовых секц
                if (sumOrdinary > 0 && IsInteger(sumOrdinary, out sumOrdinaryInt))
                {                    
                    resVars.Add(new[] { sumOrdinaryInt, sumDom });
                }                               
            }
            return resVars;
        }

        private bool IsInteger (double d, out int sumOrdinaryInt)
        {
            sumOrdinaryInt = Convert.ToInt32(d);
            var res = Math.Abs(d - sumOrdinaryInt) <= 0.21;
            return res;
        }

        private List<int> GetVarSumDominants (ReqCountFlat req)
        {
            List<int> resVarsSumDom = new List<int>();
            if (dominantSections.TotalCountFlatD ==0)
            {
                // Нет доминант в расчете
                resVarsSumDom.Add(0);
            }
            else
            {
                // сумма квартир доминант максимальная
                int maxSumDom =Convert.ToInt32(req.Count+req.Offset / factorDom);
                if (ordinarySections.TotalCountFlatD ==0)
                {
                    // Нет рядовых секций в расчете. 
                    // Берем одну сумму доминант которое дает нужный процентаж
                    resVarsSumDom.Add(maxSumDom);
                }
                else
                {
                    // От нуля до макимума, с шагом 1кв
                    resVarsSumDom = Enumerable.Range(0, maxSumDom+1).ToList();
                }
            }
            return resVarsSumDom;
        }

        private void DefSectionsAndCountFlatD ()
        {
            var groupSecByDom = selSecsByCountFlat.GroupBy(g => g.IsDominant);
            foreach (var item in groupSecByDom)
            {                
                if (item.Key)
                {
                    dominantSections.Add(item.ToList());
                }
                else
                {
                    ordinarySections.Add(item.ToList());                    
                }                
            }
        }
    }
}

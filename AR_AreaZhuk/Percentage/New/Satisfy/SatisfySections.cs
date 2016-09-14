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

        List<Tuple<List<int[]>, List<int[]>>>[] varsSum;

        /// <summary>
        /// Список подходящих кодов секций под процентаж
        /// </summary>
        public List<string[]> SectionsCodes  = new List<string[]>();

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
            Combina.Init();
            // разделение секций на рядовые и доминанты
            DefSectionsAndCountFlatD();
            totalCountFlatD = ordinarySections.TotalCountFlatD + dominantSections.TotalCountFlatD;
            // Кол квартир на каждое требования (процентажное)
            reqCountFlat = ReqCountFlat.GetRequirementCountFlats(requirments, totalCountFlatD);

            // варианты сумм квартир рядовых и доминантных секций для каждого требования                        
            varsSum = new List<Tuple<List<int[]>, List<int[]>>>[reqCountFlat.Length];
            for(int r =0; r< reqCountFlat.Length; r++)
            {
                varsSum[r] = new List<Tuple<List<int[]>, List<int[]>>>();
                var reqFlat = reqCountFlat[r];
                // Список вариантов сумм для типов секций (рядовых и доминант)
                List<int[]> varsSumFlatBySecType = GetVarsSumSecType(reqFlat);
                
                // Получить список вариантов кодов секций для каждого сочетания сумм квартир по типам секций
                foreach (var sumFlatBySecType in varsSumFlatBySecType)
                {
                    // варианты кодов рядовых секций
                    var varSecsOrdinary = ordinarySections.GetSectionsByReqSum(sumFlatBySecType[0], r);
                    if (varSecsOrdinary.Count == 0) continue;
                    // варианты кодов доминантных секций
                    var varSecsDom = dominantSections.GetSectionsByReqSum(sumFlatBySecType[1], r);
                    if (varSecsDom.Count == 0) continue;

                    // прошедший вариант                     
                    varsSum[r].Add(new Tuple<List<int[]>, List<int[]>>(varSecsOrdinary, varSecsDom));
                }
                if (varsSum[r].Count == 0)
                {
                    return;
                }
            }
            // Выбор кодов екций

            // Подстановка кода первого требования в первый вариант суммы кодов рядовых и доминант            
            int indexReq = 0;
            var ordinarysCode = ordinarySections.Sections.Select(s => s.SectionsByCodeIndexes).ToList();
            var domsCode = dominantSections.Sections.Select(s => s.SectionsByCodeIndexes).ToList();
            foreach (var sum in varsSum[indexReq])
            {
                var secsOrd = GetSubstCodesSec(sum.Item1, ordinarysCode);
                var secsDom = GetSubstCodesSec(sum.Item2, domsCode);
            }
        }

        private List<List<SectionByCode>> GetSubstCodesSec (List<int[]> varsSum, List<Dictionary<int, SectionByCode>> ordinarysCode)
        {
            List<List<SectionByCode>> res = new List<List<SectionByCode>> ();
            foreach (var varSum in varsSum)
            {
                List<SectionByCode> secsCodesVar = new List<SectionByCode>();
                for (int i = 0; i < varSum.Length; i++)
                {
                    SectionByCode secCode;
                    if (ordinarysCode[i].TryGetValue(varSum[i], out secCode))
                    {
                        secsCodesVar.Add(secCode);
                    }
                }
                if (secsCodesVar.Count != 0)
                {
                    res.Add(secsCodesVar);
                }
            }
            return res;
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
                int sumOrdinary = Math.Abs(Convert.ToInt32(req.Count - (sumDom * factorDom)));                                
                resVars.Add(new[] { sumOrdinary, sumDom });
            }
            return resVars;
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
                int maxSumDom =Convert.ToInt32(req.Count / factorDom);
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

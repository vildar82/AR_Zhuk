using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Percentage.New
{
    class ChoiceSections
    {        
        private List<SectionByCountFlat> sectionsByCountFlat;
        ReqCountFlat[] reqCountFlat;

        public ChoiceSections (List<SectionByCountFlat> sectionsByCountFlat, ReqCountFlat[] reqsFlat)
        {
            this.reqCountFlat = reqsFlat;
            this.sectionsByCountFlat = sectionsByCountFlat;            
        }

        public List<HouseInfo> Select ()
        {
            List<HouseInfo> resHouses = new List<HouseInfo>();

            // Определение перебираемых индексов по каждому требованию
            SelectedCode[] selectedCode = SelectedCode.GetSelectedCodes(sectionsByCountFlat, reqCountFlat);
            // Перебор первых индексов требований 
            while (IncrementCalcIndexCode(selectedCode.Length-1, selectedCode, 0))
            {

            }

            return resHouses;
        }

        private bool IncrementCalcIndexCode (int indexSel, int[] selectedFirstCodeReq, int indexReq)
        {
            bool res = true;
            if (indexSel ==-1)
            {
                res = false;
            }
            else
            {
                selectedFirstCodeReq[indexSel]++;
                // Проверка индекса
                if (selectedFirstCodeReq[indexSel]== sectionsByCountFlat[indexSel].SectionsByCode.Count)
                {
                    selectedFirstCodeReq[indexSel] = 0;
                    // Сдвиг индекса
                    res = IncrementCalcIndexCode(indexSel-1, selectedFirstCodeReq, indexReq);
                }
                else
                {
                    // проверка кол. квартир процентажное
                    double curCount = 0;
                    for (int i = 0; i < selectedFirstCodeReq.Length; i++)
                    {
                        curCount += sectionsByCountFlat[i].SectionsByCode[selectedFirstCodeReq[i]].SectionsByCodeNextReq[indexReq].CountFlatPercentage;
                    }
                    var reqFlat = reqCountFlat[indexReq];
                    if (curCount - reqFlat.Count > reqFlat.Offset)
                    {
                        res = false;
                    }
                }
            }
            return res;
        }

        
    }
}

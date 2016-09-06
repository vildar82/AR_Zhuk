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
        SelectedCode[] selectedCode;

        public ChoiceSections (List<SectionByCountFlat> sectionsByCountFlat, ReqCountFlat[] reqsFlat)
        {
            this.reqCountFlat = reqsFlat;
            this.sectionsByCountFlat = sectionsByCountFlat;            
        }

        public SuccessHouses Select ()
        {
            SuccessHouses successHouses = new SuccessHouses();
            successHouses.GOS = new List<GeneralObject>();            

            // Определение перебираемых индексов по каждому требованию
            selectedCode = SelectedCode.GetSelectedCodes(sectionsByCountFlat, reqCountFlat);
            // Перебор кодов секций удовлетворяющих процентажу
            do
            {
                // проверка кол. квартир процентажное
                if (CheckSelCodePercentage())
                {
                    // Прошедшие секции
                    var successSections = GetSelectedSections();
                    List<GeneralObject> gos = GetHouses(successSections);
                    successHouses.GOS.AddRange(gos);
                }

            } while (IncrementSelectedCode(selectedCode.Length - 1));
            
            return successHouses;
        }

        private bool CheckSelCodePercentage ()
        {
            bool res = true;
            for (int r = 0; r < reqCountFlat.Length; r++)
            {
                double curCount = 0;
                curCount = selectedCode.Sum(s => s.GetCountFlat(r));
                var reqFlat = reqCountFlat[r];
                if (Math.Abs(curCount - reqFlat.Count) > reqFlat.Offset)
                {
                    res = false;
                }
            }
            return res;
        }

        private List<GeneralObject> GetHouses (List<SectionByCode> successSections)
        {
            List<GeneralObject> res = new List<GeneralObject>();
            // Все комбинации секций
            int[] selectedSuccessSec = new int[successSections.Count];

            do
            {                
                HouseInfo hi = new HouseInfo();
                hi.Sections = new List<FlatInfo>();
                var sections = new List<FlatInfo>();
                for (int i = 0; i < successSections.Count; i++)
                {
                    var sec = successSections[i].Sections[selectedSuccessSec[i]];
                    sections.Add(sec);
                }
                // группировка по домам
                var houses = sections.GroupBy(g => g.SpotOwner).Select(s => new HouseInfo { Sections = s.ToList() }).ToList();
                var go = new GeneralObject { Houses = houses };
                res.Add(go);

            } while (IncrementSuccesSec(selectedSuccessSec.Length-1, selectedSuccessSec, successSections));

            return res;
        }

        private bool IncrementSuccesSec (int index, int[] selectedSuccessSec, List<SectionByCode> successSections)
        {
            bool res = true;
            if (index ==-1)
            {
                res = false;
            }
            else
            {
                selectedSuccessSec[index]++;
                if (selectedSuccessSec[index] == successSections[index].Sections.Count)
                {
                    selectedSuccessSec[index] = 0;
                    res = IncrementSuccesSec(index - 1, selectedSuccessSec, successSections);
                }
            }
            return res;
        }

        private List<SectionByCode> GetSelectedSections ()
        {
            var res = selectedCode.Select(s => s.GetSelectedSections()).ToList();
            return res;
        }

        private bool IncrementSelectedCode (int indexSel)
        {
            bool res = true;
            if (indexSel ==-1)
            {
                res = false;
            }
            else
            {
                if (!selectedCode[indexSel].IncrementSelIndex())
                {
                    selectedCode[indexSel].ResetSelIndex();
                    res = IncrementSelectedCode(indexSel - 1);
                }                                
            }
            return res;
        }       
    }
}

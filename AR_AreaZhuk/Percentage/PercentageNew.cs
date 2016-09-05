using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_AreaZhuk.Percentage.New;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Percentage
{
    class PercentageNew : IPercentage
    {
        List<List<HouseInfo>> totalObject;
        List<GeneralObject> gos;
        int[] selectedHouses;
        int[] selectedSecByCountFlat;        
        List<SectionInPlace> sectionsInPlaces;
        List<Requirment> requirments;
        int floorsMain;
        int floorsDom;
        double factorDom;
        double totalCountFlat;
        ReqCountFlat[] reqCountFlat;

        public List<GeneralObject> Calc (List<List<HouseInfo>> totalObject)
        {
            this.totalObject = totalObject;
            gos = new List<GeneralObject>();
            requirments = MainForm.ProjectInfo.requirments;
            floorsMain = MainForm.ProjectInfo.CountFloorsMain;
            floorsDom = MainForm.ProjectInfo.IsEnabledDominant ? MainForm.ProjectInfo.CountFloorsDominant : floorsMain;
            factorDom = floorsDom / (double)floorsMain;

            // перебор ввариантов домов
            selectedHouses = new int[totalObject.Count];
            do
            {
                // Секции по этому варианту домов                
                sectionsInPlaces = SectionInPlace.GetSections(selectedHouses, totalObject, factorDom);
                if (sectionsInPlaces.Count == 0)
                    continue;

                // Перебор секций по кол квартир
                selectedSecByCountFlat = new int[sectionsInPlaces.Count];
                do
                {
                    // Подбор секций под процентаж
                    var secsByCountFlat = GetCurrentSelectedSectionsByCountFlat();
                    // Кол квартир на одном этаже (процентажное)
                    totalCountFlat = secsByCountFlat.Sum(s => s.CountFlatWoLLU * s.FactorDom);
                    // Кол квартир на каждое требования (процентажное)
                    reqCountFlat = GetRequirementCountFlats();
                    
                    ChoiceSections choice = new ChoiceSections(secsByCountFlat, reqCountFlat);
                    var succesHouses = choice.Select();

                } while (IncremenSectionByCountFlat(sectionsInPlaces.Count - 1));
            } while (IncrementHouse(totalObject.Count - 1));
            return gos;
        }

        private bool IncrementHouse (int index)
        {
            bool res = true;
            if (index == -1)
            {
                res = false;
            }
            else
            {
                selectedHouses[index]++;
                if (selectedHouses[index]== totalObject[index].Count)
                {
                    selectedHouses[index] = 0;
                    IncrementHouse(index - 1);
                }
            }
            return res;
        }

        private bool IncremenSectionByCountFlat (int index)
        {
            bool res = true;
            if (index == -1)
            {
                res = false;
            }
            else
            {
                selectedSecByCountFlat[index]++;
                if (selectedSecByCountFlat[index] == sectionsInPlaces[index].SectionsByCountFlat.Count)
                {
                    selectedSecByCountFlat[index] = 0;
                    IncremenSectionByCountFlat(index - 1);
                }
            }
            return res;
        }

        private List<SectionByCountFlat> GetCurrentSelectedSectionsByCountFlat ()
        {
            List<SectionByCountFlat> curSecsByCountFlats = new List<SectionByCountFlat>();
            for (int i = 0; i < selectedSecByCountFlat.Length-1; i++)
            {
                curSecsByCountFlats.Add(sectionsInPlaces[i].SectionsByCountFlat[selectedSecByCountFlat[i]]);
            }
            return curSecsByCountFlats;
        }

        private ReqCountFlat[] GetRequirementCountFlats ()
        {
            ReqCountFlat[] resReqs = new ReqCountFlat[requirments.Count];
            for (int r = 0; r < requirments.Count; r++)
            {
                resReqs[r] = new ReqCountFlat(requirments[r], totalCountFlat);
            }
            return resReqs;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AR_AreaZhuk.Controller;
using AR_AreaZhuk.Percentage.New;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Percentage
{
    class PercentageNew
    {
        public event EventHandler<EventIntArg> ChangeCount;

        List<List<HouseInfo>> totalObject;
        List<GeneralObject> gos;
        int[] selectedHouses;
        int[] selectedSecByCountFlat;        
        List<List<SectionByCountFlat>> sectionsByCountFlats;
        List<Requirment> requirments;
        int floorsMain;
        int floorsDom;
        double factorDom;                
        ProjectInfo ProjectInfo;

        public List<GeneralObject> Calc (List<List<HouseInfo>> totalObject, BackgroundWorker worker)
        {
            Calculate.Init();

            this.totalObject = totalObject;
            gos = new List<GeneralObject>();
            ProjectInfo = MainForm.ProjectInfo;
            requirments = ProjectInfo.requirments;
            floorsMain = ProjectInfo.CountFloorsMain;
            floorsDom = ProjectInfo.IsEnabledDominant ? ProjectInfo.CountFloorsDominant : floorsMain;
            factorDom = floorsDom / (double)floorsMain;            

            // перебор ввариантов домов
            selectedHouses = new int[totalObject.Count];
            do
            { 
                if (worker.CancellationPending)
                {
                    break;
                }

                Debug.WriteLine("selectedHouses: " + string.Join(".", selectedHouses));

                // Секции по этому варианту домов                
                sectionsByCountFlats = GetSections(selectedHouses, totalObject);
                if (sectionsByCountFlats.Count == 0)
                    continue;

                // Перебор секций по кол квартир
                selectedSecByCountFlat = new int[sectionsByCountFlats.Count];
                do
                {
                    if (worker.CancellationPending)
                    {
                        break;
                    }

                    Debug.WriteLine("selectedSecByCountFlat: " + string.Join(".", selectedSecByCountFlat));

                    // Подбор секций под процентаж
                    var selSecsByCountFlat = GetCurrentSelectedSectionsByCountFlat();

                    var satisfySecs = new SatisfySections(selSecsByCountFlat, ProjectInfo.requirments, factorDom);
                    satisfySecs.Calc();

                    // Проверка процентажа                    

                    // Прошел вариант

                    // расчет реальных процентажей
                    //succesHouse.SuccesProjectInfo = GetSuccesProjectInfo(succesHouse.GOS[0].Houses);

                    // Подсчет площадей
                    //foreach (var go in succesHouse.GOS)
                    //{
                    //    var piGo = succesHouse.SuccesProjectInfo.Copy();
                    //    go.SpotInf = piGo;
                    //    AreaCalcHelper.Calc(go);
                    //    gos.Add(go);
                    //}

                    // Обновление числа вариантов на форме
                    ChangeCount?.Invoke(null, new EventIntArg(gos.Count));

                } while (IncremenSectionByCountFlat(sectionsByCountFlats.Count - 1));
            } while (IncrementHouse(totalObject.Count - 1));
            return gos;
        }

        public List<List<SectionByCountFlat>> GetSections (int[] selectedHouses, List<List<HouseInfo>> totalObject)
        {
            List<List<SectionByCountFlat>> secsByCountflats = new List<List<SectionByCountFlat>>();

            List<Section> allSections = new List<Section>();
            for (int i = 0; i < selectedHouses.Length; i++)
            {
                var sections = totalObject[i][selectedHouses[i]].SectionsBySize.ToList();
                allSections.AddRange(sections);
            }
            if (MainForm.ProjectInfo.IsEnableDominantsOffset)
            {
                // Все доминанты (их шаги)
                List<int> dominantsStep = allSections.Where(s => s.IsDominant).Select(s => s.CountStep).ToList();
                if (dominantsStep.Count > 1)
                {
                    if (dominantsStep.Max() - dominantsStep.Min() > MainForm.ProjectInfo.DominantOffSet)
                    {
                        // Условие разности кол шагов доминант не удовлетворено
                        return secsByCountflats;
                    }
                }
            }

            for (int i = 0; i < allSections.Count; i++)
            {
                var sec = allSections[i];
                var secsByCountFlat = sec.Sections.GroupBy(g => g.CountFlats).
                    Select(g => new SectionByCountFlat(i, g.Key - 1, g.ToList())).ToList();                
                secsByCountflats.Add(secsByCountFlat);
            }

            return secsByCountflats;
        }

        private ProjectInfo GetSuccesProjectInfo (List<HouseInfo> houses)
        {
            ProjectInfo sucPi = ProjectInfo.Copy();

            var secs = houses.SelectMany(s => s.Sections).ToList();
            var totalCountFlat = secs.Sum(s => s.CountFlats - 1);

            for (int r = 0; r < sucPi.requirments.Count; r++)
            {
                var req = sucPi.requirments[r];
                req.RealCountFlats = secs.Sum(s => s.CodeArray[r]);
                req.RealCountFlats = Convert.ToInt32((req.RealCountFlats / (double)totalCountFlat) * 100);                
            }
            return sucPi;
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
                if (selectedSecByCountFlat[index] == sectionsByCountFlats[index].Count)
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
            for (int i = 0; i < selectedSecByCountFlat.Length; i++)
            {
                curSecsByCountFlats.Add(sectionsByCountFlats[i][selectedSecByCountFlat[i]]);
            }
            return curSecsByCountFlats;
        }        
    }
}

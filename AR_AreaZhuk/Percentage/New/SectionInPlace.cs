using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Percentage.New
{
    class SectionInPlace
    {        
        private List<Section> sections;

        public bool IsDominant { get; private set; }
        public int CountStep { get; private set; }
        public List<SectionByCountFlat> SectionsByCountFlat { get; private set; }
        public double FactorFloor { get; private set; }

        public SectionInPlace (List<Section> sections, double factorfloor)
        {
            this.sections = sections;
            var f = sections[0];
            IsDominant = f.IsDominant;
            CountStep = f.CountStep;
            FactorFloor = factorfloor;
            SectionsByCountFlat = SectionByCountFlat.GetSections(sections, factorfloor);
        }        

        public static List<SectionInPlace> GetSections (int[] selectedHouses, List<List<HouseInfo>> totalObject,double factorDom)
        {
            List<SectionInPlace> secsInPlaces = new List<SectionInPlace>();
                        
            List<List<Section>> allSections = new List<List<Section>>();
            for (int i = 0; i < selectedHouses.Length; i++)
            {
                var sections = totalObject[i][selectedHouses[i]].SectionsBySize.ToList();
                allSections.Add(sections);                
            }
            if (MainForm.ProjectInfo.IsEnableDominantsOffset)
            {
                // Все доминанты (их шаги)
                List<int> dominantsStep = allSections.Where(s => s[0].IsDominant).Select(s => s[0].CountStep).ToList();
                if (dominantsStep.Count > 1)
                {
                    if (dominantsStep.Max() - dominantsStep.Min() > MainForm.ProjectInfo.DominantOffSet)
                    {
                        // Условие разности кол шагов доминант не удовлетворено
                        return secsInPlaces;
                    }
                }
            }

            foreach (var secs in allSections)
            {
                var secInPlace = new SectionInPlace(secs, secs[0].IsDominant? factorDom : 1);                
                secsInPlaces.Add(secInPlace);
            }            
            
            return secsInPlaces;
        }        
    }
}

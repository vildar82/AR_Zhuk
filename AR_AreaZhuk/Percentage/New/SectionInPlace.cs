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
        private List<FlatInfo> sections;

        public bool IsDominant { get; private set; }
        public int CountStep { get; private set; }
        public List<SectionByCountFlat> SectionsByCountFlat { get; private set; }
        public double FactorFloor { get; private set; }

        public SectionInPlace (List<FlatInfo> sections, double factorfloor)
        {
            this.sections = sections;
            var f = sections[0];
            IsDominant = f.IsDominant;
            CountStep = f.CountStep;
            FactorFloor = factorfloor;
            SectionsByCountFlat = SectionByCountFlat.GetSections(sections, factorfloor);
        }        

        public static List<List<SectionByCountFlat>> GetSections (int[] selectedHouses, List<List<HouseInfo>> totalObject,double factorDom)
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

            foreach (var secs in allSections)
            {
                var secInPlace = new SectionInPlace(secs.Sections, secs.IsDominant? factorDom : 1);
                secsByCountflats.Add(secInPlace.SectionsByCountFlat);
            }            
            
            return secsByCountflats;
        }        
    }
}

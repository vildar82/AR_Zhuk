using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Percentage
{
    static class PercentageHelper
    {
        public static bool GetHouseSections (int[] selectedHouse, List<List<HouseInfo>> totalObject, out List<List<FlatInfo>> sections)
        {
            sections = new List<List<FlatInfo>>();
            for (int i = 0; i < selectedHouse.Length; i++)
            {
                sections.AddRange(totalObject[i][selectedHouse[i]].SectionsBySize.Select(s => s.Sections));
            }
            if (MainForm.ProjectInfo.IsEnableDominantsOffset)
            {
                // Все доминанты (их шаги)
                List<int> dominantsStep = sections.Where(s => s[0].IsDominant).Select(s => s[0].CountStep).ToList();
                if (dominantsStep.Count > 1)
                {
                    if (dominantsStep.Max() - dominantsStep.Min() > MainForm.ProjectInfo.DominantOffSet)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

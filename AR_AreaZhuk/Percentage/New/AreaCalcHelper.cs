using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_AreaZhuk.Controller;
using AR_AreaZhuk.PIK1TableAdapters;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Percentage.New
{
    static class AreaCalcHelper
    {
        private static PIK1.C_Flats_PIK1_AreasDataTable flatsAreas = new PIK1.C_Flats_PIK1_AreasDataTable();

        static AreaCalcHelper()
        {
            C_Flats_PIK1_AreasTableAdapter pikFlats = new C_Flats_PIK1_AreasTableAdapter();
            flatsAreas = pikFlats.GetData();
        }

        public static GeneralObject Calc (GeneralObject go)
        {   
            int countFlats = 0;
            double totalArea = 0;
            double liveArea = 0;

            int countContainsLargeFlatSections = 0;
            double k1 = 0;
            double k2 = 0;
            int countSectionsIndex =0;

            foreach (var hi in go.Houses)
            {
                foreach (var sec in hi.Sections)
                {
                    countSectionsIndex++;
                    double levelArea = 0;
                    double levelAreaOffLLU = 0;
                    double levelAreaOnLLU = 0;

                    if (sec.Flats.Any(x => x.SubZone.Equals("3")) ||
                        sec.Flats.Any(x => x.SubZone.Equals("4")))
                        countContainsLargeFlatSections++;
                    foreach (var flat in sec.Flats)
                    {
                        var currentFlatAreas = flatsAreas.First(x => x.Short_Type.Equals(flat.ShortType));
                        var areas = Calculate.GetAreaFlat(sec.Floors, flat, currentFlatAreas);
                        totalArea += areas[0];
                        liveArea += areas[1];
                        levelArea += areas[2];
                        levelAreaOffLLU += areas[3];
                        levelAreaOnLLU += areas[4];
                    }
                    k1 += levelAreaOffLLU / levelArea;
                    k2 += levelAreaOffLLU / levelAreaOnLLU;

                    countFlats += sec.CountFlats - 1;
                }
            }
            k1 = k1 / countSectionsIndex;
            k2 = k2 / countSectionsIndex;            
            
            go.SpotInf.CountContainsSections = countContainsLargeFlatSections;
            go.SpotInf.K1 = k1;
            go.SpotInf.K2 = k2;
            go.SpotInf.TotalStandartArea = totalArea;
            go.SpotInf.TotalLiveArea = liveArea;
            go.SpotInf.TotalFlats = countFlats;
            go.SpotInf.TypicalSections = GetCountTypicalSections(go.Houses);           
            
            return go;
        }

        private static string GetCountTypicalSections (List<HouseInfo> houses)
        {
            var typicalSect = houses.SelectMany(s=>s.Sections).GroupBy(g => g.IdSection).
                    Select(s => s.Count()).Where(w => w > 1).OrderBy(o => o).
                    Aggregate(string.Empty, (s, i) => s + i + ";");
            return typicalSect;
        }
    }
}

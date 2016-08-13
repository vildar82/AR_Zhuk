using AR_Zhuk_DataModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_AreaZhuk.Controller
{
   public static class Calculate
    {
       public static double[] GetAreaFlat(int floors, RoomInfo flat, PIK1.C_Flats_PIK1_AreasRow currentFlatAreas)
       {
           double[] area = new double[5];//1 - общая площадь, 2 - жилая площадь,3 - площадь этажа/4 - площадь этажа без ЛЛУ/5 - площадь этажа с учетом ЛЛУ
           double areaTotalStandart = 0;
           double areaTotalStrong = 0;
           double areaLiveStandart = 0;
           double areaLiveStrong = 0;
           double areaLevel = 0;
           double areaOffLLULevel = 0;
           double areaOnLLULevel = 0;

           string corrector = currentFlatAreas.Correction_Low18;
           if (floors >= 18)
               corrector = currentFlatAreas.Correction_More18;
           string[] str = corrector.Split('|');
           double correctArea = str.Sum(t => double.Parse(t));
           switch (flat.Joint)
           {
               #region Joint.None
               case Joint.None:
               {
                       areaLevel = currentFlatAreas.Area_Level_Combo; //S этажа (совмещение)
                       if (floors < 18)
                       {
                           areaTotalStandart = currentFlatAreas.Area_Total_Low18; //S общая  (стандарт/совмещение/<18)
                           areaLiveStandart = currentFlatAreas.Area_Live_Low18;//S жилая (стандарт/совмещение/<18)
                       }
                       else
                       {
                           areaTotalStandart = currentFlatAreas.Area_Total_Standart_More18;//S общая  (стандарт/совмещение/>=18)
                           areaTotalStrong = currentFlatAreas.Area_Total_Strong_More18;//S общая (усиление/совмещение/>=18)

                           areaLiveStandart = currentFlatAreas.Area_Live_Standart_More18;//S жилая (стандарт/совмещение/>=18)
                           areaLiveStrong = currentFlatAreas.Area_Live_Strong_More18;//S жилая (усиление/совмещение/>=18)
                       }
                       break;
                   }
               #endregion

               #region Joint.End
               case Joint.End://Торцы
                   {
                       areaLevel = currentFlatAreas.Area_Level_End; //S этажа (торец)
                       if (areaLevel.Equals(0))
                           areaLevel = currentFlatAreas.Area_Level_Combo;
                       if (floors < 18)
                       {
                           areaTotalStandart = currentFlatAreas.Area_Total_End_Low18;//S общая (стандарт/торец/<18)
                           if (areaTotalStandart.Equals(0))
                               areaTotalStandart = currentFlatAreas.Area_Total_Low18;

                           areaLiveStandart = currentFlatAreas.Area_Live_End_Low18;//S жилая (стандарт/торец/<18)
                           if (areaLiveStandart.Equals(0))
                               areaLiveStandart = currentFlatAreas.Area_Live_Low18;
                       }
                       else
                       {
                           areaTotalStandart = currentFlatAreas.Area_Total_Standart_End_More18;//S общая  (стандарт/торец/>=18)
                           areaTotalStrong = currentFlatAreas.Area_Total_Strong_End_More18;//S общая (усиление/торец/>=18)
                           if (areaTotalStandart.Equals(0))
                               areaTotalStandart = currentFlatAreas.Area_Total_Standart_More18;
                           if (areaTotalStrong.Equals(0))
                               areaTotalStrong = currentFlatAreas.Area_Total_Strong_More18;

                           areaLiveStandart = currentFlatAreas.Area_Live_Standart_End_More18;//S жилая (стандарт/торец/>=18 )
                           areaLiveStrong = currentFlatAreas.Area_Live_Strong_End_More18;//S жилая (усиление/торец/>=18 )
                           if (areaLiveStandart.Equals(0))
                               areaLiveStandart = currentFlatAreas.Area_Live_Standart_More18;
                           if (areaLiveStrong.Equals(0))
                               areaLiveStrong = currentFlatAreas.Area_Live_Strong_More18;
                       }
                       break;
                   }
               #endregion

               #region Joint.Seam
               case Joint.Seam://Дефшвы
                   {
                       areaLevel = currentFlatAreas.Area_Level_Seam; //S этажа (торец)
                       if (areaLevel.Equals(0))
                           areaLevel = currentFlatAreas.Area_Level_Combo;
                       if (floors < 18)
                       {
                           areaTotalStandart = currentFlatAreas.Area_Total_Standart_Seam_Low18;//S общая (стандарт/дефшов/>=18)
                           if (areaTotalStandart.Equals(0))
                               areaTotalStandart = currentFlatAreas.Area_Total_Low18;

                           areaLiveStandart = currentFlatAreas.Area_Live_Seam_Low18;//S жилая (стандарт/дефшов/<18)
                           if (areaLiveStandart.Equals(0))
                               areaLiveStandart = currentFlatAreas.Area_Live_Low18;
                       }
                       else
                       {
                           areaTotalStandart = currentFlatAreas.Area_Total_Standart_Seam_More18;//S общая (усиление/дефшов/>=18)
                           areaTotalStrong = currentFlatAreas.Area_Total_Strong_Seam_More18;//Поправка на первый жилой этаж (<18)
                           if (areaTotalStandart.Equals(0))
                               areaTotalStandart = currentFlatAreas.Area_Total_Standart_More18;
                           if (areaTotalStrong.Equals(0))
                               areaTotalStrong = currentFlatAreas.Area_Total_Strong_More18;

                           areaLiveStandart = currentFlatAreas.Area_Live_Standart_Seam_More18;//S жилая  (стандарт/дефшов/>=18)
                           areaLiveStrong = currentFlatAreas.Area_Live_Strong_Seam_More18;//S жилая (усиление/дефшов/>=18)
                           if (areaLiveStandart.Equals(0))
                               areaLiveStandart = currentFlatAreas.Area_Live_Standart_More18;
                           if (areaLiveStrong.Equals(0))
                               areaLiveStrong = currentFlatAreas.Area_Live_Strong_More18;

                       }
                       break;
                   }
               #endregion:
           }
           area[2] = areaLevel;
          
           areaOffLLULevel = areaTotalStrong;
           if(Math.Round(areaTotalStrong,0).Equals(0))
               areaOffLLULevel = areaTotalStandart;

           if (!Math.Round(areaTotalStrong, 0).Equals(0))
               areaOnLLULevel = areaTotalStrong + currentFlatAreas.Area_OutFlat_Strong;
           else areaOnLLULevel = areaTotalStandart + currentFlatAreas.Area_OutFlat_Combo;
           
           //areaOnLLULevel = areaTotalStrong;
           //if (Math.Round(areaTotalStrong, 0).Equals(0))
           //    areaOnLLULevel = areaTotalStandart;


           area[3] = areaOffLLULevel;
           area[4] = areaOnLLULevel;
           if (floors <= 17)
           {
               area[0] += areaTotalStandart + correctArea;//Один этаж считается с поправкой на вентблоки
               area[0] += (floors - 2) * areaTotalStandart; //3-... этаж
               area[1] += (floors - 1) * areaLiveStandart;
           }
           else if (floors >= 25)
           {
               area[0] += areaTotalStrong + correctArea;//Один этаж считается с поправкой на вентблоки
               area[0] += (8) * areaTotalStrong; //3-10 этаж
               area[0] += (floors - 10) * areaTotalStandart; //11-... этаж

               area[1] += (9) * areaLiveStrong; //2-10 этаж жилая площадь
               area[1] += (floors - 10) * areaLiveStandart; //11-... этаж Жилая плоащдь
           }
           else if (floors >= 18 & floors < 25)
           {
               area[0] += areaTotalStrong + correctArea;//Один этаж считается с поправкой на вентблоки
               area[0] += (3) * areaTotalStrong; //3-5 этаж
               area[0] += (floors - 5) * areaTotalStandart; //6-... этаж

               area[1] += (4) * areaLiveStrong; //2-5 этаж жилая плоащдь
               area[1] += (floors - 6) * areaLiveStandart; //6-... этаж жилая плоащь
           }
           return area;
       }
    }
}

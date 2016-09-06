using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_DataModel
{

    public class ProjectInfo
    {
        public string PathInsolation { get; set; }

        public double SpotArea = 2280.96 - 1.152;
        public string GUID { get; set; }
        public int TotalFlats { get; set; }
        public int TotalSections { get; set; }
        public string TypicalSections { get; set; }        
        /// <summary>
        /// Общая площадь с учетом ЛЛУ       
        /// </summary>
        public double TotalStandartArea { get; set; }
        /// <summary>
        /// Общая площадь без учета ЛЛУ
        /// </summary>
        public double TotalArea { get; set; }
        public double TotalLiveArea { get; set; }
        public double LevelArea { get; set; }
        public int CountContainsSections { get; set; }
        /// <summary>
        /// Коэф TotalStandartArea/S этажа
        /// </summary>
        public double K1 { get; set; }        
        /// <summary>
        /// Коэф TotalStandartArea/TotalArea
        /// </summary>
        public double K2 { get; set; }
        /// <summary>
        /// Размер объекта застройки
        /// Правая нижняя ячейка Excel.
        /// Стартовая ячейка 1,1.
        /// </summary>
        public Cell Size { get; set; }
        /// <summary>
        /// Ячейки инсоляции всех домов
        /// </summary>
        public List<Module> InsModulesAll { get; set; }
        public List<Requirment> requirments = new List<Requirment>();
        public List<SpotOption> SpotOptions { get; set; }
        public int DominantOffSet { get; set; }
        public bool IsEnableDominantsOffset { get; set; }
        /// <summary>
        /// Основная этажность секций
        /// </summary>
        public int CountFloorsMain { get; set; }
        /// <summary>
        /// Этажность доминантных секций
        /// </summary>
        public int CountFloorsDominant { get; set; }
        public bool IsEnabledDominant { get; set; }

        public ProjectInfo Copy()
        {
            ProjectInfo s = (ProjectInfo)MemberwiseClone();
            s.requirments = new List<Requirment>();
            foreach (var r in requirments)
            {
                var newR = r.Clone();
                //newR.RealCountFlats = 0;
                //newR.RealPercentage = 0;
                s.requirments.Add(newR);
            }            
            return s;
        }

        public void SortRequirmentsByUser ()
        {
            requirments.Sort((r1, r2) => r1.UserSortIndex.CompareTo(r2.UserSortIndex));
        }

        public void SortRequirmentsForCalculate ()
        {
            requirments.Sort((r1, r2) => r1.Percentage.CompareTo(r2.Percentage));
        }
    }

    public class Requirment
    {
        public double RealPercentage { get; set; }
        public int RealCountFlats { get; set; }
        public string SubZone { get; set; }
        public string CodeZone { get; set; }
        public int MinArea { get; set; }
        public int MaxArea { get; set; }
        public int Percentage { get; set; }
        public double NearPercentage { get; set; }
        public double OffSet { get; set; }
        // public  List<RoomInfo> RoomsGeneral { get; set; }
        public int CountFlats { get; set; }
        public int MaxCountFlat { get; set; }
        public int MinCountFlat { get; set; }
        public int UserSortIndex { get; set; }

        public Requirment () { }

        public Requirment(string subZone, int minArea, int maxArea, int percentage, int minCountFlats, int maxCountFlats, int realCount, double realPercentage, double offset,string codeZone)
        {
            this.SubZone = subZone;
            this.MinArea = minArea;
            this.MaxArea = maxArea;
            this.Percentage = percentage;
            this.MaxCountFlat = maxCountFlats;
            this.MinCountFlat = minCountFlats;
            // this.RoomsGeneral = rooms;
            this.RealCountFlats = realCount;
            this.RealPercentage = realPercentage;
            this.OffSet = offset;
            this.CodeZone = codeZone;
        }

        public List<Requirment> Copy(List<Requirment> reqTemp)
        {
            List<Requirment> reqs = new List<Requirment>();
            foreach (var r in reqTemp)
            {
                reqs.Add(r.Clone());
            }
            return reqs;
        }
        
        public Requirment Clone()
        {
            return (Requirment)MemberwiseClone();
        }
    }
}

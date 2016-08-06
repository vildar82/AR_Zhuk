﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_DataModel
{

    public class SpotInfo
    {        
        public double SpotArea = 2280.96 - 1.152;
        public string GUID { get; set; }
        public int TotalFlats { get; set; }
        public int TotalSections { get; set; }
        public string TypicalSections { get; set; }
        public double RealArea { get; set; }
        /// <summary>
        /// Размер объекта застройки
        /// Правая нижняя ячейка Excel.
        /// Стартовая ячейка 1,1.
        /// </summary>
        public Cell Size { get; set; }
        public List<Requirment> requirments = new List<Requirment>();

        public SpotInfo CopySpotInfo(SpotInfo sp)
        {
            SpotInfo s = new SpotInfo();
            foreach (var r in sp.requirments)
            {
                s.requirments.Add(new Requirment(r.SubZone, r.MinArea, r.MaxArea, r.Percentage, r.MinCountFlat, r.MaxCountFlat, 0, 0, r.OffSet,r.CodeZone));
            }
            return s;
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

        public double OffSet { get; set; }
        // public  List<RoomInfo> RoomsGeneral { get; set; }
        public int CountFlats { get; set; }
        public int MaxCountFlat { get; set; }
        public int MinCountFlat { get; set; }

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
                reqs.Add(new Requirment(r.SubZone, r.MinArea, r.MaxArea, r.Percentage, r.MinCountFlat, r.MaxCountFlat, r.RealCountFlats, r.RealPercentage, r.OffSet,r.CodeZone));
            }
            return reqs;
        }

        public Requirment()
        {
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AR_Zhuk_DataModel
{
    public class FlatInfo

    {    
        [XmlIgnore]
        public Section Section { get; set; }
        public int IdSection { get; set; }
        public int CountStep { get; set; }
        public bool IsInvert { get; set; }
        public bool IsVertical { get; set; }
        public bool IsCorner { get; set; }
        public double Area { get; set; }
        public int Floors { get; set; }
        public string Code { get; set; }
        public string SpotOwner { get; set; }
        public int NumberInSpot { get; set; }

        public List<RoomInfo> Flats = new List<RoomInfo>();
        public int CountFlats { get { return Flats.Count; } }

        public FlatInfo Copy()
        {
            var res = (FlatInfo)MemberwiseClone();
            return res;
        }
    }
}

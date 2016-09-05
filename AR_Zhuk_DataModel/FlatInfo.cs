using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_DataModel
{
    public class FlatInfo
    {
        public string IdenticalCode { get; set; }
        public bool IsDominant { get; set; }
        /// <summary>
        /// Стартовая ячейка секции - левый верхний угол
        /// Для построения картинки
        /// </summary>
        public Cell ImageStart { get; set; }
        /// <summary>
        /// Угол поворота секции - по часовой стрелке.
        /// 0 - в стандартном положении секции.
        /// </summary>
        public int ImageAngle { get; set; }        
        public int IdSection { get; set; }
        public int CountStep { get; set; }
        public bool IsInvert { get; set; }
        public bool IsVertical { get; set; }
        public bool IsCorner { get; set; }
        public double Area { get; set; }
        public int Floors { get; set; }
        public string Code { get; set; }
        public int[] CodeArray { get; set; }
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

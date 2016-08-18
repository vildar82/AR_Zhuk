using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_Schema.Insolation
{
    public class LightingWindow : ICloneable
    {
        /// <summary>
        /// Индекс шага модуля окна в квартире
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Номер помещения (два окна могут быть в одном помещении. их инсоляция не складывается) 
        /// </summary>
        public int RoomNumber { get; set; }
        public string InsValue { get; set; }

        public LightingWindow (int index, int number)
        {
            Index = index;
            RoomNumber = number;
        }

        public object Clone ()
        {
            return MemberwiseClone();
        }
    }
}

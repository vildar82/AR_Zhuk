using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Percentage.New
{
    class ReqCountFlat
    {
        /// <summary>
        /// Требуемое кол. квартир процентажное
        /// </summary>
        public double Count { get; private set; }
        /// <summary>
        /// Допуск - кол.квартир процентажное
        /// </summary>
        public double Offset { get; internal set; }

        public ReqCountFlat (Requirment req, double totalCountFlat)
        {
            Count = req.Percentage * totalCountFlat * 0.01;
            Offset = req.OffSet * totalCountFlat * 0.01;
        }
    }
}

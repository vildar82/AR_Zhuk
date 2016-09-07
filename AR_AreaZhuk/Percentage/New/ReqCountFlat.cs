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

        public ReqCountFlat (Requirment req, double totalCountFlatD)
        {
            Count = req.Percentage * totalCountFlatD * 0.01;
            Offset = req.OffSet * totalCountFlatD * 0.01;
        }

        public static ReqCountFlat[] GetRequirementCountFlats (List<Requirment> requirments, double totalCountFlatD)
        {
            ReqCountFlat[] resReqs = new ReqCountFlat[requirments.Count];
            for (int r = 0; r < requirments.Count; r++)
            {
                resReqs[r] = new ReqCountFlat(requirments[r], totalCountFlatD);
            }
            return resReqs;
        }
    }
}

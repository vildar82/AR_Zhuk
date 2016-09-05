using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Percentage
{
    public interface IPercentage
    {
        List<GeneralObject> Calc (List<List<HouseInfo>> totalObject);
    }
}

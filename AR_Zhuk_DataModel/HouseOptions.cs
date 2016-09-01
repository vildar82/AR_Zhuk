using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_DataModel
{
    /// <summary>
    /// Параметры дома
    /// </summary>
    public class SpotOption
    {
        public string Name { get; set; }        
        /// <summary>
        /// Позиции доминантных секций - 1,2,3, предпоследняя, последняя
        /// </summary>
        public List<bool> DominantPositions { get; set; }

        public SpotOption(string houseName, List<bool> dominantPos)
        {
            Name = houseName;            
            DominantPositions = dominantPos;
        }
        public SpotOption () { }
    }
}

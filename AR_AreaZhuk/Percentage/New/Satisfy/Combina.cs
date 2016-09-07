using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_AreaZhuk.Percentage.New.Satisfy
{
    public static class Combina
    {
        // минимальная и максимальная сумма квартир для кол секций
        static int[] sumMinCodes;
        static int[] sumMaxCodes;
        static List<List<int>> _codes;
        static int _sum;

        /// <summary>
        /// варианты сочетания чисел дающих заданную сумму
        /// </summary>
        /// <param name="codes">Список секций и набора чисел в ней (кол квартир)</param>        
        /// <returns>Список вариантов сочетания чисел дающих заданную сумму. int[] - соответствует кол секций</returns>
        public static List<int[]> CodeCombinations (List<List<int>> codes, int sum)
        {
            _codes = codes;
            _sum = sum;
            List<int[]> resCombo = new List<int[]>();

            DefMinMaxSums();            

            if (!CheckResMinMaxSum(sum, 0))
            {
                // из заданных секций нельзя подобрать требуемую сумму квартир
                return resCombo;
            }

            // перебор секций                       
            int[] curCode = new int[codes.Count];
            IterateCodes(sum, 0, ref resCombo, ref curCode);            

            return resCombo;
        }

        private static void IterateCodes (int sumRest, int indexSec, ref List<int[]> resCodes, ref int[] curCode)
        {            
            // Индеск за пределами диапазона
            if (indexSec == _codes.Count)
            {
                if (sumRest == 0)
                {
                    resCodes.Add(curCode.ToArray());                    
                }                
                return;
            }   
            else if (indexSec == _codes.Count - 1)
            {
                // В последней секции проверяем есть ли кол квартир равное текущему остатку - только одно значение может подходить
                if (_codes[indexSec].Contains(sumRest))
                {                    
                    curCode[indexSec]= sumRest;
                    IterateCodes(0, indexSec + 1, ref resCodes, ref curCode);
                    return;
                }
                else
                {
                    // нет нужно го индекса                    
                    return;
                }
            }
            if (!CheckResMinMaxSum(sumRest, indexSec))
            {                
                return;
            }

            foreach (var item in _codes[indexSec])
            {                
                var rest = sumRest - item;                
                curCode[indexSec] = item;
                IterateCodes(rest, indexSec + 1, ref resCodes, ref curCode);                
            }            
        }

        private static bool CheckResMinMaxSum (int sumRest, int indexSec)
        {
            return sumMinCodes[indexSec] <= sumRest && sumMaxCodes[indexSec] >= sumRest;
        }

        private static void DefMinMaxSums ()
        {
            sumMinCodes = new int[_codes.Count];
            sumMaxCodes = new int[_codes.Count];
            int sumMin =0;
            int sumMax = 0;
            // перебор секций начиная с последней
            //var codesRev = Enumerable.Reverse(_codes).ToList();
            for (int i = 0; i < _codes.Count; i++)
            {
                var code = _codes[i];
                sumMin += code[0];
                sumMax += code[code.Count - 1];
                sumMinCodes[i] = sumMin;
                sumMaxCodes[i] = sumMax;
            }
            sumMinCodes = sumMinCodes.Reverse().ToArray();
            sumMaxCodes = sumMaxCodes.Reverse().ToArray();         
        }
    }
}

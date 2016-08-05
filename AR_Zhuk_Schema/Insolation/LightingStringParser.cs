using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_Schema.Insolation
{
    public static class LightingStringParser
    {
        static bool isRange;
        static bool isLeaf; // окно в большой комнате с несколькими окнами
        static bool prevIsSide; // предыдущий индекс - это боковушка
        static Dictionary<string, Lighting> dictLightings = new Dictionary<string, Lighting>();

        public static Lighting GetLightings (string lightingString, bool isTopSide)
        {            
            if (string.IsNullOrEmpty(lightingString))
            {
                return null;
            }

            Lighting light;
            if (!dictLightings.TryGetValue(lightingString, out light))
            {
                light = new Lighting();                
                light.Indexes = new List<int>();
                light.SideIndexes = new List<int>();
                light.IsTopSide = isTopSide;              

                isRange = false;
                isLeaf = false;
                prevIsSide = false;

                for (int i = 0; i < lightingString.Length; i++)
                {
                    char item = lightingString[i];
                    if (char.IsDigit(item))
                    {
                        prevIsSide = false;
                        AddLightingValue((int)char.GetNumericValue(item), light.Indexes);
                        continue;
                    }

                    if (item == 'B')
                    {
                        // Боковая освещенность
                        AddSideLightingValue(lightingString, light.SideIndexes, ref i);
                        prevIsSide = true;
                        continue;
                    }

                    isRange = false;
                    isLeaf = false;

                    if (item == '-')
                    {
                        isRange = true;
                        continue;
                    }

                    if (item == '|')
                    {
                        isLeaf = true;
                        // изменение знака предыдущего индекса
                        if (!prevIsSide)
                        {
                            var lastLight = light.Indexes.Last();
                            light.Indexes[light.Indexes.Count - 1] = lastLight * -1;
                        }
                        else
                        {
                            var lastLight = light.SideIndexes.Last();
                            light.SideIndexes[light.SideIndexes.Count - 1] = lastLight * -1;
                        }
                        continue;
                    }
                }

                // Определение стороны по боковой инсоляции
                if (light.SideIndexes.Count > 0)
                {
                    // освещенность заканчивается боковой инсоляцией
                    if (prevIsSide)
                    {
                        // Верх - левая сторона: Низ - правая строна
                        light.Side = isTopSide ? Side.Left : Side.Right;                        
                    }
                    else
                    {
                        // Освещенность заканчивается обычными рядовым шагом, значит начиналась с боковой
                        // Верх - правая сторона : Низ - левая строна
                        light.Side = isTopSide ? Side.Right : Side.Left;                        
                    }
                }                
                dictLightings.Add(lightingString, light);
            }            
            return light;
        }        

        private static void AddLightingValue (int value, List<int> lightings)
        {
            int factorLeaf = isLeaf ? -1 : 1;
            if (isRange)
            {
                for (int i =Math.Abs(lightings.Last()) + 1; i <= value; i++)
                {
                    lightings.Add(i * factorLeaf);
                }
            }
            else
            {
                lightings.Add(value*factorLeaf);
            }
        }

        private static void AddSideLightingValue (string lightingString, List<int> sideLightings, 
            ref int iLightingString)
        {
            int factorLeaf = isLeaf ? -1 : 1;
            // индекс стороны
            int indexSide = GetSideIndex(ref iLightingString, lightingString);
            if (isRange)
            {
                for (int i = Math.Abs(sideLightings.Last())+1; i <= indexSide; i++)
                {
                    sideLightings.Add(i*factorLeaf);
                }
            }
            else
            {
                sideLightings.Add(indexSide*factorLeaf);
            }
        }

        private static int GetSideIndex (ref int i, string lightingString)
        {
            int resSideIndex = 1;
            if (i < lightingString.Length-1)
            {                
                var item = lightingString[i+1];
                if (char.IsDigit(item))
                {
                    resSideIndex = (int)char.GetNumericValue(item);
                    i++;
                }
            }
            return resSideIndex;
        }        
    }
    public class Lighting
    {
        public List<int> Indexes { get; set; }
        public List<int> SideIndexes { get; set; }
        public Side Side { get; set; }
        public bool IsTopSide { get; set; }
    }
}
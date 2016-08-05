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

        public static List<int> GetLightings (string lightingString, out List<int> sideLightings, bool isTopSide, out Side endSide)
        {
            Lighting light;
            List<int> lightings;

            if (!dictLightings.TryGetValue(lightingString, out light))
            {
                endSide = Side.None;
                lightings = new List<int>();
                sideLightings = new List<int>();

                if (string.IsNullOrEmpty(lightingString))
                {
                    return null;
                }

                isRange = false;
                isLeaf = false;
                prevIsSide = false;

                for (int i = 0; i < lightingString.Length; i++)
                {
                    char item = lightingString[i];
                    if (char.IsDigit(item))
                    {
                        prevIsSide = false;
                        AddLightingValue((int)char.GetNumericValue(item), lightings);
                        continue;
                    }

                    if (item == 'B')
                    {
                        // Боковая освещенность
                        AddSideLightingValue(lightingString, sideLightings, ref i);
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
                            var lastLight = lightings.Last();
                            lightings[lightings.Count - 1] = lastLight * -1;
                        }
                        else
                        {
                            var lastLight = sideLightings.Last();
                            sideLightings[sideLightings.Count - 1] = lastLight * -1;
                        }
                        continue;
                    }
                }

                // Определение стороны по боковой инсоляции
                if (sideLightings.Count > 0)
                {
                    // освещенность заканчивается боковой инсоляцией
                    if (prevIsSide)
                    {
                        if (isTopSide)
                        {
                            // Верх - левая сторона
                            endSide = Side.Left;
                        }
                        else
                        {
                            // Низ - правая строна
                            endSide = Side.Right;
                        }
                    }
                    else
                    {
                        // Освещенность заканчивается обычными рядовым шагом, значит начиналась с боковой
                        if (isTopSide)
                        {
                            // Верх - правая сторона
                            endSide = Side.Right;
                        }
                        else
                        {
                            // Низ - левая строна
                            endSide = Side.Left;
                        }
                    }
                }
                light = new Lighting(lightings, lightings, endSide, isTopSide);
                dictLightings.Add(lightingString, light);
            }
            else
            {
                lightings = light.Indexes;
                sideLightings = light.SideIndexes;
                endSide = light.Side;
            }
            return lightings;
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

        private class Lighting
        {
            public readonly List<int> Indexes;
            public readonly List<int> SideIndexes;
            public readonly Side Side;
            public readonly bool IsTopSide;

            public Lighting(List<int> indexes, List<int> sideIndexes, Side side, bool isTopSide)
            {
                Indexes = indexes;
                SideIndexes = sideIndexes;
                Side = side;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_Zhuk_Schema.Insolation
{
    public static class LightingRoomParser
    {
        static bool isRange;
        static bool isLeaf; // окно в большой комнате с несколькими окнами
        static bool prevIsSide; // предыдущий индекс - это боковушка
        static int roomNumber;
        static Dictionary<string, LightingRoom> dictLightings = new Dictionary<string, LightingRoom>();

        public static LightingRoom GetLightings (RoomInfo room, bool isCorner)
        {
            LightingRoom lightRoomRes;
            string key = room.Type + (isCorner ? "c" : "");
            if (!dictLightings.TryGetValue(key, out lightRoomRes))
            {
                lightRoomRes = new LightingRoom();

                LightingWindow sideLightings;
                Side side = Side.None;
                Side sideOther = Side.None;                

                if (room.SelectedIndexTop != 0)
                {              
                    lightRoomRes.IndexesTop = GetLightingsOneSideRooms(room.LightingTop, true, out sideLightings, out side);
                    lightRoomRes.SideIndexTop = sideLightings;
                }

                if (room.SelectedIndexBottom != 0)
                {                    
                    lightRoomRes.IndexesBot = GetLightingsOneSideRooms(room.LightingNiz, false, out sideLightings, out sideOther);
                    lightRoomRes.SideIndexBot = sideLightings;
                }

                if (side != Side.None && sideOther != Side.None && side != sideOther)
                {
                    throw new Exception("Ошибка. У квартиры сотора верхней боковой инсоляции не совпадает со стороной нижней боковой инсоляции.");
                }
                lightRoomRes.Side = side != Side.None ? side : sideOther;

                dictLightings.Add(key, lightRoomRes);
            }
            lightRoomRes = (LightingRoom)lightRoomRes.Clone();
            return lightRoomRes;
        }

        private static List<LightingWindow> GetLightingsOneSideRooms (string lightingString, bool isTop,
            out LightingWindow sideLighting, out Side side)
        {            
            sideLighting = null;
            List<LightingWindow> lightingsRes = new List<LightingWindow>();
            side = Side.None;

            if (string.IsNullOrEmpty(lightingString))
            {
                return lightingsRes;
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
                    AddLightingValue((int)char.GetNumericValue(item), lightingsRes);
                    continue;
                }

                if (item == 'B')
                {
                    // Боковая освещенность
                    AddSideLightingValue(lightingString,ref sideLighting, ref i);
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
                    roomNumber++;
                    isLeaf = true;
                    // изменение знака предыдущего индекса
                    if (prevIsSide)
                    {
                        sideLighting.RoomNumber = roomNumber;
                    }
                    else
                    {
                        var roomLight = lightingsRes.Last();                        
                        roomLight.RoomNumber = roomNumber;
                    }                    
                    continue;
                }
            }
            // Определение стороны по боковой инсоляции
            if (sideLighting != null)
            {
                // освещенность заканчивается боковой инсоляцией
                if (prevIsSide)
                {
                    // Верх - левая сторона: Низ - правая строна
                    side = isTop ? Side.Left : Side.Right;
                }
                else
                {
                    // Освещенность заканчивается обычными рядовым шагом, значит начиналась с боковой
                    // Верх - правая сторона : Низ - левая строна
                    side = isTop ? Side.Right : Side.Left;
                }
            }

            return lightingsRes;
        }               

        private static void AddLightingValue (int value, List<LightingWindow> lightings)
        {            
            if (isRange)
            {
                for (int i =Math.Abs(lightings.Last().Index) + 1; i <= value; i++)
                {
                    lightings.Add(new LightingWindow(i, 0));
                }
            }
            else
            {                
                int number = isLeaf? roomNumber: 0;                
                lightings.Add(new LightingWindow (value, number));
            }
        }

        private static void AddSideLightingValue (string lightingString,ref LightingWindow sideLighting,
            ref int iLightingString)
        {
            // индекс стороны
            int indexSide = GetSideIndex(ref iLightingString, lightingString);
            int number = isLeaf ? roomNumber : 0;
            sideLighting = new LightingWindow(indexSide, number);
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
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_Zhuk_Schema.Insolation
{
    class InsolationSection : IInsolation
    {
        public static readonly List<RoomInsolation> RoomInsolations = new List<RoomInsolation>()
            {
                new RoomInsolation ("Однокомнатная или студия", 1, new List<string>() { "C" }),
                new RoomInsolation ("Двухкомнатная", 2, new List<string>() { "C", "2B" }),
                new RoomInsolation("Трехкомнатная", 3, new List<string>() { "C", "2B" }),
                new RoomInsolation ("Четырехкомнатная", 4, new List<string>() { "2C", "C+2B" })
            };                

        public InsolationSection()
        {   
        }

        /// <summary>
        /// Проверка инсоляции секции
        /// </summary>
        /// <param name="section">Проверяемая секция</param>
        /// <returns>Секции прошедшие инсоляцию</returns>
        public List<FlatInfo> GetInsolationSections (Section section)
        {
            IInsCheck insCheck = InsCheckFactory.CreateInsCheck(this, section);                  
            var resFlats = insCheck.CheckSections(section);
            if (ProjectScheme.MaxSectionBySize != 0)
            {
                // Рандомно выбрать заданное кол секций
                resFlats = GetRandomMaxCountSections(resFlats, ProjectScheme.MaxSectionBySize);
            }
            return resFlats;
        }

        private List<FlatInfo> GetRandomMaxCountSections (List<FlatInfo> sections, int maxSectionbySize)
        {
            List<FlatInfo> res = new List<FlatInfo>();
            if (sections.Count == 0 || sections.Count < maxSectionbySize)
                return sections;

            int maxValue = sections.Count-1;
            Random rnd = new Random();

            for (int i = 0; i < maxSectionbySize; i++)
            {
                var r = rnd.Next(maxValue);
                var s = sections[r];
                res.Add(s);
            }
            return res;
        }                

        public RoomInsolation FindRule (RoomInfo flat)
        {
            var rule = RoomInsolations.Where(x => x.CountRooms == Convert.ToInt32(flat.SubZone)).FirstOrDefault();
            return rule;
        }

        public List<RoomInfo> GetSideFlatsInSection (List<RoomInfo> sectionFlats, bool isTop, SectionType sectionType)
        {
            List<RoomInfo> topFlats = new List<RoomInfo>();
                        
            bool isCornerRight = sectionType == SectionType.CornerRight;

            if (isTop)
            {
                int indexFirstBottomFlat = 0;
                for (int i = 0; i < sectionFlats.Count; i++)
                {
                    indexFirstBottomFlat = i;
                    if (sectionFlats[i].SelectedIndexTop == 0)
                        break;
                }

                for (int i = indexFirstBottomFlat; i < sectionFlats.Count; i++)
                {
                    if (sectionFlats[i].SelectedIndexTop == 0) continue;
                    topFlats.Add(sectionFlats[i]);
                }

                for (int i = 0; i < indexFirstBottomFlat; i++)
                {
                    if (sectionFlats[i].SelectedIndexTop == 0) continue;
                    topFlats.Add(sectionFlats[i]);
                }
            }
            else
            {
                for (int i = 0; i < sectionFlats.Count; i++)
                {
                    if (sectionFlats[i].SelectedIndexTop != 0) continue;
                    topFlats.Add(sectionFlats[i]);
                }
            }
            
            return topFlats;
        }        

        public FlatInfo NewFlats (Section section, FlatInfo flat, bool isInvert)
        {
            FlatInfo resFlats = flat.Copy();
            resFlats.NumberInSpot = section.NumberInSpot;
            resFlats.SpotOwner = section.SpotOwner;      
            resFlats.IsInvert = isInvert;
            resFlats.IsVertical = section.IsVertical;
            resFlats.ImageAngle = section.ImageAngle;
            resFlats.ImageStart = section.ImageStart;
            resFlats.Floors = section.Floors;
            resFlats.IsDominant = section.IsDominant;                
            if (isInvert)
            {
                resFlats.ImageAngle += 180;
            }
            resFlats.Flats = flat.Flats;
            resFlats.Flats = flat.Flats.Select(f => f.Clone()).ToList();

            //// Временно - подмена индекса освещенностим для боковых квартир!!!???
            //foreach (var itemFlat in resFlats.Flats)
            //{
            //    var sideFlat = SideFlatFake.GetSideFlat(itemFlat);
            //    if (sideFlat != null)
            //    {
            //        itemFlat.LightingTop = sideFlat.LightingTop;
            //        itemFlat.LightingNiz = sideFlat.LightingBot;
            //    }
            //}
            //#else
            //            resFlats.Flats = flat.Flats;
            //#endif            
            return resFlats;
        }        
    }

    /// <summary>
    /// Правила инсоляции для квартиры (общие правила по типам квартир - 1,2,3,4 комнатной)
    /// </summary>
    public class RoomInsolation
    {
        /// <summary>
        /// Допустимые индексы инсоляции
        /// </summary>
        public static List<string> AllowedIndexes = new List<string> { "A", "B", "C", "D" };

        /// <summary>
        /// Название типа квартиры
        /// </summary>
        public string NameType { get; private set; }
        /// <summary>
        /// Количество комнат 1,2,3,4
        /// </summary>
        public int CountRooms { get; private set; }
        /// <summary>
        /// правила инсоляции (нужно чтобы удовлетворялось одно из них)
        /// </summary>
        public List<InsRule> Rules { get; private set; }

        public RoomInsolation (string name, int countRooms, List<string> rulesExpressions)
        {
            this.NameType = name;
            this.CountRooms = countRooms;
            Rules = ParseRules(rulesExpressions);
        }

        private List<InsRule> ParseRules (List<string> rulesExpressions)
        {
            List<InsRule> rules = new List<InsRule>();
            foreach (var ruleExpr in rulesExpressions)
            {
                InsRule rule = new InsRule(ruleExpr);
                rules.Add(rule);
            }
            return rules;
        }
    }

    /// <summary>
    /// Инсоляционное правило для квартиры - состоит из одного или нескольких требований (перечисленных через + в выражении требования)
    /// </summary>
    public class InsRule
    {
        /// <summary>
        /// Требование инсоляции (B - одно требование; B+2C - два требования, 1B и 2C инсолируемых помещения(окна) в квартире)
        /// </summary>
        public List<InsRequired> Requirements = new List<InsRequired>();
        public int TotalInsPoints { get; private set; }

        /// <summary>
        /// Требования инсоляции (C, 2D, C+2B)
        /// </summary>        
        public InsRule (string ruleExpr)
        {
            var indexes = ruleExpr.Split('+');
            foreach (var item in indexes)
            {
                var requireAdd = new InsRequired(item.Trim());
                Requirements.Add(requireAdd);
            }
            Requirements = Requirements.OrderByDescending(o => o.InsIndex).ToList();
            TotalInsPoints = Requirements.Sum(r => r.InsPoints);
        }
    }

    /// <summary>
    /// Инсоляционное требование - один индекс и кол инсолируемых комнат(окон)
    /// </summary>
    public class InsRequired
    {
        /// <summary>
        /// Требуемое кол инсолиуемых окон
        /// </summary>
        public int CountLighting { get; private set; }
        /// <summary>
        /// Требуемый индекс инсоляции (A, B, C, D)
        /// </summary>
        public string InsIndex { get; private set; }
        public int InsPoints { get; private set; }

        public InsRequired (string insValue, int count)        
        {
            InsIndex = insValue;
            CountLighting = count;
            var pts = GetInsPoints(insValue);
            InsPoints = pts * count;
        }        

        /// <summary>
        /// item - требование инсоляции - 2C
        /// </summary>
        /// <param name="item"></param>
        public InsRequired (string item)
        {
            string insIndex;
            CountLighting = 0;
            InsIndex = string.Empty;
            CountLighting = GetCountLighting(item, out insIndex);
            InsIndex = insIndex;

            if (!RoomInsolation.AllowedIndexes.Contains(InsIndex))
            {
                throw new Exception("Недопустимый индекс инсоляции в правилах - " + InsIndex + ".\n " +
                    "Допустимые индексы инсоляции " + string.Join(", ", RoomInsolation.AllowedIndexes));
            }

            InsPoints = GetInsPoints(insIndex) * CountLighting;
        }

        private int GetInsPoints (string insValue)
        {
            switch (insValue)
            {
                case "B":
                    return 1;
                case "C":
                case "D":
                    return 100;
            }
            return 0;
        }

        private int GetCountLighting (string item, out string insIndex)
        {
            var resCountLighting = 1;
            insIndex = item;
            // первый символ это требуемое число инсолируемых окон для данного индекса инсоляции, или пусто если 1.
            var firstChar = item.First();
            if (char.IsDigit(firstChar))
            {
                resCountLighting = (int)char.GetNumericValue(firstChar);
                insIndex = item.Substring(1);
            }
            return resCountLighting;
        }

        /// <summary>
        /// Проверка - проходит расчетный индекс инсоляции
        /// </summary>
        /// <param name="insIndexProject">Расчетный индекс инсоляции (по Excel)</param>
        /// <returns>Да, если расчетный индекс инсоляции выше или равен требуемому</returns>
        public bool IsPassed (string insIndexProject)
        {
            // Если проектный индекс больше требуемого, то проходит            
            var res = insIndexProject.CompareTo(InsIndex) >= 0;
            return res;
        }

        public void AddCount (int count)
        {
            CountLighting += count;
            InsPoints = GetInsPoints(InsIndex) * CountLighting;
        }
    }    
}

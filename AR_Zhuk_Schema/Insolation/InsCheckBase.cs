using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_Zhuk_Schema.Insolation
{
    abstract class InsCheckBase : IInsCheck
    {
        protected readonly IInsolation insService;
        internal readonly Section section;
        internal List<FlatInfo> sections;

        internal FlatInfo checkSection;
        internal bool isRightOrTopLLu;
        internal readonly bool isVertical;

        protected List<RoomInfo> topFlats;
        protected List<RoomInfo> bottomFlats;

        private string insSideLeftTopStandart;
        private string insSideLeftBotStandart;
        private string insSideRightTopStandart;
        private string insSideRightBotStandart;

        protected string insSideLeftTop;
        protected string insSideLeftBot;
        protected string insSideRightTop;
        protected string insSideRightBot;

        // Текущие проверяемые значения
        protected RoomInfo flat;
        protected bool isTop;
        protected bool isCurSide;
        protected int curFlatIndex;
        protected List<RoomInfo> curSideFlats;
        protected bool specialFail;

        protected abstract bool CheckFlats ();

        public InsCheckBase (IInsolation insService, Section section)
        {
            this.insService = insService;
            this.section = section;
            this.isVertical = section.IsVertical;

            DefineInsSideCells();
        }

        public bool CheckSection (FlatInfo sect, bool isRightOrTopLLu)
        {
            bool res = false;
            this.isRightOrTopLLu = isRightOrTopLLu;
            checkSection = sect;

            topFlats = insService.GetSideFlatsInSection(sect.Flats, true, section.SectionType);
            bottomFlats = insService.GetSideFlatsInSection(sect.Flats, false, section.SectionType);

            // Временно!!! подмена индекса угловой квартиры 2KL2
            if (section.SectionType == SectionType.CornerLeft || section.SectionType == SectionType.CornerRight)
            {
                var cornerFlat = section.SectionType == SectionType.CornerLeft ? bottomFlats.First() : bottomFlats.Last();
                if (cornerFlat.ShortType == "2KL2")
                {
                    cornerFlat.LightingNiz = cornerFlat.Type == "PIK1_2KL2_A0" ? "2|3,4" : "1,2|3";
                    cornerFlat.SelectedIndexBottom = 4;
                }
            }


            InvertInsSide(isRightOrTopLLu);
            

            // Проверка инсоляции квартир сверху
            isTop = true;
            curSideFlats = topFlats;
            res = CheckFlats();

            if (res) // прошла инсоляция верхних квартир
            {
                // Проверка инсоляции квартир снизу                
                isTop = false;
                curSideFlats = bottomFlats;
                res = CheckFlats();
            }
            return res;
        }

        

        /// <summary>
        /// Требования инсоляции удовлетворены
        /// Сумма остатка требуемых помещений равна 0
        /// </summary>
        /// <param name="requires">требования инсоляции</param>        
        protected bool RequirementsIsEmpty (List<InsRequired> requires)
        {
            var balance = requires.Sum(s => Math.Floor(s.CountLighting));
            var res = balance <= 0;
            return res;
        }

        /// <summary>
        /// Проверка правила инсоляции
        /// </summary>
        /// <param name="requires">требования инсоляции</param>
        /// <param name="light">индексы освещенности квартиры</param>
        /// <param name="ins">инсоляция по матрице</param>
        /// <param name="step">шаг в секции до этой квартиры</param>
        protected void CheckLighting (ref List<InsRequired> requires, List<int> light, string[] ins, int step)
        {
            if (light == null || ins == null || requires.Sum(r => r.CountLighting) <= 0) return;

            foreach (var item in light)
            {
                if (item.Equals(0)) break;

                double ligthWeight;
                int lightIndexInFlat = GetLightingValue(item, out ligthWeight);

                string insIndexProject = ins[step + lightIndexInFlat];

                CalcRequire(ref requires, ligthWeight, insIndexProject);
            }
        }

        /// <summary>
        /// Определение индекса окна инсоляции и веса инсоляции
        /// Определение веса инсоляции окна 1 - одно инсолируемое окно в помещении
        /// 0,5 - когда два окна в одном помещении
        /// </summary>
        /// <param name="item">Индекс инсоляционного окна в квартире - может быть с минусом, когда 2 окна в одном помещении</param>
        /// <param name="ligthWeight">Вес инсоляционнного окна - 1, или 0,5 если два окна в одном помещении</param>
        /// <returns>Индекс инсоляционного окна (без знака минус)</returns>
        protected static int GetLightingValue (int item, out double ligthWeight)
        {
            ligthWeight = 1;
            int lightIndexInFlat;
            if (item > 0)
            {
                lightIndexInFlat = item - 1;
            }
            else
            {
                // несколько окон в одном помещении в квартире (для инсоляции считается только одно окно в одном помещении)
                lightIndexInFlat = -item - 1;
                ligthWeight = 0.5;
            }
            return lightIndexInFlat;
        }

        protected static void CalcRequire (ref List<InsRequired> requires, double countLigth, string insIndexProject)
        {
            if (!string.IsNullOrWhiteSpace(insIndexProject))
            {
                for (int i = 0; i < requires.Count; i++)
                {
                    var require = requires[i];
                    if (require.CountLighting > 0 && require.IsPassed(insIndexProject))
                    {
                        require.CountLighting -= countLigth;
                        requires[i] = require;
                    }
                }
            }
        }

        /// <summary>
        /// Это первая торцевая квартира, на текущей стророне
        /// </summary>        
        protected bool IsEndFirstFlatInSide ()
        {
            bool res = false;
            if (isTop)
            {
                if (curFlatIndex == 0)
                {
                    res = true;
                }
            }
            else
            {
                if (curFlatIndex == 0 && topFlats.Last().SelectedIndexBottom == 0)
                {
                    res = true;
                }
            }
            return res;
        }

        /// <summary>
        /// Это последняя торцевая квартра на стороне
        /// </summary>
        /// <returns></returns>
        protected bool IsEndLastFlatInSide ()
        {
            bool res = false;
            if (isTop)
            {
                if (curFlatIndex == curSideFlats.Count - 1)
                {
                    res = true;
                }
            }
            else
            {
                if (curFlatIndex == (curSideFlats.Count - 1) && topFlats.First().SelectedIndexBottom == 0)
                {
                    res = true;
                }
            }
            return res;
        }

        /// <summary>
        /// Проверка инсоляции боковин
        /// </summary>        
        protected void CheckLightingSide (ref List<InsRequired> requires, Lighting lightingCurSide, Lighting lightingOtherSide,
            bool isFirstFlatInSide, bool isLastFlatInSide)
        {
            // Если это не боковая квартра по типу (не заданы боковые индексы инсоляции), то у такой квартиры не нужно проверять боковую инсоляцию
            bool flatHasSide = lightingCurSide.Side != Side.None; //flatLightIndexSideCurSide.Count != 0 || flatLightIndexSideOtherSide.Count != 0;
            if (!flatHasSide)
            {
                return;
            }

            // Квартира боковая по типу (заданы боковые индексы инсоляции)

            // Если это не крайняя квартира на стороне, то такую секцию нельзя пропускать дальше            
            if (!isFirstFlatInSide && !isLastFlatInSide)
            {
                specialFail = true;
                return;
            }
            bool isStoppor = IsStoppor(lightingCurSide, lightingOtherSide);

            var endFlat = lightingCurSide.Side;
            var endSideSection = GetSectionEndSide();
            if (endFlat != endSideSection && isStoppor)
            {
                specialFail = true;
                return;
            }

            // Если требования инсоляции уже удовлетворены, то не нужно проверять дальше
            if (RequirementsIsEmpty(requires))
            {
                return;
            }

            int flatLightingSide = 0;
            int flatLightingSideOther = 0;
            string insSideValue = null;
            string insSideOtherValue = null;

            if (endFlat == Side.Right)
            {
                // Правый торец
                if (isTop)
                {
                    // Праввая верхняя ячейка инсоляции
                    insSideValue = insSideRightTop;
                    flatLightingSide = lightingCurSide.SideIndexes[0];
                    // для верхних квартир проверить нижнюю ячейку инсоляции
                    insSideOtherValue = insSideRightBot;
                    if (lightingOtherSide != null)
                        flatLightingSideOther = lightingOtherSide.SideIndexes[0];
                }
                else
                {
                    // Праввая нижняя ячейка инсоляции
                    insSideValue = insSideRightBot;
                    flatLightingSide = lightingCurSide.SideIndexes[0];
                }
            }
            else if (endFlat == Side.Left)
            {
                // Левый торец
                if (isTop)
                {
                    // Левая верхняя ячейка инсоляции
                    insSideValue = insSideLeftTop;
                    flatLightingSide = lightingCurSide.SideIndexes[0];
                    // для верхних квартир проверить нижнюю ячейку инсоляции
                    insSideOtherValue = insSideLeftBot;
                    if (lightingOtherSide != null)
                        flatLightingSideOther = lightingOtherSide.SideIndexes[0];
                }
                else
                {
                    // Левая нижняя ячейка инсоляции
                    insSideValue = insSideLeftBot;
                    flatLightingSide = lightingCurSide.SideIndexes[0];
                }
            }

            double lightWeight;
            int indexLighting = GetLightingValue(flatLightingSide, out lightWeight);
            CalcRequire(ref requires, lightWeight, insSideValue);

            indexLighting = GetLightingValue(flatLightingSideOther, out lightWeight);
            CalcRequire(ref requires, lightWeight, insSideOtherValue);
        }

        private bool IsStoppor (Lighting lightingCurSide, Lighting lightingOtherSide)
        {
            // Если боковое окно единственное в помещени, то такую квартиру нельзя ставить в глухой торец (без окна с торца на улицу)
            // Если сторона квартиры не соответствует стороне торца, такую секцию нельзя пропускать дальше 
            // Только если индекс боковины не половинчатый - если не половинчатый, то боковое окно - будет заткнуто торцом и в комнате не останется окон
            var flatLightIndexSideCurSide = lightingCurSide.SideIndexes;
            var flatLightIndexSideOtherSide = lightingOtherSide == null ? null : lightingOtherSide.SideIndexes;
            var res = (flatLightIndexSideCurSide != null && flatLightIndexSideCurSide.Count != 0 && flatLightIndexSideCurSide[0] == 1);
            if (res) return res;
            res = (flatLightIndexSideOtherSide != null && flatLightIndexSideOtherSide.Count != 0 && flatLightIndexSideOtherSide[0] == 1);
            return res;
        }

        private Side GetSectionEndSide ()
        {
            Side res = Side.None;
            // Определение стороны торца секции
            if (section.IsStartSectionInHouse)
            {
                if (isRightOrTopLLu)
                    res = section.Direction > 0 ? Side.Left : Side.Right;
                else
                    res = section.Direction > 0 ? Side.Right : Side.Left;
            }
            else if (section.IsEndSectionInHouse)
            {
                if (isRightOrTopLLu)
                    res = section.Direction > 0 ? Side.Right : Side.Left;
                else
                    res = section.Direction > 0 ? Side.Left : Side.Right;
            }
            return res;
        }

        private void DefineInsSideCells ()
        {
            if (section.IsCorner)
            {
                if (section.InsSideStart != null)
                {
                    if (section.Direction > 0)
                    {
                        if (section.SectionType == SectionType.CornerLeft)
                        {                            
                            insSideRightBotStandart = section.InsSideStart[0].InsValue;
                            insSideRightTopStandart = section.InsSideStart[1].InsValue;
                        }
                        else
                        {
                            insSideLeftTopStandart = section.InsSideStart[0].InsValue;
                            insSideLeftBotStandart = section.InsSideStart[1].InsValue;
                        }                        
                    }
                    else
                    {
                        if (section.SectionType == SectionType.CornerLeft)
                        {
                            insSideRightTopStandart = section.InsSideStart[0].InsValue;
                            insSideRightBotStandart = section.InsSideStart[1].InsValue;                            
                        }
                        else
                        {
                            insSideLeftTopStandart = section.InsSideStart[0].InsValue;
                            insSideLeftBotStandart = section.InsSideStart[1].InsValue;
                        }
                    }
                }
                if (section.InsSideEnd != null)
                {
                    if (section.Direction > 0)
                    {
                        if (section.SectionType == SectionType.CornerLeft)
                        {
                            insSideRightTopStandart = section.InsSideStart[0].InsValue;
                            insSideRightBotStandart = section.InsSideStart[1].InsValue;                            
                        }
                        else
                        {
                            insSideLeftTopStandart = section.InsSideStart[0].InsValue;
                            insSideLeftBotStandart = section.InsSideStart[1].InsValue;
                        }
                    }
                    else
                    {
                        if (section.SectionType == SectionType.CornerLeft)
                        {
                            insSideRightTopStandart = section.InsSideStart[0].InsValue;
                            insSideRightBotStandart = section.InsSideStart[1].InsValue;
                        }
                        else
                        {
                            insSideLeftBotStandart = section.InsSideStart[0].InsValue;
                            insSideLeftTopStandart = section.InsSideStart[1].InsValue;                            
                        }
                    }
                }
            }
            else
            {
                if (section.InsSideStart != null)
                {
                    if (section.Direction > 0)
                    {
                        insSideLeftTopStandart = section.InsSideStart[0].InsValue;
                        insSideLeftBotStandart = section.InsSideStart[1].InsValue;
                    }
                    else
                    {
                        insSideRightBotStandart = section.InsSideStart[0].InsValue;
                        insSideRightTopStandart = section.InsSideStart[1].InsValue;
                    }
                }
                if (section.InsSideEnd != null)
                {
                    if (section.Direction > 0)
                    {
                        insSideRightTopStandart = section.InsSideStart[0].InsValue;
                        insSideRightBotStandart = section.InsSideStart[1].InsValue;
                    }
                    else
                    {
                        insSideLeftTopStandart = section.InsSideStart[0].InsValue;
                        insSideLeftBotStandart = section.InsSideStart[1].InsValue;
                    }
                }
            }            
        }

        private void InvertInsSide (bool isRightOrTopLLu)
        {
            if (isRightOrTopLLu)
            {
                insSideLeftTop = insSideLeftTopStandart;
                insSideLeftBot = insSideLeftBotStandart;
                insSideRightTop = insSideRightTopStandart;
                insSideRightBot = insSideRightBotStandart;
            }
            else
            {
                insSideLeftTop = insSideRightBotStandart;
                insSideLeftBot = insSideRightTopStandart;
                insSideRightTop = insSideLeftBotStandart;
                insSideRightBot = insSideLeftTopStandart;
            }
        }
    }
}
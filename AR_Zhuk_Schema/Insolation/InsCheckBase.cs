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

        public InsCheckBase (IInsolation insService, Section section)
        {
            this.insService = insService;
            this.section = section;
            this.isVertical = section.IsVertical;

            DefineInsSideCells();
        }

        public abstract List<FlatInfo> CheckSections (Section section);                               

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
        /// Проверка - затыкается ли квартира торцом (боковое окна выходит на торец другой секции)
        /// </summary>        
        protected bool CheckFlatSideStopper (bool isFirstFlatInSide, bool isLastFlatInSide, LightingRoom roomLighting)
        {
            // Если это не боковая квартра по типу (не заданы боковые индексы инсоляции), то у такой квартиры не нужно проверять боковую инсоляцию
            bool flatHasSide = (roomLighting.Side != Side.None);
            if (!flatHasSide)
            {
                return true;
            }

            // Квартира боковая по типу (заданы боковые индексы инсоляции)

            // Если это не крайняя квартира на стороне, то такую секцию нельзя пропускать дальше            
            if (!isFirstFlatInSide && !isLastFlatInSide)
            {                
                return false;
            }

            bool isStoppor = IsStoppor(roomLighting.SideIndexTop, roomLighting.SideIndexBot);

            var endFlat = roomLighting.Side;
            var endSideSection = GetSectionEndSide();
            if (endFlat != endSideSection && isStoppor)
            {
                return false;
            }

            return true;
        }

        private bool IsStoppor (LightingWindow lightingSideTop, LightingWindow lightingSideBot)
        {
            // Если боковое окно единственное в помещени, то такую квартиру нельзя ставить в глухой торец (без окна с торца на улицу)
            // Если сторона квартиры не соответствует стороне торца, такую секцию нельзя пропускать дальше 
            // Только если индекс боковины не половинчатый - если не половинчатый, то боковое окно - будет заткнуто торцом и в комнате не останется окон
            if ((lightingSideTop != null && lightingSideTop.RoomNumber == 0) ||
                (lightingSideBot != null && lightingSideBot.RoomNumber == 0))
            {
                // Одно из боковых окон единственное в помещении - оно затыкается торцом
                return true;
            }
            return false;
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
                            insSideRightTopStandart = section.InsSideEnd[0].InsValue;
                            insSideRightBotStandart = section.InsSideEnd[1].InsValue;
                        }
                        else
                        {
                            insSideLeftTopStandart = section.InsSideEnd[0].InsValue;
                            insSideLeftBotStandart = section.InsSideEnd[1].InsValue;
                        }
                    }
                    else
                    {
                        if (section.SectionType == SectionType.CornerLeft)
                        {
                            insSideRightTopStandart = section.InsSideEnd[0].InsValue;
                            insSideRightBotStandart = section.InsSideEnd[1].InsValue;
                        }
                        else
                        {
                            insSideLeftBotStandart = section.InsSideEnd[0].InsValue;
                            insSideLeftTopStandart = section.InsSideEnd[1].InsValue;
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
                        insSideRightTopStandart = section.InsSideEnd[0].InsValue;
                        insSideRightBotStandart = section.InsSideEnd[1].InsValue;
                    }
                    else
                    {
                        insSideLeftTopStandart = section.InsSideEnd[1].InsValue;
                        insSideLeftBotStandart = section.InsSideEnd[0].InsValue;
                    }
                }
            }
            insSideLeftTop = insSideLeftTopStandart;
            insSideLeftBot = insSideLeftBotStandart;
            insSideRightTop = insSideRightTopStandart;
            insSideRightBot = insSideRightBotStandart;
        }
        protected void InvertInsSide (bool isRightOrTopLLu)
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
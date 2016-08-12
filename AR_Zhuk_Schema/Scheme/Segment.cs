using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;
using AR_Zhuk_Schema.Insolation;

namespace AR_Zhuk_Schema.Scheme
{
    /// <summary>
    /// Линейный сегмент дома
    /// Direction - направление сегмента - от старта к концу
    /// Определение сторон - взгляд от стартового торца в направлении сегмента - определяет левую и правую сторону сегмента.
    /// </summary>
    public class Segment
    {
        private ISchemeParser parser;
        public readonly int StartLevel;
        public readonly int EndLevel;

        public readonly Cell CellStartLeft;
        public readonly Cell CellStartRight;
        public readonly Cell CellEndLeft;
        public readonly Cell CellEndRight;
        public readonly Cell Direction;
        /// <summary>
        /// Перпендикулярное направление от Левой к Правой стороне
        /// </summary>
        public readonly Cell DirectionLeftToRight;
        public readonly List<Module> ModulesLeft;
        public readonly List<Module> ModulesRight;
        /// <summary>
        /// Боковая инсоляция стартового торца дома - слева-напрво (от главного направления)
        /// </summary>
        public readonly List<Module> ModulesSideStart;
        /// <summary>
        /// Боковая инсоляция в конце дома - слева-напрво (от главного направления)
        /// </summary>
        public readonly List<Module> ModulesSideEnd;
        public readonly SegmentEnd StartType;
        public readonly SegmentEnd EndType;
        public readonly bool IsVertical;
        

        public HouseSpot HouseSpot { get; private set; }

        /// <summary>
        /// Номер сегмента в доме
        /// </summary>
        public int Number { get; private set; }
        /// <summary>
        /// Кол шагов в сегменте.
        /// Для углового сегмента в начале секции вычтены 3 лишних шага - боковых
        /// </summary>
        public int CountSteps { get; private set; }

        internal Segment (Cell cellStartLeft, Cell cellStartRight, Cell direction, ISchemeParser parser, HouseSpot houseSpot)
        {
            HouseSpot = houseSpot;
            Number = houseSpot.Segments.Count + 1;

            CellStartLeft = cellStartLeft;
            CellStartRight = cellStartRight;
            Direction = direction;
            DirectionLeftToRight = direction.ToRight();
            this.parser = parser;            
            
            ModulesLeft = parser.GetSteps(cellStartLeft, direction, out CellEndLeft);
            ModulesRight = parser.GetSteps(cellStartRight, direction, out CellEndRight);

            // Кол шагов в секции
            CountSteps = ModulesLeft.Count > ModulesRight.Count ? ModulesLeft.Count : ModulesRight.Count;                        

            IsVertical = defineVertical();

            StartLevel = GetMaxLevel(cellStartLeft, cellStartRight, direction);
            EndLevel = GetMaxLevel(CellEndLeft, CellEndRight, direction.Negative);

            // Определение вида торцов сегмента
            StartType = defineEndType(CellStartLeft, CellStartRight, direction.Negative, true);
            EndType = defineEndType(CellEndLeft, CellEndRight, direction, false);

            // Если старт секции угловой - отнимаем три лишних шага            
            if (StartType == SegmentEnd.CornerLeft || StartType == SegmentEnd.CornerRight)
            {                
                CountSteps -= (HouseSpot.WidthOrdinary - 1);                
            }

            // боковая инсоляция
            ModulesSideStart = DefineSideModules(StartType, true);
            ModulesSideEnd = DefineSideModules(EndType, false);            
        }

        internal List<Module> GetModules (List<Module> sourceModules, int startStep, int countSteps)
        {
            List<Module> resModules;
            if ((StartType == SegmentEnd.CornerLeft || StartType == SegmentEnd.CornerRight) &&
                sourceModules.Count > CountSteps)
            {
                // три лишних модуля в начале
                startStep += HouseSpot.WidthOrdinary - 1;                
            }
            resModules = sourceModules.Skip(startStep - 1).Take(countSteps).ToList();
            return resModules;
        }

        /// <summary>
        /// Опреление стартовой ячейки секции - левая верхняя
        /// </summary>
        /// <param name="from">стартовый шаг секции</param>  
        /// <param name="step">Отступ шагов в основном направлении от заданной ячейки</param>
        /// <param name="isLeft">Левая сторона сегмента или правая</param>
        internal Cell GetSectionStartCell (Cell from, int step, bool isLeft)
        {
            if ((StartType == SegmentEnd.CornerLeft && !isLeft) ||
                StartType == SegmentEnd.CornerRight && isLeft)
            {
                step += HouseSpot.WidthOrdinary - 1;
            }
            var resCell = from.Offset(Direction * (step-1));
            return resCell;
        }

        internal Side DefineLluPriority (Cell size)
        {
            Side resPriorLluSide = Side.None;
            // Если это угловой дом (сегмент угловой), то приоритетная сторона внутренняя (т.е. где меньше ячеек инсоляции)
            if (StartType == SegmentEnd.CornerLeft || StartType == SegmentEnd.CornerRight ||
                EndType == SegmentEnd.CornerLeft || EndType == SegmentEnd.CornerRight)
            {
                resPriorLluSide = ModulesLeft.Count < ModulesRight.Count ? Side.Left : Side.Right;
            }
            else
            {
                // Для дома из одного прямого сегмента, приоритетная сторона, та котрая ближе к центру застройки
                var center = new Cell(size.Row / 2, size.Col / 2);
                var countLeft = Math.Abs(center.Row - CellStartLeft.Row) + Math.Abs(center.Col - CellStartLeft.Col);
                var countRight = Math.Abs(center.Row - CellStartRight.Row) + Math.Abs(center.Col - CellStartRight.Col);
                resPriorLluSide = countLeft < countRight ? Side.Left : Side.Right;
            }
            return resPriorLluSide;
        }

        /// <summary>
        /// Проверка попадает ли шаг в мертвую зону сегмента (угол)
        /// </summary>        
        internal bool StepInDeadZone (int startStep, int endStep)
        {
            int endToEnd = CountSteps - endStep;
            if ((EndType == SegmentEnd.CornerLeft || EndType == SegmentEnd.CornerRight) &&
                endToEnd < HouseSpot.CornerSectionMinStep)
            {
                int startToEnd = CountSteps - startStep;
                // Если стартовый шаг попадает в угловой конец сегмента 
                bool isStartKink = startToEnd == HouseSpot.WidthOrdinary;// стартовый загиб угловой секции
                if (startToEnd < HouseSpot.CornerSectionMinStep - 2 && !isStartKink)
                {
                    return true;
                }            

                // Конечный шаг                            
                if (endToEnd > 0)
                {
                    // Это не угловая секция, она должна встать до угла
                    if (endToEnd < HouseSpot.CornerSectionMinStep - 1 && endToEnd != HouseSpot.WidthOrdinary + 1)
                    {
                        return true;
                    }
                }
                else
                {
                    // Это угловая секция - она должа встать на допустимый шаг на след сегменте
                    int minStepInNextSeg = HouseSpot.CornerSectionMinStep - HouseSpot.WidthOrdinary - 1;// =3, -1 шаг зашиба
                    endToEnd = Math.Abs(endToEnd);

                    if (isStartKink)
                    {
                        // Загиб вначале - хвост на след сегменте
                        if (endToEnd < minStepInNextSeg)
                        {
                            return true;
                        }
                    }
                    else
                    {                        
                        if (endToEnd != 1)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Определение вертикальности сегмента
        /// </summary>
        /// <returns></returns>
        private bool defineVertical ()
        {
            bool isVertical = Direction.Row != 0 ? true : false;
            return isVertical;
        }

        /// <summary>
        /// Определение торцов сегмента - начало или конец дома, нормальный или угловой торец
        /// </summary>
        /// <param name="directionOut">Направление от торца во вне сегмента</param>
        private SegmentEnd defineEndType (Cell cellEndLeft, Cell cellEndRight, Cell directionOut, bool isStartEnd)
        {
            SegmentEnd endType;
            // Если ячейки на одном уровне, то это не угловой торец
            if (isOnSomeStep(cellEndLeft, cellEndRight))
            {
                endType = parser.IsInsCell(cellEndLeft.Offset(directionOut)) ? SegmentEnd.Normal : SegmentEnd.End;                
            }
            else
            {
                // угловой торец
                endType = GetCornerEnd(cellEndLeft, cellEndRight, isStartEnd);
            }
            return endType;           
        }

        

        /// <summary>
        /// Определение - на одном ли шаге сегмента находятся ячейки
        /// </summary>        
        private bool isOnSomeStep (Cell cell1, Cell cell2)
        {
            bool res = IsVertical ? cell1.Row == cell2.Row : cell1.Col == cell2.Col;            
            return res;
        }

        /// <summary>
        /// Определение типа углового торца
        /// </summary>
        /// <param name="cellLeft">Крайняя левая точка сегмента (по направлению)</param>
        /// <param name="cellRight">Крайняя правая точка сегмента</param>
        /// <param name="isStartEnd">Это стартовый торец</param>
        /// <returns></returns>
        private SegmentEnd GetCornerEnd (Cell cellLeft, Cell cellRight, bool isStartEnd)
        {
            SegmentEnd resEndCornerType;
            //int dir = IsVertical ? Direction.Row : Direction.Col;
            int levelLeft = GetCellLevel(cellLeft);
            //int levelRight = GetCellLevel(cellEndRight) * dir;
            if (isStartEnd)
            {
                resEndCornerType = levelLeft == StartLevel ? SegmentEnd.CornerRight : SegmentEnd.CornerLeft;                
            }
            else
            {
                resEndCornerType = levelLeft == EndLevel ? SegmentEnd.CornerRight : SegmentEnd.CornerLeft;
            }
            return resEndCornerType;
        }

        private List<Module> DefineSideModules (SegmentEnd end, bool isStart)
        {
            List<Module> res = null;
            if (end == SegmentEnd.End)
            {
                var dir = DirectionLeftToRight;// Direction.ToRight();
                if (isStart)
                {
                    Cell lastCell;
                    res = parser.GetSteps(CellStartLeft, dir, out lastCell);
                }
                else
                {
                    Cell lastCell;
                    res = parser.GetSteps(CellEndLeft, dir, out lastCell);
                }
                if (res != null && res.Count == 4)
                {
                    res.RemoveAt(0);
                    res.RemoveAt(res.Count - 1);
                }
            }
            return res;
        }

        /// <summary>
        /// Максимальный уровень по основному направлению
        /// </summary>
        /// <param name="cell1"></param>
        /// <param name="cell2"></param>
        /// <returns></returns>
        private int GetMaxLevel (Cell cell1, Cell cell2, Cell directionInner)
        {
            int level = 0;
            int dir = IsVertical ? directionInner.Row : directionInner.Col;
            int l1 = GetCellLevel(cell1);
            int l1d = l1 * dir;
            int l2 = GetCellLevel(cell2);
            int l2d = l2 * dir;
            level = l1d < l2d ? l1 : l2;            
            return level;
        }

        private int GetCellLevel (Cell cell)
        {
            var res = IsVertical ? cell.Row : cell.Col;
            return res;
        }        
    }
}
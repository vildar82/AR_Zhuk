﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;
using AR_Zhuk_Schema.Scheme.SpatialIndex;

namespace AR_Zhuk_Schema.Scheme
{
    /// <summary>
    /// Пятно одного дома
    /// Состоит из прямых участков (сегментов)
    /// </summary>
    public class HouseSpot
    {
        /// <summary>
        /// Ширина обычной секции в шагах
        /// </summary>
        public const int WidthOrdinary = 4;
        /// <summary>
        /// Минимальный шаг угловой секции
        /// </summary>
        public const int CornerSectionMinStep = 8;        

        private RTree<Segment> tree = new RTree<Segment>();
        private readonly Cell cellStart;
        private readonly ISchemeParser parser;

        public string SpotName { get; private set; }
        public bool IsTower { get; private set; }
        public List<Segment> Segments = new List<Segment>();
        public HouseOptions HouseOptions { get; set; }
        /// <summary>
        /// Кол-во шагов в доме
        /// </summary>
        public int CountSteps { get; private set; }

        public HouseSpot (string spotName, Cell cellStart, ISchemeParser parser)
        {
            SpotName = spotName;
            this.cellStart = cellStart;
            this.parser = parser;
            DefineSpot();
        }

        public void DefineSpot ()
        {
            // Определение начального сегмента дома
            DefineStartSegment();
            DefineOtherSegments();
        }

        public Segment GetSegmentAtStep (int step, out int stepsInSegment)
        {
            stepsInSegment = step;
            Segment res = null;
            foreach (var segment in Segments)
            {
                if (stepsInSegment <= segment.CountSteps)
                {
                    res = segment;
                    break;
                }
                else
                {
                    stepsInSegment -= segment.CountSteps;
                }
            }
            return res;
        }
                
        public Section GetSection (int startStepInHouse, int sectionCountStep)
        {
            Section section = null; 
            int startStepInSeg;
            var segment = GetSegmentAtStep(startStepInHouse, out startStepInSeg);
            int endStepInSeg = startStepInSeg + sectionCountStep-1;
            
            // Еслм начальный шаг или конечный секции попали в мертвую зону (угол), то такой дом нельзя скомпановать
            if (segment.StepInDeadZone(startStepInSeg, endStepInSeg))
            {
                return null;
            }

            section = new Section();
            section.CountModules = sectionCountStep * WidthOrdinary;
            section.CountStep = sectionCountStep;

            // Определение типа секции - угловая или рядовая
            if (endStepInSeg > segment.CountSteps)
            {
                // Угловая                
                section.IsCorner = true;

                if (segment.EndType != SegmentEnd.CornerLeft && segment.EndType != SegmentEnd.CornerRight)
                {
                    // какая-то ошибка.
                    throw new InvalidOperationException("Ожидался угловой сегмент. Непредвиденная ошибка.");
                }                

                // Проверка минимальности шага угловой секции
                if (section.CountStep < HouseSpot.CornerSectionMinStep)
                {
                    return null;
                }

                //if (segment.EndType == SegmentEnd.CornerLeft)
                //    section.SectionType = SectionType.CornerLeft;
                //else
                //    section.SectionType = SectionType.CornerRight;                

                var nextSegment = Segments[segment.Number];

                int countStepInThisSeg = segment.CountSteps - startStepInSeg;
                if (countStepInThisSeg > WidthOrdinary + 1)
                {
                    // Хвост угловой секции на этом сегменте

                    // тип угловой секции
                    section.SectionType = segment.EndType == SegmentEnd.CornerRight ? SectionType.CornerLeft : SectionType.CornerRight;

                    section.IsVertical = segment.IsVertical;
                    section.Direction = segment.IsVertical ? segment.Direction.Row : segment.Direction.Col;

                    // Верхняя инслоляция
                    section.InsTop = segment.GetModules(segment.ModulesLeft, startStepInSeg, segment.CountSteps);
                    section.InsTop.Add(nextSegment.ModulesLeft.First());
                    // Нижняя инсоляция
                    section.InsBot = segment.GetModules(segment.ModulesRight, startStepInSeg, segment.CountSteps);
                    int modulesInNextSeg = 1 + (WidthOrdinary - 1); // 1 шаг загиба + 3 боковые ячейки
                    section.InsBot.AddRange(nextSegment.ModulesRight.Take(modulesInNextSeg));
                    section.InsBot.Reverse();                    
                }
                else
                {
                    // Хвост угловой секции на след сегменте

                    // тип угловой секции
                    section.SectionType = segment.EndType == SegmentEnd.CornerRight ? SectionType.CornerRight : SectionType.CornerLeft;

                    section.IsVertical = nextSegment.IsVertical;
                    section.Direction = nextSegment.IsVertical ? nextSegment.Direction.Row : nextSegment.Direction.Col;

                    // Верхняя инсоляция
                    section.InsTop = new List<Module> { segment.ModulesLeft.Last() };
                    section.InsTop.AddRange(nextSegment.ModulesLeft.Take(section.CountStep - 1));
                    section.InsTop.Reverse();
                    // Нижняя инсоляция
                    int skipModules = segment.ModulesLeft.Count - (WidthOrdinary + 1); // кол модулей до последних 5
                    section.InsBot = segment.ModulesRight.Skip(skipModules).ToList();
                    section.InsBot.AddRange(nextSegment.ModulesRight.Take(section.CountStep - 2)); // -1 шаг загиба, -1 - первый шаг на текущем сегменте (последний в сегменте)
                }

                section.IsLeftBottomCorner = section.SectionType == SectionType.CornerLeft;
                section.IsRightBottomCorner = section.SectionType == SectionType.CornerRight;                
            }
            else
            {
                // Рядовая                
                section.SectionType = SectionType.Ordinary;
                section.IsVertical = segment.IsVertical;
                section.Direction = segment.IsVertical ? segment.Direction.Row : segment.Direction.Col;

                // Инсоляция левая
                var insLeft = segment.GetModules(segment.ModulesLeft, startStepInSeg, section.CountStep);
                // правая
                var insRight = segment.GetModules(segment.ModulesRight, startStepInSeg, section.CountStep);
                // определение инсоляции по верху и по низу секции для правого/верхнего расположения ЛЛУ
                if (section.Direction > 0)
                {                    
                    section.InsTop = insLeft;
                    section.InsTop.Reverse();
                    section.InsBot = insRight;
                }
                else
                {                    
                    section.InsTop = insRight;
                    section.InsBot = insLeft;
                    section.InsBot.Reverse();
                }
            }

            // Боковая инсоляция в торце
            if (segment.StartType == SegmentEnd.End && startStepInHouse==1 ||
                segment.EndType == SegmentEnd.End && section.CountStep == endStepInSeg)
            {                
                section.InsSide = segment.ModulesSide;
            }            

            return section;
        }        

        protected void AddSegment (Segment segment)
        {
            Segments.Add(segment);
            // добавление прямоугольника сегмента в дерево, для проверки попадания любой ячейки в этот дом
            Rectangle r = GetRectangle(segment);
            tree.Add(r, segment);

            CountSteps += segment.CountSteps;
        }

        /// <summary>
        /// Проверка входит ли ячейка в этот дом
        /// </summary>      
        public bool HasCell (Cell cell)
        {
            bool res = false;
            // 1 ячейка отступа от границы дома - т.к. она не может использоваться другим домом
            Rectangle r = new Rectangle(cell.Col - 1, cell.Row - 1, cell.Col + 1, cell.Row + 1, 0, 0);
            var segments = tree.Intersects(r);
            if (segments != null && segments.Count > 0)
            {
                res = true;
            }
            return res;
        }        

        /// <summary>
        /// определение стартового сегмента
        /// </summary>
        private void DefineStartSegment ()
        {
            // Наименьшая длина дома от стартовой точки - определяет начало стартового сегмента.
            // Вправо от стартовой точки 
            Cell lastCellLeft;            
            var modulesLeft = parser.GetSteps(cellStart, Cell.Right, out lastCellLeft);
            if (modulesLeft.Count == WidthOrdinary)
            {
                // Сегмент вертикальный сверху-вниз
                var startSegment = new Segment(lastCellLeft, cellStart, Cell.Down, parser, this);
                AddSegment(startSegment);
                return;
            }

            // Вниз от стартовой точки 
            Cell lastCellDown;
            var modulesDown = parser.GetSteps(cellStart, Cell.Down, out lastCellDown);
            if (modulesDown.Count == WidthOrdinary)
            {
                // Сегмент горизонтальный слева-направо
                var startSegment = new Segment(cellStart, lastCellDown, Cell.Right, parser, this);
                AddSegment(startSegment);
                return;
            }

            // от нижней точки вправо
            Cell lastCell;
            var modules = parser.GetSteps(lastCellDown, Cell.Right, out lastCell);
            if (modules.Count == WidthOrdinary)
            {
                // Сегмент вертикальный снизу-вверх
                var startSegment = new Segment(lastCellDown, lastCell, Cell.Up, parser, this);
                AddSegment(startSegment);
                return;
            }

            // Последний заворот, от последней точки вверх
            Cell lastCellLast;
            modules = parser.GetSteps(lastCell, Cell.Up, out lastCellLast);
            if (modules.Count == WidthOrdinary)
            {
                // Сегмент горизонтальный слева-направо
                var startSegment = new Segment(lastCell, lastCellLast, Cell.Left, parser, this);
                AddSegment(startSegment);
                return;
            }

            // Значит это башня    
            if (modulesLeft.Count < modulesDown.Count)
            {
                var segment = new Segment(lastCellLeft, cellStart, Cell.Down, parser, this);
                AddSegment(segment);
                return;
            }
            else
            {
                var segment = new Segment(cellStart, lastCellDown, Cell.Left, parser, this);
                AddSegment(segment);
                return;
            }            

            // Сюда не должен никогда попасть
            throw new InvalidOperationException("Не определено начало дома");
        }

        /// <summary>
        /// Определение остальных сегментов дома
        /// </summary>
        private void DefineOtherSegments ()
        {
            // Последний сегмент в доме
            var lastSegment = Segments.Last();

            Segment newSegment = null;
            if (lastSegment.EndType == SegmentEnd.Normal)
            {
                newSegment = new Segment(lastSegment.CellEndLeft.Offset(lastSegment.Direction),
                    lastSegment.CellEndRight.Offset(lastSegment.Direction), lastSegment.Direction, parser, this);

            }
            else if (lastSegment.EndType == SegmentEnd.CornerLeft)
            {
                Cell newSegmentDir = lastSegment.Direction.ToLeft();

                newSegment = new Segment(lastSegment.CellEndLeft.Offset(lastSegment.Direction).Offset(newSegmentDir),
                    lastSegment.CellEndRight.Offset(newSegmentDir), 
                    newSegmentDir, parser, this);
            }
            else if (lastSegment.EndType == SegmentEnd.CornerRight)
            {
                Cell newSegmentDir = lastSegment.Direction.ToRight();

                newSegment = new Segment(lastSegment.CellEndLeft.Offset(newSegmentDir),
                    lastSegment.CellEndRight.Offset(lastSegment.Direction).Offset(newSegmentDir),
                    newSegmentDir, parser, this);                
            }
            else
            {
                // Конец дома
                return;
            }
            AddSegment(newSegment);
            DefineOtherSegments();
        }

        private Rectangle GetRectangle (Segment segment)
        {
            Cell startRightMin;
            Cell endLeftMax;

            // Стартовый 
            if (segment.StartType == SegmentEnd.Normal || segment.StartType == SegmentEnd.End)
            {
                startRightMin = segment.CellStartRight;
            }
            else
            {
                // угловой торец у сегмента
                if (segment.IsVertical)
                {
                    startRightMin = segment.CellStartRight;
                    startRightMin.Row = segment.StartLevel;
                }
                else
                {
                    startRightMin = segment.CellStartRight;
                    startRightMin.Col = segment.StartLevel;
                }
            }

            // Конечный торец
            if (segment.EndType == SegmentEnd.Normal || segment.EndType == SegmentEnd.End)
            {
                endLeftMax = segment.CellEndLeft;
            }
            else
            {
                // угловой торец у сегмента
                if (segment.IsVertical)
                {
                    endLeftMax = segment.CellEndLeft;
                    endLeftMax.Row = segment.EndLevel;
                }
                else
                {
                    endLeftMax = segment.CellEndLeft;
                    endLeftMax.Col = segment.EndLevel;
                }
            }

            Rectangle r = new Rectangle(startRightMin.Col, startRightMin.Row, endLeftMax.Col, endLeftMax.Row, 0, 0);
            return r;
        }
    }
}

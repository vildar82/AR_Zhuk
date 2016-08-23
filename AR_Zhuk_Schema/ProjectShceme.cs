using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;
using AR_Zhuk_Schema.Insolation;
using AR_Zhuk_Schema.Scheme;
using AR_Zhuk_Schema.Scheme.Cutting;
using AR_Zhuk_Schema.Scheme.SpatialIndex;

namespace AR_Zhuk_Schema
{
    /// <summary>
    /// Объект - проектируемый объект застройки
    /// </summary>
    public class ProjectScheme
    {
        private List<HouseOptions> houseOptions;
        private SpotInfo sp;
        private RTree<Segment> tree = new RTree<Segment>();

        /// <summary>
        /// Схема пятен домов в объекте застройки
        /// </summary>
        public List<HouseSpot> HouseSpots { get; private set; }        

        public ProjectScheme (List<HouseOptions> houseOptions, SpotInfo sp)
        {
            this.houseOptions = houseOptions;
            this.sp = sp;
        }

        /// <summary>
        /// Чтенее файла схемы инсоляции и определение пятен домов
        /// </summary>
        /// <param name="schemeFile">Excel файл схемы объекта застройки и инсоляции</param>
        /// <exception cref="Exception">Недопустимое имя пятна дома.</exception>
        public void ReadScheme (string schemeFile)
        {
            // Чтение матрицы ячеек первого листа в Excel файле
            ISchemeParser parserExcel = new ParserExcel(this);
            parserExcel.Parse(schemeFile);
            HouseSpots = parserExcel.HouseSpots;

            // Размер застройки
            var bounds = tree.getBounds();
            sp.Size = new Cell(Convert.ToInt32(bounds.max[1]) + 1, Convert.ToInt32(bounds.max[0]) + 1);

            // Инсоляция - все ячейки            
            List<Module> insModulesAll = new List<Module>();

            foreach (var houseSpot in HouseSpots)
            {
                var houseOpt = houseOptions.Find(o => o.HouseName == houseSpot.SpotName);
                if (houseOpt == null)
                {
                    string allowedHouseNames = string.Join(",", houseOptions.Select(h => h.HouseName));
                    throw new Exception("Имя пятна дома определенное в файле инсоляции - '" + houseSpot.SpotName + "' не соответствует одному из допустимых значений: " + allowedHouseNames);
                }
                houseSpot.HouseOptions = houseOpt;

                // добавление ячеек инсоляции
                foreach (var segment in houseSpot.Segments)
                {
                    insModulesAll.AddRange(segment.ModulesLeft);
                    insModulesAll.AddRange(segment.ModulesRight);
                    if (segment.ModulesSideEnd != null)
                       insModulesAll.AddRange(segment.ModulesSideEnd);
                    if (segment.ModulesSideStart != null)
                        insModulesAll.AddRange(segment.ModulesSideStart);
                }
                // Определение приоритетной стороны для ЛЛУ в доме
                houseSpot.PriorityLluSide = houseSpot.Segments.First().DefineLluPriority(sp.Size);
            }

            // Инсоляция - все ячейки            
            sp.InsModulesAll = insModulesAll;                        
        }        

        /// <summary>
        /// Получение всех вариантов домов для всех пятен домов
        /// <param name="maxSectionBySize">Максимальное кол-во вариантов секций одного размера загружаемых из базы. 0 - все.</param>
        /// <param name="maxHousesBySpot">Максимаельное кол вариантов домов по размерностям секций в одном пятне дома</param>
        /// </summary>        
        public List<List<HouseInfo>> GetTotalHouses (int maxSectionBySize = 0, int maxHousesBySpot=0)
        {
            CuttingFactory.ResetData();
            List<List<HouseInfo>> totalHouses = new List<List<HouseInfo>>();
            foreach (var item in HouseSpots)
            {
                ICutting cutting = CuttingFactory.Create(item, sp, maxSectionBySize, maxHousesBySpot);
                var houses = cutting.Cut();
                if (houses.Count != 0)
                {
#if !TEST
                    // Отбор минимальной размерности дома   
                    if (houses.Count != 0)
                        houses = houses.GroupBy(h => h.SectionsBySize.Count).OrderBy(o => o.Key).FirstOrDefault().ToList();
#endif

                    totalHouses.Add(houses);
                }
            }
            return totalHouses;
        }

        internal void AddSegment (Segment segment)
        {
            // добавление прямоугольника сегмента в дерево, для проверки попадания любой ячейки в этот дом
            Rectangle r = GetRectangle(segment);            
            tree.Add(r, segment);
        }

        internal bool HasCell (Cell cell)
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AR_AreaZhuk;
using AR_AreaZhuk.Controller;
using AR_AreaZhuk.PIK1TableAdapters;
using AR_Zhuk_DataModel;
using AR_Zhuk_Schema.DB;
using AR_Zhuk_Scheme_ConsoleTest.Scheme;
using BeetlyVisualisation;
using OfficeOpenXml;

namespace AR_Zhuk_Scheme_ConsoleTest
{
    class Program
    {
        static string curDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        [STAThread]
        static void Main (string[] args)
        {
            //TestCountStepInSection();
            //TestMessage();            
            //DBService dbServ = new DBService();
            //dbServ.SaveDbFlats();
            //AnalizSectionsSteps();
            //BankSectionsStatistics();
            //BankSectionsStatisticsShortType();
            //StatisticsSectionsByFlatsCount();
            //StatisticCoefficientK1K2();            
            //GenerateImagesBankSection();
            //return;

            TextWriterTraceListener writer = new TextWriterTraceListener(Console.Out);
            Debug.Listeners.Add(writer);

            TestProjectScheme Test = new TestProjectScheme();
            Test.TestTotalHouses();

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void TestCountStepInSection ()
        {
            DBService dbServ = new DBService(null);
            dbServ.LoadDbFlatsFromFile();

            var failStepSects = DBService.dictDbFlats.SelectMany(s =>
            {
                var fs = s.Value.Select(w =>
                {
                    var countBot = w.Sum(f => f.SelectedIndexBottom);
                    var countTop = w.Sum(f => f.SelectedIndexTop);
                    if (s.Key.Type!="Рядовая")
                    {
                        countBot -= 3;
                        countTop += 4;
                    }
                    if (countBot != s.Key.Step || countTop != s.Key.Step)
                    {
                        return new { key = s.Key, sect = w };
                    }
                    else
                    {
                        return null;
                    }
                }).ToList();
                return fs;
            }).Where(w=>w!=null).ToList();
            var countFailSect = failStepSects.Count;
        }

        private static void TestMessage ()
        {
            AR_Zhuk_DataModel.Messages.Informer.AddMessage("sdfs sdfg sdfg dsfg sdfg sdfg sdfg dsfg fg sdfg dfsg sdfg dsfg dsfg fag sdfg dsafg dfsag dfsg sd dsgh ag afg sdfg dsfg sdfg sdfg");
            AR_Zhuk_DataModel.Messages.Informer.AddMessage("sdfs sdfg sdfg dsfg sdfg sdfg sdfg dsfg fg sdfg dfsg sdfg dsfg dsfg fag sdfg dsafg dfsag dfsg sd dsgh ag afg sdfg dsfg sdfg sdfg");
            AR_Zhuk_DataModel.Messages.Informer.AddMessage("sdfs sdfg sdfg dsfg sdfg sdfg sdfg dsfg fg sdfg dfsg sdfg dsfg dsfg fag sdfg dsafg dfsag dfsg sd dsgh ag afg sdfg dsfg sdfg sdfg");
            AR_Zhuk_DataModel.Messages.Informer.Show();
        }

    private static void GenerateImagesBankSection ()
        {
            Console.WriteLine("GenerateImagesBankSection");
            DBService dbServ = new DBService(null);
            dbServ.LoadDbFlatsFromFile();
            ImageCombiner imgComb = new ImageCombiner();
            int count = 0;
            foreach (var type in DBService.dictDbFlats)
            {
                foreach (var sect in type.Value)
                {
                    var fi = GetSectinByDbFlats(sect, type.Key);
                    string pathFlatsImg = Path.Combine(curDir, "00_PNG_ПИК1");
                    var img = imgComb.GenerateImageOneSection(fi, pathFlatsImg);
                    string file = Path.Combine(curDir, $@"Секции\{(count++).ToString("000000")}_id-{fi.IdSection}_code-{fi.Code}_{type.Key.ToString()}.png");
                    Console.WriteLine(file);
                    img.Save(file, ImageFormat.Png);                    
                }
            }
        }

        private static FlatInfo GetSectinByDbFlats (List<DbFlat> dbFlats, SelectSectionParam ssp)
        {
            var rooms = dbFlats.Select(s =>
            {
                var r = s.GetRoomInfo();
                r.CodeReqIndex = GetRoomCode(r);
                return r;
            }).ToList();
            var fi = new FlatInfo();
            fi.IdSection = dbFlats[0].ID_Section;
            fi.Flats = rooms;
            fi.Code = GetCodeSection(fi.Flats);
            if (ssp.Type != "Рядовая")
                fi.IsCorner = true;                        
            return fi;
        }

        private static string GetCodeSection (List<RoomInfo> rooms)
        {
            string code = "";
            var dictCodesReq = rooms.Where(f => f.SubZone != "0").
                GroupBy(g => g.CodeReqIndex).ToDictionary(t=>t.Key, v=>v.Count());
            for (int i = 0; i < 5; i++)
            {
                int count = 0;
                dictCodesReq.TryGetValue(i, out count);
                code += count;
            }
            return code;
        }

        private static int GetRoomCode (RoomInfo r)
        {            
            switch (r.SubZone)
            {
                case "01":
                    return 0;
                case "1":
                    return 1;
                case "2":
                    return 2;
                case "3":
                    return 3;
                case "4":
                    return 4;
                default:
                    return 0;
            }            
        }

        static void StatisticCoefficientK1K2 ()
        {
            C_Flats_PIK1_AreasTableAdapter pikFlats = new C_Flats_PIK1_AreasTableAdapter();
            var flatsAreas = new PIK1.C_Flats_PIK1_AreasDataTable();
            flatsAreas = pikFlats.GetData();

            DBService dbServ = new DBService(null);
            dbServ.LoadDbFlatsFromFile();
            //var sections = DBService.dictDbFlats.Values.SelectMany(s => s).ToList();

            // Шагов, этажность , секция, кол квартир, К1, К2
            var sectCoeffsK1K2 = new List<Tuple<SelectSectionParam,string, int, double, double, string>>(); 

            foreach (var item in DBService.dictDbFlats)
            {
                var sectBySize = item.Value;
                double levelArea=0;
                double levelAreaOffLLU=0;
                double levelAreaOnLLU=0;

                foreach (var sect in sectBySize)
                {
                    string sectString = string.Empty;
                    List<RoomInfo> rooms = new List<RoomInfo>();
                    foreach (var flat in sect)
                    {
                        var ri = flat.GetRoomInfo();
                        ri.CodeReqIndex = GetRoomCode(ri);
                        rooms.Add(ri);
                        var currentFlatAreas = flatsAreas.First(x => x.Short_Type.Equals(flat.ShortType));
                        var areas = Calculate.GetAreaFlat(15, ri, currentFlatAreas);
                        levelArea += areas[2];
                        levelAreaOffLLU += areas[3];
                        levelAreaOnLLU += areas[4];
                        sectString += flat.ShortType + "_";
                    }
                    var k1 = levelAreaOffLLU / levelArea;
                    var k2 = levelAreaOffLLU / levelAreaOnLLU;

                    string code = GetCodeSection(rooms);

                    sectCoeffsK1K2.Add(new Tuple<SelectSectionParam, string, int, double, double, string>(
                            item.Key, sectString, sect.Count - 1, k1, k2, code));                    
                }                
            }            

            using (var xlPackage = new ExcelPackage())
            {
                DoubleEqualityComparer doubleComparer = new DoubleEqualityComparer(0.01);
                var wsK1 = xlPackage.Workbook.Worksheets.Add("К1");
                FillCoefK(sectCoeffsK1K2, wsK1, "К1", (k)=> k.GroupBy(g => g.Item4, doubleComparer));
                var wsK2 = xlPackage.Workbook.Worksheets.Add("К2");
                FillCoefK(sectCoeffsK1K2, wsK2, "К2", (k) => k.GroupBy(g => g.Item5, doubleComparer));
                xlPackage.SaveAs(new FileInfo("BankSectionsCoeficientK1K2.xlsx"));
            }
        }

        private static void FillCoefK (List<Tuple<SelectSectionParam, string, int, double, double, string>> sectCoeffsK1K2, 
            ExcelWorksheet ws, string hK, 
            Func<IGrouping<int, Tuple<SelectSectionParam, string, int, double, double, string>>,
                 IEnumerable<IGrouping<double, Tuple<SelectSectionParam, string, int, double, double, string>>>> groupK)
        {            
            int row = 1;
            ws.Cells[row, 1].Value = "Кол секций по типам квартир в банке секций.";
            ws.Cells[row, 2].Value = "Общее кол секций в банке:";
            ws.Cells[row, 3].Value = sectCoeffsK1K2.Count;
            row++;
            ws.Cells[row, 1].Value = "Тип";
            ws.Cells[row, 2].Value = "Этажность";
            ws.Cells[row, 3].Value = "Шаг";
            ws.Cells[row, 4].Value = "Кол квартир без ЛЛУ";
            ws.Cells[row, 5].Value = "Код";
            ws.Cells[row, 6].Value = hK;
            ws.Cells[row, 7].Value = "Кол секций";
            row++;
            foreach (var groupType in sectCoeffsK1K2.GroupBy(g => g.Item1).OrderBy(o => o.Key))
            {
                foreach (var groupCountFlat in groupType.GroupBy(g => g.Item3).OrderBy(o => o.Key))
                {                    
                    foreach (var groupByK1 in groupK(groupCountFlat))
                    {
                        foreach (var groupCode in groupByK1.GroupBy(g => g.Item6))
                        {
                            ws.Cells[row, 1].Value = groupType.Key.Type;
                            ws.Cells[row, 2].Value = groupType.Key.Levels;
                            ws.Cells[row, 3].Value = groupType.Key.Step;
                            ws.Cells[row, 4].Value = groupCountFlat.Key;
                            ws.Cells[row, 5].Value = groupCode.Key;
                            ws.Cells[row, 6].Value = groupByK1.Key.ToString("0.00");
                            ws.Cells[row, 7].Value = groupByK1.Count();
                            row++;
                        }
                    }
                }
            }
        }
        
        static void StatisticsSectionsByFlatsCount()
        {
            DBService dbServ = new DBService(null);
            dbServ.LoadDbFlatsFromFile();
            var sectionsByFlatsCount = DBService.dictDbFlats.Values.SelectMany(s => s).
                GroupBy(g => g.Count).OrderBy(o => o.Key);

            using (var xlPackage = new ExcelPackage())
            {
                var ws = xlPackage.Workbook.Worksheets.Add("Статистика секций по кол-ву квартир.");

                int row = 1;
                ws.Cells[row, 1].Value = "Кол секций по типам квартир в банке секций.";
                ws.Cells[row, 2].Value = "Общее кол секций в банке:";
                ws.Cells[row, 3].Value = sectionsByFlatsCount.Sum(s => s.Count());
                row++;
                ws.Cells[row, 1].Value = "Кол квартир";
                ws.Cells[row, 2].Value = "Кол секций";                
                row++;                                
                foreach (var group in sectionsByFlatsCount)
                {
                    ws.Cells[row, 1].Value = group.Key;
                    ws.Cells[row, 2].Value = group.Count();
                    row++;
                }
                xlPackage.SaveAs(new FileInfo("StatisticsCountFlatInSections.xlsx"));
            }
        }

        static void BankSectionsStatistics()
        {
            DBService dbServ = new DBService(null);
            dbServ.LoadDbFlatsFromFile();

            var sections = DBService.dictDbFlats.Values.SelectMany(s=>s).ToList();

            List<RoomReq> roomType = new List<RoomReq> {
                new RoomReq ("01", 22,25, "Студия"),
                new RoomReq ("01", 25,30, "Студия"),
                new RoomReq ("01", 30,35,"Студия"),
                new RoomReq ("1", 30,35,"1-комнатная"),
                new RoomReq ("1", 35,40,"1-комнатная"),
                new RoomReq ("1", 40,48,"1-комнатная"),
                new RoomReq ("2", 45,52,"2-комнатная"),
                new RoomReq ("2", 52,60,"2-комнатная"),
                new RoomReq ("2", 60,71,"2-комнатная"),
                new RoomReq ("3", 60,70,"3-комнатная"),
                new RoomReq ("3", 70,80,"3-комнатная"),
                new RoomReq ("3", 80,95,"3-комнатная"),
                new RoomReq ("4", 80,90,"4-комнатная"),
                new RoomReq ("4", 90,110,"4-комнатная"),
                new RoomReq ("4", 110,130,"4-комнатная"),
            };

            foreach (var type in roomType)
            {
                var sectTypeRoom = sections.Sum(s => 
                                 (s.Any(r => r.SubZone == type.SubZone &&
                                 r.AreaTotalStandart >= type.MinArea &&
                                 r.AreaTotalStandart < type.MaxArea)) ? 1 : 0);
                type.CountSections = sectTypeRoom;
            }

            using (var xlPackage = new ExcelPackage())
            {                
                var ws = xlPackage.Workbook.Worksheets.Add("Статистика по типам квартир");

                int row = 1;
                int colType = 1;
                int colAreas = 2;
                int colCountSect = 3;

                ws.Cells[row, 1].Value = "Кол секций по типам квартир в банке секций.";
                ws.Cells[row, 2].Value = "Общее кол секций в банке = ";
                ws.Cells[row, 3].Value = sections.Count;
                row++;
                ws.Cells[row, colType].Value = "Тип квартиры";
                ws.Cells[row, colAreas].Value = "Диапазон площади";
                ws.Cells[row, colCountSect].Value = "Кол-во секций";
                row++;                
                foreach (var type in roomType)
                {
                    ws.Cells[row, colType].Value = type.Name;
                    ws.Cells[row, colAreas].Value = $"{type.MinArea}-{type.MaxArea}";
                    ws.Cells[row, colCountSect].Value = type.CountSections;
                    row++;
                }
                xlPackage.SaveAs(new FileInfo("BankSectionsStatistics.xlsx"));
            }
        }

        static void BankSectionsStatisticsShortType ()
        {
            DBService dbServ = new DBService(null);
            dbServ.LoadDbFlatsFromFile();

            var sections = DBService.dictDbFlats.Values.SelectMany(s => s).ToList();

            List<RoomReq> roomType = new List<RoomReq> {
                new RoomReq ("1NM1"),
                new RoomReq ("1NM2"),
                new RoomReq ("1NM3"),
                new RoomReq ("1NS1"),
                new RoomReq ("1NS2"),
                new RoomReq ("1KL1"),
                new RoomReq ("1KS1"),
                new RoomReq ("2KL1"),
                new RoomReq ("2KL2"),
                new RoomReq ("2KL3"),
                new RoomReq ("2KL4"),
                new RoomReq ("2NM1"),
                new RoomReq ("2NM2"),
                new RoomReq ("2NS1"),
                new RoomReq ("3KL1"),
                new RoomReq ("3KL2"),
                new RoomReq ("3KL3"),
                new RoomReq ("3NL1"),
                new RoomReq ("3NL2"),
                new RoomReq ("3NL3"),
                new RoomReq ("4KL1"),
                new RoomReq ("4KL2"),
                new RoomReq ("4NL1"),
                new RoomReq ("4NL2")
            };

            foreach (var type in roomType)
            {
                type.CountSections = sections.Sum(s =>(s.Any(r => r.ShortType == type.Name) ? 1 : 0));
                type.CountRooms = sections.Sum(s => (s.Sum(r => (r.ShortType == type.Name) ? 1 : 0)));                
            }

            using (var xlPackage = new ExcelPackage())
            {
                var ws = xlPackage.Workbook.Worksheets.Add("Статистика по коротким типам квартир");

                int row = 1;
                int colType = 1;                
                int colCountSect = 2;
                int colCountRooms = 3;

                ws.Cells[row, 1].Value = "Кол секций по коротким типам квартир в банке секций.";
                ws.Cells[row, 2].Value = "Общее кол секций в банке:";
                ws.Cells[row, 3].Value = sections.Count;
                row++;
                ws.Cells[row, colType].Value = "Тип квартиры";                
                ws.Cells[row, colCountSect].Value = "Кол-во секций";
                ws.Cells[row, colCountRooms].Value = "Кол-во квартир";
                row++;
                foreach (var type in roomType)
                {
                    ws.Cells[row, colType].Value = type.Name;                    
                    ws.Cells[row, colCountSect].Value = type.CountSections;
                    ws.Cells[row, colCountRooms].Value = type.CountRooms;
                    row++;
                }
                xlPackage.SaveAs(new FileInfo("ShortTypeRoomStatistics.xlsx"));
            }
        }

        class RoomReq
        {
            public string Name { get; set; }
            public string SubZone { get; set; }
            public int MinArea { get; set; }
            public int MaxArea { get; set; }
            public int CountSections { get; set; }
            public int CountRooms { get; set; }

            public RoomReq(string zone, int minArea, int maxArea, string name)
            {
                Name = name;
                SubZone = zone;
                MinArea = minArea;
                MaxArea = maxArea;
            }
            public RoomReq(string name)
            {
                Name = name;
            }
        }

        static void AnalizSectionsSteps ()
        {
            DBService dbServ = new DBService(null);
            dbServ.LoadDbFlatsFromFile();
            var ordinarySects = DBService.dictDbFlats.Where(w => w.Key.Type == "Рядовая");

            int countFailSteps = 0;

            foreach (var sects in ordinarySects)
            {
                var step = sects.Key.Step;
                foreach (var sect in sects.Value)
                {
                    int top = 0;
                    int bot = 0;
                    foreach (var flat in sect)
                    {
                        top += flat.SelectedIndexTop;
                        bot += flat.SelectedIndexBottom;
                    }
                    if (top != step || bot != step)
                    {
                        countFailSteps++;
                    }
                }                                
            }
        }        
    }

    // Сравнение чисел
    public class DoubleEqualityComparer : IEqualityComparer<double>
    {
        private readonly double threshold;

        public DoubleEqualityComparer (double threshold = 1)
        {
            this.threshold = threshold;
        }

        public bool Equals (double x, double y)
        {
            return Math.Abs(x - y) < threshold;
        }

        public int GetHashCode (double obj)
        {
            return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
        static void Main (string[] args)
        {
            //DBService dbServ = new DBService();
            //dbServ.SaveDbFlats();
            //AnalizSectionsSteps();
            //BankSectionsStatistics();
            //BankSectionsStatisticsShortType();
            //StatisticsSectionsByFlatsCount();
            StatisticCoefficientK1K2();

            TextWriterTraceListener writer = new TextWriterTraceListener(Console.Out);
            Debug.Listeners.Add(writer);

            TestProjectScheme test = new TestProjectScheme();
            test.TestTotalHouses();

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        static void StatisticCoefficientK1K2 ()
        {
            C_Flats_PIK1_AreasTableAdapter pikFlats = new C_Flats_PIK1_AreasTableAdapter();
            var flatsAreas = new PIK1.C_Flats_PIK1_AreasDataTable();
            flatsAreas = pikFlats.GetData();

            DBService dbServ = new DBService(null);
            dbServ.LoadDbFlatsFromFile();
            var sections = DBService.dictDbFlats.Values.SelectMany(s => s).ToList();

            var sectCoeffsK1K2 = new List<Tuple<string, double, double>>(); 

            foreach (var sect in sections)
            {                
                double levelArea=0;
                double levelAreaOffLLU=0;
                double levelAreaOnLLU=0;

                string sectString = string.Empty;
                foreach (var flat in sect)
                {
                    var ri = flat.GetRoomInfo();
                    var currentFlatAreas = flatsAreas.First(x => x.Short_Type.Equals(flat.ShortType));
                    var areas = Calculate.GetAreaFlat(15, ri, currentFlatAreas);                    
                    levelArea += areas[2];
                    levelAreaOffLLU += areas[3];
                    levelAreaOnLLU += areas[4];
                    sectString += flat.ShortType + "_";
                }
                var k1= levelAreaOffLLU / levelArea;
                var k2= levelAreaOffLLU / levelAreaOnLLU;

                sectCoeffsK1K2.Add(new Tuple<string, double, double>( sectString, k1, k2));                                              
            }

            using (var xlPackage = new ExcelPackage())
            {
                var ws = xlPackage.Workbook.Worksheets.Add("Коэффициенты К1 К2 всех секций.");

                int row = 1;
                ws.Cells[row, 1].Value = "Кол секций по типам квартир в банке секций.";
                ws.Cells[row, 2].Value = "Общее кол секций в банке:";
                ws.Cells[row, 3].Value = sections.Count;
                row++;
                ws.Cells[row, 1].Value = "Секция";
                ws.Cells[row, 2].Value = "К1";
                ws.Cells[row, 3].Value = "К2";
                row++;
                foreach (var item in sectCoeffsK1K2)
                {
                    ws.Cells[row, 1].Value = item.Item1;
                    ws.Cells[row, 2].Value = item.Item2;
                    ws.Cells[row, 3].Value = item.Item3;
                    row++;
                }
                xlPackage.SaveAs(new FileInfo("BankSectionsCoeficientK1K2.xlsx"));
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
}

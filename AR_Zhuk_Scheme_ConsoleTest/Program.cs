using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            TextWriterTraceListener writer = new TextWriterTraceListener(Console.Out);
            Debug.Listeners.Add(writer);            

            TestProjectScheme test = new TestProjectScheme();
            test.TestTotalHouses();

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        static void BankSectionsStatistics()
        {
            DBService dbServ = new DBService();
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
        class RoomReq
        {
            public string Name { get; set; }
            public string SubZone { get; set; }
            public int MinArea { get; set; }
            public int MaxArea { get; set; }
            public int CountSections { get; set; }

            public RoomReq(string zone, int minArea, int maxArea, string name)
            {
                Name = name;
                SubZone = zone;
                MinArea = minArea;
                MaxArea = maxArea;
            }
        }

        static void AnalizSectionsSteps ()
        {
            DBService dbServ = new DBService();
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;
using AR_Zhuk_Schema;
using AR_Zhuk_Schema.Scheme;

namespace AR_Zhuk_Scheme_ConsoleTest.Scheme
{
    class TestProjectScheme
    {
        public void TestTotalHouses ()
        {
            CreateHouseImage testCreateHouse = new CreateHouseImage();

            // Исходнве данные
            string insolationFile = @"c:\work\test\АР\ЖУКИ\Саларьево 3 вар 2-Задание по инсоляции ПИК1.xlsx";                                
            List<HouseOptions> options = new List<HouseOptions>() {
                 new HouseOptions("P1", 15, 25, new List<bool> { false, false, false, false,  true}),
                 new HouseOptions("P2", 15, 25, new List<bool> { true, false, false, false, false}),
                 new HouseOptions("P3", 15, 25, new List<bool> { true, false, false, false, false}),
                 new HouseOptions("P4", 15, 25, new List<bool> { true, false, false, false, false})
            };
            SpotInfo sp = GetSpotInformation();

            // схема проекта
            ProjectScheme projectSpot = new ProjectScheme(options, sp);
            // Чтение файла схемы объекта
            projectSpot.ReadScheme(insolationFile);

            // Получение всех домов
            Stopwatch timer = new Stopwatch();
            timer.Start();

            var totalHouses = projectSpot.GetTotalHouses(0,0);            

            timer.Stop();
            Console.WriteLine("Получение всех домов = " + timer.Elapsed.TotalSeconds);

            Console.WriteLine($"Пятен = {totalHouses.Count}; Домов = {totalHouses.Sum(s => s.Count)} - {string.Join(",", totalHouses.Select(t => t.Count))}");
            
            Console.WriteLine($"{string.Join("; ", totalHouses.Select(h=>h[0].SectionsBySize[0].SpotOwner + " домов=" +  h.Count))}");
            testCreateHouse.TestCreateImage(totalHouses);//, new List<List<int>> { new List<int> { 9,14,9,12 } });
        }

        public static SpotInfo GetSpotInformation ()
        {
            SpotInfo spotInfo = new SpotInfo();
            spotInfo.requirments.Add(new Requirment("Студия", 21, 40, 10, 0, 0, 0, 0, 3, "01"));
            //spotInfo.requirments.Add(new Requirment("Студия", 33, 35, 8, 0, 8, 0, 0, 4, "01"));
            spotInfo.requirments.Add(new Requirment("Однокомн.", 30, 50, 30, 0, 0, 0, 0, 3, "1"));
            spotInfo.requirments.Add(new Requirment("Двухкомн.", 30, 80, 40, 0, 0, 0, 0, 3, "2"));
            //spotInfo.requirments.Add(new Requirment("Двухкомн.", 57, 60, 11, 0, 3, 0, 0, 3, "2"));
            //spotInfo.requirments.Add(new Requirment("Двухкомн.", 68, 71, 12, 0, 3, 0, 0, 3, "2"));
            spotInfo.requirments.Add(new Requirment("Трехкомн.", 60, 100, 16, 0, 0, 0, 0, 3, "3"));
            //spotInfo.requirments.Add(new Requirment("Четырехкомн.", 80, 120, 3, 0, 0, 0, 0, 3, "4"));
            return spotInfo;
        }
    }
}
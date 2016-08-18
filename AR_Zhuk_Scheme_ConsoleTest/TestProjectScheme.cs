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
            string insolationFile = @"c:\work\test\АР\ЖУКИ\Задание по инсоляции ПИК1-новый тест.xlsx";
            List<HouseOptions> options = new List<HouseOptions>() {
                 new HouseOptions("P1", 9, 18, new List<bool> { false, false, false, false, true }),
                 new HouseOptions("P2", 9, 18, new List<bool> { true, false, false, false, false })
            };
            SpotInfo sp = GetSpotInformation();

            // схема проекта
            ProjectScheme projectSpot = new ProjectScheme(options, sp);
            // Чтение файла схемы объекта
            projectSpot.ReadScheme(insolationFile);

            // Получение всех домов
            Stopwatch timer = new Stopwatch();
            timer.Start();

            var totalHouses = projectSpot.GetTotalHouses(1000);

            timer.Stop();
            Console.WriteLine("Получение всех домов = " + timer.Elapsed.TotalSeconds);

            Console.WriteLine($"Пятен = {totalHouses.Count}; Домов = {totalHouses.Sum(s => s.Count)} - {string.Join(",", totalHouses.Select(t => t.Count))}");
            
            Console.WriteLine($"{string.Join("; ", totalHouses.Select(h=>h[0].SectionsBySize[0].SpotOwner + " домов=" +  h.Count))}");
            testCreateHouse.TestCreateImage(totalHouses);
        }

        public static SpotInfo GetSpotInformation ()
        {
            SpotInfo spotInfo = new SpotInfo();
            spotInfo.requirments.Add(new Requirment("Студия", 22, 23, 15, 15, 3, 0, 0, 1, "01"));
            spotInfo.requirments.Add(new Requirment("Студия", 33, 35, 8, 0, 8, 0, 0, 4, "01"));
            spotInfo.requirments.Add(new Requirment("Однокомн.", 33, 47, 17, 0, 4, 0, 0, 1, "1"));
            spotInfo.requirments.Add(new Requirment("Двухкомн.", 45, 47, 16, 0, 3, 0, 0, 4, "2"));
            spotInfo.requirments.Add(new Requirment("Двухкомн.", 57, 60, 11, 0, 3, 0, 0, 8, "2"));
            spotInfo.requirments.Add(new Requirment("Двухкомн.", 68, 71, 12, 0, 3, 0, 0, 1, "2"));
            spotInfo.requirments.Add(new Requirment("Трехкомн.", 85, 95, 21, 0, 2, 0, 0, 1, "3"));
            return spotInfo;
        }
    }
}
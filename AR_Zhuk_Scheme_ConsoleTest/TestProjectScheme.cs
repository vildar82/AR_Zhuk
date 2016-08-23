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
            string insolationFile = @"z:\Revit_server\13. Settings\02_RoomManager\Жуки\Тесты\Жуки - тест\Бумеранг\Бумеранг.xlsx";
                                //  @"c:\work\test\АР\ЖУКИ\Задание по инсоляции ПИК1_TEST.xlsx";
            List<HouseOptions> options = new List<HouseOptions>() {
                 new HouseOptions("P1", 15, 25, new List<bool> { false, false, false, false,  true}),
                 new HouseOptions("P2", 15, 25, new List<bool> { true, false, false, false, false})
            };
            SpotInfo sp = GetSpotInformation();

            // схема проекта
            ProjectScheme projectSpot = new ProjectScheme(options, sp);
            // Чтение файла схемы объекта
            projectSpot.ReadScheme(insolationFile);

            // Получение всех домов
            Stopwatch timer = new Stopwatch();
            timer.Start();

            var totalHouses = projectSpot.GetTotalHouses();

            timer.Stop();
            Console.WriteLine("Получение всех домов = " + timer.Elapsed.TotalSeconds);

            Console.WriteLine($"Пятен = {totalHouses.Count}; Домов = {totalHouses.Sum(s => s.Count)} - {string.Join(",", totalHouses.Select(t => t.Count))}");
            
            Console.WriteLine($"{string.Join("; ", totalHouses.Select(h=>h[0].SectionsBySize[0].SpotOwner + " домов=" +  h.Count))}");
            testCreateHouse.TestCreateImage(totalHouses);//, new List<List<int>> { new List<int> { 9,14,9,12 } });
        }

        public static SpotInfo GetSpotInformation ()
        {
            SpotInfo spotInfo = new SpotInfo();
            spotInfo.requirments.Add(new Requirment("Студия", 21, 35, 10, 0, 0, 0, 0, 3, "01"));
            //spotInfo.requirments.Add(new Requirment("Студия", 33, 35, 8, 0, 8, 0, 0, 4, "01"));
            spotInfo.requirments.Add(new Requirment("Однокомн.", 33, 48, 31, 0, 0, 0, 0, 3, "1"));
            spotInfo.requirments.Add(new Requirment("Двухкомн.", 45, 80, 40, 0, 0, 0, 0, 3, "2"));
            //spotInfo.requirments.Add(new Requirment("Двухкомн.", 57, 60, 11, 0, 3, 0, 0, 3, "2"));
            //spotInfo.requirments.Add(new Requirment("Двухкомн.", 68, 71, 12, 0, 3, 0, 0, 3, "2"));
            spotInfo.requirments.Add(new Requirment("Трехкомн.", 75, 95, 16, 0, 0, 0, 0, 3, "3"));
            spotInfo.requirments.Add(new Requirment("Четырехкомн.", 95, 130, 3, 0, 0, 0, 0, 3, "4"));
            return spotInfo;
        }
    }
}
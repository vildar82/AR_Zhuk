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
            ProjectInfo sp = GetSpotInformation();
            

            // схема проекта
            ProjectScheme projectSpot = new ProjectScheme(sp);
            // Чтение файла схемы объекта
            projectSpot.ReadScheme();

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

        public static ProjectInfo GetSpotInformation ()
        {
            ProjectInfo spotInfo = new ProjectInfo();
            spotInfo.requirments.Add(new Requirment("Студия", 21, 40, 10, 0, 0, 0, 0, 3, "01"));
            //spotInfo.requirments.Add(new Requirment("Студия", 33, 35, 8, 0, 8, 0, 0, 4, "01"));
            spotInfo.requirments.Add(new Requirment("Однокомн.", 30, 50, 30, 0, 0, 0, 0, 3, "1"));
            spotInfo.requirments.Add(new Requirment("Двухкомн.", 30, 80, 40, 0, 0, 0, 0, 3, "2"));
            //spotInfo.requirments.Add(new Requirment("Двухкомн.", 57, 60, 11, 0, 3, 0, 0, 3, "2"));
            //spotInfo.requirments.Add(new Requirment("Двухкомн.", 68, 71, 12, 0, 3, 0, 0, 3, "2"));
            spotInfo.requirments.Add(new Requirment("Трехкомн.", 60, 100, 16, 0, 0, 0, 0, 3, "3"));
            //spotInfo.requirments.Add(new Requirment("Четырехкомн.", 80, 120, 3, 0, 0, 0, 0, 3, "4"));
            spotInfo.CountFloorsMain = 15;
            spotInfo.CountFloorsDominant = 25;            
            spotInfo.PathInsolation = @"c:\work\test\АР\ЖУКИ\Саларьево 3 вар 2-Задание по инсоляции ПИК1.xlsx";
            spotInfo.SpotOptions = new List<SpotOption>() {
                 new SpotOption("P1", new List<bool> { false, false, false, false,  true}),
                 new SpotOption("P2", new List<bool> { true, false, false, false, false}),
                 new SpotOption("P3", new List<bool> { true, false, false, false, false}),
                 new SpotOption("P4", new List<bool> { true, false, false, false, false})
            };            
            return spotInfo;
        }
    }
}
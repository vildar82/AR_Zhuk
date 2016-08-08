using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AR_Zhuk_DataModel;

namespace AR_Zhuk_Scheme_ConsoleTest
{
    class CreateHouseImage
    {
        static long countFile = 0;
        static string contFileString;
        static string testsResultFolder;
        static string steps;
        static int countSteps;
        static string curDir;

        public CreateHouseImage()
        {
            curDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            testsResultFolder = Path.Combine(curDir, "Результаты");

            if (!Directory.Exists(testsResultFolder))
            {
                Directory.CreateDirectory(testsResultFolder);
            }
            else
            {
                var files = Directory.GetFiles(testsResultFolder);
                foreach (var item in files)
                {
                    File.Delete(item);
                }
            }            
        }

        public void TestCreateImage (HouseInfo house)
        {
            List<HouseInfo> houses = new List<HouseInfo>();
            var countVarHauses = house.SectionsBySize.Max(s => s.Sections.Count);
            for (int i = 0; i < countVarHauses; i++)
            {
                HouseInfo houseVar = new HouseInfo();
                houseVar.SpotInf = house.SpotInf;
                houseVar.Sections = new List<FlatInfo>();
                foreach (var item in house.SectionsBySize)
                {
                    FlatInfo flat;
                    if (i >= item.Sections.Count)
                    {
                        flat = item.Sections.Last();
                    }
                    else
                    {
                        flat = item.Sections[i];
                    }
                    houseVar.Sections.Add(flat);
                }
                houses.Add(houseVar);
            }

            foreach (var item in houses)
            {
                TestCreateImageOneHouse(item);
            }
        }

        public void TestCreateImageOneHouse (HouseInfo house)
        {
            GeneralObject go = new GeneralObject();
            go.SpotInf = house.SpotInf;
            //double area = GetTotalArea(house);                        
            //go.SpotInf.RealArea = area;
            go.GUID = Guid.NewGuid().ToString();
            // ob.Add(go);
            go.Houses = new List<HouseInfo>();

            //house.Sections = new List<FlatInfo>();
            //foreach (var item in house.SectionsBySize)
            //{
            //    house.Sections.Add(item.Sections[0]);
            //}
            //go.Houses.Add(house);

            go.Houses.Add(house);

            string spotName = house.Sections[0].SpotOwner;
            string curSteps = string.Join(".", house.Sections.Select(s=>s.CountStep.ToString()));            

            if (steps != curSteps)
            {
                steps = curSteps;
                countFile = 0;
                countSteps++;
            }

            string insPassed = house.Sections.Any(s => s.Flats.Any(f => !f.IsInsPassed)) ? "Fail": "Passed";            

            countFile++;
            contFileString = countSteps.ToString("0000") + "_" +  countFile.ToString("0000");

            string ids = string.Join("_", house.Sections.Select(s => s.IdSection.ToString()));
            string name = $"{contFileString}_{spotName}_{steps}_{ids}_{insPassed}.png";            

            string imagePath = Path.Combine(testsResultFolder, name);

            string sourceImgFlats = @"z:\Revit_server\13. Settings\02_RoomManager\00_PNG_ПИК1\";
            string ExcelDataPath = Path.Combine(curDir, "БД_Параметрические данные квартир ПИК1 -Не трогать.xlsx");

            BeetlyVisualisation.ImageCombiner imgComb = new BeetlyVisualisation.ImageCombiner(go, ExcelDataPath, sourceImgFlats, 72);
            var img = imgComb.generateGeneralObject();
            img.Save(imagePath, ImageFormat.Png);

            // Лог дома
            //LogHouse(house, contFileString);
        }

        private static void LogHouse (HouseInfo house, string countFileString)
        {
            StringBuilder logHouse = new StringBuilder();
            logHouse.Append("HouseCount=").Append(countFileString).AppendLine();
            foreach (var section in house.Sections)
            {                
                logHouse.Append("ID=").Append(section.IdSection.ToString()).Append(", IsInvert=").Append(section.IsInvert).AppendLine();
                foreach (var flat in section.Flats)
                {
                    logHouse.Append("Flat=").Append(flat.Type).Append(", isInsPassed=").Append(flat.IsInsPassed).AppendLine();
                }
            }
            Trace.Write(logHouse);
        }
    }
}

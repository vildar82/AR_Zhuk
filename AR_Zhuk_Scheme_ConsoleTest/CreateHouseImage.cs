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
        static string curDir;
        Random rnd = new Random();

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

        public void TestCreateImage (List<List<HouseInfo>> houses)
        {
            var countHousesVar = houses.Max(h => h.Count);
            for (int i = 0; i < countHousesVar; i++)
            {
                List<HouseInfo> housesVar = new List<HouseInfo>();
                foreach (var house in houses)
                {
                    HouseInfo hi;
                    if (i < house.Count)
                    {
                        hi = house[i];
                    }
                    else
                    {
                        hi = house.Last();
                    }
                    housesVar.Add(hi);
                }
                var countsectBySize = housesVar.Max(h => h.SectionsBySize.Max(s=>s.Sections.Count));
                for (int s = 0; s < 1; s++)
                {
                    List<HouseInfo> hbs = new List<HouseInfo>();                    
                    foreach (var house in housesVar)
                    {
                        HouseInfo hi = new HouseInfo();
                        hi.SpotInf = house.SpotInf;

                        hi.Sections = new List<FlatInfo>();

                        foreach (var sbs in house.SectionsBySize)
                        {
                            var iSbs = rnd.Next(sbs.Sections.Count - 1);
                            hi.Sections.Add(sbs.Sections[iSbs]);
                        }
                        hbs.Add(hi);
                    }
                    TestCreateImageOneHouse(hbs);
                }               
            }
        }

        public void TestCreateImageOneHouse (List<HouseInfo> houses)
        {
            GeneralObject go = new GeneralObject();
            go.SpotInf = houses[0].SpotInf;
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

            go.Houses = houses;
            
            countFile++;
            contFileString = countFile.ToString("0000");
            var hName = string.Join("_", houses.Select(h => h.Sections[0].SpotOwner + "_" + string.Join(".", h.Sections.Select(s => (s.IsCorner ? "c":"")+ s.CountStep.ToString()))));

            string name = $"{contFileString}_{hName}.png";
            

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

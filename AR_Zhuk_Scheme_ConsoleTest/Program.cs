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

namespace AR_Zhuk_Scheme_ConsoleTest
{
    class Program 
    {
        static void Main (string[] args)
        {
            //DBService dbService = new DBService();
            //dbService.SaveDbFlats();

            // Визуализация секций в банке
            VisualBankSections();

            TextWriterTraceListener writer = new TextWriterTraceListener(Console.Out);
            Debug.Listeners.Add(writer);            

            TestProjectScheme test = new TestProjectScheme();
            test.TestTotalHouses();

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void VisualBankSections ()
        {
            DBService dbService = new DBService(null, 0);

            int count = 0;
            string imageOutputPath = @"C:\temp\";
            string imagePath = @"\\dsk2.picompany.ru\project\CAD_Settings\Revit_server\13. Settings\02_RoomManager\00_PNG_ПИК1";
            string ExcelDataPath = @"z:\Revit_server\13. Settings\02_RoomManager\БД_Параметрические данные квартир ПИК1.xlsx";

            GeneralObject go = new GeneralObject();
            go.GUID = "Test";
            go.SpotInf = new SpotInfo();
            go.SpotInf.Size = new Cell(15, 15);
            go.SpotInf.InsModulesAll = new List<Module>();
            go.Houses = new List<HouseInfo>();
            HouseInfo hi = new HouseInfo();            
            go.Houses.Add(hi);

            foreach (var dbSects in DBService.dictDbFlats)
            {                
                var secs = dbSects.Value.GroupBy(g => g.ID_Section);
                foreach (var sec in secs)
                {
                    List<RoomInfo> rooms = new List<RoomInfo>();
                    foreach (var flat in sec)
                    {
                        var r = flat.GetRoomInfo();
                        rooms.Add(r);
                    }
                    FlatInfo fi = new FlatInfo();
                    fi.Flats = rooms;
                    hi.Sections = new List<FlatInfo>() { fi };
                    ImageCombiner ic = new ImageCombiner(go, ExcelDataPath, imagePath, 72);
                    //ic.CombineImages(imageOutputPath);
                    var img = ic.generateGeneralObject();
                    img.Save(Path.Combine(imageOutputPath, count.ToString("000000") + ".png"), ImageFormat.Png);
                    count++;
                }                
            }   
        }
    }
}

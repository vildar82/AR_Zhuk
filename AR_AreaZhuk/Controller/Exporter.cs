using AR_AreaZhuk.PIK1TableAdapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;
using OfficeOpenXml;
using System.IO;

namespace AR_AreaZhuk.Controller
{
   public static class Exporter
    {
       public static void ExportFlatsToSQL(string excelPath)
       {
           ExportBasicInfo(excelPath);
           ExportFlatsAreas(excelPath);
       }

       static void ExportFlatsAreas(string excelPath)
       {
            FrameWork fw = new FrameWork();
           C_Flats_PIK1_AreasTableAdapter flatsPik1Areas = new C_Flats_PIK1_AreasTableAdapter();
           var flatsAreas = flatsPik1Areas.GetData();
           using (var xlPackage = new ExcelPackage(new FileInfo(excelPath)))
           {
               int counter = 2;
               var worksheet = xlPackage.Workbook.Worksheets[2];
               while (worksheet.Cells[counter, 1].Value != null)
               {
                   string shortType = Convert.ToString(worksheet.Cells[counter, 2].Value);
                   double area_Total_Low18 = fw.DoubleConvert(worksheet.Cells[counter, 4].Value);
                   double area_Total_Standart_More18 = fw.DoubleConvert(worksheet.Cells[counter, 5].Value);
                   double area_Total_Strong_More18 = fw.DoubleConvert(worksheet.Cells[counter, 6].Value);
                   double area_Total_End_Low18 = fw.DoubleConvert(worksheet.Cells[counter, 7].Value);
                   double area_Total_Standart_End_More18 = fw.DoubleConvert(worksheet.Cells[counter, 8].Value);
                   double area_Total_Strong_End_More18 = fw.DoubleConvert(worksheet.Cells[counter, 9].Value);
                   double area_Total_Standart_Seam_Low18 = fw.DoubleConvert(worksheet.Cells[counter, 10].Value);
                   double area_Total_Standart_Seam_More18 = fw.DoubleConvert(worksheet.Cells[counter, 11].Value);
                   double area_Total_Strong_Seam_More18 = fw.DoubleConvert(worksheet.Cells[counter, 12].Value);
                   string correction_Low18 = Convert.ToString(worksheet.Cells[counter, 13].Value);
                   string correction_More18 = Convert.ToString(worksheet.Cells[counter, 14].Value);
                   double area_Live_Low18 = fw.DoubleConvert(worksheet.Cells[counter, 15].Value);
                   double area_Live_Standart_More18 = fw.DoubleConvert(worksheet.Cells[counter, 16].Value);
                   double area_Live_Strong_More18 = fw.DoubleConvert(worksheet.Cells[counter, 17].Value);
                   double area_Live_End_Low18 = fw.DoubleConvert(worksheet.Cells[counter, 18].Value);
                   double area_Live_Standart_End_More18 = fw.DoubleConvert(worksheet.Cells[counter, 9].Value);
                   double area_Live_Strong_End_More18 = fw.DoubleConvert(worksheet.Cells[counter, 20].Value);
                   double area_Live_Seam_Low18 = fw.DoubleConvert(worksheet.Cells[counter, 21].Value);
                   double area_Live_Standart_Seam_More18 = fw.DoubleConvert(worksheet.Cells[counter, 22].Value);
                   double area_Live_Strong_Seam_More18 = fw.DoubleConvert(worksheet.Cells[counter, 23].Value);
                   double area_Level = fw.DoubleConvert(worksheet.Cells[counter, 24].Value);
                   if (flatsAreas.Any(x => x.Short_Type.Equals(shortType)))
                       flatsPik1Areas.UpdateAreaFlat(area_Total_Low18, area_Total_Standart_More18,
                           area_Total_Strong_More18, area_Total_End_Low18, area_Total_Standart_End_More18,
                           area_Total_Strong_End_More18, area_Total_Standart_Seam_Low18, area_Total_Standart_Seam_More18,
                           area_Total_Strong_Seam_More18,
                           correction_Low18, correction_More18, area_Live_Low18, area_Live_Standart_More18,
                           area_Live_Strong_More18,
                           area_Live_End_Low18, area_Live_Standart_End_More18, area_Live_Strong_End_More18,
                           area_Live_Seam_Low18, area_Live_Standart_Seam_More18,
                           area_Live_Strong_Seam_More18, area_Level, shortType);
                   else
                       flatsPik1Areas.Insert(shortType, area_Total_Low18, area_Total_Standart_More18,
                           area_Total_Strong_More18, area_Total_End_Low18, area_Total_Standart_End_More18,
                           area_Total_Strong_End_More18, area_Total_Standart_Seam_Low18, area_Total_Standart_Seam_More18,
                           area_Total_Strong_Seam_More18,
                           correction_Low18, correction_More18, area_Live_Low18, area_Live_Standart_More18,
                           area_Live_Strong_More18,
                           area_Live_End_Low18, area_Live_Standart_End_More18, area_Live_Strong_End_More18,
                           area_Live_Seam_Low18, area_Live_Standart_Seam_More18,
                           area_Live_Strong_Seam_More18, area_Level);

                   counter++;
               }
           }
       }

       private static void ExportBasicInfo(string excelPath)
       {
           FrameWork fw = new FrameWork();
           var roomInfo = fw.GetRoomData(excelPath);
           PIK1TableAdapters.C_Flats_PIK1TableAdapter flatsPIK1 = new C_Flats_PIK1TableAdapter();
           var flats = flatsPIK1.GetData();
           foreach (var rr in roomInfo)
           {
               if (flats.Any(x => x.Type.Equals(rr.Type)))
                   flatsPIK1.UpdateFlat(rr.ShortType, rr.AreaLive, rr.AreaTotalStandart, rr.AreaTotal,Convert.ToInt16(rr.AreaModules), 0, 0, rr.LinkageDO, rr.LinkagePOSLE,
                       rr.FactorSmoke, rr.LightingNiz, rr.LightingTop, rr.IndexLenghtTOP, rr.IndexLenghtNIZ, rr.SubZone,rr.LinkageOR, rr.Type);
               
               else
                   flatsPIK1.InsertFlat(rr.Type, rr.ShortType, rr.AreaLive, rr.AreaTotalStandart, rr.AreaTotal,Convert.ToInt16(rr.AreaModules), 0, 0, rr.LinkageDO, rr.LinkagePOSLE,
                       rr.FactorSmoke, rr.LightingNiz, rr.LightingTop, rr.IndexLenghtTOP, rr.IndexLenghtNIZ, rr.SubZone,rr.LinkageOR);
               
           }
       }


       public static void ExportSectionsToSQL(int countModules,string typeSection,int countFloors,bool isCornerLeft,bool isCornerRight,List<RoomInfo> roomInfo)
       {

           FrameWork fw = new FrameWork();
            //var roomInfo = fw.GetRoomData("");
           var sections = fw.GenerateSections(roomInfo, countModules, isCornerLeft, isCornerRight, countFloors);
           string floors = "10-18";
           if(countFloors==25)
               floors = "19-25";
           else if (countFloors == 9)
               floors = "9";
           PIK1TableAdapters.C_SectionsTableAdapter sects = new C_SectionsTableAdapter();
           PIK1TableAdapters.F_nn_FlatsInSectionTableAdapter flatInSection = new F_nn_FlatsInSectionTableAdapter();
           PIK1TableAdapters.C_Flats_PIK1TableAdapter flatsISectionDB = new C_Flats_PIK1TableAdapter();
           foreach (var section in sections)
           {
               var idSection = sects.InsertSection(countModules / 4, typeSection, floors);
               foreach (var flat in section.Flats)
               {
                   try
                   {
                       var idFlat = flatsISectionDB.GetIdFlat(flat.Type, flat.LinkageDO.Trim(), flat.LinkagePOSLE.Trim());
                       flatInSection.InsertFlatInSection(Convert.ToInt32(idSection), Convert.ToInt32(idFlat), flat.SelectedIndexBottom,
                           flat.SelectedIndexTop);
                   }
                   catch
                   {

                   }

               }
           }
           Environment.Exit(48);

       }


       
    }
}

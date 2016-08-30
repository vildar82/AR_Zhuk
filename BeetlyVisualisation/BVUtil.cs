using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;
using OfficeOpenXml;
using System.IO;

namespace BeetlyVisualisation
{
    public static class Utils
    {
        

        public static GeneralObject getHouseInfo(string XmlPath)
        {
            Serializer ser = new Serializer();
            GeneralObject houseInfo = ser.DeserializeXmlFile(XmlPath);
            return houseInfo;
        }

        internal static void AddInfoForVisualisation(GeneralObject GenObject, string ExcelDataPath)
        {

            List<FlatType> fTypes = Utils.getFlatTypesFromXLSX(ExcelDataPath);


            foreach (HouseInfo hi in GenObject.Houses)
            {
                foreach (FlatInfo fi in hi.Sections)
                {

                    foreach (RoomInfo ri in fi.Flats)
                    {
                        // Получение данных по типу квартиры
                        FlatType ft = fTypes.Find(x => x.Type == ri.Type);
                        ft.SetRoominFoParameters(ri);
                    }
                }
            }
        }

        internal static void AddRoomInfoForViz(FlatInfo fi, string ExcelDataPath)
        {
            List<FlatType> fTypes = Utils.getFlatTypesFromXLSX(ExcelDataPath);
            foreach (RoomInfo ri in fi.Flats)
            {
                // Получение данных по типу квартиры
                FlatType ft = fTypes.Find(x => x.Type == ri.Type);
                ft.SetRoominFoParameters(ri);
            }
        }

        /// <summary>
        /// Получение дополнительной информации из XLSX для построения изображений
        /// </summary>
        /// <param name="XLSXPath"></param>
        /// <returns></returns>
        public static List<FlatType> getFlatTypesFromXLSX(string XLSXPath)
        {
            string TypeName;
            int flatTypeColumn = 3;

            string HorisontalModules;
            int HorModulesColumn = 19;

            string CurrentOffsetX;
            int CurOffcetColumn = 20;

            string NextOffsetX;
            int NextOffsetColumn = 21;

            string FlatUntil;
            int FlatUntilColumn = 6;

            string FlatAfter;
            int FlatAfterColumn = 7;

            List<FlatType> fTypes = new List<FlatType>();

            string tempFileXls = Path.GetTempFileName();
            try
            {                
                File.Copy(XLSXPath, tempFileXls, true);

                var package = new ExcelPackage(new FileInfo(tempFileXls));

                ExcelWorksheet wSheet = package.Workbook.Worksheets[1];

                for (int i = wSheet.Dimension.Start.Row + 1;
                    i <= wSheet.Dimension.End.Row;
                    i++
                    )
                {
                    TypeName = getCell(i, flatTypeColumn, wSheet);

                    if (TypeName != null)
                    {
                        HorisontalModules = getCell(i, HorModulesColumn, wSheet);
                        CurrentOffsetX = getCell(i, CurOffcetColumn, wSheet);
                        NextOffsetX = getCell(i, NextOffsetColumn, wSheet);
                        FlatUntil = getCell(i, FlatUntilColumn, wSheet);
                        FlatAfter = getCell(i, FlatAfterColumn, wSheet);

                        fTypes.Add(new FlatType(TypeName, HorisontalModules, CurrentOffsetX, NextOffsetX, FlatUntil, FlatAfter));
                    }
                }
            }
            catch {}
            finally
            {
                File.Delete(tempFileXls);
            }

            return fTypes;
        }

        /// <summary>
        /// Получение значения ячейки
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="wSheet"></param>
        /// <returns></returns>
        private static string getCell(int row, int column, ExcelWorksheet wSheet)
        {
            object value = wSheet.Cells[row, column].Value;

            return value != null ? value.ToString() : null;
        }








        public static void HouseInfoToLISP(GeneralObject GenObject)
        {
            StringBuilder sb = new StringBuilder();

            List<string> lines = new List<string>();

            string pointPairFormat = "(\"{0}\" . {1})";



            foreach (HouseInfo hi in GenObject.Houses)
            {


                foreach (FlatInfo fi in hi.Sections)
                {
                    //sb.Append("(");

                    foreach (RoomInfo ri in fi.Flats)
                    {
                        // Корректировка неверно выданных данных
                        if (ri.Type == "PIK1_3NL2_Z0")
                        {
                            ri.SelectedIndexTop = 4;
                            ri.SelectedIndexBottom = 0;
                        }

                        sb.Append(BuildDataRows(pointPairFormat, ri));
                    }
                    // sb.Append(") ");
                    lines.Add(sb.ToString());
                    sb.Clear();

                }
            }



            using (StreamWriter sw = new StreamWriter("res.txt"))
            {
                foreach (string line in lines)
                {

                    sw.WriteLine(line);
                }
            }

            //string listFlats = sb.ToString();
        }

        private static StringBuilder BuildDataRows(string Format, RoomInfo ri)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("(");

            sb.AppendFormat(Format, "Type", "\"" + ri.Type + ri.ImageNameSuffix + "\"");
            sb.AppendFormat(Format, "SelectedIndexTop", ri.SelectedIndexTop);
            sb.AppendFormat(Format, "SelectedIndexBottom", ri.SelectedIndexBottom);
            sb.AppendFormat(Format, "HorisontalModules", ri.HorisontalModules);
            sb.AppendFormat(Format, "OffsetX", ri.CurrentOffsetX);
            sb.AppendFormat(Format, "NextOffsetX", ri.NextOffsetX);

            sb.Append(")");

            return sb;
        }


        /// <summary>
        /// Кэширование 
        /// </summary>
        /// <param name="SourcePath"></param>
        public static void cashImages(string SourcePath, string tempPath)
        {
            if (System.IO.Directory.Exists(SourcePath))
            {

                System.IO.Directory.CreateDirectory(tempPath);

                Console.WriteLine(tempPath);

                foreach (string filename in Directory.GetFiles(SourcePath, "*.png"))
                {
                    string target = Path.Combine(tempPath, System.IO.Path.GetFileName(filename));
                    System.IO.File.Copy(filename, target, true);
                }

            }

        }

        /// <summary>
        /// Получение задания по инсоляции
        /// </summary>
        /// <returns></returns>
        public static List<int[]> getSpotInfo(int endRow, int endColumn, string ExcelDataPath)
        {
            var package = new ExcelPackage(new System.IO.FileInfo(ExcelDataPath));

            ExcelWorksheet wSheet = package.Workbook.Worksheets[1];


            List<int[]> spotData = new List<int[]>();

            for (int i = wSheet.Dimension.Start.Row;
    i <= endRow;
    i++
    )
            {
                for (int j = wSheet.Dimension.Start.Column; j <= endColumn; j++)
                {
                    string cell = getCell(i, j, wSheet);
                    if (cell != null && cell.Contains("|"))
                    {
                        string ins = cell.Split('|')[1];
                        int insI;

                        switch (ins)
                        {
                            case "A":
                            case "А":
                                insI = 0;
                                break;

                            case "B":
                            case "В":
                                insI = 1;
                                break;

                            case "C":
                            case "С":
                                insI = 2;
                                break;

                            default:
                                insI = 0;
                                break;
                        }

                        spotData.Add(new int[] { i, j, insI });
                    }

                }


            }

            return spotData;
        }
    }
}

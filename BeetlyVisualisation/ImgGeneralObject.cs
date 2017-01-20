#define Debug

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace BeetlyVisualisation
{
    class ImgGeneralObject
    {
        /// <summary>
        /// Список изображений домов
        /// </summary>
        public List<ImgHouse> ImgHouses;

        /// <summary>
        /// Длина изображения в модулях
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Высота изображения в модулях
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Длина изображения объекта в модулях
        /// </summary>
        public int GOWidth { get; set; }

        /// <summary>
        /// Высота изображения объекта в модулях
        /// </summary>
        public int GOHeight { get; set; }


        /// <summary>
        /// Размер модуля в пикселях
        /// </summary>
        public int moduleWidth { get; set; }

        //// разрыв между изображениями домов в модулях
        //int gap = 0;

        private List<Module> ins;

        private List<Requirment> req;

        private ProjectInfo Spotinfo;

        private string imgPath;


        public ImgGeneralObject(int ModuleWidth, ProjectInfo spi, string imgPath)
        {
            this.ImgHouses = new List<ImgHouse>();

            this.moduleWidth = ModuleWidth;


            this.req = spi.requirments;

            this.imgPath = imgPath;

            this.GOWidth = spi.Size.Col + 2;
            this.GOHeight = spi.Size.Row + 2;

            this.Width = spi.Size.Col + 3;
            this.Height = spi.Size.Row + 3;

            this.ins = spi.InsModulesAll;

#if DEBUG
            this.Width += 35;
#endif
            this.Width += 27;

            this.Spotinfo = spi;

            //this.gap = 2;
        }


        public Bitmap Generate(string seria)
        {
            Bitmap table = drawResultTable();
            Bitmap legend = drawLegend();

            int width = this.Width * this.moduleWidth;
            int height = this.Height * this.moduleWidth;

            int AnnotHeigth = table.Height + 7 * moduleWidth + legend.Height;
            
            if(height < AnnotHeigth)
            {
                height = AnnotHeigth;
            }

            var bitmap = new Bitmap(width, height);

            using (var canvas = Graphics.FromImage(bitmap))
            {
                canvas.Clear(Color.White);
                canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;


                DrawSpotInfo(canvas);

                foreach (ImgHouse imgHouse in this.ImgHouses)
                {
                    foreach (ImgSection imgS in imgHouse.ImgSections)
                    {



                        int imgSectionWidth = 0;
                        int imgSectionHeight = 0;

                        if (imgS.Angle == 0 || imgS.Angle == 180)
                        {
                            imgSectionWidth = imgS.Lenght * moduleWidth;
                            imgSectionHeight = imgS.Height * moduleWidth;
                        }
                        else
                        {
                            imgSectionWidth = imgS.Height * moduleWidth;
                            imgSectionHeight = imgS.Lenght * moduleWidth;
                        }









                        imgS.CoordX = imgS.SectionInfo.ImageStart.Col;
                        imgS.CoordY = imgS.SectionInfo.ImageStart.Row;
                        imgS.CoordX *= moduleWidth;
                        imgS.CoordY *= moduleWidth;

                        Image frame = imgS.BmpSection ?? new Bitmap(imgSectionWidth, imgSectionHeight);

                        canvas.DrawImage(frame,
                 new Rectangle(imgS.CoordX,
                               imgS.CoordY,
                               imgSectionWidth,
                               imgSectionHeight),
                 new Rectangle(0,
                               0,
                               //imgFlat.CoordY * ModuleWidth,
                               //-ModuleWidth,
                               frame.Width,
                               frame.Height),
                 GraphicsUnit.Pixel);

                        string floors = imgS.SectionInfo.Floors.ToString() + " эт.";


                        int floorsTextX = 0;
                        int floorsTextY = 0;

                        if (imgS.Angle == 0 || imgS.Angle == 180)
                        {
                            floorsTextX = moduleWidth * imgS.Lenght / 2 + imgS.CoordX;
                            floorsTextY = imgS.CoordY + imgS.Height * moduleWidth + moduleWidth / 2;

                            if (imgS.SectionInfo.IsCorner && imgS.Angle == 180)
                            {
                                // Если секция угловая левая, то обозначение сдвигается вправо

                                if(imgS.SectionInfo.Flats[1].ShortType.Contains("3NL2"))
                                {
                                    floorsTextX -= moduleWidth * 2;
                                }
                                else
                                {
                                    floorsTextX += moduleWidth;
                                }

                                
                                floorsTextY -= moduleWidth;
                            }

                        }
                        else
                        {
                            floorsTextX = imgS.CoordX + moduleWidth * imgS.Height + moduleWidth;
                            floorsTextY = imgS.CoordY + moduleWidth * imgS.Lenght / 2;

                            if (imgS.SectionInfo.IsCorner && imgS.SectionInfo.IsVertical)
                            {

                                if (imgS.SectionInfo.Flats[1].ShortType.Contains("3NL2") && imgS.SectionInfo.ImageAngle != 270)
                                {
                                    floorsTextX -= moduleWidth;
                                }
                                else
                                {
                                    if(!imgS.SectionInfo.Flats[1].ShortType.Contains("3NL2") && imgS.SectionInfo.ImageAngle != 270)
                                    {
                                        floorsTextX -= moduleWidth;
                                    }
                                    
                                }

                                // floorsTextY -= moduleWidth/2;
                            }
                        }
                        //floorsTextX *= moduleWidth;
                        //floorsTextY *= moduleWidth;

                        canvas.DrawString(floors, new Font("Tahoma", 40), Brushes.Black, floorsTextX, floorsTextY);
                    }
                }

#if (DEBUG)
                DrawDebugInfo(canvas);
#endif                
                DrawGrid(canvas);

                // Наименование расчета - по имени файла инсоляции                                
                string title = seria+". "+Path.GetFileNameWithoutExtension(Spotinfo.PathInsolation) + ", " + DateTime.Now;
                canvas.DrawString(title, new Font("Tahoma", 40), Brushes.Black, (GOWidth + 3) * moduleWidth, 0);

                placeResultTable(canvas, table, (GOWidth + 3) * moduleWidth, moduleWidth*2);

                placeLegend(canvas, legend, (GOWidth + 3) * moduleWidth, moduleWidth * 18);

                placeLogo(canvas, (GOWidth + 20) * moduleWidth, height - 6*moduleWidth);

                canvas.Save();
            }

            return bitmap;
        }

        private void DrawSpotInfo(Graphics canvas)
        {
            //string ExcelDataPath = @"C:\Users\fazleevaa\Desktop\Задание по инсоляции БУМЕРАНГ_42.xlsx";

            //List<int[]> si = Utils.getSpotInfo(this.Height, this.Width, ExcelDataPath);

            List<int[]> si = getSpotInfo();

            foreach (int[] item in si)
            {
                DrawIns(canvas, 0, item[2], item[1], item[0]);
            }
        }

        /// <summary>
        /// Заполнение одной клетки
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="side"></param>
        /// <param name="colorIndex"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        private void DrawIns(Graphics canvas, int side, int colorIndex, int X, int Y)
        {

            Brush br = null;

            switch (colorIndex)
            {
                case 0:
                    br = new SolidBrush(Color.FromArgb(255, (byte)255, (byte)0, (byte)0));
                    break;
                case 1:
                    br = new SolidBrush(Color.FromArgb(255, (byte)255, (byte)255, (byte)0));
                    break;
                case 2:
                    br = new SolidBrush(Color.FromArgb(255, (byte)255, (byte)192, (byte)0));
                    break;

                case 3:
                    br = new SolidBrush(Color.FromArgb(255, (byte)0, (byte)127, (byte)255));
                    break;

                default:
                    br = new SolidBrush(Color.FromArgb(255, (byte)0, (byte)255, (byte)0));
                    break;
            }

            X *= moduleWidth;
            Y *= moduleWidth;



            Bitmap bg = new Bitmap(moduleWidth, moduleWidth / 3);
            Bitmap bv = new Bitmap(moduleWidth / 3, moduleWidth);

            int bY = Y + moduleWidth * 2 / 3;
            int rX = X + moduleWidth * 2 / 3;



            drawOneIns(canvas, bg, X, Y + moduleWidth, br);
            drawOneIns(canvas, bv, X + moduleWidth, Y, br);
            drawOneIns(canvas, bg, X, bY - moduleWidth, br);
            drawOneIns(canvas, bv, rX - moduleWidth, Y, br);

        }


        /// <summary>
        /// Заполнение одной стороны
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="b"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="br"></param>
        private void drawOneIns(Graphics canvas, Bitmap b, int X, int Y, Brush br)
        {
            using (Graphics grp = Graphics.FromImage(b))
            {
                grp.FillRectangle(
                    br, 0, 0, b.Width, b.Height);
            }

            canvas.DrawImage(b,
                 new Rectangle(X,
                               Y,
                               b.Width,
                               b.Height),
                new Rectangle(0,
                              0,
                              b.Width,
                              b.Height),
                              GraphicsUnit.Pixel);
        }



        private void DrawGrid(Graphics canvas)
        {
            for (int i = 0; i < this.GOHeight + 1; i++)
            {
                canvas.DrawLine(Pens.Gray, 0, i * moduleWidth, this.GOWidth * moduleWidth, i * moduleWidth);
            }

            for (int i = 1; i < this.GOHeight - 2; i++)
            {
                canvas.DrawString(i.ToString(), new Font("Tahoma", 20), Brushes.Black, moduleWidth / 6, moduleWidth * i + moduleWidth / 3);
            }

            for (int i = 0; i < this.GOWidth + 1; i++)
            {
                canvas.DrawLine(Pens.Gray, i * moduleWidth, 0, i * moduleWidth, this.GOHeight * moduleWidth);
            }

            for (int i = 1; i < this.GOWidth - 2; i++)
            {
                canvas.DrawString(i.ToString(), new Font("Tahoma", 20), Brushes.Black, moduleWidth * i + moduleWidth / 6, moduleWidth / 5);
            }
        }

        private void DrawDebugInfo(Graphics canvas)
        {
            int hIndex = 0;
            int sIndex = 0;

            int tX = (this.Width - 30) * this.moduleWidth;

            int tY = 1 * this.moduleWidth;


            foreach (ImgHouse imgHouse in this.ImgHouses)
            {

                hIndex++;
                string hi = "Номер дома: " + hIndex;
                tY += 2 * moduleWidth * 3 / 2;
                canvas.DrawString(hi, new Font("Tahoma", 80), Brushes.Black, tX, tY);
                tY += 2 * moduleWidth * 3 / 2;







                foreach (ImgSection imgS in imgHouse.ImgSections)
                {

                    sIndex++;
                    string si = "Секция " + sIndex + "| X: " + imgS.SectionInfo.ImageStart.Col
                        + "| Y: " + imgS.SectionInfo.ImageStart.Row
                        + "|Угол: " + imgS.Angle;
                        

                    canvas.DrawString(si, new Font("Tahoma", 80), Brushes.Black, tX, tY);
                    tY += moduleWidth * 3 / 2;


                }







            }
        }

        private void placeSpotInfo(Graphics canvas)
        {
            string ExcelDataPath = @"C:\Users\fazleevaa\Desktop\Задание по инсоляции БУМЕРАНГ_42.xlsx";
            List<FlatType> fTypes = Utils.getFlatTypesFromXLSX(ExcelDataPath);


        }

        private List<int[]> getSpotInfo()

        {

            List<int[]> spotData = new List<int[]>();


            foreach (Module m in ins)
            {

                int insI = 0;

                switch (m.InsValue)
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

                    case "D":
                        insI = 3;
                        break;

                    default:
                        insI = 0;
                        break;
                }

                spotData.Add(new int[] { m.Cell.Row, m.Cell.Col, insI });
            }
            return spotData;
        }

        /// <summary>
        /// Генерация таблицы в bmp
        /// </summary>
        /// <returns></returns>
        private Bitmap drawResultTable()
        {
            List<int> ws = new List<int> { 0, 9, 5, 5, 5 };

            int[] rh = new int[] { 2, 1 };

            List<string[]> data = new List<string[]>();

            data.Add(new string[] { "КОЛИЧЕСТВО КОМНАТ", "ПЛОЩАДЬ, КВ.М", "% КВАРТИР ПО ЗАДАНИЮ", "% КВАРТИР В ИТОГЕ" });

            string roomNumber = string.Empty;

            foreach (Requirment rq in req)
            {

                switch (rq.CodeZone)
                {
                    case "01":
                        roomNumber = "СТУДИЯ";
                        break;
                    case "1":
                        roomNumber = "ОДНОКОМНАТНАЯ";
                        break;
                    case "2":
                        roomNumber = "ДВУХКОМНАТНАЯ";
                        break;
                    case "3":
                        roomNumber = "ТРЕХКОМНАТНАЯ";
                        break;
                    case "4":
                        roomNumber = "ЧЕТЫРЕХКОМНАТНАЯ";
                        break;

                    default:
                        roomNumber = rq.SubZone;
                        break;
                }

                data.Add(new string[] { roomNumber, rq.MinArea + "-" + rq.MaxArea, rq.Percentage.ToString(), Math.Round(rq.RealPercentage, 2).ToString() });

            }
            data.Add(new string[] { "" });
            data.Add(new string[] { "ПРОДАВАЕМАЯ ПЛОЩАДЬ ОБЪЕКТА, КВ.М:", Spotinfo.TotalStandartArea.ToString() });
            data.Add(new string[] { "ЖИЛАЯ ПЛОЩАДЬ ОБЪЕКТА, КВ.М:", Spotinfo.TotalLiveArea.ToString() });
            data.Add(new string[] { "К1", Math.Round(Spotinfo.K1, 2).ToString() });
            data.Add(new string[] { "К2", Math.Round(Spotinfo.K2, 2).ToString() });

            int X = ws.Sum() * moduleWidth;

            int Y = ((data.Count - 1) * rh[1] + rh[0]) * moduleWidth;



            Bitmap b = new Bitmap(X, Y);
            Brush br = new SolidBrush(Color.FromArgb(255, (byte)250, (byte)250, (byte)250));


            // TODO: Заливку ячеек цветом выделить в отдельную функцию
            Bitmap fillLastColumn = new Bitmap(ws.Last() * moduleWidth, Y - moduleWidth * 5);
            Brush brlc = new SolidBrush(Color.FromArgb(255, (byte)253, (byte)226, (byte)194));

            Bitmap fill3Column = new Bitmap(ws[ws.Count - 2] * moduleWidth, Y - moduleWidth * 5);
            Brush brl3c = new SolidBrush(Color.FromArgb(255, (byte)229, (byte)246, (byte)227));

            using (var canvas = Graphics.FromImage(b))
            {
                canvas.Clear(Color.White);
                canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;


                using (Graphics grp = Graphics.FromImage(b))
                {
                    grp.FillRectangle(
                        br, 0, 0, b.Width, b.Height);
                }

                using (Graphics grp1 = Graphics.FromImage(fillLastColumn))
                {
                    grp1.FillRectangle(
                        brlc, 0, 0, fillLastColumn.Width, fillLastColumn.Height);
                }

                using (Graphics grp = Graphics.FromImage(fill3Column))
                {
                    grp.FillRectangle(
                        brl3c, 0, 0, fill3Column.Width, fill3Column.Height);
                }

                canvas.DrawImage(b,
                     new Rectangle(X,
                                   Y,
                                   b.Width,
                                   b.Height),
                    new Rectangle(0,
                                  0,
                                  b.Width,
                                  b.Height),
                                  GraphicsUnit.Pixel);

                canvas.DrawImage(fillLastColumn,
                   new Rectangle(b.Width - fillLastColumn.Width,
                               0,
                               fillLastColumn.Width,
                               fillLastColumn.Height),
                   new Rectangle(0,
                               0,
                               fillLastColumn.Width,
                               fillLastColumn.Height),
                               GraphicsUnit.Pixel);

                canvas.DrawImage(fill3Column,
                   new Rectangle(b.Width - fillLastColumn.Width - fill3Column.Width,
                               0,
                               fill3Column.Width,
                               fill3Column.Height),
                   new Rectangle(0,
                               0,
                               fill3Column.Width,
                               fill3Column.Height),
                               GraphicsUnit.Pixel);



                drawStrLines(canvas, ws, rh[0], 0, data[0]);
                int rY = rh[0];

                for (int i = 1; i < data.Count; i++)
                {
                    drawStrLines(canvas, ws, rh[1], rY, data[i]);
                    rY += rh[1];
                }






                canvas.Save();

            }








            return b;
        }

        private void drawStrLines(Graphics canvas, List<int> widths, int r, int Y, string[] dat)
        {
            r = r * moduleWidth;
            Y = Y * moduleWidth;

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;


            FontStyle fs = FontStyle.Regular;
            int fh = 23;

            if (Y == 0)
            {
                fs = FontStyle.Bold;
                fh = 23;
            }




            Pen p = new Pen(Brushes.Black, 5);
            int X = 0;
            int i = 0;

            foreach (int w in widths)
            {

                X += w * moduleWidth;
                canvas.DrawLine(p, X, Y, X, r + Y);
                if (i < dat.Length)
                {
                    canvas.DrawString(dat[i], new Font("Calibri", fh, fs), Brushes.Black, X + widths[i + 1] * moduleWidth / 2, r / 2 + Y, sf);
                    i++;
                }

            }

            canvas.DrawLine(p, 0, r + Y, X, r + Y);







        }




        /// <summary>
        /// Размещение таблицы
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="table"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        private void placeResultTable(Graphics canvas, Bitmap table, int X, int Y)
        {
            canvas.DrawImage(table,
                     new Rectangle(X,
                                   Y,
                                   table.Width,
                                   table.Height),
                    new Rectangle(0,
                                  0,
                                  table.Width,
                                  table.Height),
                                  GraphicsUnit.Pixel);

            Pen p = new Pen(Brushes.Black, 5);

            canvas.DrawRectangle(p, X, Y, table.Width, table.Height);
        }


        private Bitmap drawLegend()
        {
            List<LegendLine> lData = getLegendData();

            int X = 0, Y = 30;

            Bitmap b = new Bitmap(1000, 1000);

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Center;


            FontStyle fs = FontStyle.Bold;
            int fh = 35;



            using (var canvas = Graphics.FromImage(b))
            {
                canvas.Clear(Color.White);
                canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;



                canvas.DrawString("Условные обозначения:", new Font("Calibri", fh, fs), Brushes.Black, X, Y, sf);

                Y += fh + 20;


                foreach (LegendLine ll in lData)
                {
                    drawOneStringLegend(canvas, ll.c, X, Y, ll.X, ll.Y, ll.text);


                    if (ll.text.Count > 2)
                    {
                        Y += ll.text.Count* fh;
                    }
                    else
                    {
                        Y += ll.Y * 4 / 5;
                    }


                        

                }

                


                canvas.Save();

            }


            return b;
        }


        private void drawOneStringLegend(Graphics canvas, Color c, int X, int Y, int heigth, int width, List<string> text)
        {
            Bitmap b = new Bitmap(width, heigth);
            Brush br = new SolidBrush(c);

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Center;


            FontStyle fs = FontStyle.Regular;
            int fh = 23;

            using (Graphics grp = Graphics.FromImage(b))
            {
                grp.FillRectangle(
                    br, 0, 0, b.Width, b.Height);
            }

            canvas.DrawImage(b,
     new Rectangle(X,
                   Y,
                   b.Width,
                   b.Height),
    new Rectangle(0,
                  0,
                  b.Width,
                  b.Height),
                  GraphicsUnit.Pixel);

            int txtStep = 30;

            int txtYCoord = 0;

            if (text.Count == 1)
            {
                txtYCoord = Y + heigth / 2;
            }
            else
            {
                txtYCoord = Y + fh / 2;
            }

            int txtXCoord = X + width + 30;




            foreach (string t in text)
            {

                canvas.DrawString(t, new Font("Calibri", fh, fs), Brushes.Black, txtXCoord, txtYCoord, sf);
                txtYCoord += txtStep;
            }





        }

        /// <summary>
        /// Заполнение данных по легенде
        /// в дальнейшем можно сделать возможность записи/чтения настроек в базу данных и чтение
        /// </summary>
        /// <returns></returns>
        private List<LegendLine> getLegendData()
        {

            List<LegendLine> lDat = new List<LegendLine>();




            lDat.Add(new LegendLine(
                Color.FromArgb(255, (byte)233, (byte)107, (byte)130),
                moduleWidth / 2, moduleWidth,
                new List<string> { "Квартира студия" }));

            lDat.Add(new LegendLine(
                Color.FromArgb(255, (byte)139, (byte)168, (byte)222),
                moduleWidth / 2, moduleWidth,
                new List<string> { "Однокомнатная квартира" }));

            lDat.Add(new LegendLine(
                Color.FromArgb(255, (byte)228, (byte)183, (byte)24),
                moduleWidth / 2, moduleWidth,
                new List<string> { "Двухкомнатная квартира" }));

            lDat.Add(new LegendLine(
                Color.FromArgb(255, (byte)148, (byte)183, (byte)28),
                moduleWidth / 2, moduleWidth,
                new List<string> { "Трехкомнатная квартира" }));

            lDat.Add(new LegendLine(
                Color.FromArgb(255, (byte)232, (byte)133, (byte)85),
                moduleWidth / 2, moduleWidth,
                new List<string> { "Четырехкомнатная квартира" }));

            lDat.Add(new LegendLine(
                Color.FromArgb(255, (byte)192, (byte)192, (byte)192),
                moduleWidth / 2, moduleWidth,
                new List<string> { "Помещения общего доступа" }));

            lDat.Add(new LegendLine(
                Color.FromArgb(255, (byte)255, (byte)255, (byte)255),
                moduleWidth / 2, moduleWidth,
                new List<string> { "" }));

            lDat.Add(new LegendLine(
                Color.FromArgb(255, (byte)255, (byte)192, (byte)0),
                moduleWidth / 2, moduleWidth,
                new List<string> { "Графическое отображение 2-х часового",
                    "периода непрерывной инсоляции для",
                    "одного жилого помещения" }));

            lDat.Add(new LegendLine(
                Color.FromArgb(255, (byte)255, (byte)255, (byte)0),
                moduleWidth / 2, moduleWidth,
                new List<string> { "Графическое отображение 1,5 часового",
                    "периода непрерывной инсоляции для",
                    "двух жилых помещений" }));

            lDat.Add(new LegendLine(
                Color.FromArgb(255, (byte)0, (byte)127, (byte)255),
                moduleWidth / 2, moduleWidth,
                new List<string> { "Графическое отображение 2,5 часового",
                    "периода прерывистой инсоляции для", "одного жилого помещения" }));

            lDat.Add(new LegendLine(
                Color.FromArgb(255, (byte)255, (byte)0, (byte)0),
                moduleWidth / 2, moduleWidth,
                new List<string> { "Графическое отображение периода", "инсоляции, недостаточной для", "выполнения нормативных требований" }));

            return lDat;

        }



        private void placeLegend(Graphics canvas, Bitmap legend, int X, int Y)
        {

            canvas.DrawImage(legend,
                     new Rectangle(X,
                                   Y,
                                   legend.Width,
                                   legend.Height),
                    new Rectangle(0,
                                  0,
                                  legend.Width,
                                  legend.Height),
                                  GraphicsUnit.Pixel);

            //Pen p = new Pen(Brushes.Black, 5);

            //canvas.DrawRectangle(p, X, Y, table.Width, table.Height);
        }


        private void placeLogo(Graphics canvas, int X, int Y)
        {
            int h  = 300;
            int w = h*14/10;

            string LogoPath = System.IO.Path.Combine(imgPath, "Логотип ПИК а.png");

            Image frame;

            if (System.IO.File.Exists(LogoPath))
            {
                frame = Image.FromFile(LogoPath);
            }
            else
            {
                frame = new Bitmap(w, h);
            }






            canvas.DrawImage(frame,
                             new Rectangle(X,
                                           Y,
                                           //imgFlat.CoordY * ModuleWidth * 2,
                                           //-ModuleWidth - ModuleWidth/4,
                                           w,
                                           h),
                             new Rectangle(0,
                                           0,
                                           //imgFlat.CoordY * ModuleWidth,
                                           //-ModuleWidth,
                                           frame.Width,
                                           frame.Height),
                             GraphicsUnit.Pixel);
        }




    }


    struct LegendLine
    {
        public Color c;
        public int X, Y;
        public List<string> text;

        // Constructor
        public LegendLine(Color c, int X, int Y, List<string> text)
        {
            this.c = c;
            this.X = X;
            this.Y = Y;
            this.text = text;
        }
    }
}

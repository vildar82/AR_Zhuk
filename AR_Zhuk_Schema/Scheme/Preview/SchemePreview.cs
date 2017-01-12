using AR_Zhuk_DataModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_Schema.Scheme.Preview
{
    /// <summary>
    /// Картинка схемы
    /// </summary>
    public class SchemePreview
    {
        private ProjectScheme projectScheme;
        private Graphics g;
        private int pixelInCell = 32;
        private int legendHeight = 100;
        private int widthImage;
        private int heightImage;
        private int yLegend;

        public SchemePreview(ProjectScheme projectScheme)
        {
            this.projectScheme = projectScheme;
        }

        public static void Show(Image image)
        {
            var formPreview = new UI.FormPreviewSchema(image);
            formPreview.Show();
        }             

        /// <summary>
        /// Создание картинки
        /// </summary>        
        public Image CreatePreview()
        {
            var imScheme = GetEmptyImage();
            g = Graphics.FromImage(imScheme);

            // Отрисовка всех ячеек инсоляции
            VisualAllCells();

            // Стрелки домов
            foreach (var spot in projectScheme.HouseSpots)
            {
                VisualSpot(spot);
            }

            // Легенда
            VisualLegend();

            return imScheme;
        }

        

        private Image GetEmptyImage()
        {
            widthImage = GetPixel(ProjectScheme.ProjectInfo.Size.Col);
            yLegend = GetPixel(ProjectScheme.ProjectInfo.Size.Row);
            heightImage = yLegend + legendHeight;
            var im = new Bitmap(widthImage, heightImage);
            return im;
        }        

        /// <summary>
        /// Визуализация пятна дома
        /// </summary>        
        private void VisualSpot(HouseSpot spot)
        {
            var firstSegment = spot.Segments.First();
            VisualSegmentDirection(firstSegment, spot.SpotName);
        }

        private void VisualSegmentDirection(Segment seg, string name)
        {
            var pStartLeft = GetPoint(seg.CellStartLeft);
            var pStartRight = GetPoint(seg.CellStartRight);
            var ptStartCenter = GetCenter(pStartLeft, pStartRight);
            DrawArrow(ptStartCenter, seg.Direction, name);
        }        

        private void VisualSegment(Segment seg)
        {
            var pen = new Pen(Brushes.Red, 1);
            var pStart = new Point(GetSize(seg.CellStartLeft));
            var pEnd = new Point(GetSize(seg.CellEndLeft));
            g.DrawLine(pen, pStart, pEnd);
        }

        private void VisualAllCells()
        {            
            foreach (var cell in ProjectScheme.ProjectInfo.InsModulesAll)
            {
                g.FillRectangle(GetBrush(cell), GetRectangle(cell));                
            }            
        }        

        /// <summary>
        /// Перевод из ячеек в пиксели
        /// </summary>
        /// <param name="cell"></param>        
        private int GetPixel(int cell)
        {
            return cell * pixelInCell;
        }
        private Size GetSize(Cell cell, int scale = 1)
        {
            return new Size(GetPixel(cell.Col*scale), GetPixel(cell.Row*scale));
        }
        private Point GetPoint(Cell cell)
        {
            return new Point(GetPixel(cell.Col), GetPixel(cell.Row));
        }
        private Point GetCenter(Point p1, Point p2)
        {
            return new Point((p1.X+p2.X)/2, (p1.Y + p2.Y) / 2);
        }

        private Pen GetPen(Module cell)
        {
            switch (cell.InsValue)
            {
                case "A":
                    return Pens.Red;
                case "B":
                    return Pens.Yellow;
                case "C":
                    return Pens.Orange;
                case "D":
                    return Pens.Blue;
                default:
                    return Pens.Gray;
            }
        }
        private Brush GetBrush(Module cell)
        {
            switch (cell.InsValue)
            {
                case "A":
                    return Brushes.Red;
                case "B":
                    return Brushes.Yellow;
                case "C":
                    return Brushes.Orange;
                case "D":
                    return Brushes.Blue;
                default:
                    return Brushes.Gray;
            }
        }

        private Rectangle GetRectangle(Module cell)
        {
            var xUpperLeft = cell.Cell.Col * pixelInCell - (pixelInCell / 2);
            var yUpperLeft = cell.Cell.Row * pixelInCell - (pixelInCell / 2);
            return new Rectangle(xUpperLeft, yUpperLeft, pixelInCell, pixelInCell);
        }        

        /// <summary>
        /// Рисование стрелки
        /// </summary>
        private void DrawArrow(Point p, Cell dir, string name)
        {
            var pts = new List<Point>();

            pts.Add(p);
            var p2 = p + GetSize(dir, 2);
            pts.Add(p2);

            var v = new System.Windows.Vector(GetPixel(dir.Col), GetPixel(dir.Row));

            using (var pen = new Pen(Color.Black, 3))
            {
                pen.CustomEndCap = GetCapArrow();                
                g.DrawLine(pen, p, p2);
            }

            using (var fontFamily = new FontFamily("Times New Roman"))
            {
                int fontSize = 40;
                var format = new StringFormat();
                format.LineAlignment = StringAlignment.Center;
                if (name.Length < 5)
                {
                    format.Alignment = StringAlignment.Center;
                    fontSize = 60;
                }
                using (var font = new Font(fontFamily, fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                {
                    var solidBrush = Brushes.Black;
                    

                    var pName = Point.Add(p2, GetSize(dir, 2));
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    g.DrawString(name, font, solidBrush, pName, format);
                }
            }
        }

        private CustomLineCap GetCapArrow()
        {
            using (var capPath = new GraphicsPath())
            {
                // A triangle
                capPath.AddLine(-5, 0, 5, 0);
                capPath.AddLine(-5, 0, 0, 10);
                capPath.AddLine(0, 10, 5, 0);
                return new CustomLineCap(null, capPath);
            }
        }

        private void VisualLegend()
        {
            var p = new Point(10, yLegend+50);
            DrawArrow(p, Cell.Right, "Начало нумерации секций в пятне дома.");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AR_AreaZhuk.PIK1TableAdapters;

namespace AR_AreaZhuk
{
   public class DbController
    {
       public void UpdateFlats(bool isUpdateSections)
       {
           OpenFileDialog openFileDialog = new OpenFileDialog();

           openFileDialog.InitialDirectory = "c:\\";
           openFileDialog.Filter = "Файл с квартирами (*.xlsx)|*.xlsx";
           openFileDialog.RestoreDirectory = true;
           string excelPath = "";
           if (openFileDialog.ShowDialog() == DialogResult.OK)
               excelPath = openFileDialog.FileName;
           if (excelPath == "") return;
           Exporter.ExportFlatsToSQL(excelPath);//Обновляется таблица Flats
           FrameWork fw = new FrameWork();
           var roomInfo = fw.GetRoomData(excelPath);
           if (isUpdateSections)
           {

               Parallel.For(6, 11, (q) => Exporter.ExportSectionsToSQL(q * 4, "Рядовая", 9, false, false, roomInfo)); //Рядовая 2-9 с 6 по 10 этаж
               Parallel.For(8, 12, (q) => Exporter.ExportSectionsToSQL(q * 4, "Угловая лево", 9, true, false, roomInfo)); //Угловая лево 2-9
               Parallel.For(8, 12, (q) => Exporter.ExportSectionsToSQL(q * 4, "Угловая право", 9, false, true, roomInfo)); //Угловая право 2-9
               Parallel.For(7, 15, (q) => Exporter.ExportSectionsToSQL(q * 4, "Угловая лево", 18, true, false, roomInfo)); //Угловая лево 10-18
               Parallel.For(7, 15, (q) => Exporter.ExportSectionsToSQL(q * 4, "Угловая право", 18, false, true, roomInfo)); //Угловая право 10-18
               Parallel.For(7, 15, (q) => Exporter.ExportSectionsToSQL(q * 4, "Рядовая", 25, false, false, roomInfo)); //Рядовая 19-25
               Parallel.For(7, 15, (q) => Exporter.ExportSectionsToSQL(q * 4, "Рядовая", 18, false, false, roomInfo)); //Рядовая 10-18
           }
       }
    }
}

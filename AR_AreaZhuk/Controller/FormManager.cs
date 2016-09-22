using AR_Zhuk_DataModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Zuby.ADGV;

namespace AR_AreaZhuk
{
    public static class FormManager
    {
        public static void DataReqValidator(DataGridView dg)
        {
            var selectedCell = dg.SelectedCells[0];
            int iRow = selectedCell.RowIndex;
            int iCol = selectedCell.ColumnIndex;
            if (iCol == 1)
            {
                if (!Convert.ToString(dg[iCol, iRow].Value).Contains('-'))
                {
                    MessageBox.Show("Площадь должна задаваться диапазоном, например 30-35!", "");
                    dg[iCol, iRow].Value = "30-35";
                }
                else
                {
                    string[] val = Convert.ToString(dg[iCol, iRow].Value).Split('-');
                    if (val.Length != 2)
                    {
                        MessageBox.Show("Площадь должна задаваться диапазоном, например 30-35!", "");
                        dg[iCol, iRow].Value = "30-35";
                    }
                    int valid = 0;
                    for (int i = 0; i < val.Length; i++)
                    {
                        if (int.TryParse(val[0], out valid)) continue;
                        MessageBox.Show("Площадь должна задаваться диапазоном, например 30-35!", "");
                        dg[iCol, iRow].Value = "30-35";
                        break;

                    }
                }
            }
            else if (iCol > 1)
            {
                double valid = 0;
                if (!double.TryParse(Convert.ToString(dg[iCol, iRow].Value), out valid))
                {
                    if (!double.TryParse(Convert.ToString(dg[iCol, iRow].Value.ToString().Replace('.', ',')), out valid))
                    {
                        MessageBox.Show("Необходимо ввести цифру!", "");
                    }
                }
            }
        }

        public static void Panel_Show(Panel panel, Button btn, int minSize, int maxSize)
        {
            if (panel.Height == minSize)
            {
                for (int i = minSize; i <= maxSize; i++)
                {
                    System.Windows.Forms.Application.DoEvents();
                    panel.Height = i;
                    Thread.Sleep(1);
                }
                btn.Image = Properties.Resources.up;
            }
            else
            {
                for (int i = maxSize; i >= minSize; i--)
                {
                    System.Windows.Forms.Application.DoEvents();
                    panel.Height = i;
                    Thread.Sleep(1);
                }
                btn.Image = Properties.Resources.down;
            }
        }

        public static List<Requirment> GetSpotTaskFromDG(DataGridView dg)
        {
            var resReqs = new List<Requirment>();
            // bool isValid = true;
            for (int i = 0; i < dg.RowCount - 1; i++)
            {
                string[] parse = dg[1, i].Value.ToString().Split('-');
                double off = 0;
                 if (!double.TryParse(Convert.ToString(dg[3, i].Value), out off))
                    if (!double.TryParse(Convert.ToString(dg[3, i].Value.ToString().Replace('.',',')), out off))
                    { }
                string subZone = dg[0, i].Value.ToString();
                Requirment r = new Requirment();
                r.SubZone = subZone;
                r.UserSortIndex = i;

                r.Percentage = Convert.ToInt16(dg[2, i].Value);
                if (r.Percentage <= 0)
                    continue;
                r.OffSet = off;
                r.MinArea = Convert.ToInt16(parse[0]);
                r.MaxArea = Convert.ToInt16(parse[1]);
                if (r.SubZone.StartsWith("Ст"))
                    r.CodeZone = "01";
                else if (r.SubZone.StartsWith("Одно"))
                    r.CodeZone = "1";
                else if (r.SubZone.StartsWith("Дву"))
                    r.CodeZone = "2";
                else if (r.SubZone.StartsWith("Тр"))
                    r.CodeZone = "3";
                else if (r.SubZone.StartsWith("Ч"))
                    r.CodeZone = "4";
                r.NearPercentage = 0;
                resReqs.Add(r);
            }
            //  return isValid;
            return resReqs;
        }
        public static void ViewDataProcentage(DataGridView dg2, List<GeneralObject> ob, ProjectInfo sp)
        {
            //  if (spinfos.Count == 0) return;
            DataSet dataSet = new DataSet();
            BindingSource bs = new BindingSource();
            bs.DataSource = dataSet;
            DataTable dt = new DataTable();
            dt.Columns.Add("Продаваемая площадь (м2.)", typeof(Double));
            dt.Columns.Add("Жилая площадь (м2.)", typeof(Double));
            dt.Columns.Add("K1", typeof(Double));
            dt.Columns.Add("K2", typeof(Double));
            dt.Columns.Add("Общее кол-во секций (шт.)", typeof(Int16));
            dt.Columns.Add("Кол-во секций с 3 и 4 комн. кв. (шт.)", typeof(Int16));
            dt.Columns.Add("Кол-во одинаковых секций (шт.)", typeof(string));
            dt.Columns.Add("Кол-во квартир (шт.)", typeof(Int16));

            foreach (var rew in sp.requirments)
            {
                dt.Columns.Add(string.Format("{0} {1}-{2}м2 (%)", rew.SubZone, rew.MinArea, rew.MaxArea), typeof(Double));
            }
            dt.Columns.Add("GenObject", typeof(GeneralObject));
            int counter = 0;
            foreach (var ss in ob)
            {
                if (ss == null || ss.SpotInf == null)
                    continue;
                List<double> percent = new List<double>();
                ss.SpotInf.SortRequirmentsByUser();
                foreach (var s in ss.SpotInf.requirments)
                {
                    percent.Add(Math.Round(s.RealPercentage, 1));
                }
                object[] newrow = new object[9 + percent.Count];
                newrow[0] = Math.Round(ss.SpotInf.TotalStandartArea, 1);
                newrow[1] = Math.Round(ss.SpotInf.TotalLiveArea, 1);
                newrow[2] = Math.Round(ss.SpotInf.K1, 2);
                newrow[3] = Math.Round(ss.SpotInf.K2, 2);
                newrow[4] = ss.SpotInf.TotalSections;
                newrow[5] = ss.SpotInf.CountContainsSections;
                newrow[6] = ss.SpotInf.TypicalSections;
                newrow[7] = ss.SpotInf.TotalFlats;

                for (int i = 0; i < percent.Count; i++)
                {
                    newrow[8 + i] = percent[i];
                }
                newrow[8 + percent.Count] = ss;

                dt.Rows.Add(newrow);
                counter++;
            }

            dg2.DataSource = null;
            dg2.DataSource = dt;
            dg2.Columns[dg2.Columns.Count - 1].Visible = false;
        }
    }
}
﻿using AR_Zhuk_DataModel;
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

namespace AR_AreaZhuk
{
  public static  class FormManager
    {


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

      public static SpotInfo GetSpotTaskFromDG(SpotInfo spotInfo, DataGridView dg)
      {
          spotInfo.requirments = new List<Requirment>();
          for (int i = 0; i < dg.RowCount - 1; i++)
          {
             // double off = 0;//double.Parse(Convert.ToString(dg[3, i].Value),CultureInfo.InvariantCulture);
              
             // if (double.TryParse(Convert.ToString(dg[3, i].Value).Replace('.', ','), out tmp))
              string[] parse = dg[1, i].Value.ToString().Split('-');
              double off = XmlConvert.ToDouble(Convert.ToString(dg[3, i].Value));
              string subZone = dg[0, i].Value.ToString();
              Requirment r = new Requirment();
              r.SubZone = subZone;
              //var r = spotInfo.requirments.Where(x => x.SubZone.Equals())
              //    .Where(x => x.MinArea.ToString().Equals(parse[0]))
              //    .ToList()[0];
              r.Percentage =Convert.ToInt16(dg[2, i].Value);
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

              spotInfo.requirments.Add(r);
          }
          return spotInfo;
      }
      public static void ViewDataProcentage(DataGridView dg2,List<GeneralObject> ob,SpotInfo sp)
      {
        //  if (spinfos.Count == 0) return;
          DataSet dataSet = new DataSet();
          BindingSource bs = new BindingSource();
          bs.DataSource = dataSet;
          DataTable dt = new DataTable();
          dt.Columns.Add("Площадь (м2.)", typeof(Double));
          dt.Columns.Add("Общее кол-во секций (шт.)", typeof(Int16));
          dt.Columns.Add("Кол-во одинаковых секций (шт.)", typeof(string));
          dt.Columns.Add("Кол-во квартир (шт.)", typeof(Int16));

          foreach (var rew in sp.requirments)
          {
              dt.Columns.Add(string.Format("{0} {1}-{2}м2 (%)",rew.SubZone,rew.MinArea,rew.MaxArea), typeof(Double));
          }
          dt.Columns.Add("GenObject", typeof(GeneralObject));
      // ob =   ob.OrderBy(x => x.GUID).ToList();
        //  dt.Columns.Add("Студии 22-23м2 (%)", typeof(Double));
        ////  dt.Columns.Add("Студии 33-35м2 (%)", typeof(Double));
        //  dt.Columns.Add("Однокомн. 35-47м2 (%)", typeof(Double));
        //  dt.Columns.Add("Двухкомн. 45-47м2 (%)", typeof(Double));
        ////  dt.Columns.Add("Двухкомн. 53-56м2 (%)", typeof(Double));
        ////  dt.Columns.Add("Двухкомн. 68-70м2 (%)", typeof(Double));
        //  dt.Columns.Add("Трехкомн. 85-95м2 (%)", typeof(Double));
          //dt.Columns.Add("GUID", typeof(String));
          int counter = 0;
          foreach (var ss in ob)
          {
              if (ss == null||ss.SpotInf==null)
                  continue;
              List<double> percent = new List<double>();
              foreach (var s in ss.SpotInf.requirments)
              {
                  percent.Add(Math.Round(s.RealPercentage, 1));
              }
              object[] newrow = new object[5 + percent.Count];
              newrow[0] = Math.Round(ss.SpotInf.RealArea, 1);
              newrow[1] = ss.SpotInf.TotalSections;
              newrow[2] = ss.SpotInf.TypicalSections;
              newrow[3] = ss.SpotInf.TotalFlats;
              for (int i = 0; i < percent.Count; i++)
              {
                  newrow[4 + i] = percent[i];
              }
            //  newrow[4 + percent.Count] = ss.SpotInf.GUID;
              newrow[4 + percent.Count] = ss;
                //{
                //   // Math.Round(ss.RealArea,1),ss.TotalSections,ss.TypicalSections,ss.TotalFlats, percent[0], percent[1], percent[2], percent[3], percent[4], percent[5],
                //   Math.Round(ss.RealArea,1),ss.TotalSections,ss.TypicalSections,ss.TotalFlats, percent[0], percent[1], percent[2], percent[3], ss.GUID
                //};
              
              dt.Rows.Add(newrow);
              counter++;
          }

          dg2.DataSource = dt;
          dg2.Columns[dg2.Columns.Count - 1].Visible = false;
      }
    }
}

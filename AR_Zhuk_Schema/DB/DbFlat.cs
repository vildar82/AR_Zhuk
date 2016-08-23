using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_Zhuk_Schema.DB
{
    [Serializable]
    public class DbFlat
    { 
        public int ID_Section { get; set; }        
        public int CountModules { get; set; }        
        public string TypeSection { get; set; }
        public string Levels { get; set; }        
        public string TypeFlat { get; set; }        
        public int ID_Flat { get; set; }
        public int SelectedIndexBottom { get; set; }
        public int SelectedIndexTop { get; set; }
        public string ShortType { get; set; }        
        public double AreaLive { get; set; }        
        public double AreaTotalStandart { get; set; }
        public double AreaTotalStrong { get; set; }        
        public int AreaInModule { get; set; }
        public int Expr1 { get; set; }        
        public int Expr2 { get; set; }
        public string LinkageBefore { get; set; }        
        public string LinkageAfter { get; set; }        
        public string FactorSmoke { get; set; }        
        public string LightBottom { get; set; }        
        public string LightTop { get; set; }        
        public string IndexTop { get; set; }
        public string IndexBottom { get; set; }        
        public string SubZone { get; set; }        
        public int ID_FlatInSection { get; set; }

        public DbFlat () { }
        
        public DbFlat(SAPR.FlatsInSectionsRow dbFlatRow)
        {
            this.AreaInModule = dbFlatRow.AreaInModule;
            this.AreaLive = dbFlatRow.AreaLive;
            this.AreaTotalStandart = dbFlatRow.AreaTotalStandart;
            this.AreaTotalStrong = dbFlatRow.AreaTotalStrong;
            this.CountModules = dbFlatRow.CountModules;
            this.Expr1 = dbFlatRow.Expr1;
            this.Expr2 = dbFlatRow.Expr2;
            this.FactorSmoke = dbFlatRow.FactorSmoke;
            this.ID_Flat = dbFlatRow.ID_Flat;
            this.ID_FlatInSection = dbFlatRow.ID_FlatInSection;
            this.ID_Section = dbFlatRow.ID_Section;
            this.IndexBottom = dbFlatRow.IndexBottom;
            this.IndexTop = dbFlatRow.IndexTop;
            this.Levels = dbFlatRow.Levels;
            this.LightBottom = dbFlatRow.LightBottom;
            this.LightTop = dbFlatRow.LightTop;
            this.LinkageAfter = dbFlatRow.LinkageAfter;
            this.LinkageBefore = dbFlatRow.LinkageBefore;
            this.SelectedIndexBottom = dbFlatRow.SelectedIndexBottom;
            this.SelectedIndexTop = dbFlatRow.SelectedIndexTop;
            this.ShortType = dbFlatRow.ShortType;
            this.SubZone = dbFlatRow.SubZone;
            this.TypeFlat = dbFlatRow.TypeFlat;
            this.TypeSection = dbFlatRow.TypeSection;
        }

        public RoomInfo GetRoomInfo ()
        {
            RoomInfo ri = DBService.GetRoom(this);
            return ri;
        }

        public static List<DbFlat> GetFlats (List<SAPR.FlatsInSectionsRow> dbFlatsRows)
        {
            List<DbFlat> resFlats = new List<DbFlat>();
            foreach (var item in dbFlatsRows)
            {
                var flat = new DbFlat(item);
                resFlats.Add(flat);
            }
            return resFlats;
        }
    }
}

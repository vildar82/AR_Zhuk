using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AR_Zhuk_DataModel;
using AR_Zhuk_Schema.DB.SAPRTableAdapters;

namespace AR_Zhuk_Schema.DB
{
    public class DBService : IDBService
    {        
        /// <summary>
        /// Сохранение запрошенных секций - прошедших через фильтр требований
        /// </summary>
        private static Dictionary<string, List<FlatInfo>> dictSections;
        /// <summary>
        /// Словарь предварительно загруженных секций
        /// </summary>
        public static ConcurrentDictionary<SelectSectionParam, List<List<DbFlat>>> dictDbFlats = 
                    new ConcurrentDictionary<SelectSectionParam, List<List<DbFlat>>>();
        /// <summary>
        /// Виды секций в базе - по шагу, типу и этажности (Type, Levels, CountStep)
        /// </summary>
        private static List<SelectSectionParam> sectionsTypesInDb;                

        public DBService()
        {   
            // виды секций в базе            
            LoadDbFlatsFromFile();
            sectionsTypesInDb = GetSectionsTypesIndDb();
            dictSections = new Dictionary<string, List<FlatInfo>>();            
        }

        public DBService (object nul = null) { }   

        public List<FlatInfo> GetSections (Section section, SelectSectionParam selecSectParam)
        {
            List<FlatInfo> sectionsBySyze;
            string key = section.CountStep + selecSectParam.Type + selecSectParam.Levels;

            if (!dictSections.TryGetValue(key, out sectionsBySyze))
            {
                List<List<DbFlat>> flatsDbBySections;                
                if (!dictDbFlats.TryGetValue(selecSectParam, out flatsDbBySections))
                {
                    return null;
                }
                if (flatsDbBySections.Count == 0)
                {
                    return null;
                }

                sectionsBySyze = new List<FlatInfo>();                                
                FlatInfo fl;
                bool isValidSection = true;                
                foreach (var gg in flatsDbBySections)
                {
                    fl = new FlatInfo();

                    fl.IsDominant = section.IsDominant;
                    fl.ImageAngle = section.ImageAngle;
                    fl.ImageStart = section.ImageStart;
                    fl.Floors = section.Floors;
                    fl.CountStep = section.CountStep;
                    fl.Flats = new List<RoomInfo>();
                    fl.IsCorner = section.IsCorner;
                    fl.IsVertical = section.IsVertical;
                    fl.NumberInSpot = section.NumberInSpot;
                    fl.SpotOwner = section.SpotOwner;
                    isValidSection = true;
                    for (int i = 0; i < gg.Count; i++)
                    {
                        var f = gg[i];
                        fl.IdSection = f.ID_Section;

                        RoomInfo room = null;
                        if (f.SubZone != "0")
                        {
                            for (int r = 0; r < ProjectScheme.ProjectInfo.requirments.Count; r++)
                            {
                                var req = ProjectScheme.ProjectInfo.requirments[r];
                                if (req.CodeZone == f.SubZone &&
                                    f.AreaTotalStandart >= req.MinArea &&
                                    f.AreaTotalStandart < req.MaxArea)
                                {
                                    room = GetRoom(f);
                                    room.CodeReqIndex = r;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            room = GetRoom(f);
                        }

                        if (room == null)
                        {
                            isValidSection = false;
                            break;
                        }
                        fl.Flats.Add(room);
                    }

                    if (!isValidSection)
                        continue;

                    string err;
                    if (CheckSection(fl, out  err))
                    {
                        fl.DefineIdenticalCodeSection();
                        sectionsBySyze.Add(fl);
                    }
                    else
                    {
                      //  Trace.TraceWarning("Ошибочная секция - "+ err  + "; idSection = " + fl.IdSection);
                    }

                    //if (maxSectionBySize != 0 && sectionsBySyze.Count == maxSectionBySize)
                    //{
                    //    break;
                    //}
                }
                dictSections.Add(key, sectionsBySyze);
            }
            return sectionsBySyze;
        }

        private static bool CheckSection (FlatInfo fl, out string err)
        {            
            bool isValid = true;
            err = null;
            // Должно быть больше 3 квартир в секции (без ЛЛУ)
            if (fl.Flats.Count < 5)
            {
                isValid = false;
                err = "Меньше 4 квартир в секции (без ЛЛУ)";
            }
            // Должно быть не больше 10 квартир в секции (без ЛЛУ)
            else if (fl.Flats.Count > 11)
            {
                isValid = false;
                err = "Больше 10 квартир в секции (без ЛЛУ)";
            }
            // Студий должно быть не больше 3 в секции
            else if (fl.Flats.Count(w => w.SubZone=="01")>3)
            {
                isValid = false;
                err = "Больше 3 студий в секции";
            }
            return isValid;                
        }

        public static RoomInfo GetRoom (DbFlat f)
        {
            var fflat = new RoomInfo(f.ShortType, f.SubZone, f.TypeFlat, "",
                                        "", f.LinkageBefore, f.LinkageAfter, "", "", "", f.Levels, "", "", f.LightBottom, f.LightTop,
                                        "");
            fflat.AreaModules = f.AreaInModule;
            fflat.AreaTotal = f.AreaTotalStrong;
            fflat.AreaTotalStandart = f.AreaTotalStandart;
            fflat.SelectedIndexTop = f.SelectedIndexTop;
            fflat.SelectedIndexBottom = f.SelectedIndexBottom;
            return fflat;
        }        

        public void PrepareLoadSections (List<SelectSectionParam> selectSects)
        {
            // отбор типов секций которые не загружались
            var ssToLoad = selectSects.Where(s => !dictDbFlats.ContainsKey(s) &&
                            sectionsTypesInDb.Contains(s)).ToList();
            if (ssToLoad.Count > 0)
            {
                // Паралельная загрузка секций   
                foreach (var item in ssToLoad)
                {
                    dictDbFlats.TryAdd(item, LoadFromDbSection(item));
                }
                //Parallel.ForEach(ssToLoad, (s) => dictDbFlats.TryAdd(s, LoadFromDbSection(s)));
            }
        }

        private List<List<DbFlat>> LoadFromDbSection (SelectSectionParam selectSectParam)
        {
            List<SAPR.FlatsInSectionsRow> flatsDbRows;
            FlatsInSectionsTableAdapter flatsIsSection = new FlatsInSectionsTableAdapter();
            if (ProjectScheme.MaxSectionBySize == 0)
            {
                flatsDbRows = flatsIsSection.GetFlatsInTypeSection(selectSectParam.Step,
                            selectSectParam.Type, selectSectParam.Levels).ToList();
            }
            else
            {
                flatsDbRows = flatsIsSection.GetFlatsInTypeSectionMax(ProjectScheme.MaxSectionBySize,
                            selectSectParam.Step, selectSectParam.Type, selectSectParam.Levels).ToList();
                // отсекаем первые и последние квартиры секции (она может быть неполной)                
                if (flatsDbRows.Count == ProjectScheme.MaxSectionBySize)
                {
                    //flatsDb = flatsDb.OrderBy(x => x.ID_FlatInSection).ToList();
                    var lastDbFlat = flatsDbRows.Last();
                    var idSectionLast = lastDbFlat.ID_Section;
                    do
                    {
                        flatsDbRows.Remove(lastDbFlat);
                        lastDbFlat = flatsDbRows.Last();
                    } while (lastDbFlat.ID_Section== idSectionLast);

                    var firstDbFlat = flatsDbRows.First();
                    var idSectionFirst = firstDbFlat.ID_Section;
                    do
                    {
                        flatsDbRows.Remove(firstDbFlat);
                        firstDbFlat = flatsDbRows.First();
                    } while (firstDbFlat.ID_Section == idSectionFirst);
                }
            }
            var dbFlats = DbFlat.GetFlats(flatsDbRows);
            var res = GetDbFlatsBySections(dbFlats);
            return res; 
        }

        private List<List<DbFlat>> GetDbFlatsBySections (List<DbFlat> dbFlats)
        {
            var res = dbFlats.OrderBy(x => x.ID_FlatInSection).GroupBy(x => x.ID_Section).Select(x => x.ToList()).ToList();
            return res;
        }

        private List<SelectSectionParam> GetSectionsTypesIndDb ()
        {
            List<SelectSectionParam> resSectTypesInDb = new List<SelectSectionParam>();
            vil_SectionsTypesTableAdapter adapterSectTypes = new vil_SectionsTypesTableAdapter();
            var selRes = adapterSectTypes.GetSectionTypes();
            foreach (var item in selRes)
            {
                SelectSectionParam selSectParam = new SelectSectionParam(item.CountModules, item.Type, item.Levels);
                resSectTypesInDb.Add(selSectParam);
            }
            return resSectTypesInDb;
        }

        public void SaveDbFlatsToFile ()
        {
            List<XmlDbFlats> xmlDbFlats = new List<XmlDbFlats>();

            var sectTypes = GetSectionsTypesIndDb();
            foreach (var secType in sectTypes)
            {
                var dbFlats = LoadFromDbSection(secType);                
                xmlDbFlats.Add(new XmlDbFlats { SelectParam = secType, DbFlats = dbFlats });                
            }

            try
            {
                using (Stream stream = File.Open("data.bin", FileMode.Create))
                {                    
                    using (BinaryWriter binWriter = new BinaryWriter(stream))
                    {
                        binWriter.Write(xmlDbFlats.Count);

                        foreach (var xmlFlat in xmlDbFlats)
                        {
                            binWriter.Write(xmlFlat.SelectParam.Levels);
                            binWriter.Write(xmlFlat.SelectParam.Step);
                            binWriter.Write(xmlFlat.SelectParam.Type);
                            binWriter.Write(xmlFlat.DbFlats.Count);
                            foreach (var sect in xmlFlat.DbFlats)
                            {
                                binWriter.Write(sect.Count);
                                foreach (var flat in sect)
                                {
                                    binWriter.Write(flat.AreaInModule);
                                    binWriter.Write(flat.AreaLive);
                                    binWriter.Write(flat.AreaTotalStandart);
                                    binWriter.Write(flat.AreaTotalStrong);
                                    binWriter.Write(flat.CountModules);
                                    binWriter.Write(flat.Expr1);
                                    binWriter.Write(flat.Expr2);
                                    binWriter.Write(flat.FactorSmoke);
                                    binWriter.Write(flat.ID_Flat);
                                    binWriter.Write(flat.ID_FlatInSection);
                                    binWriter.Write(flat.ID_Section);
                                    binWriter.Write(flat.IndexBottom);
                                    binWriter.Write(flat.IndexTop);
                                    binWriter.Write(flat.Levels);
                                    binWriter.Write(flat.LightBottom);
                                    binWriter.Write(flat.LightTop);
                                    binWriter.Write(flat.LinkageAfter);
                                    binWriter.Write(flat.LinkageBefore);
                                    binWriter.Write(flat.SelectedIndexBottom);
                                    binWriter.Write(flat.SelectedIndexTop);
                                    binWriter.Write(flat.ShortType);
                                    binWriter.Write(flat.SubZone);
                                    binWriter.Write(flat.TypeFlat);
                                    binWriter.Write(flat.TypeSection);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public void LoadDbFlatsFromFile ()
        {
            try
            {
                using (Stream stream = File.Open("data.bin", FileMode.Open))
                {
                    var xmlDbFlats = new List<XmlDbFlats>();
                    using (var binaryReader = new BinaryReader(stream))
                    {
                        int countXmlFlats = binaryReader.ReadInt32();
                        for (int i = 0; i < countXmlFlats; i++)
                        {
                            XmlDbFlats xmlFlat = new XmlDbFlats();
                            xmlFlat.SelectParam = new SelectSectionParam();
                            xmlFlat.SelectParam.Levels = binaryReader.ReadString();
                            xmlFlat.SelectParam.Step = binaryReader.ReadInt32();
                            xmlFlat.SelectParam.Type = binaryReader.ReadString();
                            xmlFlat.DbFlats = new List<List<DbFlat>>();
                            var countSects =  binaryReader.ReadInt32();
                            for (int s = 0; s < countSects; s++)
                            {
                                List<DbFlat> sect = new List<DbFlat>();
                                var countFlats = binaryReader.ReadInt32();
                                for (int f = 0; f < countFlats; f++)
                                {
                                    DbFlat flat = new DbFlat();
                                    flat.AreaInModule = binaryReader.ReadInt32();
                                    flat.AreaLive = binaryReader.ReadDouble();
                                    flat.AreaTotalStandart = binaryReader.ReadDouble();
                                    flat.AreaTotalStrong = binaryReader.ReadDouble();
                                    flat.CountModules = binaryReader.ReadInt32();
                                    flat.Expr1 = binaryReader.ReadInt32();
                                    flat.Expr2 = binaryReader.ReadInt32();
                                    flat.FactorSmoke = binaryReader.ReadString();
                                    flat.ID_Flat = binaryReader.ReadInt32();
                                    flat.ID_FlatInSection = binaryReader.ReadInt32();
                                    flat.ID_Section = binaryReader.ReadInt32();
                                    flat.IndexBottom = binaryReader.ReadString();
                                    flat.IndexTop = binaryReader.ReadString();
                                    flat.Levels = binaryReader.ReadString();
                                    flat.LightBottom = binaryReader.ReadString();
                                    flat.LightTop = binaryReader.ReadString();
                                    flat.LinkageAfter = binaryReader.ReadString();
                                    flat.LinkageBefore = binaryReader.ReadString();
                                    flat.SelectedIndexBottom = binaryReader.ReadInt32();
                                    flat.SelectedIndexTop = binaryReader.ReadInt32();
                                    flat.ShortType = binaryReader.ReadString();
                                    flat.SubZone = binaryReader.ReadString();
                                    flat.TypeFlat = binaryReader.ReadString();
                                    flat.TypeSection = binaryReader.ReadString();
                                    sect.Add(flat);
                                }
                                xmlFlat.DbFlats.Add(sect);
                            }
                            xmlDbFlats.Add(xmlFlat);
                        }
                    }
                    foreach (var xmlDbFlat in xmlDbFlats)
                    {
                        dictDbFlats.TryAdd(xmlDbFlat.SelectParam, xmlDbFlat.DbFlats);
                    }
                }
            }
            catch { }
        }

        public void ResetSections ()
        {   
            dictSections = new Dictionary<string, List<FlatInfo>>();            
        }
    }    

    [Serializable]
    public class SelectSectionParam : IEquatable<SelectSectionParam>, IComparable<SelectSectionParam>
    {
        public int Step { get; set; }
        public string Type { get; set; }
        public string Levels { get; set; }

        public SelectSectionParam () { }

        public SelectSectionParam(int step, string type, string levels)
        {
            Step = step;
            Type = type;
            Levels = levels;
        }

        public bool Equals (SelectSectionParam other)
        {
            return Step == other.Step && Type == other.Type && Levels == other.Levels;
        }

        public override int GetHashCode ()
        {
            return Step.GetHashCode() ^ Type.GetHashCode() ^ Levels.GetHashCode();
        }

        public int CompareTo (SelectSectionParam other)
        {
            var res = Type.CompareTo(other.Type);
            if (res != 0) return res;

            res = Levels.CompareTo(other.Levels);
            if (res != 0) return res;

            res = Step.CompareTo(other.Step);
            return res;
        }

        public override string ToString ()
        {
            return Type + "_" + Levels + "_" + Step;
        }
    }
}
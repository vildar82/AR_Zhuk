using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;
using AR_Zhuk_Schema.DB.SAPRTableAdapters;

namespace AR_Zhuk_Schema.DB
{
    public class DBService : IDBService
    {
        private static Dictionary<string, List<FlatInfo>> dictSections = new Dictionary<string, List<FlatInfo>>();
        private static ConcurrentDictionary<SelectSectionParam, List<AR_Zhuk_Schema.DB.SAPR.FlatsInSectionsRow>> dictDbFlats = 
                    new ConcurrentDictionary<SelectSectionParam, List<AR_Zhuk_Schema.DB.SAPR.FlatsInSectionsRow>>();
        

        SpotInfo sp;
        int maxSectionBySize;        

        public DBService(SpotInfo sp, int maxSectionBySize)
        {
            this.sp = sp;
            this.maxSectionBySize = maxSectionBySize;
        }

        public List<FlatInfo> GetSections (Section section, SelectSectionParam selecSectParam)
        {
            List<FlatInfo> sectionsBySyze;
            string key = section.CountStep + selecSectParam.Type + selecSectParam.Levels;

            if (!dictSections.TryGetValue(key, out sectionsBySyze))
            {
                List<AR_Zhuk_Schema.DB.SAPR.FlatsInSectionsRow> flatsDb;                
                if (!dictDbFlats.TryGetValue(selecSectParam, out flatsDb))
                {
                    Debug.Assert(false, "Загрузка из базы!!!!");
                    flatsDb = LoadFromDbSection(selecSectParam);
                    dictDbFlats.TryAdd(selecSectParam, flatsDb);
                }

                if (flatsDb.Count == 0)
                {
                    return null;
                }

                sectionsBySyze = new List<FlatInfo>();                
                flatsDb = flatsDb.OrderBy(x => x.ID_FlatInSection).ToList();                
                FlatInfo fl;
                bool isValidSection = true;
                var sections = flatsDb.GroupBy(x => x.ID_Section).Select(x => x.ToList()).ToList();                
                foreach (var gg in sections)
                {
                    fl = new FlatInfo();

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
                    bool isContains = false;
                    for (int i = 0; i < gg.Count; i++)
                    {
                        var f = gg[i];
                        fl.IdSection = f.ID_Section;
                        isContains = false;
                        if (!f.SubZone.Equals("0"))
                        {
                            isValidSection = false;
                            foreach (var r in sp.requirments.Where(x => x.CodeZone.Equals(f.SubZone)).ToList())
                            {
                                if (!(r.MinArea - 4 <= f.AreaTotalStandart & r.MaxArea + 4 >= f.AreaTotalStandart))
                                    continue;
                                isContains = true;
                                break;
                            }

                            if (!isContains)
                            {
                                isValidSection = false;
                                break;
                            }
                        }

                        var fflat = new RoomInfo(f.ShortType, f.SubZone, f.TypeFlat, "",
                            "", f.LinkageBefore, f.LinkageAfter, "", "", "", f.Levels, "", "", f.LightBottom, f.LightTop,
                            "");
                        fflat.AreaModules = f.AreaInModule;
                        fflat.AreaTotal = f.AreaTotalStrong;
                        fflat.AreaTotalStandart = f.AreaTotalStandart;
                        fflat.SelectedIndexTop = f.SelectedIndexTop;
                        fflat.SelectedIndexBottom = f.SelectedIndexBottom;
                        fl.Flats.Add(fflat);
                        
                        if (!isValidSection)
                            continue;

                    }
                    if (!isContains)
                        continue;
                    
                    sectionsBySyze.Add(fl);                    

                    if (maxSectionBySize != 0 && sectionsBySyze.Count == maxSectionBySize)
                    {
                        break;
                    }
                }
                dictSections.Add(key, sectionsBySyze);
            }
            return sectionsBySyze;
        }

        public void PrepareLoadSections (List<SelectSectionParam> selectSects)
        {
            // отбор типов секций которые не загружались
            var notInDictSS = selectSects.Where(s => !dictDbFlats.ContainsKey(s)).ToList();
            if (notInDictSS.Count > 0)
            {
                // Паралельная загрузка секций   
                foreach (var item in notInDictSS)
                {
                    try
                    {
                        dictDbFlats.TryAdd(item, LoadFromDbSection(item));
                    }
                    catch (Exception ex)
                    {
                        string err = ex.Message;
                    }
                }
                //Parallel.ForEach(notInDictSS, (s) => dictDbFlats.TryAdd(s, LoadFromDbSection(s)));
            }
        }

        private List<AR_Zhuk_Schema.DB.SAPR.FlatsInSectionsRow> LoadFromDbSection (SelectSectionParam selectSectParam)
        {
            List<AR_Zhuk_Schema.DB.SAPR.FlatsInSectionsRow> flatsDb;
            FlatsInSectionsTableAdapter flatsIsSection = new FlatsInSectionsTableAdapter();
            if (maxSectionBySize == 0)
            {
                flatsDb = flatsIsSection.GetFlatsInTypeSection(selectSectParam.Step,
                            selectSectParam.Type, selectSectParam.Levels).ToList();
            }
            else
            {
                flatsDb = flatsIsSection.GetFlatsInTypeSectionMax(maxSectionBySize,
                            selectSectParam.Step, selectSectParam.Type, selectSectParam.Levels).ToList();
                // отсекаем первые и последние квартиры секции (она может быть неполной)                
                if (flatsDb.Count == maxSectionBySize)
                {
                    //flatsDb = flatsDb.OrderBy(x => x.ID_FlatInSection).ToList();
                    var lastDbFlat = flatsDb.Last();
                    var idSectionLast = lastDbFlat.ID_Section;
                    do
                    {
                        flatsDb.Remove(lastDbFlat);
                        lastDbFlat = flatsDb.Last();
                    } while (lastDbFlat.ID_Section== idSectionLast);

                    var firstDbFlat = flatsDb.First();
                    var idSectionFirst = firstDbFlat.ID_Section;
                    do
                    {
                        flatsDb.Remove(firstDbFlat);
                        firstDbFlat = flatsDb.First();
                    } while (firstDbFlat.ID_Section == idSectionFirst);
                }
            }
            return flatsDb;            
        }
    }

    public class SelectSectionParam : IEquatable<SelectSectionParam>
    {
        public readonly int Step;
        public readonly string Type;
        public readonly string Levels;

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
    }
}
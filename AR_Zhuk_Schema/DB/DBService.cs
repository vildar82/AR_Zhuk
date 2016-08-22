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
        /// <summary>
        /// Сохранение запрошенных секций - прошедших через фильтр требований
        /// </summary>
        private static Dictionary<string, List<FlatInfo>> dictSections = new Dictionary<string, List<FlatInfo>>();
        /// <summary>
        /// Словарь предварительно загруженных секций
        /// </summary>
        private static ConcurrentDictionary<SelectSectionParam, List<SAPR.FlatsInSectionsRow>> dictDbFlats = 
                    new ConcurrentDictionary<SelectSectionParam, List<SAPR.FlatsInSectionsRow>>();
        /// <summary>
        /// Виды секций в базе - по шагу, типу и этажности (Type, Levels, CountStep)
        /// </summary>
        private static List<SelectSectionParam> sectionsTypesInDb;

        SpotInfo sp;
        int maxSectionBySize;        

        public DBService(SpotInfo sp, int maxSectionBySize)
        {
            this.sp = sp;
            this.maxSectionBySize = maxSectionBySize;
            // виды секций в базе
            sectionsTypesInDb = GetSectionsTypesIndDb();
        }        

        public List<FlatInfo> GetSections (Section section, SelectSectionParam selecSectParam)
        {
            List<FlatInfo> sectionsBySyze;
            string key = section.CountStep + selecSectParam.Type + selecSectParam.Levels;

            if (!dictSections.TryGetValue(key, out sectionsBySyze))
            {
                List<SAPR.FlatsInSectionsRow> flatsDb;                
                if (!dictDbFlats.TryGetValue(selecSectParam, out flatsDb))
                {
                    return null;
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
                    bool isContains = false;
                    for (int i = 0; i < gg.Count; i++)
                    {
                        var f = gg[i];
                        fl.IdSection = f.ID_Section;                        
#if TEST
                        isContains = true;
                        isValidSection = false;
#else
                        isContains = false;
                        if (!f.SubZone.Equals("0"))
                        {
                            isValidSection = false;

                            foreach (var r in sp.requirments.Where(x => x.CodeZone.Equals(f.SubZone)).ToList())
                            {
                                if (!(r.MinArea<= f.AreaTotalStandart & r.MaxArea > f.AreaTotalStandart))
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
#endif
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
                    
                    if (fl.Flats.Count>3)
                        sectionsBySyze.Add(fl);
                    else
                    {
                        Trace.TraceWarning("Секция меньше 3 квартир, idSection = " + fl.IdSection);
                    }                   

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

        private List<SAPR.FlatsInSectionsRow> LoadFromDbSection (SelectSectionParam selectSectParam)
        {
            List<SAPR.FlatsInSectionsRow> flatsDb;
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
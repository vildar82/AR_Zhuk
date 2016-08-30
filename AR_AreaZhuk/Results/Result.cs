using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Xml.Serialization;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Results
{
    class Result
    {
        private const string Extension = ".bet";
        private BinaryWriter writer;
        private BinaryReader reader;
        private Dictionary<string, PIK1.C_Flats_PIK1Row> dictDbFlats;

        public void Save (List<GeneralObject> gos, SpotInfo sp, List<HouseOptions> options, int dominantOffSet)
        {
            if (gos == null || gos.Count == 0)
                return;
            // файл для сохранения расчета
            var fileResult = GetFileResult(sp);            
            if (string.IsNullOrEmpty(fileResult))
                return;
            if (File.Exists(fileResult))
                File.Delete(fileResult);

            using (ZipArchive archive = ZipFile.Open(fileResult, ZipArchiveMode.Create))
            {
                var entryGos = archive.CreateEntry("gos");                                
                using (writer = new BinaryWriter(entryGos.Open()))
                {
                    WriteOptions(options);
                    writer.Write(dominantOffSet);
                    WriteSpotInfo(sp);
                    WriteInsModules(sp.InsModulesAll);
                    writer.Write(gos.Count);
                    foreach (var go in gos)
                    {
                        writer.Write(go.GUID ?? "");
                        writer.Write(go.Houses.Count);
                        foreach (var house in go.Houses)
                        {
                            WriteSections(house.Sections);
                            //WriteSpotInfo(house.SpotInf);
                        }
                        WriteSpotInfo(go.SpotInf);
                    }
                }                
            }            
        }        

        public List<GeneralObject> Load(PIK1.C_Flats_PIK1DataTable dbFlats, out SpotInfo sp, 
            out List<HouseOptions> options, out int dominantOffSet)
        {            
            List<GeneralObject> gos = new List<GeneralObject>();
            sp = null;

            dictDbFlats = new Dictionary<string, PIK1.C_Flats_PIK1Row>();
            foreach (var dbFlat in dbFlats)
            {
                if (!dictDbFlats.ContainsKey(dbFlat.Type))
                {
                    dictDbFlats.Add(dbFlat.Type, dbFlat);
                }
            }            

            var fileResult = PromptFileResult();          
            using (ZipArchive zip = ZipFile.OpenRead(fileResult))
            {   
                using (var stream = zip.GetEntry("gos").Open())
                {
                    using (reader = new BinaryReader(stream))
                    {
                        options = ReadOptions();
                        dominantOffSet = reader.ReadInt32();
                        sp = ReadSpotInfo();
                        sp.InsModulesAll = ReadInsModules();
                        var countGos = reader.ReadInt32();
                        for (int i=0; i<countGos; i++)
                        {
                            var go = new GeneralObject();
                            go.GUID = reader.ReadString();
                            go.Houses = new List<HouseInfo>();
                            var countHouse = reader.ReadInt32();
                            for (int h=0; h<countHouse; h++)
                            {
                                var hi = new HouseInfo();
                                hi.Sections = ReadSections();
                                //hi.SpotInf = ReadSpotInfo();                                
                                go.Houses.Add(hi);
                            }
                            go.SpotInf = ReadSpotInfo();
                            go.SpotInf.InsModulesAll = sp.InsModulesAll;
                            gos.Add(go);
                        }
                    }
                }
            }            
            return gos;
        }

        private string PromptFileResult ()
        {
            string fileRes = null;
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.AddExtension = true;
            openDialog.CheckFileExists = true;
            openDialog.CheckPathExists = true;
            openDialog.DefaultExt = Extension;
            openDialog.RestoreDirectory = true;
            openDialog.Title = "Загрузка файла расчета";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                fileRes = openDialog.FileName;                
            }

            return fileRes;
        }

        private string GetFileResult (SpotInfo sp)
        {
            var schema = Path.GetFileNameWithoutExtension(sp.PathInsolation);
            string fileRes = "Жуки_Расчет_" + schema + DateTime.Now.ToString("dd.MM.yyyy HH.mm") + Extension;
            SaveFileDialog saveFiledialog = new SaveFileDialog();
            saveFiledialog.AddExtension = false;
            saveFiledialog.CheckPathExists = true;
            saveFiledialog.DefaultExt = "";
            saveFiledialog.FileName = fileRes;
            saveFiledialog.RestoreDirectory = true;
            saveFiledialog.Title = "Сохранение расчета";
            if (saveFiledialog.ShowDialog() == DialogResult.OK)
            {
                return saveFiledialog.FileName;
            }
            return null;
        }

        private void WriteOptions (List<HouseOptions> options)
        {
            writer.Write(options.Count);
            foreach (var opt in options)
            {
                writer.Write(opt.CountFloorsDominant);
                writer.Write(opt.CountFloorsMain);
                writer.Write(opt.DominantPositions.Count);
                foreach (var dom in opt.DominantPositions)
                {
                    writer.Write(dom);
                }
                writer.Write(opt.HouseName);
            }
        }
        private List<HouseOptions> ReadOptions ()
        {
            List<HouseOptions> options = new List<HouseOptions>();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                HouseOptions opt = new HouseOptions();
                opt.CountFloorsDominant = reader.ReadInt32();
                opt.CountFloorsMain = reader.ReadInt32();
                var countDom = reader.ReadInt32();
                opt.DominantPositions = new List<bool>();
                for (int d = 0; d < countDom; d++)
                {
                    opt.DominantPositions.Add(reader.ReadBoolean());
                }
                opt.HouseName = reader.ReadString();
                options.Add(opt);
            }
            return options;
        }

        private void WriteSections (List<FlatInfo> sections)
        {
            writer.Write(sections.Count);
            foreach (var sect in sections)
            {                
                writer.Write(sect.Area);
                writer.Write(sect.Code);                
                writer.Write(sect.CountStep);
                WriteFlats(sect.Flats);
                writer.Write(sect.Floors);
                writer.Write(sect.IdSection);
                writer.Write(sect.ImageAngle);
                WriteCell(sect.ImageStart);
                writer.Write(sect.IsCorner);
                writer.Write(sect.IsDominant);
                writer.Write(sect.IsInvert);
                writer.Write(sect.IsVertical);
                writer.Write(sect.NumberInSpot);
                writer.Write(sect.SpotOwner);                
            }
        }
        private List<FlatInfo> ReadSections ()
        {
            List<FlatInfo> sections = new List<FlatInfo>();
            var count = reader.ReadInt32();
            for (int i =0; i< count; i++)
            {
                var sect = new FlatInfo();
                sect.Area = reader.ReadDouble();
                sect.Code = reader.ReadString();                
                sect.CountStep = reader.ReadInt32();
                sect.Flats = ReadFlats();
                sect.Floors = reader.ReadInt32();
                sect.IdSection = reader.ReadInt32();
                sect.ImageAngle = reader.ReadInt32();
                sect.ImageStart = ReadCell();
                sect.IsCorner = reader.ReadBoolean();
                sect.IsDominant = reader.ReadBoolean();
                sect.IsInvert = reader.ReadBoolean();
                sect.IsVertical = reader.ReadBoolean();
                sect.NumberInSpot = reader.ReadInt32();
                sect.SpotOwner = reader.ReadString();
                sections.Add(sect);
            }
            return sections;
        }

        private void WriteFlats (List<RoomInfo> flats)
        {
            writer.Write(flats.Count);
            foreach (var flat in flats)
            {
                writer.Write(flat.Type);                                                                                 
            }
        }
        private List<RoomInfo> ReadFlats ()
        {
            List<RoomInfo> flats = new List<RoomInfo>();
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var flat = new RoomInfo();
                var type = reader.ReadString();
                PIK1.C_Flats_PIK1Row dbFlat;
                if (dictDbFlats.TryGetValue(type, out dbFlat))
                {
                    flat.Type = type;
                    flat.AreaLive = dbFlat.AreaLive;
                    flat.AreaModules = dbFlat.AreaInModule;
                    flat.AreaTotal = dbFlat.AreaTotalStandart;
                    flat.FactorSmoke = dbFlat.FactorSmoke;
                    flat.ShortType = dbFlat.ShortType;
                    flat.SubZone = dbFlat.SubZone;
                    flat.SelectedIndexBottom =Convert.ToInt32(dbFlat.IndexBottom);
                    flat.SelectedIndexTop = Convert.ToInt32(dbFlat.IndexTop);                    
                }
                flats.Add(flat);
            }
            return flats;
        }

        private void WriteSpotInfo (SpotInfo sp)
        {            
            writer.Write(sp.CountContainsSections);
            writer.Write(sp.GUID?? "");
            writer.Write(sp.K1);
            writer.Write(sp.K2);
            writer.Write(sp.LevelArea);
            WriteRequirements(sp.requirments);
            WriteCell(sp.Size);
            writer.Write(sp.SpotArea);
            writer.Write(sp.TotalArea);
            writer.Write(sp.TotalFlats);
            writer.Write(sp.TotalLiveArea);
            writer.Write(sp.TotalSections);
            writer.Write(sp.TotalStandartArea);
            writer.Write(sp.TypicalSections ?? "");
        }
        private SpotInfo ReadSpotInfo ()
        {
            var sp = new SpotInfo();
            sp.CountContainsSections = reader.ReadInt32();
            sp.GUID = reader.ReadString();
            sp.K1 = reader.ReadDouble();
            sp.K2 = reader.ReadDouble();            
            sp.LevelArea = reader.ReadDouble();
            sp.requirments = ReadRequirements();
            sp.Size = ReadCell();
            sp.SpotArea = reader.ReadDouble();
            sp.TotalArea = reader.ReadDouble();
            sp.TotalFlats = reader.ReadInt32();
            sp.TotalLiveArea = reader.ReadDouble();
            sp.TotalSections = reader.ReadInt32();
            sp.TotalStandartArea = reader.ReadDouble();
            sp.TypicalSections = reader.ReadString();
            return sp;
        }

        private void WriteRequirements (List<Requirment> requirements)
        {
            writer.Write(requirements.Count);
            foreach (var r in requirements)
            {
                writer.Write(r.CodeZone);
                writer.Write(r.CountFlats);
                writer.Write(r.MaxArea);
                writer.Write(r.MaxCountFlat);
                writer.Write(r.MinArea);
                writer.Write(r.MinCountFlat);
                writer.Write(r.NearPercentage);
                writer.Write(r.OffSet);
                writer.Write(r.Percentage);
                writer.Write(r.RealCountFlats);
                writer.Write(r.RealPercentage);
                writer.Write(r.SubZone);                
            }
        }
        private List<Requirment> ReadRequirements ()
        {
            List<Requirment> requirements = new List<Requirment>();
            var count = reader.ReadInt32();
            for (int i=0; i<count; i++)
            {
                var r = new Requirment();
                r.CodeZone = reader.ReadString();
                r.CountFlats = reader.ReadInt32();
                r.MaxArea = reader.ReadInt32();
                r.MaxCountFlat = reader.ReadInt32();
                r.MinArea = reader.ReadInt32();
                r.MinCountFlat = reader.ReadInt32();
                r.NearPercentage = reader.ReadDouble();
                r.OffSet = reader.ReadDouble();
                r.Percentage = reader.ReadInt32();
                r.RealCountFlats = reader.ReadInt32();
                r.RealPercentage = reader.ReadDouble();
                r.SubZone = reader.ReadString();
                requirements.Add(r);
            }
            return requirements;
        }

        private void WriteCell (Cell cell)
        {
            writer.Write(cell.Row);
            writer.Write(cell.Col);
        }
        private Cell ReadCell ()
        {
            Cell cell = new Cell(reader.ReadInt32(), reader.ReadInt32());
            return cell;            
        }

        private void WriteInsModules (List<Module> insModulesAll)
        {
            writer.Write(insModulesAll.Count);
            foreach (var module in insModulesAll)
            {
                WriteCell(module.Cell);
                writer.Write(module.InsValue);
                writer.Write(module.Length);
            }
        }
        private List<Module> ReadInsModules ()
        {
            List<Module> insModulesAll = new List<Module>();
            var count = reader.ReadInt32();            
            for (int i=0; i<count; i++)
            {
                var module = new Module();
                module.Cell = ReadCell();
                module.InsValue = reader.ReadString();
                module.Length = reader.ReadDouble();
                insModulesAll.Add(module);
            }
            return insModulesAll;
        }
    }
}

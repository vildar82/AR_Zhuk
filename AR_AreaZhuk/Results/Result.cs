using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Results
{
    class Result
    {
        private const string Extension = "bin";
        private BinaryWriter writer;
        public void Save (List<GeneralObject> gos, SpotInfo sp)
        {
            if (gos == null || gos.Count == 0)
                return;
            // файл для сохранения расчета
            var fileResult = GetFileResult(gos);
            if (string.IsNullOrEmpty(fileResult))
                return;

            using (Stream stream = File.Open(fileResult, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    WriteSpotInfo(sp);
                    WriteInsModules(sp.InsModulesAll);
                    writer.Write(gos.Count);
                    foreach (var go in gos)
                    {                       
                        writer.Write(go.GUID);                        
                        writer.Write(go.Houses.Count);                        
                        foreach (var house in go.Houses)
                        {
                            WriteSection(house.Sections);                            
                        }
                        WriteSpotInfo(go.SpotInf);
                    }
                }
            }
        }

        public List<GeneralObject> Load(out SpotInfo sp)
        {
            List<GeneralObject> gos = new List<GeneralObject>();
            sp = null;

            var fileResult = PromptFileResult();
            using (Stream stream = File.Open(fileResult, FileMode.Open))
            {                
                using (var binaryReader = new BinaryReader(stream))
                {
                    int countXmlFlats = binaryReader.ReadInt32();
                    
                        
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

        private string GetFileResult (List<GeneralObject> gos)
        {
            var schema = Path.GetFileNameWithoutExtension(gos[0].SpotInf.PathInsolation);
            string fileRes = "Жуки_Расчет_" + schema + "." + Extension;
            SaveFileDialog saveFiledialog = new SaveFileDialog();
            saveFiledialog.AddExtension = true;
            saveFiledialog.CheckPathExists = true;
            saveFiledialog.DefaultExt = Extension;
            saveFiledialog.FileName = fileRes;
            saveFiledialog.RestoreDirectory = true;
            saveFiledialog.Title = "Сохранение расчета";
            if (saveFiledialog.ShowDialog() == DialogResult.OK)
            {
                return saveFiledialog.FileName;
            }
            return null;
        }

        private void WriteSection (List<FlatInfo> sections)
        {
            writer.Write(sections.Count);
            foreach (var sect in sections)
            {                
                writer.Write(sect.Area);
                writer.Write(sect.Code);
                writer.Write(sect.CountFlats);
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

        private void WriteFlats (List<RoomInfo> flats)
        {
            writer.Write(flats.Count);
            foreach (var flat in flats)
            {
                writer.Write(flat.Type);
                writer.Write(flat.AreaLive);                
                writer.Write(flat.AreaTotal);                                                                
            }
        }

        private void WriteSpotInfo (SpotInfo sp)
        {            
            writer.Write(sp.CountContainsSections);
            writer.Write(sp.GUID);
            writer.Write(sp.K1);
            writer.Write(sp.K2);
            writer.Write(sp.LevelArea);
            WriteRequirements(sp);
            WriteCell(sp.Size);
            writer.Write(sp.SpotArea);
            writer.Write(sp.TotalArea);
            writer.Write(sp.TotalFlats);
            writer.Write(sp.TotalLiveArea);
            writer.Write(sp.TotalSections);
            writer.Write(sp.TotalStandartArea);
            writer.Write(sp.TypicalSections);
        }

        private void WriteRequirements (SpotInfo sp)
        {
            writer.Write(sp.requirments.Count);
            foreach (var r in sp.requirments)
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

        private void WriteCell (Cell cell)
        {
            writer.Write(cell.Row);
            writer.Write(cell.Col);
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

        
    }
}

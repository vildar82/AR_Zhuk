using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AR_AreaZhuk.Model;
using AR_AreaZhuk.PIK1TableAdapters;
using AR_Zhuk_DataModel;
using System.Drawing.Imaging;
using AR_Zhuk_Schema;
using AR_AreaZhuk.Controller;

namespace AR_AreaZhuk
{
    public partial class MainForm : Form
    {
        public static Thread th;
        private static bool isEvent = false;
        private bool isStop = false;
        public PIK1.C_Flats_PIK1_AreasDataTable flatsAreas = new PIK1.C_Flats_PIK1_AreasDataTable();
        public PIK1.C_Flats_PIK1DataTable dbFlats = new PIK1.C_Flats_PIK1DataTable();
        BindingSource bs = new BindingSource();
        public static List<ProjectInfo> spinfos = new List<ProjectInfo>();                        
        public static ProjectInfo projectInfo;
        public static List<List<HouseInfo>> houses = new List<List<HouseInfo>>();
        public static List<GeneralObject> ob = new List<GeneralObject>();
        double maxArea = 0;
        public MainForm()
        {
            InitializeComponent();
            btnMenuGroup1.Image = Properties.Resources.up;
            btnMenuGroup2.Image = Properties.Resources.up;
            btnMenuGroup3.Image = Properties.Resources.up;

            C_Flats_PIK1_AreasTableAdapter pikFlats = new C_Flats_PIK1_AreasTableAdapter();
            flatsAreas = pikFlats.GetData();
            C_Flats_PIK1TableAdapter flatsTableAdapter = new C_Flats_PIK1TableAdapter();
            dbFlats = flatsTableAdapter.GetData();
        }        

        public void ViewProgress()
        {
            FormProgress fp = new FormProgress();
            fp.ShowDialog();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Экспорт в базу, если нужно что-то конкретное залить в базу            
            //Export();

            // Если нужно обновить базу квартир
            //if (Environment.UserName.Equals("khisyametdinovvt") | Environment.UserName.Equals("ostaninam") | Environment.UserName.Equals("inkinli"))
            //{
            //    UpdateDbFlats.Visible = true;
            //    chkUpdateSections.Visible = true;
            //}

            // Загрузка projectInfo - из конфига, или дефолт
            projectInfo = LoadSpotInfo();
            // Заполнение контролов настройками spotInfo
            FillSpotInfoControls(projectInfo);
            // ??                        
            isEvent = true;
        }

        private static void Export ()
        {
            FrameWork fw = new FrameWork();
            string excelPath = @"E:\__ROM_Типы квартир.xlsx";
            var roomInfo = fw.GetRoomData(excelPath);
            projectInfo = fw.GetDefaultSpotInfo();
            Exporter.ExportSectionsToSQL(8 * 4, "Рядовая", 9, false, false, roomInfo);//Если нужно залить 1 тип секции
            Environment.Exit(48);
        }

        //public void GetAllSectionPercentage(List<List<HouseInfo>> listSections, Requirment requirment)
        //{
        //    int counter = 0;
        //    int allCounter = 0;
        //    bool isContinue = true;
        //    houses = listSections;
        //    ParallelOptions po = new ParallelOptions();
        //    po.MaxDegreeOfParallelism = 15;
        //    Parallel.For(0, houses[0].Count, po, GetGeneralObjects);
        //}

        private void GetGeneralObjects(int index)
        {
            var house1 = houses[0][index];
            var spot1 = house1.SpotInf;
            for (int j = 0; j < houses[1].Count; j++)
            {
                var house2 = houses[1][j];                

                var spot2 = house2.SpotInf;
                int allCountFlats = 0;
                for (int k = 0; k < projectInfo.requirments.Count; k++)
                {
                    allCountFlats += spot1.requirments[k].RealCountFlats;
                    allCountFlats += spot2.requirments[k].RealCountFlats;
                }                
                ProjectInfo spGo = projectInfo.Copy();
                // dg2.Rows.Add();                
                for (int k = 0; k < projectInfo.requirments.Count; k++)
                {
                    int currentCountFlats = spot1.requirments[k].RealCountFlats;
                    currentCountFlats += spot2.requirments[k].RealCountFlats;
                    double percentOb = Convert.ToDouble(currentCountFlats) / Convert.ToDouble(allCountFlats) * 100;
                    spGo.requirments[k].RealCountFlats = currentCountFlats;
                    spGo.requirments[k].RealPercentage = percentOb;
                }

                spGo.TotalFlats = allCountFlats;

                spinfos.Add(spGo);
                int countS = house1.Sections.Count + house2.Sections.Count;
                int[] ss = new int[countS];
                for (int i = 0; i < house1.Sections.Count; i++)
                {
                    ss[i] = house1.Sections[i].IdSection;
                }
                for (int i = 0; i < house2.Sections.Count; i++)
                {
                    ss[i + house1.Sections.Count] = house2.Sections[i].IdSection;
                }
                string typicalSect = "";
                int countSovp = countS - ss.GroupBy(x => x).ToList().Count + 1;
                for (int i = 0; i < ss.Length; i++)
                {

                    int v = ss[i];
                    if (v == 0)
                        continue;
                    int t = 1;

                    for (int m = i + 1; m < ss.Length; m++)
                    {
                        if (v != ss[m]) continue;
                        t++;
                        ss[m] = 0;
                    }
                    if (t == 1) continue;
                    typicalSect += t + ";";
                }
                if (!string.IsNullOrEmpty(typicalSect))
                    typicalSect = typicalSect.Remove(typicalSect.Length - 1, 1);
                else typicalSect = "0";

                GeneralObject go = new GeneralObject();
                spGo.TotalSections = countS;
                spGo.TypicalSections = typicalSect;
                go.SpotInf = spGo;
                double area = GetTotalArea(house1);
                area += GetTotalArea(house2);
                go.Houses.Add(house1);
                go.Houses.Add(house2);
                go.SpotInf.TotalStandartArea = area;
                //go.GUID = guid;
                ob.Add(go);
            }
            //}
        }

        private double GetTotalArea(HouseInfo house1)
        {
            double totalArea = 0;

            foreach (var section in house1.Sections)
            {
                double sectionArea = 0;
                foreach (var f in section.Flats)
                {
                    if (section.Floors == 25)
                    {
                        sectionArea += 9 * f.AreaTotal;
                        sectionArea += 15 * f.AreaTotalStandart;
                    }
                    else
                    {
                        sectionArea += (section.Floors - 1) * f.AreaTotalStandart;
                    }
                }
                totalArea += sectionArea;
                section.Area = sectionArea;
            }
            return totalArea;
        }        

        public void GetHousePercentage(ref HouseInfo houseInfo)
        {
            var sp1 = projectInfo.Copy();
            for (int k = 0; k < houseInfo.Sections.Count; k++) //Квартиры
            {
                FlatInfo section = houseInfo.Sections[k];                
                for (int l = 0; l < section.Flats.Count; l++) //Квартиры
                {
                    if (section.Flats[l].SubZone.Equals("0")) continue;
                    var reqs =
                        sp1.requirments.Where(
                            x => x.CodeZone.Equals(section.Flats[l].SubZone))
                            .Where(x => x.MaxArea > section.Flats[l].AreaTotal & x.MinArea <= section.Flats[l].AreaTotal)
                            .ToList();
                    if (reqs.Count == 0) continue;
                    reqs[0].RealCountFlats += section.Floors - 1;

                }
            }
            int countFlats = 0;
            foreach (var r in sp1.requirments)
            {
                countFlats += r.RealCountFlats;
            }
            foreach (var r in sp1.requirments)
            {
                double percentage = r.RealCountFlats * 100 / countFlats;
                r.RealPercentage = percentage;
            }
            houseInfo.SpotInf = sp1;
        }

        private void btnViewPercentsge_Click (object sender, EventArgs e)
        {
            spinfos.Clear();
            ob.Clear();
            projectInfo.requirments = FormManager.GetSpotTaskFromDG(dg);
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = 15;
            Parallel.For(0, houses[0].Count, po, GetGeneralObjects);
            //  GetGeneralObjects();
            FormManager.ViewDataProcentage(dg2, ob, projectInfo);
            lblCountObjects.Text = ob.Count.ToString();
        }

        /// <summary>
        /// Сохранение результатов расчета в файл
        /// </summary>        
        private void btnSave_Click(object sender, EventArgs e)
        {
            //List<string> guids = (from DataGridViewRow row in dg2.SelectedRows select dg2[dg2.Columns.Count - 1, row.Index].Value.ToString()).ToList();
            //foreach (var g in guids)
            //{
            //    GeneralObject go = ob.First(x => x != null && x.SpotInf.GUID.Equals(g));
            //    if (go == null) break;
            //    Serializer ser = new Serializer();
            //    ser.SerializeList(go, go.SpotInf.TotalStandartArea + "m2 (" + go.SpotInf.TotalFlats.ToString() + ")");
            //}
            Results.Result result = new Results.Result();
            try
            {
                result.Save(ob, projectInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения результатов расчета.\n\r" + ex.Message);
            }            
        }
        
        private void dg_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == dg.RowCount - 1) return;
            if (!isEvent) return;


            FormManager.DataReqValidator(dg);
            int per = 0;

            for (int i = 0; i < dg.RowCount - 1; i++)
            {
                int p = 0;
                int.TryParse(Convert.ToString(dg[2, i].Value), out p);
                per += p;
                string[] deep = Convert.ToString(dg[1, i].Value).Split('-');
                if (deep.Length == 1)
                    continue;
                string subZone = Convert.ToString(dg[0, i].Value);

                if (subZone.StartsWith("Ст"))
                    subZone = "01";
                else if (subZone.StartsWith("Одно"))
                    subZone = "1";
                else if (subZone.StartsWith("Дву"))
                    subZone = "2";
                else if (subZone.StartsWith("Тр"))
                    subZone = "3";
                else if (subZone.StartsWith("Ч"))
                    subZone = "4";
                // Вильдар. 23.08.2016. Площадь квартиры должна быть меньше максимальной требуемой площади. Убрал <=, оставил <.
                var flats =
                dbFlats.Where(x => x.SubZone.Equals(subZone)).ToList().
                Where(x => x.AreaTotalStrong >= Convert.ToInt16(deep[0]) & x.AreaTotalStrong < Convert.ToInt16(deep[1])).ToList();
                dg[4, i].Value = flats.Count;
            }
            dg[2, dg.RowCount - 1].Value = per;
            dg[2, dg.RowCount - 1].Style.BackColor = Color.White;
            if (per != 100)
            {
                MessageBox.Show("Сумма указанных значений не равна 100!", "Ошибка!!!");
                dg[2, dg.RowCount - 1].Style.BackColor = Color.Red;
            }            
        }

        private void btnStartScan_Click(object sender, EventArgs e)
        {
            isStop = false;
            maxArea = 0;
            ob = new List<GeneralObject>();
            lblCountObjects.Text = ob.Count.ToString();
            // Обнулить Label кол.секци.
            SetInfoTotalSectionsCount(null);

            isEvent = false;
            //btnStartScan.Enabled = false;
            btnViewPercentsge.Enabled = true;

            // считывание настоек из контролов            
            projectInfo = GetProjectInfoFromControls();            
            // сохранение настроек            
            Serializer s = new Serializer();
            s.SerializeSpoinfo(projectInfo);

            // Сортировка требований квартирографии для расчета (начиная с минимально процентажа)            
            projectInfo.SortRequirmentsForCalculate();

            // Сброс ближайшего процентажа
            ResetNearPercentage();
            
            ProjectScheme profectShema = new ProjectScheme(projectInfo);
            profectShema.ReadScheme();
            th = new Thread(ViewProgress);
            th.Start();

            List<List<HouseInfo>> totalObject = profectShema.GetTotalHouses(
                int.Parse(textBoxMaxCountSectionsBySize.Text), int.Parse(textBoxMaxCountHousesBySpot.Text));

            Debug.WriteLine("Кол секций по totalObject = " + totalObject.Sum(t => t.Sum(c => c.SectionsBySize.Sum(f => f.Sections.Count))));

            SetInfoTotalSectionsCount(totalObject);
            
            if (totalObject.Count == 0)
            {
                isStop = true;
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            int[] selectedHouse = new int[totalObject.Count];
            int[] nearReqPercentage = new int[projectInfo.requirments.Count];

            // Перебор вариантов домов
            do
            {
                if (isStop)
                    break;

                Debug.WriteLine("Вариант дома = " + selectedHouse.Aggregate(string.Empty, (u, i) => u + i.ToString()));

                List<List<FlatInfo>> sections;
                //Получение cекций из домов                
                if (!GetHouseSections(selectedHouse, totalObject,out sections))
                    continue;

                Debug.WriteLine("Размерность секций sections = " + sections.Aggregate(string.Empty, (u, i) => u + "." + i.Count.ToString()));

                //Группировка и сортировка секций
                List<CodeSection> codeSections = GetSectionsByCode(sections);

                Debug.WriteLine("Размерность секций по кол. квартир = " + codeSections.Aggregate(string.Empty, (u, i) => u + "." + i.SectionsByCountFlats.Count));
                Debug.WriteLine("Размерность секций по кол. кодов = " + codeSections.Aggregate(string.Empty, (u, i) => u + "." + "[" + i.SectionsByCountFlats.Aggregate(string.Empty, (n, j) => n + "." + j.SectionsByCode.Count) + "]"));

                int[] selectedSectByCountFlats = new int[codeSections.Count]; //Выбранная размерность секции по кол-ву квартир
                int[] selectedSectCode = new int[codeSections.Count]; //Выбранный код секций                
                                
                //Обход сформированных секций с уникальными кодами на объект
                // Перебор размерностей дома (selectedSectByCountFlats)
                do
                {
                    if (isStop)
                        break;
                    int totalCountFlats = 0;
                    //Общее кол-во квартир в размерности
                    for (int i = 0; i < sections.Count; i++)
                    {
                        totalCountFlats += codeSections[i].SectionsByCountFlats[selectedSectByCountFlats[i]].CountFlats;
                    }

                    // Перебор кодов секций (в заданной размерности) - selectedSectCode
                    do
                    {                        
                        Debug.WriteLine("\n\rselectedSectSize = " + selectedSectByCountFlats.Aggregate(string.Empty,
                            (u, i) => u + "." + i.ToString()));
                        Debug.WriteLine("selectedSectCode = " + selectedSectCode.Aggregate(string.Empty,
                            (u, i) => u + "." + i.ToString()));

                        Application.DoEvents();
                        if (isStop)
                            break;
                        bool isValidPercentage = true;
                        string strP = string.Empty;
                        for (int q = 0; q < projectInfo.requirments.Count; q++)
                        {
                            var rr = projectInfo.requirments[q];
                            int countFlats = 0;
                            for (int i = 0; i < codeSections.Count; i++)
                            {
                                countFlats += codeSections[i].SectionsByCountFlats[selectedSectByCountFlats[i]].
                                    SectionsByCode[selectedSectCode[i]].CountFlatsByCode[q];                                    
                            }
                            //Процентаж определенного типа квартир в объекте
                            double percentage = countFlats * 100d / totalCountFlats;

                            // Ближайший процентаж квартиры - если текущий процент квартиры ближе к требуемому, то записываем его как ближайший
                            double arround = Math.Abs(percentage - rr.Percentage);
                            double arround2 = Math.Abs(rr.NearPercentage - rr.Percentage);
                            if (arround < arround2 || rr.NearPercentage == 0)
                                rr.NearPercentage = Math.Round(percentage, 1);
                            // проверка процентажа квартиры
                            if (arround <= rr.OffSet)
                            {
                                isValidPercentage = true;
                                strP += (Math.Round(percentage, 0)).ToString() + ";";
                            }
                            else
                            {
                                isValidPercentage = false;
                                break;
                            }
                        }
                        if (isValidPercentage)  //Процентаж прошел
                        {                          
                            // Сбор секции прошедшего варианта  
                            var successGOs = GetSuccesGeneralObjects(codeSections, selectedSectByCountFlats, selectedSectCode, strP);
                            ob.AddRange(successGOs);
                            lblCountObjects.Text = ob.Count.ToString();
                        }

                    } while (IncrementSectionCode(sections.Count - 1, selectedSectCode, codeSections, selectedSectByCountFlats));

                } while (IncrementSectionSize(sections.Count - 1, selectedSectByCountFlats, codeSections));

            } while (IncrementSelectedHouse(selectedHouse.Length - 1, selectedHouse, totalObject));

            // Сортировка квартирографии по заданному пользователем порядку            
            projectInfo.SortRequirmentsByUser();

            FormManager.ViewDataProcentage(dg2, ob, projectInfo);
            for (int q = 0; q < projectInfo.requirments.Count; q++)
            {
                dg[5, q].Value = projectInfo.requirments[q].NearPercentage;
            }
            th.Abort();
            lblCountObjects.Text = ob.Count.ToString();
            isEvent = true;
            sw.Stop();
            //MessageBox.Show((sw.ElapsedMilliseconds / 1000).ToString());
            lblTime.Visible = true;
            lblTime.Text = (sw.ElapsedMilliseconds / 1000).ToString();
            bs.DataSource = dg2.DataSource;
            lblMaxArea.Text = maxArea.ToString();
            lblTotalCount.Text = ob.Count.ToString();
            //  this.pb.Image = global::AR_AreaZhuk.Properties.Resources.объект;

            // Показать сообщения если они есть.
            AR_Zhuk_DataModel.Messages.Informer.Show();
        }

        private List<GeneralObject> GetSuccesGeneralObjects (List<CodeSection> codeSections, 
            int[] selectedSectByCountFlats, int[] selectedSectCode, string strP)
        {
            List<GeneralObject> successGOs = new List<GeneralObject>();
            //сбор секций
            List<Code> listCodes = new List<Code>();
            for (int i = 0; i < codeSections.Count; i++)
                listCodes.Add(codeSections[i].SectionsByCountFlats[selectedSectByCountFlats[i]].SectionsByCode[selectedSectCode[i]]);

            //сортровка по пятнам и очередности расположения
            listCodes = listCodes.OrderBy(x => x.SpotOwner).ThenBy(x => x.NumberSection).ToList();
                        
            int[] indexSelectedId = new int[listCodes.Count];

            //isContinue = true;
            int countSectionsIndex = listCodes.Count - 1;

            string[] strPercent = strP.Split(';');
            // Перебор id секций
            do
            {
                GeneralObject go = new GeneralObject();

                var sections = new List<FlatInfo>();
                int countFlats = 0;
                double totalArea = 0;
                double liveArea = 0;

                int countContainsLargeFlatSections = 0;
                double k1 = 0;
                double k2 = 0;
                for (int j = 0; j <= countSectionsIndex; j++)
                {
                    double levelArea = 0;
                    double levelAreaOffLLU = 0;
                    double levelAreaOnLLU = 0;

                    var sec = listCodes[j].IdSections[indexSelectedId[j]];
                    sections.Add(sec);
                    if (sec.Flats.Any(x => x.SubZone.Equals("3")) ||
                        sec.Flats.Any(x => x.SubZone.Equals("4")))
                        countContainsLargeFlatSections++;
                    foreach (var flat in sec.Flats)
                    {
                        var currentFlatAreas = flatsAreas.First(x => x.Short_Type.Equals(flat.ShortType));
                        var areas = Calculate.GetAreaFlat(sec.Floors, flat, currentFlatAreas);
                        totalArea += areas[0];
                        liveArea += areas[1];
                        levelArea += areas[2];
                        levelAreaOffLLU += areas[3];
                        levelAreaOnLLU += areas[4];
                    }
                    k1 += levelAreaOffLLU / levelArea;
                    k2 += levelAreaOffLLU / levelAreaOnLLU;

                    countFlats += listCodes[j].CountFlats;
                }
                k1 = k1 / (countSectionsIndex + 1);
                k2 = k2 / (countSectionsIndex + 1);
                
                List<HouseInfo> housesInSpot = new List<HouseInfo>();
                foreach (var house in sections.GroupBy(g=>g.SpotOwner))
                {
                    HouseInfo h = new HouseInfo();                    
                    h.Sections.AddRange(house);                    
                    housesInSpot.Add(h);
                }
                ProjectInfo spGo = projectInfo.Copy();
                spGo.CountContainsSections = countContainsLargeFlatSections;
                spGo.K1 = k1;
                spGo.K2 = k2;
                spGo.TotalStandartArea = totalArea;
                spGo.TotalLiveArea = liveArea;
                spGo.TotalFlats = countFlats;
                spGo.TypicalSections = GetCountTypicalSections(sections);

                for (int k = 0; k < projectInfo.requirments.Count; k++)
                    spGo.requirments[k].RealPercentage = Convert.ToInt16(strPercent[k]);
                go.Houses = housesInSpot;
                go.SpotInf = spGo;
                successGOs.Add(go);
                if (maxArea < totalArea)
                    maxArea = totalArea;
                Application.DoEvents();
            } while (IncrementIdSection(countSectionsIndex, indexSelectedId, listCodes));
            return successGOs;
        }        

        private static List<CodeSection> GetSectionsByCode(List<List<FlatInfo>> sections)
        {
            List<CodeSection> codeSections = new List<CodeSection>();
            foreach (var sectionsInPlace in sections.OrderBy(x => x.Count))
            {                
                CodeSection codeSection = new CodeSection();
                codeSection.CountFloors = sectionsInPlace[0].Floors;

                //Группировка по коду
                List<Code> codes = sectionsInPlace.OrderByDescending(x => x.CountFlats).GroupBy(g => g.Code).
                    OrderByDescending(o=>o.Key).
                    Select(g =>
                    {
                                var firstSect = g.First();
                                var code = new Code(g.Key, g.ToList(),
                                    (codeSection.CountFloors - 1) * (firstSect.CountFlats - 1),
                                    firstSect.NumberInSpot, firstSect.SpotOwner, codeSection.CountFloors);
                                return code;
                    }).ToList();
                //Группировка  по кол-ву квартир в секции
                codeSection.SectionsByCountFlats = codes.GroupBy(g => g.CountFlats).OrderByDescending(o => o.Key).
                    Select(g =>
                    {
                        var flatsInSection = new FlatsInSection();
                        flatsInSection.CountFlats = g.Key;
                        flatsInSection.SectionsByCode = g.ToList();
                        return flatsInSection;
                    }).ToList();                
                
                codeSections.Add(codeSection);                
            }
            return codeSections;
        }

        private static string GetCountTypicalSections(int[] idSections)
        {
            string typicalSect = "";
            for (int k = 0; k < idSections.Length; k++)
            {
                int v = idSections[k];
                if (v == 0)
                    continue;
                int t = 1;

                for (int m = k + 1; m < idSections.Length; m++)
                {
                    if (v != idSections[m]) continue;
                    t++;
                    idSections[m] = 0;
                }
                if (t == 1) continue;
                typicalSect += t + ";";
            }
            if (!string.IsNullOrEmpty(typicalSect))
                typicalSect = typicalSect.Remove(typicalSect.Length - 1, 1);
            else typicalSect = "0";
            return typicalSect;
        }

        private bool GetHouseSections(int[] selectedHouse, List<List<HouseInfo>> totalObject,out List<List<FlatInfo>> sections)
        {
            sections = new List<List<FlatInfo>>();
            for (int i = 0; i < selectedHouse.Length; i++)
            {
                sections.AddRange(totalObject[i][selectedHouse[i]].SectionsBySize.Select(s => s.Sections));
            }
            if (projectInfo.IsEnableDominantsOffset)
            {
                // Все доминанты (их шаги)
                List<int> dominantsStep = sections.Where(s => s[0].IsDominant).Select(s => s[0].CountStep).ToList();
                if (dominantsStep.Count > 1)
                {
                    if (dominantsStep.Max() - dominantsStep.Min() > projectInfo.DominantOffSet)
                    {
                        return false;
                    }
                }
            }            
            return true;
        }

        public bool IncrementSelectedHouse(int index, int[] houses, List<List<HouseInfo>> totalObject)
        {
            bool res = true;
            if (index == -1)
            {
                res = false;
            }
            else
            {                
                houses[index]++;
                if (houses[index] >= totalObject[index].Count)
                {
                    houses[index] = 0;
                    res = IncrementSelectedHouse(index - 1, houses, totalObject);
                }
            }
            return res;  
        }

        public bool IncrementIdSection(int index, int[] selectedIdSection, List<Code> codes)
        {
            bool res = true;
            if (index == -1)
            {
                res = false;
            }
            else
            {                
                selectedIdSection[index]++;
                if (selectedIdSection[index] >= codes[index].IdSections.Count)
                {
                    selectedIdSection[index] = 0;
                    res = IncrementIdSection(index - 1, selectedIdSection, codes);
                }
            }
            return res;
        }

        private bool IncrementSectionCode (int index, int[] selectedSectCode, List<CodeSection> codeSections, int[] selectedSectByCountFlats)
        {
            bool res = true;
            if (index ==-1)
            {                
                res = false;
            }
            else
            {
                selectedSectCode[index]++;
                if (selectedSectCode[index] == codeSections[index].SectionsByCountFlats[selectedSectByCountFlats[index]].SectionsByCode.Count)
                {
                    selectedSectCode[index] = 0;                    
                    res = IncrementSectionCode(index - 1, selectedSectCode, codeSections, selectedSectByCountFlats);                    
                }
            }
            return res;
        }

        private bool IncrementSectionSize (int index, int[] selectedSectByCountFlats, List<CodeSection> codeSections)
        {
            bool res = true;
            if (index ==-1)
            {
                res = false;
            }
            else
            {
                selectedSectByCountFlats[index]++;
                if (selectedSectByCountFlats[index] == codeSections[index].SectionsByCountFlats.Count)
                {
                    selectedSectByCountFlats[index] = 0;                                        
                    res = IncrementSectionSize(index - 1, selectedSectByCountFlats, codeSections);                    
                }
            }
            return res;
        }                

        private void pb_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) return;
            FormPreviewImage v = new FormPreviewImage(pb.Image);
            v.Show();
        }

        private void dg2_SortStringChanged(object sender, EventArgs e)
        {
            isEvent = false;
            BindingSource bs = new BindingSource();
            bs.DataSource = dg2.DataSource;
            bs.Sort = dg2.SortString;
            dg2.DataSource = bs;
            lblCountObjects.Text = dg2.RowCount.ToString();
            isEvent = true;
        }

        private void dg2_FilterStringChanged(object sender, EventArgs e)
        {
            isEvent = false;

            bs.Filter = dg2.FilterString;
            dg2.DataSource = bs;
            lblCountObjects.Text = dg2.RowCount.ToString();
            isEvent = true;
        }

        private void chkDominantOffset_CheckedChanged(object sender, EventArgs e)
        {
            txtOffsetDominants.Enabled = chkDominantOffset.Checked;
            projectInfo.IsEnableDominantsOffset = chkDominantOffset.Checked;
            projectInfo.DominantOffSet = Convert.ToInt32(txtOffsetDominants.Text);
        }

        private void dg2_SelectionChanged(object sender, EventArgs e)
        {
            if (!isEvent)
                return;

            GeneralObject go = (GeneralObject)dg2["GenObject", dg2.SelectedRows[0].Index].Value;
            if (go == null) return;
            if (go.Image == null)
            {
                string imagePath = @"\\dsk2.picompany.ru\project\CAD_Settings\Revit_server\13. Settings\02_RoomManager\00_PNG_ПИК1\";
                string ExcelDataPath = @"\\dsk2.picompany.ru\project\CAD_Settings\Revit_server\13. Settings\02_RoomManager\БД_Параметрические данные квартир ПИК1.xlsx";

                BeetlyVisualisation.ImageCombiner imgComb = new BeetlyVisualisation.ImageCombiner(go, ExcelDataPath, imagePath, 72);
                //Serializer ser = new Serializer();
                //ser.SerializeList(go, Guid.NewGuid().ToString());//Создание xml
                go.Image = imgComb.generateGeneralObject();
            }
            pb.Image = go.Image;
        }

        private void txtOffsetDominants_ValueChanged(object sender, EventArgs e)
        {
            projectInfo.DominantOffSet = Convert.ToInt32(txtOffsetDominants.Value);
        }

        private void btnMenuGroup1_Click_1(object sender, EventArgs e)
        {
            FormManager.Panel_Show(pnlMenuGroup1, btnMenuGroup1, 25, 315);
        }

        private void btnMenuGroup2_Click(object sender, EventArgs e)
        {
            FormManager.Panel_Show(pnlMenuGroup2, btnMenuGroup2, 25, 260);
        }

        private void btnMenuGroup3_Click(object sender, EventArgs e)
        {
            FormManager.Panel_Show(pnlMenuGroup3, btnMenuGroup3, 25, 273);
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Картинка (*.png)|*.png";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pb.Image.Save(dialog.FileName, ImageFormat.Png);
            }
        }

        private void chkEnableDominant_CheckedChanged(object sender, EventArgs e)
        {
            numDomCountFloor.Enabled = chkEnableDominant.Checked;            
        }

        private void GetFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string initDirectory = "c:\\";
            if (!string.IsNullOrWhiteSpace(projectInfo.PathInsolation))
                initDirectory = Path.GetDirectoryName(projectInfo.PathInsolation);
            openFileDialog.InitialDirectory = initDirectory;
            openFileDialog.Filter = "Файл задание инсоляции (*.xlsx)|*.xlsx";
            openFileDialog.RestoreDirectory = true;
            string path = "";            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog.FileName;
            }            
            SetLinkFileInsolation(path);                    
        }

        private void SetLinkFileInsolation (string path)
        {            
            if (File.Exists(path))
            {
                labelInsolationFile.Links.Clear();
                labelInsolationFile.Text = Path.GetFileNameWithoutExtension(path);
                LinkLabel.Link link = new LinkLabel.Link();
                link.LinkData = path;
                labelInsolationFile.Links.Add(link);
                labelInsolationFile.LinkClicked += (o, l) => Process.Start(l.Link.LinkData as string);

                btnStartScan.Enabled = true;

                projectInfo.PathInsolation = path;
            }                     
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            isEvent = false;
            if (dg.SelectedCells[0].RowIndex == dg.RowCount - 1)
                return;
            dg.Rows.Insert(dg.SelectedCells[0].RowIndex+1, 1);
            for (int i = 0; i < dg.Columns.Count; i++)
            {
                dg[i, dg.RowCount - 1].Value = null;
                dg.Rows[dg.RowCount - 1].ReadOnly = false;
            }

            isEvent = true;
            FillDgReqsTotal();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (dg.SelectedCells[0].RowIndex == dg.RowCount - 1)
                return;
            dg.Rows.RemoveAt(dg.SelectedCells[0].RowIndex);
        }

        private void dg_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == dg.RowCount - 1) return;
        }

        private void UpdateDbFlats_Click(object sender, EventArgs e)
        {
            DbController dbController = new DbController();
            dbController.UpdateFlats(chkUpdateSections.Checked);

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            isStop = true;
        }

        private void dg_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {

        }

        private void SetInfoTotalSectionsCount (List<List<HouseInfo>> totalObject)
        {
            int count = 0;
            string housesCount = "";
            if (totalObject != null)
            {
                count = totalObject.Sum(t => t.Sum(h => h.SectionsBySize.Sum(s => s.Sections.Count)));
                housesCount = string.Join(",", totalObject.Select(s => s.Count > 0 ? s[0].SectionsBySize[0].SpotOwner + " " + s.Count : ""));
            }
            labelCountSectionsTotal.Text = "Кол. секций: " + count;
            toolTip1.SetToolTip(labelCountSectionsTotal, housesCount);
        }

        private void dg2_Click (object sender, EventArgs e)
        {
            if (dg2.SelectedRows.Count>0)
                dg2_SelectionChanged(sender, e);
        }

        private void btnLoad_Click (object sender, EventArgs e)
        {            
            try
            {
                Results.Result res = new Results.Result();
                ProjectInfo pi;
                var gos = res.Load(dbFlats, out pi);
                if (gos.Count == 0)
                    return;
                
                // Заполнение DataGrid домов
                isEvent = false;
                FormManager.ViewDataProcentage(dg2, gos, pi);
                isEvent = true;
                // Запись требований
                FillSpotInfoControls(pi);                                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить результаты.\n\r" + ex.Message);
                return;
            }            
        }

        private void ResetNearPercentage ()
        {
            for (int i = 0; i < dg.Rows.Count; i++)
            {
                dg[5, i].Value = 0;
            }            
        }

        private ProjectInfo LoadSpotInfo ()
        {
            var fw = new FrameWork();
            Serializer s = new Serializer();
            var spotInfo = s.LoadSpotInfoFromFile();
            if (spotInfo == null)
            {
                spotInfo = fw.GetDefaultSpotInfo();
            }
            return spotInfo;
        }

        private void FillSpotInfoControls (ProjectInfo pi)
        {
            // Файл инсоляции
            SetLinkFileInsolation(pi.PathInsolation);

            //
            // Квартирография
            //
            FillDgReq(pi);
            FillDgReqsTotal();

            //
            // Условия
            //
            // Пятна
            FillSpotControls(pi.SpotOptions);
            // Разность доминант                        
            txtOffsetDominants.Value = pi.DominantOffSet;
            chkDominantOffset.Checked = pi.IsEnableDominantsOffset;                            
            // Этажночсти
            numMainCountFloor.Value = pi.CountFloorsMain;
            numDomCountFloor.Value = pi.CountFloorsDominant;
            chkEnableDominant.Checked = pi.IsEnabledDominant;
        }

        private void FillDgReq (ProjectInfo sp)
        {
            dg.Rows.Clear();
            foreach (var r in sp.requirments)
            {
                dg.Rows.Add();
                dg[0, dg.RowCount - 1].Value = r.SubZone;
                dg[1, dg.RowCount - 1].Value = r.MinArea + "-" + r.MaxArea;
                dg[2, dg.RowCount - 1].Value = r.Percentage;
                dg[3, dg.RowCount - 1].Value = r.OffSet;
                var flats =
                    dbFlats.Where(x => x.SubZone.Equals(r.CodeZone)).ToList().Where(x => x.AreaTotalStrong >= r.MinArea & x.AreaTotalStrong <= r.MaxArea).ToList();
                dg[4, dg.RowCount - 1].Value = flats.Count;
                dg[5, dg.RowCount - 1].Value = r.NearPercentage;
            }
            dg.Rows.Add();
        }

        private void FillDgReqsTotal ()
        {
            int per = 0;
            for (int i = 0; i < dg.RowCount; i++)
            {
                per += Convert.ToInt16(dg[2, i].Value);
            }
            // dg.Rows.Add();
            dg[1, dg.RowCount - 1].Value = "Всего:";
            dg[2, dg.RowCount - 1].Value = per;
            dg.Rows[dg.RowCount - 1].ReadOnly = true;
        }

        private void FillSpotControls (List<SpotOption> spots)
        {
            if (spots == null) return;
            for (int i = 0; i < 4; i++)
            {
                var opt = spots[i];
                for (int j = 0; j < 5; j++)
                {
                    var dom = opt.DominantPositions[j];
                    ((CheckedListBox)this.Controls.Find("chkListP" + (i + 1).ToString(), true)[0]).SetItemChecked(j, dom);
                }
            }            
        }

        private ProjectInfo GetProjectInfoFromControls ()
        {
            ProjectInfo resPI = new ProjectInfo();

            // Файл инсоляции
            resPI.PathInsolation = projectInfo.PathInsolation;            

            // Квартирография
            resPI.requirments = FormManager.GetSpotTaskFromDG(dg);           

            // Пятна объекта
            resPI.SpotOptions = GetSpotOptionsFromControls();

            // Этажность
            resPI.CountFloorsDominant = Convert.ToInt32(numDomCountFloor.Value);
            resPI.CountFloorsMain = Convert.ToInt32(numMainCountFloor.Value);
            resPI.IsEnabledDominant = chkEnableDominant.Checked;

            // Разность доминант
            resPI.IsEnableDominantsOffset = chkDominantOffset.Checked;
            resPI.DominantOffSet = Convert.ToInt32(txtOffsetDominants.Value);

            return resPI;
        }

        private List<SpotOption> GetSpotOptionsFromControls ()
        {
            List<SpotOption> options = new List<SpotOption>();
            for (int i = 1; i < 5; i++)
            {
                List<bool> dominantsPositions = new List<bool>();
                for (int j = 0; j < 5; j++)
                {
                    dominantsPositions.Add(
                        ((CheckedListBox)this.Controls.Find("chkListP" + i.ToString(), true)[0]).GetItemChecked(j));
                }
                SpotOption houseOption = new SpotOption("P" + i.ToString(), dominantsPositions);
                options.Add(houseOption);
            }
            return options;
        }

        private void MainForm_FormClosed (object sender, FormClosedEventArgs e)
        {
            ProgressThreadStop();
        }

        public static void ProgressThreadStop()
        {
            if (th != null && th.ThreadState == System.Threading.ThreadState.Running)
                th.Abort();
        }

        private string GetCountTypicalSections (List<FlatInfo> sections)
        {
            var typicalSect = sections.GroupBy(g => g.IdSection).Select(s=>s.Count()).Where(w => w > 1).OrderBy(o => o).
                Aggregate(string.Empty, (s, i) => s + i + ";");
            return typicalSect;
        }
    }
}
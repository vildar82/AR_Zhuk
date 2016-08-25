using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AR_AreaZhuk.Model;
using AR_AreaZhuk.PIK1TableAdapters;
using OfficeOpenXml.Drawing.Chart;
using AR_Zhuk_DataModel;
using System.Drawing.Imaging;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using AR_Zhuk_Schema;
using OfficeOpenXml;
using Zuby.ADGV;
using AR_AreaZhuk.Controller;


namespace AR_AreaZhuk
{
    public partial class MainForm : Form
    {
        public MainForm()
        {

            InitializeComponent();
        }
        private bool isStop = false;
        public PIK1.C_Flats_PIK1_AreasDataTable flatsAreas = new PIK1.C_Flats_PIK1_AreasDataTable();

        public PIK1.C_Flats_PIK1DataTable dbFlats = new PIK1.C_Flats_PIK1DataTable();
        public bool IsRemainingDominants { get; set; }
        public int DominantOffSet { get; set; }
        BindingSource bs = new BindingSource();
        public static List<SpotInfo> spinfos = new List<SpotInfo>();
        public string PathToFileInsolation { get; set; }
        public static double offset = 5;
        public static bool isSave = false;
        public static int countGood = 0;
        public static bool IsExit = false;
        public static bool isContinue = true;
        public static bool isContinue2 = true;
        public static SpotInfo spotInfo = new SpotInfo();
        public static List<List<HouseInfo>> houses = new List<List<HouseInfo>>();
        public static List<GeneralObject> ob = new List<GeneralObject>();

        public void ViewProgress()
        {
            FormProgress fp = new FormProgress();
            fp.ShowDialog();
        }
        private void MainForm_Load(object sender, EventArgs e)
        {

            FrameWork fw = new FrameWork();
           // string excelPath = @"E:\__ROM_Типы квартир.xlsx";
           // var roomInfo = fw.GetRoomData(excelPath);
           // spotInfo = fw.GetSpotInformation();
           //Exporter.ExportSectionsToSQL(8 * 4, "Рядовая", 9, false, false, roomInfo);//Если нужно залить 1 тип секции
           // Environment.Exit(48);
            if (Environment.UserName.Equals("khisyametdinovvt") | Environment.UserName.Equals("ostaninam") | Environment.UserName.Equals("inkinli"))
            {
                UpdateDbFlats.Visible = true;
                chkUpdateSections.Visible = true;
            }
            C_Flats_PIK1_AreasTableAdapter pikFlats = new C_Flats_PIK1_AreasTableAdapter();
            flatsAreas = pikFlats.GetData();
            C_Flats_PIK1TableAdapter flatsTableAdapter = new C_Flats_PIK1TableAdapter();
            dbFlats = flatsTableAdapter.GetData();
            DominantOffSet = 5;
            chkListP1.SetItemChecked(chkListP1.Items.Count - 1, true);
            chkListP2.SetItemChecked(chkListP2.Items.Count - 1, true);
            btnMenuGroup1.Image = Properties.Resources.up;
            btnMenuGroup2.Image = Properties.Resources.up;
            btnMenuGroup3.Image = Properties.Resources.up;
            spotInfo = fw.GetSpotInformation();
            Serializer s = new Serializer();
            var newSpotInfo = s.GetLastSpot();
            if (newSpotInfo.requirments.Count != 0)
                spotInfo = newSpotInfo;
            //  spotInfo = s.GetLastSpot() ?? new SpotInfo();
            foreach (var r in spotInfo.requirments)
            {
                dg.Rows.Add();
                dg[0, dg.RowCount - 1].Value = r.SubZone;
                dg[1, dg.RowCount - 1].Value = r.MinArea + "-" + r.MaxArea;
                dg[2, dg.RowCount - 1].Value = r.Percentage;
                dg[3, dg.RowCount - 1].Value = r.OffSet;
                var flats =
                    dbFlats.Where(x => x.SubZone.Equals(r.CodeZone)).ToList().Where(x => x.AreaTotalStrong >= r.MinArea & x.AreaTotalStrong <= r.MaxArea).ToList();
                dg[4, dg.RowCount - 1].Value = flats.Count;
                dg[5, dg.RowCount - 1].Value = 0;
            }
            dg.Rows.Add();
            FillDgReqs();
            isEvent = true;
        }

        private void FillDgReqs()
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
                bool isValid = false;

                var spot2 = house2.SpotInf;
                int allCountFlats = 0;
                for (int k = 0; k < spotInfo.requirments.Count; k++)
                {
                    allCountFlats += spot1.requirments[k].RealCountFlats;
                    allCountFlats += spot2.requirments[k].RealCountFlats;
                }
                SpotInfo spGo = new SpotInfo();
                spGo = spot1.CopySpotInfo(spotInfo);
                // dg2.Rows.Add();
                int countValid = 0;
                for (int k = 0; k < spotInfo.requirments.Count; k++)
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


        public bool SetIndexesSection(int[] indexes, int[] sizes, int index, List<HouseInfo> listSections)
        {
            if (index == 0)
            {
                isContinue2 = false;
                return false;
            }
            indexes[index] = 0;
            indexes[index - 1]++;

            if (indexes[index - 1] >= listSections[index - 1].Sections.Count)
            {
                SetIndexesSection(indexes, sizes, index - 1, listSections);
            }
            return true;
        }

        public bool SetIndexesSize(int[] indexes, int index, int[] masSizes)//List<HouseInfo> listSections)
        {
            if (index == 0)
            {
                isContinue = false;
                return false;
            }
            indexes[index] = 0;
            indexes[index - 1]++;

            if (indexes[index - 1] >= masSizes.Length)
            {
                SetIndexesSize(indexes, index - 1, masSizes);
            }
            return isContinue;
        }


        public void GetHousePercentage(ref HouseInfo houseInfo, SpotInfo sp1)
        {
            sp1 = sp1.CopySpotInfo(spotInfo);
            for (int k = 0; k < houseInfo.Sections.Count; k++) //Квартиры
            {
                FlatInfo section = houseInfo.Sections[k];
                double areaSection = 0;
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

        private void button1_Click(object sender, EventArgs e)
        {
            spinfos.Clear();
            ob.Clear();
            FormManager.GetSpotTaskFromDG(spotInfo, dg);
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = 15;
            Parallel.For(0, houses[0].Count, po, GetGeneralObjects);
            //  GetGeneralObjects();
            FormManager.ViewDataProcentage(dg2, ob, spotInfo);
            lblCountObjects.Text = ob.Count.ToString();
        }



        private void btnSave_Click(object sender, EventArgs e)
        {
            List<string> guids = (from DataGridViewRow row in dg2.SelectedRows select dg2[dg2.Columns.Count - 1, row.Index].Value.ToString()).ToList();
            foreach (var g in guids)
            {
                GeneralObject go = ob.First(x => x != null && x.SpotInf.GUID.Equals(g));
                if (go == null) break;
                Serializer ser = new Serializer();
                ser.SerializeList(go, go.SpotInf.TotalStandartArea + "m2 (" + go.SpotInf.TotalFlats.ToString() + ")");

            }

        }

        private static bool isEvent = false;
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
            double maxArea = 0;
            ob = new List<GeneralObject>();
            lblCountObjects.Text = ob.Count.ToString();
            SetInfoTotalSectionsCount(null);

            isEvent = false;
            btnStartScan.Enabled = false;
            btnViewPercentsge.Enabled = true;
            Requirment requirment = new Requirment();
            FrameWork fw = new FrameWork();
            spotInfo = fw.GetSpotInformation();
            spotInfo.PathInsolation = PathToFileInsolation;
            FormManager.GetSpotTaskFromDG(spotInfo, dg);//Условия квартирографии 
            Serializer s = new Serializer();
            s.SerializeSpoinfo(spotInfo);
            List<HouseOptions> options = new List<HouseOptions>();
            for (int i = 1; i < 5; i++)
            {
                List<bool> dominantsPositions = new List<bool>();

                for (int j = 0; j < 5; j++)
                {
                    dominantsPositions.Add(
                        ((CheckedListBox)this.Controls.Find("chkListP" + i.ToString(), true)[0]).GetItemChecked(j));
                }
                HouseOptions houseOption = new HouseOptions("P" + i.ToString(), Convert.ToInt16(numMainCountFloor.Value), Convert.ToInt16(numDomCountFloor.Value), dominantsPositions);
                options.Add(houseOption);
            }
            ProjectScheme profectShema = new ProjectScheme(options, spotInfo);
            profectShema.ReadScheme(PathToFileInsolation);
            Thread th = new Thread(ViewProgress);
            th.Start();
            List<List<HouseInfo>> totalObject = profectShema.GetTotalHouses(
                int.Parse(textBoxMaxCountSectionsBySize.Text), int.Parse(textBoxMaxCountHousesBySpot.Text));
            SetInfoTotalSectionsCount(totalObject);
            isContinue = true;
            if (totalObject.Count == 0)
                isContinue=false;
            System.Diagnostics.Stopwatch sw = new Stopwatch();
            sw.Start();
            int counterGood = 0;
            int[] selectedHouse = new int[totalObject.Count];
            int[] nearReqPercentage = new int[spotInfo.requirments.Count];
            while (isContinue)
            {
                if (isStop)
                    break;
                List<List<FlatInfo>> sections = new List<List<FlatInfo>>();
                //Получение cекций из домов
                if (GetHouseSections(selectedHouse, totalObject, sections)) continue;
                if (!isContinue)
                    break;
                int counter = 0;
                //Группировка и сортировка секций
                List<CodeSection> codeSections = GetSectionsByCode(sections, counter);
                int[] selectedSectSize = new int[40];                                    //Выбранная размерность дома
                int[] selectedSectCode = new int[40];                                    //Выбранный код секций
                isContinue2 = true;


                double totalCountFlats = 0;
                //Общее кол-во квартир в объекте
                for (int i = 0; i < sections.Count; i++)
                {
                    totalCountFlats += codeSections[i].SectionsByCountFlats[selectedSectSize[i]].Count;
                }
                //Обход сформированных секций с уникальными кодами на объект
                while (isContinue2)
                {
                   
                    Application.DoEvents();
                    if (isStop)
                        break;
                    List<Code> listCodes = new List<Code>();
                    if (codeSections[0].SectionsByCountFlats.Count == selectedSectSize[0]) //Последний рассматриваемый вариант
                    {
                        isContinue2 = false;
                        break;
                    }
                    bool isValidPercentage = true;
                    string strP = "";
                    for (int q = 0; q < spotInfo.requirments.Count; q++)
                    {
                        var rr = spotInfo.requirments[q];
                        int countFlats = 0;                        
                        for (int i = 0; i < codeSections.Count; i++)
                        {                            
                            countFlats += Convert.ToInt16(codeSections[i].SectionsByCountFlats[selectedSectSize[i]].
                                SectionsByCode[selectedSectCode[i]].CodeStr[q].ToString()) * (codeSections[i].CountFloors - 1); ;
                            // Вильдар - 23.08.2016 по-моему тут была лишняя проверка - проверять процентаж не сложив сумму квартир по всем секциямс                            
                        }
                        //Процентаж определенного типа квартир в объекте
                        double percentage = countFlats * 100 / totalCountFlats;                        

                        // Вильдар. 23.08.2016
                        // Ближайший процентаж квартиры - если текущий процент квартиры ближе к требуемому, то записываем его как ближайший
                        double arround = Math.Abs(percentage - rr.Percentage);
                        double arround2 = Math.Abs(rr.NearPercentage - rr.Percentage);
                        if (arround < arround2 || rr.NearPercentage == 0)
                            rr.NearPercentage = Math.Round(percentage, 0);
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
                    //if (counterGood > 500)
                    //{
                    //    break;
                    //}
                    if (isValidPercentage)  //Процентаж прошел
                    {

                        counterGood++;
                        //сбор секций
                        for (int i = 0; i < sections.Count; i++)
                            listCodes.Add(codeSections[i].SectionsByCountFlats[selectedSectSize[i]].SectionsByCode[selectedSectCode[i]]);

                        //сортровка по пятнам и очередности расположения
                        listCodes = listCodes.OrderBy(x => x.SpotOwner).ThenBy(x => x.NumberSection).ToList();

                        List<HouseInfo> hoyses = new List<HouseInfo>();
                        int[] indexSelectedId = new int[listCodes.Count];

                        isContinue = true;
                        int countSections = listCodes.Count - 1;

                        string[] strPercent = strP.Split(';');
                        while (true)
                        {
                            GeneralObject go = new GeneralObject();
                            HouseInfo hi1 = new HouseInfo();
                            hi1.Sections = new List<FlatInfo>();
                            int countFlats = 0;
                            int[] idSections = new int[listCodes.Count];
                            double totalArea = 0;
                            double liveArea = 0;

                            int countContainsSections = 0;
                            double k1 = 0;
                            double k2 = 0;
                            for (int j = 0; j <= countSections; j++)
                            {
                                double levelArea = 0;
                                double levelAreaOffLLU = 0;
                                double levelAreaOnLLU = 0;
                                foreach (var secByPosition in sections)
                                {
                                    if (!(secByPosition[0].SpotOwner.Equals(listCodes[j].SpotOwner) &
                                          secByPosition[0].NumberInSpot.Equals(listCodes[j].NumberSection)))
                                        continue;
                                    var sec = secByPosition.Where(x => x.IdSection.Equals(listCodes[j].IdSections[indexSelectedId[j]])).ToList();
                                    if (sec.Count == 0) continue;
                                    idSections[j] = sec[0].IdSection;
                                    hi1.Sections.Add(sec[0]);
                                    if (sec[0].Flats.Any(x => x.SubZone.Equals("3")) |
                                        sec[0].Flats.Any(x => x.SubZone.Equals("4")))
                                        countContainsSections++;
                                    foreach (var flat in sec[0].Flats)
                                    {
                                        var currentFlatAreas =
                                            flatsAreas.First(x => x.Short_Type.Equals(flat.ShortType));
                                        var areas = Calculate.GetAreaFlat(sec[0].Floors, flat, currentFlatAreas);
                                        totalArea += areas[0];
                                        liveArea += areas[1];
                                        levelArea += areas[2];
                                        levelAreaOffLLU += areas[3];
                                        levelAreaOnLLU += areas[4];

                                    }
                                    k1 += levelAreaOffLLU / levelArea;
                                    k2 += levelAreaOffLLU / levelAreaOnLLU;
                                    break;
                                }


                                countFlats += listCodes[j].CountFlats;
                            }
                            k1 = k1 / (countSections + 1);
                            k2 = k2 / (countSections + 1);

                            
                            var objectByHouses =
                                hi1.Sections.GroupBy(x => x.SpotOwner).Select(x => x.ToList()).ToList();
                            List<HouseInfo> housesInSpot = new List<HouseInfo>();
                            foreach (var house in objectByHouses)
                            {
                                HouseInfo h = new HouseInfo();
                                foreach (var section in house.OrderBy(x => x.NumberInSpot).ToList())
                                {
                                    h.Sections.Add(section);
                                }
                                housesInSpot.Add(h);
                            }
                            var typicalSect = GetCountTypicalSections(idSections);
                            SpotInfo spGo = new SpotInfo();
                            spGo = spotInfo.CopySpotInfo(spotInfo);
                            spGo.CountContainsSections = countContainsSections;
                            spGo.K1 = k1;
                            spGo.K2 = k2;
                            spGo.TotalStandartArea = totalArea;
                            spGo.TotalLiveArea = liveArea;
                            spGo.TotalFlats = countFlats;
                            spGo.TypicalSections = typicalSect;
                            spGo.TotalSections = countSections + 1;
                           
                            for (int k = 0; k < spotInfo.requirments.Count; k++)
                                spGo.requirments[k].RealPercentage = Convert.ToInt16(strPercent[k]);
                            go.Houses = housesInSpot;
                            go.SpotInf = spGo;
                            ob.Add(go);
                            lblCountObjects.Text = ob.Count.ToString();
                            if (maxArea < totalArea)
                                maxArea = totalArea;
                            Application.DoEvents();


                            indexSelectedId[countSections]++;
                            if (listCodes[countSections].IdSections.Count <= indexSelectedId[countSections])
                                IncrementIdSection(countSections - 1, indexSelectedId, listCodes);

                            if (listCodes[0].IdSections.Count == indexSelectedId[0])
                            {
                                //  isContinue = false;
                                break;
                            }
                            // isContinue = true;     
                            if (isStop)
                                break;
                        }
                        counterGood++;
                    }

                    selectedSectCode[sections.Count - 1]++;
                    if (selectedSectCode[sections.Count - 1] >= codeSections[codeSections.Count - 1].
                        SectionsByCountFlats[selectedSectSize[codeSections.Count - 1]].SectionsByCode.Count)
                        IncrementSectionCode(selectedSectCode, selectedSectSize, codeSections.Count - 1, codeSections, ref totalCountFlats);
                }
                //if (selectedHouse.Length == 1)
                //    break;
            }

            FormManager.ViewDataProcentage(dg2, ob, spotInfo);
            for (int q = 0; q < spotInfo.requirments.Count; q++)
            {
                dg[5, q].Value = spotInfo.requirments[q].NearPercentage;
            }
            th.Abort();
            lblCountObjects.Text = ob.Count.ToString();
            isEvent = true;
            sw.Stop();
            //MessageBox.Show((sw.ElapsedMilliseconds / 1000).ToString());
            lblTime.Visible = true;
            lblTime.Text = (sw.ElapsedMilliseconds/1000).ToString();
            bs.DataSource = dg2.DataSource;
            lblMaxArea.Text = maxArea.ToString();
            lblTotalCount.Text = ob.Count.ToString();
            //  this.pb.Image = global::AR_AreaZhuk.Properties.Resources.объект;

        }        

        private static List<CodeSection> GetSectionsByCode(List<List<FlatInfo>> sections, int counter)
        {
            List<CodeSection> codeSections = new List<CodeSection>();
            foreach (var ss in sections.OrderBy(x => x.Count))
            {
                List<Code> codes = new List<Code>();
                CodeSection codeSection = new CodeSection();
                codeSection.CountFloors = ss[0].Floors;

                //Группировка по коду
                foreach (var s in ss.OrderByDescending(x => x.CountFlats))
                {
                    if (codes.Any(x => x.CodeStr.Equals(s.Code)))
                        codes.First(x => x.CodeStr.Equals(s.Code)).IdSections.Add(s.IdSection);
                    else
                    {
                        codes.Add(new Code(s.Code, s.IdSection, (s.Floors - 1) * (s.CountFlats - 1),
                            s.NumberInSpot, s.SpotOwner));
                    }
                }
                //Группировка  по кол-ву квартир в секции
                foreach (
                    var c in codes.OrderBy(x => x.CountFlats).OrderByDescending(x => x.CodeStr).ToList())
                {
                    FlatsInSection flatsInSection = new FlatsInSection();
                    flatsInSection.Count = c.CountFlats;
                    flatsInSection.SectionsByCode.Add(c);
                    if (codeSection.SectionsByCountFlats.Any(x => x.Count.Equals(c.CountFlats)))
                        codeSection.SectionsByCountFlats.First(x => x.Count.Equals(c.CountFlats))
                            .SectionsByCode.Add(c);
                    else codeSection.SectionsByCountFlats.Add(flatsInSection);
                }
                //Сортировка по кол-ву квартир
                codeSection.SectionsByCountFlats =
                    codeSection.SectionsByCountFlats.OrderByDescending(x => x.Count).ToList();
                codeSections.Add(codeSection);
                counter++;
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

        private bool GetHouseSections(int[] selectedHouse, List<List<HouseInfo>> totalObject, List<List<FlatInfo>> sections)
        {
            for (int i = 0; i < selectedHouse.Length; i++)
            {
                sections.AddRange(totalObject[i][selectedHouse[i]].SectionsBySize.Select(s => s.Sections));
            }
            if (IsRemainingDominants)
            {
                int offsetDom = DominantOffSet;
                List<FlatInfo> dominants = new List<FlatInfo>();
                foreach (var s in sections)
                {
                    if (!s[0].IsDominant)
                        continue;
                    dominants.Add(s[0]);
                }
                int remaining = Math.Abs(dominants[0].CountStep - dominants[dominants.Count - 1].CountStep);
                if (remaining > offsetDom)
                {
                    selectedHouse[selectedHouse.Length - 1]++;
                    if (selectedHouse[selectedHouse.Length - 1] >= totalObject[selectedHouse.Length - 1].Count)
                        IncrementSelectedHouse(selectedHouse.Length - 1, selectedHouse, totalObject);
                    return true;
                }
            }
            selectedHouse[selectedHouse.Length - 1]++;

            isContinue = true;
            if (selectedHouse[selectedHouse.Length - 1] >= totalObject[selectedHouse.Length - 1].Count)
                IncrementSelectedHouse(selectedHouse.Length - 1, selectedHouse, totalObject);
            return false;
        }


        public void IncrementSelectedHouse(int index, int[] houses, List<List<HouseInfo>> totalObject)
        {
            if (index == 0)
            {
              //  if (houses.Length > 1)
                    isContinue = false;
                return;
            }
            houses[index] = 0;
            houses[index - 1]++;
            if (houses[index - 1] >= totalObject[index - 1].Count)
            {
                IncrementSelectedHouse(index - 1, houses, totalObject);
            }
        }

        public void IncrementIdSection(int index, int[] selectedIdSection, List<Code> codes)
        {
            if (index == -1)
                return;
            selectedIdSection[index + 1] = 0;
            selectedIdSection[index]++;
            if (selectedIdSection[index] >= codes[index].IdSections.Count)
            {
                IncrementIdSection(index - 1, selectedIdSection, codes);
            }
        }


        public void IncrementSectionCode(int[] selectedSectCode, int[] selectedSectSize, int index, List<CodeSection> sections, ref double totalCountFlats)
        {
            //try
            //{
                // if (index == 0) было 0
                if (index == 0)
                    return;
                //Текущий выбранный код обнуляем
                selectedSectCode[index] = 0;
                //Изменяем выбранную размерность
                selectedSectSize[index]++;

                if (selectedSectSize[index] >= sections[index].SectionsByCountFlats.Count)
                {

                    IncrementSectionSize(selectedSectCode, selectedSectSize, index - 1, sections, false);

                    if (sections[0].SectionsByCountFlats.Count <= selectedSectSize[0])
                        return;

                }
                totalCountFlats = 0;
                //Общее кол-во квартир в размерности
                for (int i = 0; i < sections.Count; i++)
                {
                    totalCountFlats += sections[i].SectionsByCountFlats[selectedSectSize[i]].Count;
                }
            //}
            //catch
            //{
            //    MessageBox.Show("");
            //}
        }

        public void IncrementSectionSize(int[] selectedSectCode, int[] selectedSize, int index, List<CodeSection> sections, bool isSize)
        {
            if (index == 0)//было 0
            {
                return;
            }

            selectedSize[index + 1] = 0;
            if (!isSize)
            {
                selectedSectCode[index]++;
                if (sections[index].SectionsByCountFlats[selectedSize[index]].SectionsByCode.Count - 1 <=
                    selectedSectCode[index])
                {
                    selectedSize[index]++;
                    selectedSectCode[index] = 0;


                    if (selectedSize[index] >= sections[index].SectionsByCountFlats.Count)
                    {
                        IncrementSectionSize(selectedSectCode, selectedSize, index - 1, sections, true);
                    }
                }
            }
            else
            {
                selectedSize[index + 1] = 0;
                selectedSize[index]++;

                if (selectedSize[index] >= sections[index].SectionsByCountFlats.Count)
                {
                    selectedSize[index] = 0;
                    if (index != 0)
                    {
                        selectedSize[index - 1]++;

                        for (int i = index - 1; i < sections.Count; i++)
                        {
                            selectedSectCode[i] = 0;
                        }

                        if (selectedSize[index - 1] >= sections[index - 1].SectionsByCountFlats.Count)
                        {
                            IncrementSectionSize(selectedSectCode, selectedSize, index - 1, sections, true);
                        }
                    }
                }
            }

            return;
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

        private void chkDominant_CheckedChanged(object sender, EventArgs e)
        {
            txtOffsetDominants.Enabled = chkDominant.Checked;
            IsRemainingDominants = chkDominant.Checked;
            DominantOffSet = Convert.ToInt16(txtOffsetDominants.Text);
        }

        private void dg2_SelectionChanged(object sender, EventArgs e)
        {
            if (!isEvent)
                return;

            GeneralObject go = (GeneralObject)dg2["GenObject", dg2.SelectedRows[0].Index].Value;
            if (go == null) return;
            string imagePath = @"\\dsk2.picompany.ru\project\CAD_Settings\Revit_server\13. Settings\02_RoomManager\00_PNG_ПИК1\";

            string ExcelDataPath = @"\\dsk2.picompany.ru\project\CAD_Settings\Revit_server\13. Settings\02_RoomManager\БД_Параметрические данные квартир ПИК1.xlsx";

            BeetlyVisualisation.ImageCombiner imgComb = new BeetlyVisualisation.ImageCombiner(go, ExcelDataPath, imagePath, 72);
            //Serializer ser = new Serializer();
            //ser.SerializeList(go, Guid.NewGuid().ToString());//Создание xml
            var im = imgComb.generateGeneralObject();
            pb.Image = im;
        }

        private void txtOffsetDominants_ValueChanged(object sender, EventArgs e)
        {
            DominantOffSet = Convert.ToInt16(txtOffsetDominants.Value);
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
            if (!string.IsNullOrWhiteSpace(spotInfo.PathInsolation))
                initDirectory = Path.GetDirectoryName(spotInfo.PathInsolation);
            openFileDialog.InitialDirectory = initDirectory;
            openFileDialog.Filter = "Файл задание инсоляции (*.xlsx)|*.xlsx";
            openFileDialog.RestoreDirectory = true;
            PathToFileInsolation = "";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                PathToFileInsolation = openFileDialog.FileName;
            }
            if (PathToFileInsolation == "")
                btnStartScan.Enabled = false;
            else btnStartScan.Enabled = true;
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
            FillDgReqs();
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
    }
}

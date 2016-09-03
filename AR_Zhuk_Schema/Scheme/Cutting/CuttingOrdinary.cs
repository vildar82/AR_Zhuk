using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;
using AR_Zhuk_Schema.DB;
using AR_Zhuk_Schema.Insolation;

namespace AR_Zhuk_Schema.Scheme.Cutting
{
    class CuttingOrdinary : ICutting
    {
        private const string SectionOrdinaryName = "Рядовая";
        private const string SectionCornerLeftName = "Угловая лево";
        private const string SectionCornerRightName = "Угловая право";
        private const string SectionTowerName = "Башня";

        public readonly List<int> SectionSteps;

        private List<string> failedSections;
        private Dictionary<string, Section> passedSections;
        private List<string> failedHouseSteps;

        private HouseSpot houseSpot;
        private IDBService dbService;
        private IInsolation insService;

        bool failCutting = true;
        bool failFilterReqs = true;
        bool? failIns = null;

        public CuttingOrdinary(HouseSpot houseSpot, IDBService dbService, IInsolation insService)
        {
            this.houseSpot = houseSpot;
            this.dbService = dbService;
            this.insService = insService;                        

            // Определение шагов секций
            SectionSteps = GetSectionSteps();
        }        

        public List<HouseInfo> Cut()
        {
            Debug.WriteLine("Нарезка дома - " + houseSpot.SpotName + ", Дата = " + DateTime.Now);

            failedSections = new List<string>();
            passedSections = new Dictionary<string, Section>();
            failedHouseSteps = new List<string>();

            List<HouseInfo> resHouses = new List<HouseInfo>();

            // Все варианты домов по шагам секций
            List<int> stepsSet;
            var housesSteps = GetAllSteps(out stepsSet);           

            // Загрузка секций из базы
            InitLoadDBSections(stepsSet);

            // Подстановка секций под каждый вариант
            int countHouseSectionsPassed = 0;
            for (int h = 0; h < housesSteps.Count; h++)
            {
                var houseSteps = housesSteps[h];
                if (countHouseSectionsPassed > 0 && houseSteps.Length > countHouseSectionsPassed)
                    break;

                var houseVar = GetHouseVariant(houseSteps);

                if (houseVar != null && houseVar.Count > 0)
                {
                    HouseInfo hi = new HouseInfo();
                    hi.SpotInf = ProjectScheme.ProjectInfo;
                    hi.SectionsBySize = houseVar;

                    resHouses.Add(hi);
                    
                    countHouseSectionsPassed = houseSteps.Length;
                }
            }

            Debug.WriteLine("failedSections=" + failedSections.Count);
            Debug.WriteLine("passedSections=" + passedSections.Count);
            Debug.WriteLine("failedHouseSteps=" + failedHouseSteps.Count);           

            if (ProjectScheme.MaxHousesBySpot != 0)
            {
                // Рандомно выбрать нужное кол домой из всех вариантов
                resHouses = GetRandomMaxHauses(resHouses, ProjectScheme.MaxHousesBySpot);
            }

            if (resHouses.Count==0)
            {
                // Нет вариантов дома - определить причину и выдать сообщение
                DefineFailReason();
            }
            return resHouses;
        }

        private List<HouseInfo> GetRandomMaxHauses (List<HouseInfo> houses, int maxHousesBySpot)
        {
            List<HouseInfo> res = new List<HouseInfo>();
            if (houses.Count == 0 || houses.Count < maxHousesBySpot)
                return houses;            
            
            Random rnd = new Random();
            int maxValue = houses.Count - 1;
            for (int i = 0; i < maxHousesBySpot; i++)
            {
                var r = rnd.Next(maxValue);
                var h = houses[r];
                res.Add(h);
            }
            return res;
        }

        private List<Section> GetHouseVariant (int[] houseSteps)
        {
            string houseSize = string.Join(".", houseSteps);
#if TEST
            Debug.WriteLine("Размерность дома: " + houseSize);
#endif
            if (failedHouseSteps.Any(f => houseSize.StartsWith(f)))
            {
#if TEST
                Debug.WriteLine("Is Failed size start with");
#endif
                return null;
            }

            List<Section> resHouseSections = new List<Section>();

            // Нарезка секций одной размерности дома
            List<SectionBySize> sectionsBySize;

            if (CutSectionsBySize(houseSteps, out sectionsBySize))
            {
                if (failIns == null)
                    failIns = true;

                // Определение торцов секции
                DefineSectionEnds(ref sectionsBySize);

                foreach (var sectionBySize in sectionsBySize)
                {
                    if (!sectionBySize.isFromPassedDict)                    
                    {
                        // Проверка инсоляции секции                
                        List<FlatInfo> flatsCheckedIns = insService.GetInsolationSections(sectionBySize.Section);
                        if (flatsCheckedIns.Count == 0)
                        {
                            Debug.WriteLine("fail ins - key=" + sectionBySize.Key);
                            failedSections.Add(sectionBySize.Key);
                            resHouseSections = null;
                            break;
                        }
                        sectionBySize.Section.Sections = flatsCheckedIns;
                        passedSections.Add(sectionBySize.Key, sectionBySize.Section);
                        failIns = false;
                    }
                    resHouseSections.Add(sectionBySize.Section);                    
                }                
            }
#if TEST
            if (resHouseSections != null && resHouseSections.Count > 0)
            {
                Debug.WriteLine("Passed!     Passed!       Passed!     Passed! - " + houseSize);
            }
#endif
            return resHouseSections;
        }

        private bool CutSectionsBySize(int[] houseSteps,out List<SectionBySize> sectionsBySize)
        {
            sectionsBySize = new List<SectionBySize>();
            bool fail = false;
            string houseStepsPassed = string.Empty;            
            bool addToFailed = true;
            int curStepInHouse = 1;
            int sectionsInHouse = houseSteps.Length;
            // Определение этажности для каждого номера секции
            var dominantsNumberSect = DefineDominantsHouseVar(houseSteps.Length);
            string key = string.Empty;
            // Перебор нарезанных секций в доме
            for (int numberSect = 1; numberSect <= sectionsInHouse; numberSect++)
            {
                SectionBySize sectBySyze = new SectionBySize();
                fail = false;
                Section section = null;

                // Размер секции - шагов
                int sectSize = houseSteps[numberSect - 1];
                var sectCountStep = SectionSteps[sectSize];

                houseStepsPassed += sectSize + ".";

                // ключ размерности секции
                key = GetSectionDataKey(sectCountStep, numberSect, curStepInHouse, sectionsInHouse);
                sectBySyze.Key = key;
                if (failedSections.Contains(key))
                {
#if TEST
                    Debug.WriteLine("failedSection - " + key);
#endif                    
                    fail = true;
                    addToFailed = false;
                    break;
                }

                if (!passedSections.TryGetValue(key, out section))
                {
#if TEST
                    Debug.WriteLine("new sect key = " + key);
#endif
                    sectBySyze.isFromPassedDict = false;

                    //
                    // Отрезка секции из дома
                    //
                    section = houseSpot.GetSection(curStepInHouse, sectCountStep);
                    if (section == null)
                    {
                        Debug.WriteLine("fail нарезки - key=" + key);
                        fail = true;
                        addToFailed = true;
                        break;
                    }
                    failCutting = false;

                    section.NumberInSpot = numberSect;
                    section.SpotOwner = houseSpot.SpotName;

                    section.IsStartSectionInHouse = numberSect == 1;
                    section.IsEndSectionInHouse = numberSect == sectionsInHouse;

                    // Этажность секции, тип
                    var type = GetSectionType(section.SectionType);
                    section.Floors = GetSectionFloors(ref section, dominantsNumberSect);
                    var levels = GetSectionLevels(section.Floors);                    

                    //
                    // Запрос секций из базы
                    //
                    SelectSectionParam selSectParam = new SelectSectionParam(section.CountStep, type, levels);
                    section.Sections = dbService.GetSections(section, selSectParam);
                    if (section.Sections == null || section.Sections.Count == 0)
                    {
                        Debug.WriteLine("fail no in db - key=" + key + "; type=" + type + "; levels=" + levels);
                        fail = true;
                        addToFailed =  true;
                        break;
                    }
                    failFilterReqs = false;
                }
                else
                {
                    sectBySyze.isFromPassedDict = true;
                }
                curStepInHouse += sectCountStep;
                sectBySyze.Section = section;                
                sectionsBySize.Add(sectBySyze);
            }

            if (fail)
            {                
                if (addToFailed)
                    failedSections.Add(key);
                failedHouseSteps.Add(houseStepsPassed);
            }
            return !fail;
        }       

        private void InitLoadDBSections(List<int> stepsSet)
        {
            // Загрузка типов секций для этого дома
            // размерности секций
            var sectSteps = stepsSet.Select(s => SectionSteps[s]).ToList();
            List<string> types = new List<string> { SectionOrdinaryName };
            if (houseSpot.Segments.Count > 1)
            {
                types.Add(SectionCornerLeftName);
                types.Add(SectionCornerRightName);
            }
            List<string> levels = new List<string>();
            levels.Add(GetSectionLevels(houseSpot.CountFloorsMain));
            levels.Add(GetSectionLevels(houseSpot.CountFloorsDominant));

            List<SelectSectionParam> selectSects = new List<SelectSectionParam>();

            foreach (var typeSect in types)
            {
                foreach (var levelSect in levels)
                {
                    foreach (var step in sectSteps)
                    {
                        SelectSectionParam selSectParam = new SelectSectionParam(step, typeSect, levelSect);
                        selectSects.Add(selSectParam);
                    }
                }
            }
            dbService.PrepareLoadSections(selectSects);
        }

        private static string GetSectionDataKey(int sectCountStep, int numberSect, int startStepSect,int sectionsInHouse)
        {
            string key ="h" + sectionsInHouse + "n" + numberSect + "z" + sectCountStep + "s" + startStepSect;
            return key;
        }        

        private int GetSectionFloors(ref Section section,List<int> dominantsNumberSect)
        {            
            int floors = houseSpot.CountFloorsMain;
            if (!section.IsCorner)
            {
                bool isDominant = dominantsNumberSect.Contains(section.NumberInSpot);
                if (isDominant)
                {
                    floors = houseSpot.CountFloorsDominant;
                }
                section.IsDominant = isDominant;
            }
            return floors;
        }

        private List<int> DefineDominantsHouseVar (int sectionsInHouse)
        {
            List<int> dominantsNumSect = new List<int>();
            var sectNums = Enumerable.Range(1, sectionsInHouse);
            // Первая
            if (houseSpot.SpotOptions.DominantPositions[0])            
                dominantsNumSect.Add(1);
            // Вторая
            if (houseSpot.SpotOptions.DominantPositions[1])
                dominantsNumSect.Add(2);
            // Третья
            if (houseSpot.SpotOptions.DominantPositions[2])
                dominantsNumSect.Add(3);
            sectNums = sectNums.Reverse();
            // предпоследняя
            if (houseSpot.SpotOptions.DominantPositions[3])
                dominantsNumSect.Add(sectNums.Skip(1).FirstOrDefault());
            // последняя
            if (houseSpot.SpotOptions.DominantPositions[4])
                dominantsNumSect.Add(sectNums.First());
            return dominantsNumSect;
        }        

        public static string GetSectionLevels(int countFloors)
        {
            string floors = "10-18";
            if (countFloors > 18 & countFloors <= 25)
                floors = "19-25";
            if (countFloors <= 9)
                floors = "9";
            return floors;
        }

        public static string GetSectionType(SectionType sectionType)
        {
            switch (sectionType)
            {
                case SectionType.Ordinary:
                    return SectionOrdinaryName;
                case SectionType.CornerLeft:
                    return SectionCornerLeftName;
                case SectionType.CornerRight:
                    return SectionCornerRightName;
                case SectionType.Tower:
                    return SectionTowerName;
            }
            return null;
        }

        private List<int[]> GetAllSteps(out List<int> stepsSet)
        {
            int houseSteps = houseSpot.CountSteps;
            int sectMinStep = SectionSteps[0];
            int maxSectionsInHouse = houseSteps / sectMinStep;
            int[] selectedSectionsStep = new int[maxSectionsInHouse];

            List<int[]> houses = new List<int[]>();

            HashSet<int> steps = new HashSet<int>();

            bool isContinue = true;
            while (isContinue)
            {
                int countStepRest = houseSteps;
                for (int i = 0; i < maxSectionsInHouse; i++)
                {
                    countStepRest = countStepRest - SectionSteps[selectedSectionsStep[i]];
                    if (countStepRest == 0)
                    {
                        List<int> selSectSteps = new List<int>();
                        for (int k = 0; k <= i; k++)
                        {
                            var step = selectedSectionsStep[k];
                            selSectSteps.Add(step);
                            steps.Add(step);
                        }
                        houses.Add(selSectSteps.ToArray());

                        selectedSectionsStep[i]++;
                        if (selectedSectionsStep[i] >= SectionSteps.Count)
                        {
                            if (!SetIndexesSize(ref selectedSectionsStep, i, SectionSteps))
                                isContinue = false;
                        }
                        break;
                    }
                    else if (countStepRest < sectMinStep)
                    {
                        selectedSectionsStep[i]++;
                        if (selectedSectionsStep[i] >= SectionSteps.Count)
                        {
                            if (!SetIndexesSize(ref selectedSectionsStep, i, SectionSteps))
                                isContinue = false;
                        }
                        break;
                    }
                }
            }
            stepsSet = steps.ToList();
            // Сортировка по возрастанию кол секций в доме
            houses.Sort((h1, h2) => h1.Length.CompareTo(h2.Length));
            return houses;
        }

        public bool SetIndexesSize(ref int[] indexes, int index, List<int> masSizes)
        {
            bool res = true;
            if (index == 0)
            {
                return false;
            }
            indexes[index] = 0;
            indexes[index - 1]++;

            if (indexes[index - 1] >= masSizes.Count)
            {
                res = SetIndexesSize(ref indexes, index - 1, masSizes);
            }
            return res;
        }

        /// <summary>
        /// Определение торцов в секции
        /// </summary>        
        private void DefineSectionEnds (ref List<SectionBySize> sections)
        {
            for (int i = 0; i < sections.Count; i++)
            {
                int floorsPrev = 0;
                int floorsNext = 0;

                var section = sections[i].Section;
                if (section.NumberInSpot != 1)
                {
                    var sectionPrev = sections[i - 1].Section;
                    floorsPrev = sectionPrev.Floors; //GetSectionFloors(ref sectionPrev, sections.Count);
                }
                if (section.NumberInSpot != sections.Count)
                {
                    var sectionNext = sections[i + 1].Section;
                    floorsNext = sectionNext.Floors; //GetSectionFloors(ref sectionNext, sections.Count);
                }

                var jointStart = GetJoint(section.Floors, floorsPrev);
                var jointEnd = GetJoint(section.Floors, floorsNext);

                if (section.IsCorner)
                {
                    // Угловая левая
                    if (section.SectionType == SectionType.CornerLeft)
                    {
                        if (section.IsCornerStartTail)
                        {
                            section.JointRight = jointStart;
                            section.JointLeft = jointEnd;
                        }
                        else
                        {
                            section.JointRight = jointEnd;
                            section.JointLeft = jointStart;
                        }
                    }
                    // Угловая правая
                    else
                    {
                        if (section.IsCornerStartTail)
                        {
                            section.JointLeft = jointStart;
                            section.JointRight = jointEnd;
                        }
                        else
                        {
                            section.JointRight = jointStart;
                            section.JointLeft = jointEnd;
                        }
                    }
                }
                else
                {
                    if (section.Direction > 0)
                    {
                        section.JointLeft = jointStart;
                        section.JointRight = jointEnd;
                    }
                    else
                    {
                        section.JointRight = jointStart;
                        section.JointLeft = jointEnd;
                    }
                }
            }
        }

        private Joint GetJoint(int floors, int floorsJoint)
        {
            if (floorsJoint == 0)
                return Joint.End;

            if (floors > floorsJoint)
            {
                return Joint.End;
            }
            else if (floors == floorsJoint)
            {
                return Joint.None;
            }
            else
            {
                return Joint.Seam;
            }
        }

        private List<int> GetSectionSteps ()
        {
            List<int> resSectSteps = new List<int>();
            // Если нет этажности больше 9 - то шаги от 6 до 11
            if (houseSpot.CountFloorsMain < 10 && 
                !houseSpot.SpotOptions.DominantPositions.Any(d=>d))
            {
                resSectSteps.AddRange(Enumerable.Range(6, 6));
            }
            else if (houseSpot.CountFloorsMain > 9)
            {
                resSectSteps.AddRange(Enumerable.Range(7, 8));
            }
            else
            {
                resSectSteps.AddRange(Enumerable.Range(6, 9));
            }
            return resSectSteps;
        }

        private void DefineFailReason ()
        {
            string failReason = "Не удалось подобрать варианты домов для пятна " + houseSpot.SpotName + ": \n";
            // Файл при нарезке
            if (failCutting)
            {
                failReason += "Габариты пятна не соответствуют требованиям раскладки секций по серии ПИК1. Измените пятно.";
            }
            // Через требования квартирографии (типы квартир и площади) ничего не прошло
            else if (failFilterReqs)
            {
                failReason += "Требования квартирографии не пропускают секции. Измените требования - диапазоны площадей.";
            }            
            else if (failIns == null)
            {
                // До инсоляции не дошло. Т.е. некоторые секции проходили и через отрезку и через квартирографию, но целиком дом через них не прошел                
                failReason += "Требования квартирографии и этажностей не пропускают секции. Попробуйте изменить параметры квартирографии или этажности (доминанты).";
            }
            else
            {
                failReason += "Требования раскладки секций по серии ПИК1 не совместимы с инсоляцией. Попробуйте изменить габариты пятна или этажность (доминанту).";
            }         
            
            AR_Zhuk_DataModel.Messages.Informer.AddMessage(failReason);
        }
    }
}
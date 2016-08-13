using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;
using AR_Zhuk_Schema.Insolation;

namespace AR_Zhuk_Schema.Insolation
{
    /// <summary>
    /// Проверка инсоляции рядовой секции
    /// </summary>
    class InsCheckOrdinary : InsCheckBase
    {
        private Dictionary<string, RoomInfo> dictInsFlats = new Dictionary<string, RoomInfo>();

        readonly string[] insTopStandart;
        readonly string[] insBotStandart;
        readonly string[] insBotStandartReverse;        
        readonly string[] insTopInvert;
        readonly string[] insBotInvert;
        readonly string[] insBotInvertReverse;        
        
        string[] insTopSide;
        string[] insBotSide;        

        Joint jointCurLeft;
        Joint jointCurRight;

        bool isFirstFlatInSide;
        bool isLastFlatInSide;        

        public InsCheckOrdinary (IInsolation insService, Section section) 
            : base(insService, section)
        {
            insTopStandart = section.InsTop.Select(m => m.InsValue).ToArray();
            insBotStandart = section.InsBot.Select(m => m.InsValue).ToArray();            
            insTopInvert = insBotStandart;
            insBotInvert = insTopStandart;
            insBotStandartReverse = insBotStandart.Reverse().ToArray();
            insBotInvertReverse = insBotInvert.Reverse().ToArray();
        }

        public override List<FlatInfo> CheckSections (Section section)
        {
            List<FlatInfo> resFlats = new List<FlatInfo>();
            foreach (var sectFlats in section.Sections)
            {
                sectFlats.Code = insService.GetFlatCode(sectFlats);

                // Для рядовой секции - проверка инсоляции с приоритетной стороны                    
                var flats = insService.NewFlats(section, sectFlats, isInvert: !section.PriorityLluSideIsTop);
                // Проверка однотипной секции
                if (!insService.IsIdenticalSection(flats, resFlats))
                {
                    if (CheckSection(flats, isRightOrTopLLu: section.PriorityLluSideIsTop))
                    {
                        // Прошла инсоляция с приоритетной стороны. С неприоритетной не надо проверять.
                        resFlats.Add(flats);
                    }
                    else
                    {
                        // Проверка инсоляции с неприоритетной стороны секции                        
                        flats = insService.NewFlats(section, sectFlats, isInvert: section.PriorityLluSideIsTop);
                        // Проверка однотипной секции
                        if (!insService.IsIdenticalSection(flats, resFlats))
                        {                            
                            if (CheckSection(flats, isRightOrTopLLu: !section.PriorityLluSideIsTop))
                            {
                                // Прошла инсоляция с неприоритетной стороны.
                                resFlats.Add(flats);
                            }
#if TEST
                            else
                            {
                                resFlats.Add(flats);
                            }
#endif
                        }
                    }
                }
            }
            return resFlats;
        }

        public bool CheckSection (FlatInfo sect, bool isRightOrTopLLu)
        {
            bool res = false;
            this.isRightOrTopLLu = isRightOrTopLLu;
            checkSection = sect;

            topFlats = insService.GetSideFlatsInSection(sect.Flats, true, section.SectionType);
            bottomFlats = insService.GetSideFlatsInSection(sect.Flats, false, section.SectionType);            

            InvertInsSide(isRightOrTopLLu);

            // Проверка инсоляции квартир сверху
            isTop = true;
            curSideFlats = topFlats;
            res = CheckFlats();

            if (res) // прошла инсоляция верхних квартир
            {
                // Проверка инсоляции квартир снизу                
                isTop = false;
                curSideFlats = bottomFlats;
                res = CheckFlats();
            }
            return res;
        }

        protected bool CheckFlats ()
        {
            bool res = false;            

            if (isRightOrTopLLu)
            {
                if (isTop)
                {
                    insTopSide = insTopStandart;
                    insBotSide = insBotStandartReverse;
                }
                else
                {
                    insTopSide = null;
                    insBotSide = insBotStandart;
                }
                jointCurLeft = section.JointLeft;
                jointCurRight = section.JointRight;            
            }
            else
            {
                if (isTop)
                {
                    insTopSide = insTopInvert;
                    insBotSide = insBotInvertReverse;
                }
                else
                {
                    insBotSide = insBotInvert;
                    insTopSide = null;
                }
                jointCurLeft = section.JointRight;
                jointCurRight = section.JointLeft;
            }

            if (isTop)
            {
                res = CheckCellIns();
            }
            else
            {
                var startStep = topFlats.Last().SelectedIndexBottom;                
                res = CheckCellIns(startStep);
            }
            return res;
        }

        private bool CheckCellIns (int startStep = 0)
        {            
            int step = startStep;

            for (int i = 0; i < curSideFlats.Count; i++)
            {                
                flat = curSideFlats[i];

                string keyInsFlat = GetKey(step);
                RoomInfo insFlatValue;
                if (!dictInsFlats.TryGetValue(keyInsFlat, out insFlatValue))
                {
                    curFlatIndex = i;
                    bool flatPassed = true;                    
                    isFirstFlatInSide = IsEndFirstFlatInSide();
                    isLastFlatInSide = IsEndLastFlatInSide();

                    if (flat.SubZone != "0")
                    {
                        LightingRoom lightingRoom = null;
                        // Ошибка, если у квартиры снизу есть верхняя инсоляция
                        if (!isTop && flat.SelectedIndexTop != 0)
                        {
                            flatPassed = false;
                        }
                        else
                        {
                            lightingRoom = LightingRoomParser.GetLightings(flat, false);
                            // Ошибка, если у не торцевой квартиры будет боковая инсоляция
                            if (!isFirstFlatInSide && !isLastFlatInSide &&
                                (lightingRoom.SideIndexTop != null || lightingRoom.SideIndexBot!= null))
                            {
                                flatPassed = false;
                            }
                        }

                        if (flatPassed)
                        {
                            var ruleInsFlat = insService.FindRule(flat);
                            if (ruleInsFlat == null)
                            {
                                // Атас, квартира не ЛЛУ, но без правил инсоляции
                                throw new Exception("Не определено правило инсоляции для квартиры - " + flat.Type);
                            }

                            // Запись значений инсоляции в квартире
                            lightingRoom.FillIns(step, insTopSide, insBotSide, insSideLeftBot, insSideLeftTop, insSideRightBot, insSideRightTop);

                            // Проверка на затык бокового окна (если есть)
                            if (CheckFlatSideStopper(isFirstFlatInSide, isLastFlatInSide, lightingRoom))
                            {
                                flatPassed = false;
                                foreach (var rule in ruleInsFlat.Rules)
                                {
                                    if (lightingRoom.CheckInsRule(rule))
                                    {
                                        flatPassed = true;
                                        break;
                                    }
                                }
                            }                        
                        }
                    }                    

                    flat.IsInsPassed = flatPassed;

                    // Определение торца квартиры
                    DefineJoint(ref flat, isFirstFlatInSide, isLastFlatInSide, isTop);

                    dictInsFlats.Add(keyInsFlat, flat);
                }
                else
                {
                    flat.IsInsPassed = insFlatValue.IsInsPassed;
                    flat.Joint = insFlatValue.Joint;
                }
#if !TEST
                // Если квартира не прошла инсоляцию - вся секция не прошла
                if (!flat.IsInsPassed)
                {
                    return false;
                }
#endif
                // Сдвиг шага
                step += isTop ? flat.SelectedIndexTop : flat.SelectedIndexBottom;
            }
            // Все квартиры прошли инсоляцию
            return true;
        }

        private string GetKey (int step)
        {
            string res = (isTop ? "1" : "0") + (isRightOrTopLLu? "1":"0") + step + flat.Type;
            return res;
        }

        private void DefineJoint (ref RoomInfo flat, bool isFirstFlatInSide, bool isLastFlatInSide, bool isTop)
        {
            if (isFirstFlatInSide)
            {
                flat.Joint = isTop ? jointCurRight : jointCurLeft;
            }
            else if (isLastFlatInSide)
            {
                flat.Joint = isTop ? jointCurLeft : jointCurRight;
            }
        }        
    }
}
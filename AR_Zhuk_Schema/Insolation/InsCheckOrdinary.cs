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
        readonly string[] insTopInvert;
        readonly string[] insBotInvert;        

        Lighting lightingCurSide;
        Lighting lightingOtherSide;
        
        string[] insCurSide;
        string[] insOtherSide;

        string insSideCurTopLeft;
        string insSideCurBotLeft;
        string insSideCurTopRight;
        string insSideCurBotRight;

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

            // Временно!!! подмена индекса угловой квартиры 2KL2
            if (section.SectionType == SectionType.CornerLeft || section.SectionType == SectionType.CornerRight)
            {
                var cornerFlat = section.SectionType == SectionType.CornerLeft ? bottomFlats.First() : bottomFlats.Last();
                if (cornerFlat.ShortType == "2KL2")
                {
                    cornerFlat.LightingNiz = cornerFlat.Type == "PIK1_2KL2_A0" ? "2|3,4" : "1,2|3";
                    cornerFlat.SelectedIndexBottom = 4;
                }
            }

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

        protected override bool CheckFlats ()
        {
            bool res = false;            

            if (isRightOrTopLLu)
            {
                if (isTop)
                {
                    insCurSide = insTopStandart;
                    insOtherSide = insBotStandart;                    
                }
                else
                {
                    insCurSide = insBotStandart;
                    insOtherSide = insTopStandart;
                }
                jointCurLeft = section.JointLeft;
                jointCurRight = section.JointRight;            
            }
            else
            {
                if (isTop)
                {
                    insCurSide = insTopInvert;
                    insOtherSide = insBotInvert;
                }
                else
                {
                    insCurSide = insBotInvert;
                    insOtherSide = insTopInvert;
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
                specialFail = false;
                flat = curSideFlats[i];

                string keyInsFlat = GetKey(step);
                RoomInfo insFlatValue;
                if (!dictInsFlats.TryGetValue(keyInsFlat, out insFlatValue))
                {
                    curFlatIndex = i;
                    bool flatPassed = false;
                    string lightingCurSideString = null;
                    string lightingOtherSideString = null;
                    isFirstFlatInSide = IsEndFirstFlatInSide();
                    isLastFlatInSide = IsEndLastFlatInSide();

                    if (flat.SubZone == "0")
                    {
                        // без правил инсоляции может быть ЛЛУ
                        flatPassed = true;
                    }
                    else
                    {
                        if (isTop)
                        {
                            lightingCurSideString = flat.LightingTop;
                            lightingOtherSideString = flat.LightingNiz;
                        }
                        else
                        {
                            lightingCurSideString = flat.LightingNiz;
                        }

                        lightingCurSide = LightingStringParser.GetLightings(lightingCurSideString, isTop);
                        // Для верхних крайних верхних квартир нужно проверить низ
                        lightingOtherSide = null;
                        if (isTop)
                        {
                            if (lightingOtherSideString != null && (isFirstFlatInSide || isLastFlatInSide))
                            {
                                lightingOtherSide = LightingStringParser.GetLightings(lightingOtherSideString, false);
                            }
                        }

                        var ruleInsFlat = insService.FindRule(flat);
                        if (ruleInsFlat == null)
                        {
                            // Атас, квартира не ЛЛУ, но без правил инсоляции
                            throw new Exception("Не определено правило инсоляции для квартиры - " + flat.Type);
                        }

                        foreach (var rule in ruleInsFlat.Rules)
                        {
                            if (CheckRule(rule, step))
                            {
                                // Правило удовлетворено, оставшиеся правила можно не проверять
                                // Евартира проходит инсоляцию
                                flatPassed = true;
                                break;
                            }
                        }
                    }

                    if (specialFail)
                    {
                        flatPassed = false;
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

        private bool CheckRule (InsRule rule, int step)
        {
            // подходящие окна в квартиирах будут вычитаться из требований
            var requires = rule.Requirements.ToList();                       

            // Проверка окон с этой строны
            isCurSide = true;
            CheckLighting(ref requires, lightingCurSide.Indexes, insCurSide, step);           

            // Проверка окон с другой стороны
            isCurSide = false;
            if (lightingOtherSide!= null)
                CheckLighting(ref requires, lightingOtherSide.Indexes, insOtherSide, step);

            // проверка боковин            
            CheckLightingSide(ref requires, lightingCurSide, lightingOtherSide, isFirstFlatInSide, isLastFlatInSide);

            // Если все требуемые окно были вычтены, то сумма остатка будет <= 0
            // Округление вниз - от окон внутри одного помещения
            var isPassed = RequirementsIsEmpty(requires);                    
            return isPassed;            
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
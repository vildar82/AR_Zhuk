using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_Zhuk_Schema.Insolation
{
    /// <summary>
    /// Проверка инсоляции угловой секции
    /// </summary>
    class InsCheckCorner : InsCheckBase
    {
        private Dictionary<string, RoomInfo> dictInsFlats = new Dictionary<string, RoomInfo>();

        int indexBot =0;
        readonly string[] insTop;
        readonly string[] insBot;
        readonly string[] insBotReverse;

        public InsCheckCorner (IInsolation insService, Section section)
            : base(insService, section)
        {
            insTop = section.InsTop.Select(s => s.InsValue).ToArray();
            insBot = section.InsBot.Select(s => s.InsValue).ToArray();
            insBotReverse = insBot.Reverse().ToArray();
        }

        public override List<FlatInfo> CheckSections (Section section)
        {
            List<FlatInfo> resInsFlats = new List<FlatInfo>();
            HashSet<string> passedSections = new HashSet<string>();
            foreach (var sectFlats in section.Sections)
            {
                sectFlats.Code = insService.GetFlatCode(sectFlats);
                //string flatsHash = insService.GetFlatsHash(sectFlats);
                if (passedSections.Contains(sectFlats.Code))
                {
                    continue;
                }                

                // для угловой - проверка инсоляции в одном ее положении
                var flats = insService.NewFlats(section, sectFlats, isInvert: false);
                // Проверка инсоляции угловой секции                        
                if (CheckSection(flats))
                {
                    passedSections.Add(sectFlats.Code);
                    resInsFlats.Add(flats);
                }
#if TEST
                else
                {
                    passedSections.Add(sectFlats.Code);
                    resInsFlats.Add(flats);
                }                
#endif                
            }
            return resInsFlats;
        }

        public bool CheckSection (FlatInfo sect)
        {
            bool res = false;            
            checkSection = sect;

            topFlats = insService.GetSideFlatsInSection(sect.Flats, true, section.SectionType);
            bottomFlats = insService.GetSideFlatsInSection(sect.Flats, false, section.SectionType);

            //// Временно!!! подмена индекса угловой квартиры 2KL2
            //if (section.SectionType == SectionType.CornerLeft || section.SectionType == SectionType.CornerRight)
            //{
            //    var cornerFlat = section.SectionType == SectionType.CornerLeft ? bottomFlats.First() : bottomFlats.Last();
            //    if (cornerFlat.ShortType == "2KL2")
            //    {
            //        cornerFlat.LightingNiz = cornerFlat.Type == "PIK1_2KL2_A0" ? "2|3,4" : "1,2|3";
            //        cornerFlat.SelectedIndexBottom = 4;
            //    }
            //}

            // Проверка инсоляции квартир сверху
            isTop = true;            
            curSideFlats = topFlats;
            res = CheckSideFlats();

            if (res) // прошла инсоляция верхних квартир
            {
                // Проверка инсоляции квартир снизу                
                isTop = false;                
                curSideFlats = bottomFlats;
                res = CheckSideFlats();
            }
            return res;
        }        

        /// <summary>
        /// Проверка инсоляции верхних квартир
        /// </summary>
        private bool CheckSideFlats ()
        {
            int step = isTop ? 0 : indexBot;
            for (int i = 0; i < curSideFlats.Count; i++)
            {
                flat = curSideFlats[i];

                string keyInsFlat = GetKey(step);
                RoomInfo insFlatValue;
                if (!dictInsFlats.TryGetValue(keyInsFlat, out insFlatValue))
                {
                    curFlatIndex = i;
                    bool flatPassed = true;
                    bool isFirstFlatInSide = IsEndFirstFlatInSide();
                    bool isLastFlatInSide = IsEndLastFlatInSide();

                    if (flat.SubZone == "0")
                    {
                        // прыжок на ширину ЛЛУ без 1 шага (уходит внутрь угла и не занимает шаг секции)
                        step += flat.SelectedIndexTop;
                        continue;
                    }

                    LightingRoom roomLighting = null;

                    // Ошибка, если у квартиры снизу есть верхняя инсоляция
                    if (!isTop && flat.SelectedIndexTop != 0)
                    {
                        flatPassed = false;
                    }
                    else
                    {
                        roomLighting = LightingRoomParser.GetLightings(flat, true);
                        // Ошибка, если у не торцевой квартиры будет боковая инсоляция
                        if (!isFirstFlatInSide && !isLastFlatInSide &&
                            (roomLighting.SideIndexTop != null || roomLighting.SideIndexBot != null))
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
                        string[] insBotCur;
                        if (isFirstFlatInSide)
                        {                            
                            insBotCur =isTop? insBotReverse : insBot;
                        }
                        else
                        {
                            if (isTop)
                            {
                                insBotCur = insBot;
                                indexBot = flat.SelectedIndexBottom>0? flat.SelectedIndexBottom: 0;
                            }
                            else
                            {
                                insBotCur = insBot;
                            }                            
                        }
                        int stepTop;
                        int stepBot;
                        if (isTop)
                        {
                            stepTop = step;
                            stepBot = 0;
                        }
                        else
                        {
                            stepTop = 0;
                            stepBot = step;
                        }
                        roomLighting.FillIns(stepTop, stepBot, insTop, insBotCur, insSideLeftBot, insSideLeftTop, insSideRightBot, insSideRightTop);

                        // Проверка на затык бокового окна (если есть)
                        flatPassed = false;
                        if (CheckFlatSideStopper(isFirstFlatInSide, isLastFlatInSide, roomLighting))
                        {                            
                            foreach (var rule in ruleInsFlat.Rules)
                            {
                                if (roomLighting.CheckInsRule(rule))
                                {
                                    flatPassed = true;
                                    break;
                                }
                            }
                        }
                    }
                    // Для тесовой визуализации
                    flat.IsInsPassed = flatPassed;

                    // Определение торца квартиры     
                    DefineJoint(ref flat, isFirstFlatInSide, isLastFlatInSide, isTop);
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
                step += isTop ? flat.SelectedIndexTop : flat.SelectedIndexBottom;
            }
            return true;
        }

        private string GetKey (int step)
        {
            string res = (isTop ? "1" : "0") + step + flat.Type;
            return res;
        }

        private void DefineJoint (ref RoomInfo flat, bool isFirstFlatInSide, bool isLastFlatInSide, bool isTop)
        {
            if (section.SectionType == SectionType.CornerLeft)
            {
                if (isFirstFlatInSide)
                {
                    if (isTop)
                        flat.Joint = section.JointRight;
                }
                else if (isLastFlatInSide)
                {                    
                    flat.Joint = isTop ? section.JointLeft : section.JointRight;
                }
            }
            else
            {
                if (isFirstFlatInSide)
                {
                    flat.Joint = isTop ? section.JointRight : section.JointLeft;                    
                }
                else if (isLastFlatInSide)
                {
                    if (isTop)
                        flat.Joint = section.JointLeft;
                }
            }
        }        
    }
}

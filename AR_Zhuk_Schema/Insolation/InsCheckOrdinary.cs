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
                        if (lightingOtherSide != null && (isFirstFlatInSide || isLastFlatInSide))
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

                if (!flatPassed || specialFail)
                {
                    // квартира не прошла инсоляцию - вся секция не проходит                    
                    return false;
                }
                flat.IsInsPassed = true;

                // Сдвиг шага
                step += isTop ? flat.SelectedIndexTop : flat.SelectedIndexBottom;
            }
            // Все квартиры прошли инсоляцию
            return true;
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
        

        ///// <summary>
        ///// Определение с какого торца секции расположена квартира
        ///// </summary>        
        //private EnumEndSide GetEndFlatSide ()
        //{
        //    EnumEndSide res = EnumEndSide.None;
        //    if (isFirstFlatInSide)
        //    {
        //        if (isTop)
        //        {
        //            res = EnumEndSide.Right;
        //        }
        //        else
        //        {
        //            res = EnumEndSide.Left;
        //        }
        //    }
        //    else if (isLastFlatInSide)
        //    {
        //        if (isTop)
        //        {
        //            res = EnumEndSide.Left;
        //        }
        //        else
        //        {
        //            res = EnumEndSide.Right;
        //        }
        //    }            
        //    return res;
        //}
    }
}
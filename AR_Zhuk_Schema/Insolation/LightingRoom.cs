using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_Schema.Insolation
{
    public class LightingRoom : ICloneable
    {
        public List<LightingWindow> IndexesTop { get;  set; }
        public List<LightingWindow> IndexesBot { get;  set; }
        public LightingWindow SideIndexTop { get;  set; }
        public LightingWindow SideIndexBot { get;  set; }
        public Side Side { get; set; }

        public List<InsRequired> TotalRoomIns { get; private set; }

        public object Clone ()
        {
            LightingRoom roomClone = new LightingRoom() {
                Side = Side,
                IndexesTop = IndexesTop == null ? null: (List<LightingWindow>)IndexesTop.Clone(),
                IndexesBot = IndexesBot == null ? null : (List<LightingWindow>)IndexesBot.Clone(),
                SideIndexTop = SideIndexTop == null ? null : (LightingWindow)SideIndexTop.Clone(),
                SideIndexBot = SideIndexBot == null ? null : (LightingWindow)SideIndexBot.Clone()
            };
            return roomClone;
        }

        internal void FillIns (int step, string[] insTopSide, string[] insBotSide, 
            string insSideLeftBot, string insSideLeftTop, string insSideRightBot, string insSideRightTop)
        {
            Fill(IndexesTop, insTopSide, step);
            Fill(IndexesBot, insBotSide, step);
            if (Side == Side.Left)
            {
                if (SideIndexTop != null) SideIndexTop.InsValue = insSideLeftTop;
                if (SideIndexBot != null) SideIndexBot.InsValue = insSideLeftBot;
            }
            else if (Side == Side.Right)
            {
                if (SideIndexTop != null) SideIndexTop.InsValue = insSideRightTop;
                if (SideIndexBot != null) SideIndexBot.InsValue = insSideRightBot;
            }
            SumIns();
        }

        private void SumIns ()
        {
            TotalRoomIns = new List<InsRequired>();
            List<LightingWindow> allWindows = new List<LightingWindow>();
            if (IndexesTop != null) allWindows.AddRange(IndexesTop);
            if (IndexesBot != null) allWindows.AddRange(IndexesBot);
            if (SideIndexTop!=null) allWindows.Add(SideIndexTop);
            if (SideIndexBot != null) allWindows.Add(SideIndexBot);

            var rooms = allWindows.GroupBy(w => w.RoomNumber);
            foreach (var room in rooms)
            {
                if (room.Key == 0)
                {
                    var room0insValues = room.Where(r => r.InsValue != "A" && !string.IsNullOrEmpty(r.InsValue)).
                                                GroupBy(r => r.InsValue);
                    foreach (var ins in room0insValues)
                    {                        
                        TotalRoomIns.Add(new InsRequired(ins.Key, ins.Count()));
                    }
                }
                else
                {
                    // Нужно выбрать 1 макимальную инсоляцию из комнаты
                    var maxInsValue = room.Where(r=>r.InsValue!= "A" && !string.IsNullOrEmpty(r.InsValue)).
                                        GroupBy(r => r.InsValue).OrderByDescending(r => r.Key).FirstOrDefault();
                    if (maxInsValue != null)
                    {
                        var totalIns = TotalRoomIns.FirstOrDefault(r => r.InsIndex == maxInsValue.Key);
                        if (totalIns == null)
                        {
                            TotalRoomIns.Add(new InsRequired(maxInsValue.Key, 1));
                        }
                        else
                        {
                            totalIns.CountLighting += 1;
                        }
                    }                   
                }
            }
        }

        private void Fill (List<LightingWindow> windows, string[] ins, int step)
        {
            if (windows == null || ins == null) return;
            foreach (var item in windows)
            {
                item.InsValue = ins[item.Index-1 + step];
            }
        }

        internal bool CheckInsRule (InsRule rule)
        {
            // Проверка, общая инсоляция квартиры (TotalRoomIns) больше или равна требуемому правилу?
            foreach (var req in rule.Requirements)
            {
                var roomIns = TotalRoomIns.FirstOrDefault(r => r.InsIndex.CompareTo(req.InsIndex)>=0);                
                if (roomIns == null || roomIns.CountLighting < req.CountLighting)
                {                 
                    return false;
                }
            }
            return true;
        }
    }    
}

using System;

namespace AR_AreaZhuk.Percentage
{
    public class EventIntArg : EventArgs
    {
        public int Count { get; private set; }
        public EventIntArg(int count)
        {
            Count = count;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_DataModel.Messages
{
    public static class Informer
    {
        private static List<Info> messages = new List<Info>();        

        /// <summary>
        /// Показ сообщений пользователю и очиста списка сообщений
        /// </summary>
        public static void Show()
        {
            if (messages.Count==0)
            {
                return;
            }
            Windows.WindowInfo windowInfo = new Windows.WindowInfo(new Windows.MessagesViewModel (messages));
            messages.Clear();
            windowInfo.ShowDialog();
        }

        public static void AddMessage(string msg)
        {
            messages.Add(new Info (msg));
        }
    }
}

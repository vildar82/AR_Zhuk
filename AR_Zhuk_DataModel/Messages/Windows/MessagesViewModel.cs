using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_DataModel.Messages.Windows
{
    internal class MessagesViewModel
    {
        public ObservableCollection<Info> Messages { get; set; }

        public MessagesViewModel(List<Info> messages)
        {
            Messages = new ObservableCollection<Info>();
            foreach (var item in messages)
            {
                Messages.Add(item);
            }
        }
    }
}

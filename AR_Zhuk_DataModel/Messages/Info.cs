using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_DataModel.Messages
{
    internal class Info
    {
        public string Message { get; set; }

        public Info (string msg)
        {
            Message = msg;
        }

        public override string ToString ()
        {
            return Message;
        }
    }
}

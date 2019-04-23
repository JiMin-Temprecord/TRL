using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRL
{
    class WakeUpByteWritter : IByteWriter
    {
        public byte[] WriteBytes(byte[] sendMessage)
        {
            sendMessage[0] = 0X00;
            sendMessage[1] = 0X55;
            return sendMessage;
        }
    }
}

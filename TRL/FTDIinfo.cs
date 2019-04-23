using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRL
{
    public class FTDIInfo
    {
        public FTDIInfo (string portName, uint deviceID)
        {
            PortName = portName;
            DeviceID = deviceID;
        }

        public string PortName { get; }
        public uint DeviceID { get; }

    }
}

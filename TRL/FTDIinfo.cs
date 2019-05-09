using Windows.Devices.Usb;

namespace TRL
{
    public class FTDIInfo
    {
        public FTDIInfo (UsbDevice usbDevice, string ftdiID)
        {
            PortName = ftdiID;
            UsbDevice = usbDevice;
        }

        public string PortName { get; }
        public UsbDevice UsbDevice { get; }

    }
}

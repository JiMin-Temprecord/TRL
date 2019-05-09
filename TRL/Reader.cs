using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Usb;
using Windows.Foundation;

namespace TRL
{
    public class Reader
    {
        uint READER_VID = 0x0403;
        uint READER_PID = 0xD469;
        uint READER_PID1 = 0xD468;
        uint READER_PID2 = 0x6015;

        UsbDevice usbDevice = null;

        #region FindReader
        public UsbDevice FindReader()
        {
            InitializeTemprecordReaderDeviceWatcher(READER_VID, READER_PID);
            InitializeTemprecordReaderDeviceWatcher(READER_VID, READER_PID1);
            InitializeTemprecordReaderDeviceWatcher(READER_VID, READER_PID2);
            
            return usbDevice;
        }
        void InitializeTemprecordReaderDeviceWatcher(uint VID, uint PID)
        {
            var ReaderSelector = UsbDevice.GetDeviceSelector(VID, PID);
            var deviceWatcher = DeviceInformation.CreateWatcher(ReaderSelector);

            AddDeviceWatcher(deviceWatcher);
            deviceWatcher.Start();
        }
        void AddDeviceWatcher(DeviceWatcher deviceWatcher)
        {
            deviceWatcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>(this.OnDeviceAdded);
            deviceWatcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(this.OnDeviceRemoved);
        }
        void OnDeviceAdded(DeviceWatcher sender, DeviceInformation deviceInformation)
        {
            Debug.WriteLine(deviceInformation.Id + " was found.");
            SetupFTDI(deviceInformation.Id);
        }
        void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInformationUpdate)
        {
            Debug.WriteLine(deviceInformationUpdate.Id + " was removed.");
        }
        async void SetupFTDI(String deviceID)
        {
            try
            {
                Debug.WriteLine("Opened Device for Communication.");
                var temp = await UsbDevice.FromIdAsync(deviceID);
                if (temp != null)
                    usbDevice = temp;
                //var ftdiInfo = new FTDIInfo(usbDevice, deviceID);
            }

            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message.ToString());
            }

            finally
            {
                if (usbDevice != null)
                {
                    //This set up the 8-bit, no parity bit, one stop bit .... usb comms
                    var setupPacket = new UsbSetupPacket
                    {
                        RequestType = new UsbControlRequestType
                        {
                            Direction = UsbTransferDirection.Out,
                            Recipient = UsbControlRecipient.Device,
                            ControlTransferType = UsbControlTransferType.Vendor
                        },
                        Request = 0x03,
                        Value = 0x809C,
                        Length = 0
                    };
                    UInt32 FTDI_init = await usbDevice.SendControlOutTransferAsync(setupPacket);//.controlTransfer(0x40, 0x03, 0x809C, 0x0000, null, 0, 0);
                    Debug.WriteLine("F.T.D.I Initialized !");

                    //Need to send a second set for the reader with the transparent cable to work. This wakes up the FTDI chip in it
                    setupPacket = new UsbSetupPacket
                    {
                        RequestType = new UsbControlRequestType
                        {
                            Direction = UsbTransferDirection.Out,
                            Recipient = UsbControlRecipient.Device,
                            ControlTransferType = UsbControlTransferType.Vendor
                        },
                        Request = 0x01,
                        Value = 0x0101,
                        Length = 0
                    };
                    UInt32 bytesTransferred = await usbDevice.SendControlOutTransferAsync(setupPacket);//.controlTransfer(0x40, 0x01, 0x0101, 0x0000, null, 0, 0);
                    Debug.WriteLine("FTDI Wakeup (For the Transparent Cable Logger)");
                }
            }
        }
        #endregion
    }
}

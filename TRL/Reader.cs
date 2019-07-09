using System;
using System.Diagnostics;
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

        bool usbExist = false;
        bool goingones = false;

        UsbDevice usbDevice = null;
        UsbSetupPacket setupPacket = null;
        DeviceWatcher deviceWatcher = null;
        DeviceWatcher TempReader1Watcher = null;
        DeviceWatcher TempReader2Watcher = null;
        DeviceWatcher TempReader3Watcher = null;

        #region FindReader
        public UsbDevice FindReader()
        {
            if (goingones == false)
            {
                goingones = true;

                InitializeTemprecordReaderDeviceWatcher(READER_VID, READER_PID);

                InitializeTemprecordReaderDeviceWatcher(READER_VID, READER_PID1);

                InitializeTemprecordReaderDeviceWatcher(READER_VID, READER_PID2);
            }

            return usbDevice;
        }
        void InitializeTemprecordReaderDeviceWatcher(uint VID, uint PID)
        {

            switch (PID)
            {
                case 0xD469:
                    var ReaderSelector1 = UsbDevice.GetDeviceSelector(VID, PID);

                    TempReader1Watcher = DeviceInformation.CreateWatcher(ReaderSelector1);

                    AddDeviceWatcher(TempReader1Watcher);
                    TempReader1Watcher.Start();
                    break;
                case 0xD468:
                    var ReaderSelector2 = UsbDevice.GetDeviceSelector(VID, PID);

                    TempReader2Watcher = DeviceInformation.CreateWatcher(ReaderSelector2);

                    AddDeviceWatcher(TempReader2Watcher);
                    TempReader2Watcher.Start();
                    break;
                case 0x6015:
                    var ReaderSelector = UsbDevice.GetDeviceSelector(VID, PID);
                    TempReader3Watcher = DeviceInformation.CreateWatcher(ReaderSelector);

                    AddDeviceWatcher(TempReader3Watcher);
                    TempReader3Watcher.Start();
                    break;


            }
        }

        void AddDeviceWatcher(DeviceWatcher deviceWatcher)
        {
            deviceWatcher.Added += new TypedEventHandler<DeviceWatcher, DeviceInformation>(this.OnDeviceAdded);
            deviceWatcher.Removed += new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(this.OnDeviceRemoved);
        }
        void OnDeviceAdded(DeviceWatcher sender, DeviceInformation deviceInformation)
        {
            //Debug.WriteLine(deviceInformation.Id + " was found. ");
            setupPacket = null;
            usbExist = true;


            SetupFTDI(deviceInformation.Id);
        }

        void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInformationUpdate)
        {
            //Debug.WriteLine(deviceInformationUpdate.Id + " was removed. ");
            TempReader1Watcher.Stop();
            TempReader2Watcher.Stop();
            TempReader3Watcher.Stop();

            usbDevice = null;
            goingones = false;
            usbExist = false;
            
        }

        public UsbDevice GetUSBDevice()
        {

            return usbDevice;
        }

        async void SetupFTDI(String deviceID)
        {
            try
            {
                //Debug.WriteLine("Opened Device for Communication.");
                usbDevice = await UsbDevice.FromIdAsync(deviceID);
            }

            catch (Exception exception)
            {
                //Debug.WriteLine("USB READER : " + exception.Message.ToString());
            }

            finally
            {
                if (usbDevice != null)
                {
                    //This set up the 8-bit, no parity bit, one stop bit .... usb comms
                    setupPacket = new UsbSetupPacket
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
                    //Debug.WriteLine("F.T.D.I Initialized !");

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
                    //Debug.WriteLine("FTDI Wakeup (For the Transparent Cable Logger)");
                }
            }
        }
        #endregion

        public bool UsbExist { get { return usbExist; } set { usbExist = value; } }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
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
        SerialDevice serialDevice = null;
        UsbSetupPacket setupPacket = null;
        DeviceWatcher TempReader1Watcher = null;
        DeviceWatcher TempReader2Watcher = null;
        DeviceWatcher TempReader3Watcher = null;
        DeviceWatcher TemprecordSerialWatcher = null;

        string PortName = string.Empty;

        #region FindReader
        public SerialDevice FindReader()
        {

            if (goingones == false)
            {
                goingones = true;

                InitializeTemprecordReaderDeviceWatcher(READER_VID, READER_PID);

                InitializeTemprecordReaderDeviceWatcher(READER_VID, READER_PID1);

                InitializeTemprecordReaderDeviceWatcher(READER_VID, READER_PID2);

                InitializaSerialCommunication();
            }

            return serialDevice;
        }

        void InitializaSerialCommunication()
        {
            var deviceList = SerialDevice.GetDeviceSelector();
            TemprecordSerialWatcher = DeviceInformation.CreateWatcher(deviceList);
            AddDeviceWatcher(TemprecordSerialWatcher);
            TemprecordSerialWatcher.Start();
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


            SetupFTDI(deviceInformation);
        }

        void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceInformationUpdate)
        {
            //Debug.WriteLine(deviceInformationUpdate.Id + " was removed. ");
            TempReader1Watcher.Stop();
            TempReader2Watcher.Stop();
            TempReader3Watcher.Stop();
            TemprecordSerialWatcher.Stop();

            usbDevice = null;
            goingones = false;
            usbExist = false;
            
        }

        public UsbDevice GetUSBDevice()
        {
            return usbDevice;
        }

        public SerialDevice GetSerialDevice()
        {
            return serialDevice;
        }

        async void SetupFTDI(DeviceInformation deviceInformation)
        {
            Debug.WriteLine("deviceID : " + deviceInformation.Id);

            try
            {
                //usbDevice = await UsbDevice.FromIdAsync(deviceID);
                serialDevice = await SerialDevice.FromIdAsync(deviceInformation.Id);
            }

            catch (Exception exception)
            {
                Debug.WriteLine("USB READER : " + exception.Message.ToString());
            }

            finally
            {
                if (usbDevice != null)
                {
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
                    UInt32 FTDI_init = await usbDevice.SendControlOutTransferAsync(setupPacket);
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
                    UInt32 bytesTransferred = await usbDevice.SendControlOutTransferAsync(setupPacket);
                }

                else if (serialDevice != null)
                {
                    if (deviceInformation.Name == "USB Reader")
                    {
                        usbExist = true;
                        serialDevice.BaudRate = 19200;
                        serialDevice.Parity = SerialParity.None;
                        serialDevice.DataBits = 8;
                        serialDevice.StopBits = SerialStopBitCount.One;
                        serialDevice.Handshake = SerialHandshake.None;
                        serialDevice.IsDataTerminalReadyEnabled = true;

                        serialDevice.ReadTimeout = new TimeSpan(0, 0, 0, 0, 100);
                        serialDevice.WriteTimeout = new TimeSpan(0, 0, 0, 0, 100);
                    }
                }
            }
        }
        #endregion

        public bool UsbExist { get { return usbExist; } set { usbExist = value; } }
    }
}

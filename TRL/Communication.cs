﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Usb;
using Windows.Storage.Streams;

namespace TRL
{
    class Communication
    {
        int recoverCount = 0;
        int length = 0;
        bool readFull = true;
        readonly int maxlenreading = 58;
        List<byte> recievemsg;
        bool MonT2Overflown = false;
        
        public async Task<StringBuilder>FindLogger(Object communicationDevice, Reader reader)
        {
            SerialDevice serialDevice = reader.GetSerialDevice();
            UsbDevice usbDevice = reader.GetUSBDevice();
            
            var msg = new StringBuilder();

            while (msg.Length < 3)
            {
                if (reader.UsbExist)
                {
                    if (usbDevice != null)
                    {
                        await WriteBytes(new WakeUpByteWritter(), usbDevice, reader);
                        Task.Delay(70).Wait(); 
                        msg = await ReadBytes(usbDevice, reader);
                    }
                    else if (serialDevice != null)
                    {
                        await WriteBytes(new WakeUpByteWritter(), serialDevice, reader);
                        msg = await ReadBytes(serialDevice, reader);
                    }

                    if (msg.Length > 3 && (msg.ToString().Substring(0, 4) == "0004" || msg.ToString().Substring(0, 4) == "001d"))
                        break;
                    else
                    {
                        msg.Clear();
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    msg.Append("No Reader");
                    break;
                }
            }
            return msg;
        }
        public async Task<bool>GenerateHexFile(Object communicationDevice, LoggerInformation loggerInformation, Reader reader)
        {
            SerialDevice serialDevice = reader.GetSerialDevice();
            UsbDevice usbDevice = reader.GetUSBDevice();

            var Hexes = new List<Hex>();

             if(usbDevice != null)
                    Hexes = await ReadLogger(usbDevice, reader, loggerInformation, Hexes);

            else if(serialDevice != null)
                Hexes = await ReadLogger(serialDevice, reader, loggerInformation, Hexes);

            if (Hexes.Count > 0)
            {
                var path = Path.GetTempPath() + loggerInformation.SerialNumber + ".hex";
                var sw = new StreamWriter(path);
                foreach (var hex in Hexes)
                {
                    sw.WriteLine(hex.ToString());
                }
                sw.Close();
                return false;
            }

            return true;
        }
        async Task<List<Hex>>ReadLogger(SerialDevice serialDevice, Reader reader, LoggerInformation loggerInformation, List<Hex> Hexes)
        {
            var ReaderAvailable = true;

            recoverCount = 0;
            await WriteBytes(new WakeUpByteWritter(), serialDevice, reader);
            var currentAddress = await ReadBytesWakeUp(serialDevice, reader, loggerInformation, recievemsg, Hexes);
            ReaderAvailable = await WriteBytes(new SetReadByteWritter(loggerInformation.LoggerType), serialDevice, reader);
            if (readFull) await ReadBytesSetRead(serialDevice, reader, currentAddress, loggerInformation, Hexes);
            while (ReaderAvailable && currentAddress != null && (currentAddress.MemoryNumber <= loggerInformation.MaxMemory))
            {
                if (readFull == true && (length >= (loggerInformation.MemoryMax[currentAddress.MemoryNumber] - currentAddress.MemoryAddress)))
                {
                    recoverCount = 0;
                    length = loggerInformation.MemoryMax[currentAddress.MemoryNumber] - currentAddress.MemoryAddress;
                    currentAddress.MemoryAddress = loggerInformation.MemoryMax[currentAddress.MemoryNumber];
                    currentAddress.LengthMSB = (byte)((length >> 8) & 0xff);
                    currentAddress.LengthLSB = (byte)(length & 0xff);
                }

                ReaderAvailable = await WriteBytes(new ReadLoggerByteWritter(currentAddress,loggerInformation.LoggerType), serialDevice, reader);
                readFull = await ReadBytesReadLogger(serialDevice, reader, loggerInformation.LoggerType, currentAddress, Hexes);

                if (readFull == true)
                {
                    currentAddress = GetNextAddress(currentAddress, loggerInformation);
                }

                if (recoverCount == 20)
                {
                    Hexes.Clear();
                    return Hexes;
                }
            }

            if (currentAddress == null)
            {
                Hexes.Clear();
                return Hexes;
            }

            return Hexes;
        }
        async Task<List<Hex>> ReadLogger(UsbDevice usbDevice, Reader reader, LoggerInformation loggerInformation, List<Hex> Hexes)
        {
            var ReaderAvailable = true;

            recoverCount = 0;
            await WriteBytes(new WakeUpByteWritter(), usbDevice, reader);
            Task.Delay(70).Wait(); 
            var currentAddress = await ReadBytesWakeUp(usbDevice, reader, loggerInformation, recievemsg, Hexes);
            ReaderAvailable = await WriteBytes(new SetReadByteWritter(loggerInformation.LoggerType), usbDevice, reader);
            Task.Delay(70).Wait(); 
            if (readFull) await ReadBytesSetRead(usbDevice, reader, currentAddress, loggerInformation, Hexes);
            while (ReaderAvailable && currentAddress != null && (currentAddress.MemoryNumber <= loggerInformation.MaxMemory))
            {
                if (readFull == true && (length >= (loggerInformation.MemoryMax[currentAddress.MemoryNumber] - currentAddress.MemoryAddress)))
                {
                    recoverCount = 0;
                    length = loggerInformation.MemoryMax[currentAddress.MemoryNumber] - currentAddress.MemoryAddress;
                    currentAddress.MemoryAddress = loggerInformation.MemoryMax[currentAddress.MemoryNumber];
                    currentAddress.LengthMSB = (byte)((length >> 8) & 0xff);
                    currentAddress.LengthLSB = (byte)(length & 0xff);
                }

                ReaderAvailable = await WriteBytes(new ReadLoggerByteWritter(currentAddress, loggerInformation.LoggerType), usbDevice, reader);
                Task.Delay(70).Wait();
                readFull = await ReadBytesReadLogger(usbDevice, reader, loggerInformation.LoggerType, currentAddress, Hexes);
                if (readFull == true)
                {
                    currentAddress = GetNextAddress(currentAddress, loggerInformation);
                }

                if (recoverCount == 20)
                {
                    Hexes.Clear();
                    return Hexes;
                }
            }

            if (currentAddress == null)
            {
                Hexes.Clear();
                return Hexes;
            }

            return Hexes;
        }
        async Task<StringBuilder> ReadBytes(SerialDevice serialDevice, Reader reader)
        {
            var msg = new StringBuilder();
            recievemsg = new List<byte>();
            var stream = serialDevice.InputStream;
            var readerStream = new DataReader(stream);

            try
            {
                var timeoutSource = new CancellationTokenSource(100); // 1000 ms
                var byteData = await readerStream.LoadAsync(70);//.AsTask(timeoutSource.Token);
                var bufferData = readerStream.ReadBuffer(byteData);
                if (bufferData.GetByte(0) == 0x00)
                {
                    for (uint i = 0; i < bufferData.Length; i++)
                        recievemsg.Add(bufferData.GetByte(i));
                }
            }
            catch (TaskCanceledException e)
            {
                readFull = false;
                recoverCount++;
                Debug.WriteLine("recoverCount : " + recoverCount);
                if (recoverCount == 20)
                    return msg.Clear();
            }
            catch (Exception e)
            {
                readFull = false;
                if (e is TimeoutException || e is NotImplementedException)
                {
                    recoverCount++;
                    Debug.WriteLine("recoverCount : " + recoverCount);
                    if (recoverCount == 20)
                        return msg.Clear();
                }

                else if (e is IOException || e is InvalidOperationException)
                {
                    msg.Clear();
                    return msg.Append("Exception");
                }
            }
            finally
            {
                readerStream.DetachStream();
                readerStream.Dispose();
            }
            recievemsg.Add(0x0d);
            recievemsg = RemoveEscChar(recievemsg);

            for (int i = 0; i < recievemsg.Count; i++)
            {
                msg = msg.Append(recievemsg[i].ToString("x02"));
            }

            Debug.WriteLine("MSG : " + msg);
            return msg;
        }
        async Task<StringBuilder> ReadBytes(UsbDevice usbDevice, Reader reader)
        {
            var msg = new StringBuilder();
            recievemsg = new List<byte>();

            UInt32 bytesRead = 0;
            var readPipe = usbDevice.DefaultInterface.BulkInPipes[0];
            var stream = readPipe.InputStream;
            var readerStream = new DataReader(stream);

            try
            {
                bytesRead = await readerStream.LoadAsync(70);
            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                {
                    recoverCount++;
                    Debug.WriteLine("recoverCount : " + recoverCount);
                    if (recoverCount == 20)
                        return msg.Clear();
                }

                else if (e is IOException || e is InvalidOperationException)
                {
                    msg.Clear();
                    return msg.Append("Exception");
                }
            }

            try
            {

                if (reader.UsbExist)
                {
                    IBuffer buffer = readerStream.ReadBuffer(bytesRead);
                    readPipe.FlushBuffer();
                
                    //FTDI Error Checking
                    if ((buffer.GetByte(0) == 0x01) && (buffer.GetByte(1) == 0x60))
                    {
                        if (buffer.Length > 2 && buffer.GetByte(2) == 0x00)
                        {
                            //Filtering out the 0x00 at index 0 and 0x60 at index 1 of the FTDI reply 
                            for (uint i = 2; i < buffer.Length; i++)
                            {
                                recievemsg.Add(buffer.GetByte(i));
                            }
                        }
                        else
                        {
                            recoverCount++;
                            Debug.WriteLine("recoverCount : " + recoverCount);
                            if (recoverCount == 20)
                                return msg.Clear();
                        }
                    }
                    else
                    {
                        Debug.WriteLine(" Debug : " + buffer.GetByte(0).ToString() + buffer.GetByte(1).ToString());
                        //reply might contain 0x01 at index 0 and 0x00 at index 1
                        Debug.WriteLine("Invalid Reply");
                    }
                }
            }
            catch (SystemException e)
            {
                Debug.WriteLine(e);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            recievemsg.Add(0x0d);
            recievemsg = RemoveEscChar(recievemsg);

            for (int i = 0; i < recievemsg.Count; i++)
            {
                msg = msg.Append(recievemsg[i].ToString("x02"));
            }

            Debug.WriteLine("MSG : " + msg);
            return msg;
        }
        async Task<AddressSection> ReadBytesWakeUp(SerialDevice serialDevice, Reader reader, LoggerInformation loggerInformation, List<byte> messageReceived, List<Hex> hexes)
        {
            SetLoggerInformation(messageReceived, loggerInformation);
            return await SetCurrentAddress(serialDevice, reader, loggerInformation, hexes);
        }
        async Task<AddressSection> ReadBytesWakeUp(UsbDevice usbDevice, Reader reader, LoggerInformation loggerInformation, List<byte> messageReceived, List<Hex> hexes)
        {
            SetLoggerInformation(messageReceived, loggerInformation);
            return await SetCurrentAddress(usbDevice, reader, loggerInformation, hexes);
        }
        async Task ReadBytesSetRead(SerialDevice serialDevice, Reader reader, AddressSection currentAddress, LoggerInformation loggerInformation, List<Hex> hexes)
        {
            var msg = await ReadBytes(serialDevice, reader);
            length = maxlenreading;

            try
            {
                switch (loggerInformation.LoggerType)
                {
                    //MonT2
                    case 1:
                        loggerInformation.MemoryStart[8] = 0x0000;
                        loggerInformation.MemoryMax[8] = ((recievemsg[13] << 8 | (recievemsg[12])) * 2);
                        if (MonT2Overflown)
                            loggerInformation.MemoryMax[8] = 0x81c0;
                        break;
                    //MonT
                    case 3:
                        loggerInformation.MemoryStart[0] = 0x0000;
                        loggerInformation.MemoryMax[0] = 0x2000;
                        break;
                    //G4
                    case 6:
                        loggerInformation.MemoryStart[4] = (recievemsg[loggerInformation.RequestMemoryStartPointer + 1]) << 8 | (recievemsg[loggerInformation.RequestMemoryStartPointer]);
                        loggerInformation.MemoryMax[4] = (recievemsg[loggerInformation.RequestMemoryMaxPointer + 1]) << 8 | (recievemsg[loggerInformation.RequestMemoryMaxPointer]);

                        if (loggerInformation.MemoryStart[4] > loggerInformation.MemoryMax[4])
                        {
                            hexes.Add(new Hex("FD0000", loggerInformation.MemoryStart[4].ToString("X04")));
                            loggerInformation.MemoryStart[4] = 0x0000;
                            loggerInformation.MemoryMax[4] = 0x8000;
                        }
                        else
                        {
                            hexes.Add(new Hex("FD0000", "0000"));
                        }

                        if (loggerInformation.MemoryMax[4] < 80)
                        {
                            loggerInformation.MemoryMax[4] = 80;
                        }
                        break;
                }
                currentAddress.LengthMSB = (byte)((length >> 8) & 0xff);
                currentAddress.LengthLSB = (byte)(length & 0xff);
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine(e);
            }
        }
        async Task ReadBytesSetRead(UsbDevice usbDevice, Reader reader, AddressSection currentAddress, LoggerInformation loggerInformation, List<Hex> hexes)
        {
            var msg = await ReadBytes(usbDevice, reader);
            length = maxlenreading;

            try
            {
                switch (loggerInformation.LoggerType)
                {
                    //MonT2
                    case 1:
                        loggerInformation.MemoryStart[8] = 0x0000;
                        loggerInformation.MemoryMax[8] = ((recievemsg[13] << 8 | (recievemsg[12])) * 2);
                        if (MonT2Overflown)
                            loggerInformation.MemoryMax[8] = 0x81c0;
                        break;
                    //MonT
                    case 3:
                        loggerInformation.MemoryStart[0] = 0x0000;
                        loggerInformation.MemoryMax[0] = 0x2000;
                        break;
                    //G4
                    case 6:
                        loggerInformation.MemoryStart[4] = (recievemsg[loggerInformation.RequestMemoryStartPointer + 1]) << 8 | (recievemsg[loggerInformation.RequestMemoryStartPointer]);
                        loggerInformation.MemoryMax[4] = (recievemsg[loggerInformation.RequestMemoryMaxPointer + 1]) << 8 | (recievemsg[loggerInformation.RequestMemoryMaxPointer]);

                        if (loggerInformation.MemoryStart[4] > loggerInformation.MemoryMax[4])
                        {
                            hexes.Add(new Hex("FD0000", loggerInformation.MemoryStart[4].ToString("X04")));
                            loggerInformation.MemoryStart[4] = 0x0000;
                            loggerInformation.MemoryMax[4] = 0x8000;
                        }
                        else
                        {
                            hexes.Add(new Hex("FD0000", "0000"));
                        }

                        if (loggerInformation.MemoryMax[4] < 80)
                        {
                            loggerInformation.MemoryMax[4] = 80;
                        }
                        break;
                }
                currentAddress.LengthMSB = (byte)((length >> 8) & 0xff);
                currentAddress.LengthLSB = (byte)(length & 0xff);
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine(e);
            }
        }
        async Task<bool> ReadBytesReadLogger(SerialDevice serialDevice, Reader reader, int loggerType, AddressSection currentAddress, List<Hex> Hexes)
        {
            length = maxlenreading;
            var msg = await ReadBytes(serialDevice, reader);
            var addressRead = "0" + currentAddress.MemoryNumber + currentAddress.MemoryAddMSB.ToString("x02") + currentAddress.MemoryAddLSB.ToString("x02");

            if ((recievemsg.Count > 8) && (recievemsg[0] == 0x00))
            {
                var finalmsg = string.Empty;

                if (msg.Length > 124)
                {
                    if (loggerType == 1)
                    {
                        finalmsg = msg.ToString(12, 116);
                    }
                    else
                    {
                        finalmsg = msg.ToString(2, 116);
                    }
                }
                else if (msg.Length < (currentAddress.LengthLSB * 2) + 6 && currentAddress.MemoryNumber == 4)
                {
                    recoverCount++;
                    return false;
                }
                else
                {
                    if (loggerType == 1)
                        finalmsg = msg.ToString(12, msg.Length - 8);
                    else
                        finalmsg = msg.ToString(2, msg.Length - 8);
                }
                Hexes.Add(new Hex(addressRead, finalmsg));
            }
            else if (loggerType == 1 && currentAddress.MemoryNumber != 8)
                return true;
            else
            {
                return false;
            }
            currentAddress.LengthMSB = (byte)((length >> 8) & 0xff);
            currentAddress.LengthLSB = (byte)(length & 0xff);
            return true;
        }
        async Task<bool> ReadBytesReadLogger(UsbDevice usbDevice, Reader reader, int loggerType, AddressSection currentAddress, List<Hex> Hexes)
        {
            length = maxlenreading;
            var msg = await ReadBytes(usbDevice, reader);
            var addressRead = "0" + currentAddress.MemoryNumber + currentAddress.MemoryAddMSB.ToString("x02") + currentAddress.MemoryAddLSB.ToString("x02");

            if ((recievemsg.Count > 8) && (recievemsg[0] == 0x00)) // 0x00 ACK
            {
                var finalmsg = string.Empty;

                if (msg.Length > 124)
                {
                    if (loggerType == 1)
                    {
                        var index = msg.ToString().IndexOf("0160");
                        finalmsg = msg.ToString(12, index - 12);
                        finalmsg += msg.ToString(index + 4, 116 - finalmsg.Length);
                    }
                    else
                    {
                        finalmsg = msg.ToString(2, 116);
                    }
                }
                else if (msg.Length < (currentAddress.LengthLSB * 2) + 6 && currentAddress.MemoryNumber == 4)
                {
                    recoverCount++;
                    return false;
                }
                else
                {
                    if (loggerType == 1)
                        finalmsg = msg.ToString(12, msg.Length - 8);
                    else
                        finalmsg = msg.ToString(2, msg.Length - 8);
                }
                Hexes.Add(new Hex(addressRead, finalmsg));
            }
            else if (loggerType == 1 && currentAddress.MemoryNumber != 8)
                return true;
            else
            {
                return false;
            }
            currentAddress.LengthMSB = (byte)((length >> 8) & 0xff);
            currentAddress.LengthLSB = (byte)(length & 0xff);
            return true;
        }
        async Task<bool> WriteBytes(IByteWriter byteWriter, SerialDevice serialDevice, Reader reader)
        {
            var sendMessage = new byte[11];
            var stream = serialDevice.OutputStream;
            var writer = new DataWriter(stream);

            sendMessage = byteWriter.WriteBytes(sendMessage);

            for (int i = 0; i < sendMessage.Length; i++)
                Debug.Write(sendMessage[i].ToString("x02") + "-");
            Debug.WriteLine("");

            writer.WriteBytes(sendMessage);

            try
            {
                if (reader.UsbExist)
                {
                    await writer.StoreAsync();
                    writer.DetachStream();
                    writer.Dispose();
                }
            }
            catch (System.Runtime.InteropServices.SEHException e)
            {
                await serialDevice.OutputStream.FlushAsync();
                writer.DetachStream();
                writer.Dispose();
                return false;
            }
            catch (Exception e)
            {

                if (e is IOException || e is InvalidOperationException)
                {
                    await serialDevice.OutputStream.FlushAsync();
                    writer.DetachStream();
                    writer.Dispose();
                    return false;
                }
            }
            return true;

        }
        async Task<bool> WriteBytes(IByteWriter byteWriter, UsbDevice usbDevice, Reader reader)
        {
            var sendMessage = new byte[11];
            var writePipe = usbDevice.DefaultInterface.BulkOutPipes[0];
            var stream = writePipe.OutputStream;
            var writer = new DataWriter(stream);

            sendMessage = byteWriter.WriteBytes(sendMessage);

            for (int i = 0; i < sendMessage.Length; i++)
                Debug.Write(sendMessage[i].ToString("x02") + "-");
            Debug.WriteLine("");

            writer.WriteBytes(sendMessage);

            try
            {
                if (reader.UsbExist)
                {
                    await writer.StoreAsync();
                    writer.DetachStream();
                    writer.Dispose();
                }
            }
            catch (System.Runtime.InteropServices.SEHException e)
            {
                await writePipe.OutputStream.FlushAsync();
                writer.DetachStream();
                writer.Dispose();
                return false;
            }
            catch (Exception e)
            {

                if (e is IOException || e is InvalidOperationException)
                {
                    await writePipe.OutputStream.FlushAsync();
                    writer.DetachStream();
                    writer.Dispose();
                    return false;
                }
            }
            return true;

        }
        async Task<AddressSection> SetCurrentAddress(SerialDevice serialDevice, Reader reader, LoggerInformation loggerInformation, List<Hex> hexes)
        {
            length = maxlenreading;
            var msg = await ReadBytes(serialDevice, reader);

            var timenow = "0000000000000000";
            var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000;
            var hextime = time.ToString("x02");
            timenow = timenow.Substring(0, (timenow.Length - hextime.Length)) + hextime;
            hexes.Add(new Hex("FF0000", timenow));

            if (msg.Length > 8)
            {
                if (loggerInformation.LoggerType == 1)
                {
                    hexes.Add(new Hex("FE0000", msg.ToString(0, (msg.Length - 6))));
                    return new AddressSection(0, 0, 0, 0x00, 0x00, 0);
                }
                else
                {
                    hexes.Add(new Hex("FE0000", msg.ToString(0, (msg.Length - 6))));
                    var memoryAddress = (recievemsg[loggerInformation.MemoryHeaderPointer + 1] & 0xFF) << 8 | (recievemsg[loggerInformation.MemoryHeaderPointer] & 0xFF);
                    var memoryAddMSB = recievemsg[loggerInformation.MemoryHeaderPointer + 1];
                    var memoryAddLSB = recievemsg[loggerInformation.MemoryHeaderPointer];
                    var lengthMSB = (byte)((length >> 8) & 0xFF);
                    var lengthLSB = (byte)(length & 0xFF);

                    return new AddressSection(lengthLSB, lengthMSB, 0, memoryAddLSB, memoryAddMSB, memoryAddress);
                }
            }

            return null;
        }
        async Task<AddressSection> SetCurrentAddress(UsbDevice usbDevice, Reader reader, LoggerInformation loggerInformation, List<Hex> hexes)
        {
            length = maxlenreading;
            var msg = await ReadBytes(usbDevice, reader);

            var timenow = "0000000000000000";
            var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000;
            var hextime = time.ToString("x02");
            timenow = timenow.Substring(0, (timenow.Length - hextime.Length)) + hextime;
            hexes.Add(new Hex("FF0000", timenow));

            if (msg.Length > 8)
            {
                if (loggerInformation.LoggerType == 1)
                {
                    hexes.Add(new Hex("FE0000", msg.ToString(0, (msg.Length - 6))));
                    return new AddressSection(0, 0, 0, 0x00, 0x00, 0);
                }
                else
                {
                    hexes.Add(new Hex("FE0000", msg.ToString(0, (msg.Length - 6))));
                    var memoryAddress = (recievemsg[loggerInformation.MemoryHeaderPointer + 1] & 0xFF) << 8 | (recievemsg[loggerInformation.MemoryHeaderPointer] & 0xFF);
                    var memoryAddMSB = recievemsg[loggerInformation.MemoryHeaderPointer + 1];
                    var memoryAddLSB = recievemsg[loggerInformation.MemoryHeaderPointer];
                    var lengthMSB = (byte)((length >> 8) & 0xFF);
                    var lengthLSB = (byte)(length & 0xFF);

                    return new AddressSection(lengthLSB, lengthMSB, 0, memoryAddLSB, memoryAddMSB, memoryAddress);
                }
            }

            return null;
        }
        void SetLoggerInformation(List<byte> messageReceived, LoggerInformation loggerInformation)
        {
            byte[] serial = { messageReceived[5], messageReceived[6], messageReceived[7], messageReceived[8] };
            loggerInformation.SerialNumber = GetSerialnumber(serial);

            switch (messageReceived[2])
            {
                case 1:
                    loggerInformation.LoggerName = "Mon T2";
                    loggerInformation.LoggerType = 1;
                    loggerInformation.JsonFile = "MonT2.json";
                    loggerInformation.MaxMemory = 0x08;                                          //MON-T2
                    loggerInformation.MemoryStart = new int[] { 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000 }; //MON-T2
                    loggerInformation.MemoryMax = new int[] { 0x0000, 0x0074, 0x0200, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x8000 };    //MON-T2

                    loggerInformation.SerialNumber = GetMonT2Serialnumber(messageReceived);

                    if ((recievemsg[23] >> 3 & 1) == 0)
                        MonT2Overflown = true;
                    break;

                case 3:
                    loggerInformation.LoggerName = "Mon T";
                    loggerInformation.LoggerType = 3;
                    loggerInformation.JsonFile = "MonT.json";
                    loggerInformation.MaxMemory = 0x02;                                                 //MON-T
                    loggerInformation.MemoryHeaderPointer = 19;                                                   //MON-T
                    loggerInformation.MemoryStart = new int[] { 0x0000, 0x0020, 0x0000, 0x0000, 0x0000 };    //MON-T
                    loggerInformation.MemoryMax = new int[] { 0x2000, 0x0100, 0x0000, 0x0000, 0x2000 };    //MON-T
                    loggerInformation.RequestMemoryStartPointer = 3;
                    loggerInformation.RequestMemoryMaxPointer = 1;
                    break;

                case 6:
                    loggerInformation.LoggerName = "G4";
                    loggerInformation.LoggerType = 6;
                    loggerInformation.JsonFile = "G4.json";
                    loggerInformation.MaxMemory = 0x04;                                                 //G4
                    loggerInformation.MemoryHeaderPointer = 13;                                                   //G4
                    loggerInformation.MemoryStart = new int[] { 0x0000, 0x0020, 0x0000, 0x0000, 0x0000 };    //G4
                    loggerInformation.MemoryMax = new int[] { 0x353C, 0x0100, 0x0000, 0x0000, 0x8000 };    //G4
                    loggerInformation.RequestMemoryStartPointer = 3;
                    loggerInformation.RequestMemoryMaxPointer = 1;
                    break;
            }
        }
        AddressSection GetNextAddress(AddressSection currentAddress, LoggerInformation loggerInformation)
        {
            if (currentAddress.MemoryAddress < loggerInformation.MemoryMax[currentAddress.MemoryNumber])
            {
                currentAddress.MemoryAddress = (((currentAddress.MemoryAddMSB & 0xff) << 8) | (currentAddress.MemoryAddLSB & 0xff));
                currentAddress.MemoryAddress += length;

                if (currentAddress.MemoryAddress >= loggerInformation.MemoryMax[currentAddress.MemoryNumber])
                {
                    length = length - currentAddress.MemoryAddress + loggerInformation.MemoryMax[currentAddress.MemoryNumber];
                    currentAddress.MemoryAddMSB = (byte)((loggerInformation.MemoryMax[currentAddress.MemoryNumber] >> 8) & 0xff);
                    currentAddress.MemoryAddLSB = (byte)(loggerInformation.MemoryMax[currentAddress.MemoryNumber] & 0xff);
                }
                else
                {
                    currentAddress.MemoryAddMSB = (byte)((currentAddress.MemoryAddress >> 8) & 0xff);
                    currentAddress.MemoryAddLSB = (byte)((currentAddress.MemoryAddress) & 0xff);
                }
                return currentAddress;
            }
            else
            {
                while (currentAddress.MemoryNumber < loggerInformation.MaxMemory)
                {
                    currentAddress.MemoryNumber++;

                    if (loggerInformation.MemoryMax[currentAddress.MemoryNumber] != 0)
                    {
                        if (loggerInformation.MemoryMax[currentAddress.MemoryNumber] > loggerInformation.MemoryStart[currentAddress.MemoryNumber])
                        {
                            length = maxlenreading;
                            currentAddress.MemoryAddress = loggerInformation.MemoryStart[currentAddress.MemoryNumber];
                            currentAddress.MemoryAddMSB = (byte)((currentAddress.MemoryAddress >> 8) & 0xff);
                            currentAddress.MemoryAddLSB = (byte)(currentAddress.MemoryAddress & 0xff);
                            return currentAddress;
                        }
                    }

                }
            }
            
            currentAddress.MemoryNumber++;
            return currentAddress;
        }
        #region Byte Mainpulation 
        public static byte[] AddCRC(int len, byte[] sendMessage)
        {
            var crc16 = 0xFFFF;

            for (int i = 0; i < len; i++)
            {
                crc16 = (UInt16)(crc16 ^ (Convert.ToUInt16(sendMessage[i]) << 8));
                for (int j = 0; j < 8; j++)
                {
                    if ((crc16 & 0x8000) == 0x8000)
                    {
                        crc16 = (UInt16)((crc16 << 1) ^ 0x1021);
                    }
                    else
                    {
                        crc16 <<= 1;
                    }
                }
            }

            sendMessage[len++] = (byte)crc16;
            sendMessage[len++] = (byte)(crc16 >> 8);
            sendMessage[len++] = 0x0d;
            return AddEscChar(len - 1, sendMessage);
        }
        static byte[] AddEscChar(int Length, byte[] sendMessage)
        {
            int mx = 0;
            byte[] temp = new byte[80];

            for (int i = 0; i < Length; i++)
            {
                if (sendMessage[i] == 0x1B) // 1B = 27
                {
                    temp[i + mx] = 0x1b; // 1B = 27
                    mx++;
                    temp[i + mx] = 0x00;
                }

                else if (sendMessage[i] == 0x0D) // 1D = 29
                {
                    temp[i + mx] = 0x1b; // 1B = 27 
                    mx++;
                    temp[i + mx] = 0x01;

                }
                else if (sendMessage[i] == 0x55) // 55 = 85
                {
                    temp[i + mx] = 0x1b; // 1B = 27
                    mx++;
                    temp[i + mx] = 0x02;
                }
                else
                {
                    temp[i + mx] = sendMessage[i];
                }
            }

            temp[Length + mx] = 0x0d;
            sendMessage = new byte[Length + mx + 1];
            Array.Copy(temp, sendMessage, Length + mx + 1);

            return sendMessage;
        }
        List<byte> RemoveEscChar(List<byte> message)
        {
            int i = 0;
            int mx = 0;

            while ((i < message.Count) && (message[i] != 0x0d))
            {
                if (message[i] == 0x1B) // 1B = 27
                {
                    switch (message[i + 1])
                    {
                        case 0x00:
                            message[mx] = 0x1B; // 1B = 27
                            i++;
                            break;

                        case 0x01:
                            message[mx] = 0x0D;  // 1D = 29
                            i++;
                            break;

                        case 0x02:
                            message[mx] = 0x55; // 55 = 85
                            i++;
                            break;
                    }
                }
                else
                {
                    message[mx] = message[i];
                }

                mx++;
                i++;
            }
            message[mx] = 0x0d;

            var temp = message;

            return temp;
        }
        #endregion
        string GetMonT2Serialnumber(List<byte> msg)
        {
            var serialNumber = string.Empty;

            for (int i = 8; i < 18; i++)
            {
                serialNumber += Convert.ToChar(msg[i]);
            }

            return serialNumber;
        }
        string GetSerialnumber(byte[] msg)
        {
            var serialnumber = "";

            if ((msg[3] & 0xF0) == 0x50)
            {
                serialnumber = "L";

                switch (msg[3] & 0x0F)
                {
                    case 0x00:
                        serialnumber += "0";
                        break;
                    case 0x07:
                        serialnumber += "T";
                        break;
                    case 0x08:
                        serialnumber += "G";
                        break;
                    case 0x09:
                        serialnumber += "H";
                        break;
                    case 0x0A:
                        serialnumber += "P";
                        break;
                    case 0x0C:
                        serialnumber += "M";
                        break;
                    case 0x0D:
                        serialnumber += "S";
                        break;
                    case 0x0E:
                        serialnumber += "X";
                        break;
                    case 0x0F:
                        serialnumber += "C";
                        break;
                    default:
                        serialnumber = "L-------";
                        break;
                }
            }
            else if ((msg[3] & 0xF0) == 0x60)//For MonT
            {
                serialnumber = "R0";
            }
            else
            {
                serialnumber = "--------";
            }

            var number = (((msg[2] & 0xFF) << 16) | ((msg[1] & 0xFF) << 8) | (msg[0]) & 0xFF).ToString();
            while (number.Length < 6)
                number = "0" + number;
            serialnumber += number.ToString();

            return serialnumber;
        }
    }
}

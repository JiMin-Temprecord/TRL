using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace TRL
{
    public class HexFileDecoder
    {
        string loggerName;
        string loggerState;
        string batteryPercentage;
        string userData = string.Empty;
        string emailID = string.Empty;

        bool loopOverwrite = false;
        bool fahrenheit = false;
        bool notOverflown = false;
        bool ch1Enabled = false;
        bool ch2Enabled = false;

        long samplePeriod = 0;
        long secondsTimer = 0;
        long utcReferenceTime = 0;
        long timeAtFirstSameple = 0;
        long ticksAtLastSample = 0;
        long ticksSinceStart = 0;
        long manufactureDate = 0;

        int numberChannel = 0;
        int userDataLength = 0;
        int startDelay = 0;
        int totalRTCticks = 0;
        int totalSamplingEvents = 0;
        int totalUses = 0;
        int loopOverwriteStartAddress = 0;
        int dataAddress = 32767;
        int recordedSamples = 0;
        int mont2Samples = 0;
        int tagNumbers = 0;
        int samplePointer = 0;
        int sensorNumber = 0;

        int[] compressionTable = new int[128];

        double lowestTemp = 0;
        double resolution = 0;
        readonly int[] highestPosition = new int[8];
        readonly int[] lowestPosition = new int[8];
        readonly int[] sensorStartingValue = { 0, 0, 0, 0, 0, 0, 0, 0 };
        readonly int[] sensorTablePointer = { 0, 0, 0, 0, 0, 0, 0, 0 };
        readonly int[] sensorType = { 0, 0, 0, 0, 0, 0, 0, 0 };

        double[] upperLimit = new double[8];
        double[] lowerLimit = new double[8];
        readonly double[] sensorMin = new double[8];
        readonly double[] sensorMax = new double[8];
        readonly double[] mean = new double[8];
        readonly double[] mkt = new double[8];
        readonly double[] withinLimit = new double[8];
        readonly double[] belowLimit = new double[8];
        readonly double[] aboveLimit = new double[8];

        List<List<double>> Data = new List<List<double>>();
        List<int> Tag = new List<int>();

        readonly int Kelvin = 27315;
        readonly double KelvinDec = 273.15;
        readonly double Delta_H = 83.14472;     //Delta H   is the Activation Energy:   83.14472    kJ/mole
        readonly double R = 8.314472;           //R is the Gas Constant: 0.008314472 J/mole/degree
        readonly long Year2000 = 946684800000;
        readonly long Year2010 = 1262304000000;
        readonly int G4MemorySize = 32767;

        readonly LoggerInformation loggerInformation;
        readonly string serialNumber;
        readonly string jsonFile;

        public HexFileDecoder(LoggerInformation loggerInformation)
        {
            this.loggerInformation = loggerInformation;
            this.serialNumber = loggerInformation.SerialNumber;
            this.jsonFile = loggerInformation.JsonFile;
        }

        public async Task<bool> SetUpDecoderUsingJsonFile()
        {
            var jsonObject = await GetJsonObject();
            var hexString = await GetHexFile();
            var hexStringArray = hexString.Split("\r\n");

            if (loggerInformation.LoggerName == DecodeConstant.G4)
            {
                loggerName = "G4";
                userDataLength = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.UserDataLength);
                numberChannel = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.NumberOfChannels);
                emailID = await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.EmailID);
                userData = await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.UserData);
                loggerState = await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.LoggerState);
                batteryPercentage = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.BatteryPercentage) + DecodeConstant.Percentage;
                loopOverwriteStartAddress = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.LoopOverwriteAddress);
                fahrenheit = await ReadBoolFromJObject(jsonObject, hexStringArray, DecodeConstant.IsFahrenhiet);
                utcReferenceTime = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.UTCReferenceTime);
                totalRTCticks = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TotalRTCTicks);
                totalSamplingEvents = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TotalSamplingEvents);
                totalUses = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TotalUses);
                startDelay = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.StartDelay);
                samplePeriod = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.SamplePeriod);
                ticksSinceStart = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TicksSinceStart);
                ticksAtLastSample = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TicksSinceLastSample);
                recordedSamples = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TotalRecordedSamples);
                timeAtFirstSameple = await ReadLongFromJObject(jsonObject, hexStringArray, DecodeConstant.TimeAtFirstSample);
                await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.CompressionTable);
                await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.Sensor);
                dataAddress = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.DataEndPointer);
                lowerLimit = await ReadArrayFromJObject(jsonObject, hexStringArray, DecodeConstant.LowerLimit);
                upperLimit = await ReadArrayFromJObject(jsonObject, hexStringArray, DecodeConstant.UpperLimit);
                await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.Data);
                return true;
            }

            if (loggerInformation.LoggerName == DecodeConstant.MonT)
            {
                loggerName = "MonT";
                numberChannel = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.NumberOfChannels);
                userDataLength = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.UserDataLength);
                userData = await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.UserData);
                loggerState = await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.LoggerState);
                fahrenheit = await ReadBoolFromJObject(jsonObject, hexStringArray, DecodeConstant.IsFahrenhiet);
                utcReferenceTime = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.UTCReferenceTime);
                totalRTCticks = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TotalRTCTicks);
                manufactureDate = await ReadLongFromJObject(jsonObject, hexStringArray, DecodeConstant.ManufactureDate);
                totalSamplingEvents = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TotalSamplingEvents);
                totalUses = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TotalUses);
                batteryPercentage = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.BatteryPercentage) + DecodeConstant.Percentage;
                loopOverwrite = await ReadBoolFromJObject(jsonObject, hexStringArray, DecodeConstant.IsLoopOverwrite);
                startDelay = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.StartDelay);
                samplePeriod = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.SamplePeriod);
                secondsTimer = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.SecondTimer);
                ticksSinceStart = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TicksSinceStart);
                ticksAtLastSample = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TicksSinceLastSample);
                timeAtFirstSameple = await ReadLongFromJObject(jsonObject, hexStringArray, DecodeConstant.MonTTimeAtFirstSample);
                lowestTemp = Convert.ToDouble(await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.LowestTemp));
                resolution = Convert.ToDouble(await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.ResolutionRatio)) / 100;
                recordedSamples = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TotalRecordedSamples);
                lowerLimit = await ReadArrayFromJObject(jsonObject, hexStringArray, DecodeConstant.LowerLimit);
                upperLimit = await ReadArrayFromJObject(jsonObject, hexStringArray, DecodeConstant.UpperLimit);
                await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.MonTData);
                return true;
            }

            if (loggerInformation.LoggerName == DecodeConstant.MonT2)
            {
                loggerName = await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.MonT2Model);
                numberChannel = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.NumberOfChannels);
                ch1Enabled = await ReadBoolFromJObject(jsonObject, hexStringArray, DecodeConstant.ChannelOneEnable);
                if (ch1Enabled) sensorNumber++;
                ch2Enabled = await ReadBoolFromJObject(jsonObject, hexStringArray, DecodeConstant.ChannelTwoEnable);
                if (ch2Enabled) sensorNumber++;
                userDataLength = 128;
                userData = await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.UserData);
                loggerState = await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.LoggerState);
                fahrenheit = await ReadBoolFromJObject(jsonObject, hexStringArray, DecodeConstant.IsFahrenhiet);
                utcReferenceTime = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.UTCReferenceTime);
                totalSamplingEvents = await ReadIntFromJObject(jsonObject, hexStringArray,DecodeConstant.TotalSamplingEvents);
                totalUses = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TotalUses);
                batteryPercentage = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.BatteryPercentage) + DecodeConstant.Percentage;
                loopOverwrite = await ReadBoolFromJObject(jsonObject, hexStringArray, DecodeConstant.LoopOverwrite);
                notOverflown = await ReadBoolFromJObject(jsonObject, hexStringArray, DecodeConstant.NotOverflown);
                startDelay = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.StartDelay);
                samplePeriod = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.SamplePeriod);
                ticksSinceStart = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TicksSinceStart);
                ticksAtLastSample = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TicksSinceLastSample);
                timeAtFirstSameple = await ReadLongFromJObject(jsonObject, hexStringArray, DecodeConstant.MonT2TimeAtFirstSample);
                recordedSamples = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.TotalRecordedSamples);
                samplePointer = await ReadIntFromJObject(jsonObject, hexStringArray, DecodeConstant.SamplePointer);
                lowerLimit[0] = Convert.ToDouble(await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.MonT2LowerLimitCh1));
                lowerLimit[1] = Convert.ToDouble(await ReadStringFromJObject(jsonObject, hexStringArray,DecodeConstant.MonT2LowerLimitCh2));
                upperLimit[0] = Convert.ToDouble(await ReadStringFromJObject(jsonObject, hexStringArray,DecodeConstant.MonT2UpperLimitCh1));
                upperLimit[1] = Convert.ToDouble(await ReadStringFromJObject(jsonObject, hexStringArray,DecodeConstant.MonT2UpperLimitCh2));
                await ReadStringFromJObject(jsonObject, hexStringArray, DecodeConstant.MonT2DecodeValues);
                return true;
            }

            return false;
        }

        public async Task<LoggerVariables> AssignLoggerValue()
        {
            await SetUpDecoderUsingJsonFile();

            var loggerVariable = new LoggerVariables();
            loggerInformation.LoggerName = loggerName;
            loggerVariable.RecordedSamples = recordedSamples;
            loggerVariable.SerialNumber = serialNumber;
            loggerVariable.LoggerState = loggerState;
            loggerVariable.BatteryPercentage = batteryPercentage;
            loggerVariable.SameplePeriod = HHMMSS(samplePeriod);
            loggerVariable.StartDelay = HHMMSS(startDelay);
            loggerVariable.FirstSample = UNIXtoUTC(timeAtFirstSameple);
            loggerVariable.LastSample = UNIXtoUTC(timeAtFirstSameple);
            loggerVariable.TagsPlaced = tagNumbers;
            loggerVariable.TotalTrip = totalUses;
            loggerVariable.UserData = userData.Substring(0,userDataLength);
            loggerInformation.EmailId = emailID;

            if (batteryPercentage == "255%")
            {
                loggerVariable.BatteryPercentage = "100%";
            }

            for (int i = 0; i < loggerVariable.RecordedSamples; i++)
            {
                loggerVariable.Time.Add(timeAtFirstSameple);
                timeAtFirstSameple = timeAtFirstSameple + samplePeriod;
            }

            if (recordedSamples > 0)
            {
                var timeLastSample = Convert.ToInt32(loggerVariable.Time[(loggerVariable.Time.Count - 1)]);
                loggerVariable.LastSample = UNIXtoUTC(timeLastSample);
            }

            AssignChannelValues(loggerVariable.ChannelOne, 0);

            if (numberChannel > 1 || ch2Enabled)
            {
                loggerVariable.IsChannelTwoEnabled = true;
                AssignChannelValues(loggerVariable.ChannelTwo, 1);
            }

            return loggerVariable;
        }
        void AssignChannelValues(ChannelConfig Channel, int i)
        {
            Channel.PresetLowerLimit = lowerLimit[i];
            Channel.PresetUpperLimit = upperLimit[i];
            Channel.Mean = mean[i];
            Channel.MKT_C = mkt[i] - KelvinDec;
            Channel.Max = sensorMax[i];
            Channel.Min = sensorMin[i];
            Channel.WithinLimits = withinLimit[i];
            Channel.OutsideLimits = aboveLimit[i] + belowLimit[i];
            Channel.AboveLimits = aboveLimit[i];
            Channel.BelowLimits = belowLimit[i];
            Channel.TimeWithinLimits = HHMMSS(withinLimit[i] * samplePeriod);
            Channel.TimeOutLimits = HHMMSS((aboveLimit[i] + belowLimit[i]) * samplePeriod);
            Channel.TimeAboveLimits = HHMMSS(aboveLimit[i] * samplePeriod);
            Channel.TimeBelowLimits = HHMMSS(belowLimit[i] * samplePeriod);

            if (Channel.AboveLimits > 0)
                Channel.BreachedAbove = DecodeConstant.Breached;

            if (Channel.BelowLimits > 0)
                Channel.BreachedBelow = DecodeConstant.Breached;

            if (Data.Count > 0)
                Channel.Data = Data[i];

            if (loggerInformation.LoggerType == 1 && i == 1)
            {
                Channel.Unit = DecodeConstant.Percentage;
            }
            else if(sensorType[i] == 0 || sensorType[i] == 6)
            {
                if (fahrenheit) Channel.Unit = DecodeConstant.Farenhiet;
                else Channel.Unit = DecodeConstant.Celcius;
            }
            else
            {
                Channel.Unit = DecodeConstant.Percentage;
            }
        }
        byte[] ReadHex(string[] currentinfo, String[] hexStringArray)
        {
            var dataFromHex = string.Empty;
            var startIndex = 99;
            var i = 0;

            while (startIndex > 58 || startIndex < 0)
            {
                var currentAddress = hexStringArray[i].Substring(0, 6);
                startIndex = Convert.ToInt32(currentinfo[0], 16) - Convert.ToInt32(currentAddress, 16);
                i++;

                if (i == hexStringArray.Length - 1)
                {
                    return new byte[] { 0x00 };
                }
            }

            i--;
            var address = hexStringArray[i].Substring(0, 6);
            var data = hexStringArray[i].Substring(7, hexStringArray[i].Length - 7);

            var lengthofData = Convert.ToInt32(currentinfo[1]);

            if (lengthofData == 32768) // if we are reading DATA
            {
                lengthofData = dataAddress;

                if (loopOverwriteStartAddress > 0)
                    lengthofData = G4MemorySize + 1;
            }

            if (lengthofData > 58)
            {
                int lengthToRead = 58 - startIndex;
                while (lengthofData > 0)
                {
                    dataFromHex += data.Substring(startIndex * 2, lengthToRead * 2);
                    i++;
                    if (hexStringArray[i] != null && hexStringArray[i].Length != 0)
                    {
                        data = hexStringArray[i].Substring(7, hexStringArray[i].Length - 7);
                        lengthofData = lengthofData - lengthToRead;
                        startIndex = 0;

                        if (lengthofData > (data.Length / 2))
                        {
                            lengthToRead = data.Length / 2;
                        }
                        else
                        {
                            lengthToRead = lengthofData;
                        }
                    }
                    else { break; }
                }

                var totallength = dataFromHex.Length;
                var bytes = new byte[totallength / 2];
                for (int j = 0; j < totallength; j += 2)
                    bytes[j / 2] = Convert.ToByte(dataFromHex.Substring(j, 2), 16);
                return bytes;
            }
            else
            {
                //check if the total length of data that needs to be read is available
                if (data.Length < startIndex * 2 + lengthofData * 2)
                {
                    var readNextLine = (startIndex * 2 + lengthofData * 2) - 58 * 2;
                    dataFromHex = data.Substring(startIndex * 2, lengthofData * 2 - readNextLine);
                    data = hexStringArray[i].Substring(7, hexStringArray[i].Length - 7);
                    dataFromHex += data.Substring(0, readNextLine);
                }
                //if not read what you need and continue to the next line.
                else
                {
                    dataFromHex = data.Substring(startIndex * 2, lengthofData * 2);
                }

                var totalDataLength = dataFromHex.Length;
                var bytes = new byte[totalDataLength / 2];
                for (int j = 0; j < totalDataLength; j += 2)
                    bytes[j / 2] = Convert.ToByte(dataFromHex.Substring(j, 2), 16);
                return bytes;
            }
        }

        #region Reading Json File
        async Task<string> ReadStringFromJObject(JObject jsonObject, String[] hexStringArray, string info)
        {
            var decodeInfo = JsontoString(jsonObject, info);
            var decodeByte = ReadHex(decodeInfo, hexStringArray);
            return await CallDecodeFunctions(decodeInfo, decodeByte);
        }

        async Task<int> ReadIntFromJObject(JObject jsonObject, String[] hexStringArray, string info)
        {
            var decodeInfo = JsontoString(jsonObject, info);
            var decodeByte = ReadHex(decodeInfo, hexStringArray);
            var intString = await CallDecodeFunctions(decodeInfo, decodeByte);
            return Convert.ToInt32(intString, 16);
        }

        async Task<long> ReadLongFromJObject(JObject jsonObject, String[] hexStringArray, string info)
        {
            var decodeInfo = JsontoString(jsonObject, info);
            var decodeByte = ReadHex(decodeInfo, hexStringArray);
            var longString = await CallDecodeFunctions(decodeInfo, decodeByte);
            return Convert.ToInt32(longString);
        }

        async Task<double[]> ReadArrayFromJObject(JObject jsonObject, String[] hexStringArray, string info)
        {
            var decodeInfo = JsontoString(jsonObject, info);
            var decodeByte = ReadHex(decodeInfo, hexStringArray);
            var limitString = await CallDecodeFunctions(decodeInfo, decodeByte);
            var limit = limitString.Split(','); ;
            return Array.ConvertAll<string, double>(limit, Double.Parse);
        }

        async Task<bool> ReadBoolFromJObject(JObject jsonObject, String[] hexStringArray, string info)
        {
            var decodeInfo = JsontoString(jsonObject, info);
            var decodeByte = ReadHex(decodeInfo, hexStringArray);
            return Convert.ToBoolean(await CallDecodeFunctions(decodeInfo, decodeByte));
        }

        async Task<String> GetHexFile()
        {
            var hexPath = Path.GetTempPath() + serialNumber + ".hex";
            var file = await StorageFile.GetFileFromPathAsync(hexPath);
            return await FileIO.ReadTextAsync(file);
        }
        async Task<JObject> GetJsonObject()
        {
            var stream = await LoadJsonFile(new Uri("ms-appx:///Json/" + jsonFile));
            using (var sr = new StreamReader(stream))
            {
                return JObject.Parse(sr.ReadToEnd());
            }
        }

        async Task<Stream> LoadJsonFile(Uri fileUri)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(fileUri);
            var randomAccessStream = await file.OpenReadAsync();
            return randomAccessStream.AsStreamForRead();
        }

        string[] JsontoString(JObject jsonObject, string info)
        {
            var add = jsonObject.GetValue(info).First.Last.ToString();
            var len = jsonObject.GetValue(info).First.Next.Last.ToString();
            var code = jsonObject.GetValue(info).First.Next.Next.Last.ToString();
            var hide = jsonObject.GetValue(info).Last.Last.ToString();

            return new string[] { add, len, code, hide };

        }
        #endregion

        #region Decoding Hex Functions 
        async Task<string> CallDecodeFunctions(string[] stringArrayInfo, byte[] decodeByte)
        {
            bool bitbool = false;
            switch (stringArrayInfo[2])
            {
                case "_1_Byte_to_Boolean":
                    return _1ByteToBoolean(decodeByte);

                case "_1_Byte_to_Decimal":
                    return _1ByteToDecimal(decodeByte);

                case "_2_Byte_to_Decimal":
                    return (((decodeByte[1] & 0xFF) << 8) | (decodeByte[0] & 0xFF)).ToString();

                case "_2_Byte_to_Decimal_Big_Endian":
                    return ToBigEndian(decodeByte);

                case "_2_Byte_to_Temperature_MonT":
                    return _2ByteToTemperatureMonT(decodeByte);

                case "_3_Byte_to_Decimal":
                    return ToLittleEndian(decodeByte);

                case "_3_Byte_to_Temperature_ARRAY":
                    return _3BytetoTemperatureArray(decodeByte);

                case "_4_Byte_Built":
                    return _4ByteBuilt(decodeByte);

                case "_4_Byte_to_Decimal":
                    return ToLittleEndian(decodeByte);

                case "_4_Byte_to_Decimal_2":
                    return ToLittleEndian(decodeByte);

                case "_4_Byte_to_UNIX":
                    return _4ByteToUNIX(decodeByte);

                case "_4_Byte_Sec_to_Date":
                    return _4ByteSectoDec(decodeByte);

                case "_8_Byte_to_Unix_UTC":
                    return ToBigEndian(decodeByte);

                case "b0":
                    bitbool = false;
                    if (GetBit(decodeByte[0], 0) != 0)
                        bitbool = true;
                    return bitbool.ToString();

                case "b1":
                    bitbool = false;
                    if (GetBit(decodeByte[0], 1) != 0)
                        bitbool = true;
                    return bitbool.ToString();

                case "b2":
                    bitbool = false;
                    if (GetBit(decodeByte[0], 2) != 0)
                        bitbool = true;
                    return bitbool.ToString();

                case "b3":
                    bitbool = false;
                    if (GetBit(decodeByte[0], 3) != 0)
                        bitbool = true;
                    return bitbool.ToString();

                case "b4":
                    bitbool = false;
                    if (GetBit(decodeByte[0], 4) != 0)
                        bitbool = true;
                    return bitbool.ToString();


                case "b5":
                    bitbool = false;
                    if (GetBit(decodeByte[0], 5) != 0)
                        bitbool = true;
                    return bitbool.ToString();

                case "b6":
                    bitbool = false;
                    if (GetBit(decodeByte[0], 6) != 0)
                        bitbool = true;
                    return bitbool.ToString();

                case "b7":
                    bitbool = false;
                    if (GetBit(decodeByte[0], 7) != 0)
                        bitbool = true;
                    return bitbool.ToString();

                case "Battery_MonT":
                    return BatteryMonT(decodeByte);

                case "CompressionTable":
                    compressionTable = CompressionTable(decodeByte);
                    break;

                case "Channel_1_MonT":
                    return "1";

                case "Decode_Delta_Data":
                    DecodeDeltaData(decodeByte);
                    break;

                case "Decode_MonT_Data":
                    DecodeMonTData(decodeByte);
                    break;

                case "DJNZ_2_Byte_Type_1":
                    decodeByte[1]--;
                    return ToLittleEndian(decodeByte);

                case "DJNZ_2_Byte_Type_2":
                    return DJNZ2ByteType2(decodeByte);

                case "DJNZ_4_Byte_Type_1":
                    return DJNZ4ByteType1(decodeByte);

                case "DJNZ_4_Byte_Type_2":
                    return DJNZ4ByteType_2(decodeByte);

                case "LoggingSelection":
                    return LoggingSelection(decodeByte);

                case "*Logger_State":
                    return LoggerState(decodeByte);

                case "MonT2_Model":
                    return MonT2Model(decodeByte);

                case "MonT2_Limit_Convert":
                    return MonT2LimitConvert(decodeByte);

                case "MonT2_SampleNum":
                    return MonT2SampleNum(decodeByte);

                case "MonT2_SensorNumber":
                    return decodeByte[0].ToString();

                case "MonT2_Value_Decode":
                    MonT2ValueDecode(decodeByte);
                    break;

                case "SamplePointer":
                    return decodeByte[0].ToString();

                case "SampleNumber_logged_MonT":
                    return SampleNumberLoggedMonT(decodeByte);

                case "SENSOR_Decoding":
                    await SensorDecoding(decodeByte);
                    break;

                case "String":
                    return String(decodeByte);

                case "Time_FirstSample_MonT":
                    return TimeFirstSampleMonT(decodeByte);

                case "Time_FirstSample_MonT2":
                    return TimeFirstSampleMonT2(decodeByte);

                case "Time_LastSample_MonT":
                    return TimeLastSampleMonT(decodeByte);

                default:
                    break;
            }

            return string.Empty;
        }
        string _1ByteToBoolean(byte[] decodeByte)
        {
            if ((decodeByte[0] & 0xff) == 0)
            {
                return "false";
            }
            else
            {
                return "true";
            }
        }
        string _1ByteToDecimal(byte[] decodeByte)
        {
            return decodeByte[0].ToString("x02");
        }
        string _2ByteToTemperatureMonT(byte[] decodeByte)
        {
            var value = (((decodeByte[1]) & 0xFF) << 8) | (decodeByte[0] & 0xFF);
            value -= 4000;
            var V = ((double)value / 100);
            return V.ToString("N2");
        }
        string _3BytetoTemperatureArray(byte[] decodeByte)
        {
            var offset = 9;
            var limitArray = new double[numberChannel];
            var limitString = string.Empty;

            for (int i = 0; i < numberChannel; i++)
            {
                var element = ((((decodeByte[(offset * i) + 2]) & 0xFF) << 16) | (((decodeByte[(offset * i) + 1]) & 0xFF) << 8) | (decodeByte[(offset * i)] & 0xFF));
                if (sensorType[i] == 0 || sensorType[i] == 6)
                {
                    element -= Kelvin;
                }

                limitArray[i] = element / 100;
            }

            for (int i = 0; i < limitArray.Length; i++)
            {
                if ((i + 1) == limitArray.Length)
                    limitString += limitArray[i].ToString();
                else limitString += limitArray[i].ToString() + ',';
            }

            return limitString;
        }
        string _4ByteBuilt(byte[] decodeByte)
        {
            long value = Convert.ToInt32(ToLittleEndian(decodeByte)) * 1000;

            if (value > 0)
            {
                value += Year2000;
                value -= Year2010;
                value /= 21600000;
                return value.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
        string _4ByteToUNIX(byte[] decodeByte)
        {
            long _4byteUnix = (Convert.ToInt32(ToLittleEndian(decodeByte), 16) * 1000);
            return CalculateTimeStarted(_4byteUnix).ToString();
        }
        string _4ByteSectoDec(byte[] decodeByte)
        {
            long _4sectobyte = (decodeByte[3] + decodeByte[2] + decodeByte[1] + decodeByte[0]) * 1000;

            if (_4sectobyte > 0)
            {
                _4sectobyte += Year2000;
                DateTime date = new DateTime(_4sectobyte);
                return date.ToString("dd/MM/yyyy HH:mm:sss");
            }
            else
                return string.Empty;
        }
        int GetBit(int Value, int bit)
        {
            return (Value >> bit) & 1;
        }
        string BatteryMonT(byte[] decodeByte)
        {
            var RCLBatteryMargin = 0.25;    // i.e. leave 25% in reserve
            var RCLSelfDischargeCurrentuA = 0.630;
            var RCLQuiescentDischargeCurrentuA = 9.000;
            var RCLConversionDischargeCurrentuA = 1300.0;
            var RCLDownloadDischargeCurrentuA = 2900.00;
            var RCLConversionDurationS = 1.0;
            var RCLDownloadDurationS = 3.5;
            var RCLAssumedDownloadsPerTrip = 2.0;

            var INITAL_BATTERY = (int)((1.0 - RCLBatteryMargin) * 0.56 * 60 * 60 * 1000000); //mAHr
            var BatteryUsed =
            (((utcReferenceTime - manufactureDate) * RCLSelfDischargeCurrentuA) +
            (totalRTCticks * RCLQuiescentDischargeCurrentuA) +
            ((totalSamplingEvents + 4681) * RCLConversionDurationS * RCLConversionDischargeCurrentuA) +
            (RCLAssumedDownloadsPerTrip * totalUses * RCLDownloadDurationS * RCLDownloadDischargeCurrentuA));

            var batteryPercentage = (int)(((INITAL_BATTERY - BatteryUsed) / INITAL_BATTERY) * 100);
            return batteryPercentage.ToString("x02");
        }
        long CalculateTimeStarted(long timeFirstSample)
        {
            var timeStarted = ((timeFirstSample + Year2000) / 1000) + startDelay;
            var ticksSinceStop = ticksSinceStart - ticksAtLastSample;
            var totalNumberofTicks = ((G4MemorySize - 9) - (6 * numberChannel - 1)) / numberChannel; // what are these numbers 
            var totalLoggingTime = totalNumberofTicks * samplePeriod;

            if (loopOverwriteStartAddress == 0)
                timeStarted = utcReferenceTime - ticksSinceStart + startDelay;
            else
                timeStarted = utcReferenceTime - totalLoggingTime - ticksSinceStop;

            return timeStarted;
        }
        Boolean CheckStartSentinel(byte[] decodeByte, int memoryStart)
        {
            var startValue = new int[8];
            var currentSensor = 0;
            var check = true;

            while ((currentSensor < numberChannel) && check)
            {
                var addMSB = (memoryStart + (2 * currentSensor) + 1) & G4MemorySize;
                var addLSB = (memoryStart + (2 * currentSensor)) & G4MemorySize;

                var VaddMSB = (memoryStart + (2 * currentSensor) + (2 * numberChannel) + 1) & G4MemorySize;
                var VaddLSB = (memoryStart + (2 * currentSensor) + (2 * numberChannel)) & G4MemorySize;

                startValue[currentSensor] = (((decodeByte[addMSB]) & 0xff) << 8) | (decodeByte[addLSB] & 0xff);
                var verifyValue = ((decodeByte[VaddMSB] & 0xff) << 8) | (decodeByte[VaddLSB] & 0xff);
                if (startValue[currentSensor] != verifyValue)
                {
                    check = false;
                }
                sensorStartingValue[currentSensor] -= verifyValue;
                sensorStartingValue[currentSensor] *= -1;

                currentSensor++;
            }

            return check;
        }
        int[] CompressionTable(byte[] decodeByte)
        {
            var value = new int[decodeByte.Length / 2];

            for (int i = 0; i < decodeByte.Length; i += 2)
            {
                compressionTable[i / 2] = (((decodeByte[i / 2] & 0xff) << 8) | (decodeByte[i + 1] & 0xff));
                value[i / 2] = compressionTable[i / 2];
            }

            return value;
        }
        void DecodeDeltaData(byte[] decodeByte)
        {
            var memoryStart = loopOverwriteStartAddress;
            if (memoryStart > 0)
            {
                memoryStart = FindStartSentinel(memoryStart - 1, 16, decodeByte);
                memoryStart++;
            }
            if (decodeByte.Length > 0)
            {
                if (CheckStartSentinel(decodeByte, memoryStart))
                {
                    memoryStart += ((6 * numberChannel) + 1);
                    memoryStart &= G4MemorySize;
                    memoryStart = FindStartSentinel(memoryStart, (8 * numberChannel), decodeByte);
                    memoryStart = FindStartSentinel(memoryStart, (8 * numberChannel), decodeByte);
                    if (memoryStart != 0xFFFF)
                    {
                        memoryStart++;
                        ReadStoreData(decodeByte, memoryStart);
                    }
                }
            }
        }
        void DecodeMonTData(byte[] decodeByte)
        {
            int a = 0;
            int b = 0;
            int array_pointer = 0;
            bool Flag_End = false;

            if (recordedSamples > 0)
            {
                var channelList = new List<double>();
                InitSensorStatisticsField(0);

                if (loopOverwrite)
                {
                    while ((b < decodeByte.Length))
                    {

                        if (decodeByte[b] == 0xFF) //0xFF = 255 Two's complement
                        {
                            break;
                        }
                        b++;
                    }
                    b++;  //Go to the index after the 0xFF
                    while ((b < decodeByte.Length)) //reads the data after the 0xFF
                    {
                        TemperatureStatistics(0, lowestTemp + ((decodeByte[b] & 0xFF) * resolution), array_pointer);
                        channelList.Add(lowestTemp + ((decodeByte[b] & 0xFF) * resolution));
                        b++;
                    }

                }

                while ((a < decodeByte.Length) && (!Flag_End))
                {
                    if (decodeByte[a] == 0xFE || decodeByte[a] == 0xFF) //0xFE = 254 Two's complement || 0xFF = 255 Two's complement
                    {
                        Flag_End = true;
                    }
                    else
                    {
                        TemperatureStatistics(0, lowestTemp + ((decodeByte[a] & 0xFF) * resolution), array_pointer);
                        channelList.Add(lowestTemp + ((decodeByte[a] & 0xFF) * resolution));
                    }
                    a++;
                }
                Data.Add(channelList);
            }
            FinalizeStatistics(0);
        }
        string DJNZ2ByteType2(byte[] decodeByte)
        {
            for (int i = 0; i < decodeByte.Length; i++)
            {
                decodeByte[i] = (byte)(0x100 - (decodeByte[i] & 0xFF));
            }
            return ToLittleEndian(decodeByte);
        }
        string DJNZ4ByteType1(byte[] decodeByte)
        {
            for (int i = 1; i < decodeByte.Length; i++)
            {
                decodeByte[i]--;
            }
            return ToLittleEndian(decodeByte);
        }
        string DJNZ4ByteType_2(byte[] decodeByte)
        {
            for (int i = 0; i < 4; i++)
            {
                int z = (0x100) - (decodeByte[i] & 0xFF);
                decodeByte[i] = (byte)z;
            }
            return ToLittleEndian(decodeByte);
        }
        int FindStartSentinel(int memoryStart, int max, byte[] decodeByte)
        {
            var maxI = (memoryStart + max+20);

            if (maxI > memoryStart)
            {
                while (maxI > memoryStart)
                {
                    if ((decodeByte[memoryStart] == 0x7F) || (decodeByte[memoryStart] == 0xff)) //represents start and stop bytes
                    {
                        return memoryStart;
                    }
                    memoryStart++;
                    memoryStart &= G4MemorySize; // 0x007fff is to allow buffer rotation in case of Header in middle of end buffer?????????????
                }
            }
            else
            {
                memoryStart = 0;
                while (memoryStart <= maxI)
                {
                    if ((decodeByte[memoryStart] == 0x7f) || (decodeByte[memoryStart] & 0xff) == 0xff) // ????????
                    {
                        return memoryStart;
                    }
                    memoryStart++;
                    memoryStart &= G4MemorySize;
                }
            }
            return 0xffff;
        }
        public string HHMMSS(double mseconds)
        {
            int hours = (int)(mseconds / 3600);
            int minutes = (int)((mseconds % 3600) / 60);
            int seconds = (int)(mseconds % 60);

            return $"{hours.ToString("00")}:{minutes.ToString("00")}:{seconds.ToString("00")}";
        }
        string LoggingSelection(byte[] decodeByte)
        {
            switch (decodeByte[0])
            {
                case 1:
                    return "Temperature";
                case 2:
                    return "Humidity";
                case 3:
                    return "Temperature-Humidity";
                default:
                    return "Undefined";
            }
        }
        string LoggerState(byte[] decodeByte)
        {
            if (loggerInformation.LoggerType == 1) // MonT2
            {
                switch (decodeByte[0])
                {
                    case 0:
                        return "Sleep";
                    case 1:
                        return "No Config";
                    case 2:
                        return "Ready";
                    case 3:
                        return "Start Delay";
                    case 4:
                        return "Logging";
                    case 5:
                        return "Stopped";
                    case 6:
                        return "Reuse";
                    case 7:
                        return "Error";
                    default:
                        return "Undefined";
                }
            }
            else
            {
                switch (decodeByte[0])
                {
                    case 0:
                        return "Ready";
                    case 1:
                        return "Delay";
                    case 2:
                        return "Logging";
                    case 3:
                        return "Stopped";
                    default:
                        return "Undefined";
                }
            }
        }
        string MonT2LimitConvert(byte[] decodeByte)
        {
            double Limit = ((((decodeByte[1] & 0xFF) << 8) | (decodeByte[0] & 0xFF)) / 10);
            return Limit.ToString();
        }
        string MonT2Model(byte[] decodeByte)
        {
            switch (decodeByte[0])
            {
                case 1:
                    return "MonT2 Plain";
                case 2:
                    return "MonT2 Plain USB";
                case 3:
                    return "MonT2 Plain RH USB";
                case 4:
                    return "MonT2 LCD";
                case 5:
                    return "MonT2 LCD USB";
                case 6:
                    return "MonT2 LCD RH USB";
                case 7:
                    return "MonT2 PDF";
                case 8:
                    return "MonT2 RH PDF";
                case 9:
                    return "MonT2 LCD PDF";
                case 10:
                    return "MonT2 LCD RH PDF";
                case 11:
                    return "MonT2 Plain RH";
                case 12:
                    return "MonT2 LCD RH";
                default:
                    return "Unknown";
            }
        }
        string MonT2SampleNum(byte[] decodeByte)
        {
            int sampleNumber = ((((decodeByte[3]) & 0xFFFFFF) << 24) | (((decodeByte[2]) & 0xFFFF) << 16) | (((decodeByte[1]) & 0xFF) << 8) | (decodeByte[0] & 0xFF));

            if (sensorNumber == 2)
            {
                sampleNumber *= 2;
            }

            mont2Samples = sampleNumber;

            if (!notOverflown)
                sampleNumber = 16384;

            return sampleNumber.ToString();
        }
        void MonT2ValueDecode(byte[] decodeByte)
        {
            var ch1List = new List<double>();
            var ch2List = new List<double>();
            var tagList = new List<int>();
            bool hasFlag;
            short sample1;
            short sample2;
            int index = 0;

            Console.WriteLine("SAMPLE NUMBER : " + mont2Samples);
            //When MonT2 is loop over writing
            if (!notOverflown && loopOverwrite)
            {
                var bytePointer = samplePointer * 2;
                var position = 0;
                var pageOffset = bytePointer % 512;
                var address = ((bytePointer / 512) + 1) * 512;
                address += pageOffset;

                var copyArrayList = new List<double>();
                //Creating a temporary array - 33280 is MAX memory size with an extra page of 512 bytes
                for (int i = 0; i < 33280; i++)
                {
                    copyArrayList.Add(decodeByte[i]);
                }
                //Copying the top half back in the array
                for (int i = address; i < 33280; i++)
                {
                    decodeByte[position] = (byte)(copyArrayList[i]);
                    position++;
                }
                //Copying the bottom half into the array
                for (int i = 0; i < samplePointer * 2; i++)
                {
                    decodeByte[position] = (byte)(copyArrayList[i]);
                    position++;
                }

            }


            if (ch1Enabled && ch2Enabled)
            {
                InitSensorStatisticsField(0);
                InitSensorStatisticsField(1);

                for (int i = 0; i < mont2Samples * 2; i++)
                {
                    sample1 = (short)(decodeByte[i] & 0xff);
                    i++;
                    sample1 |= (short)(decodeByte[i] << 8);
                    i++;
                    sample2 = (short)(decodeByte[i] & 0xff);
                    i++;
                    sample2 |= (short)(decodeByte[i] << 8);
                    hasFlag = ((sample1 & (0x0002)) != 0);

                    ch1List.Add(((double)(sample1 >> 2)) / 10);
                    ch2List.Add(((double)(sample2 >> 2)) / 10);
                    if (hasFlag)
                    {
                        tagList.Add(index);
                    }

                    TemperatureStatistics(0, ((double)(sample1 >> 2)) / 10, index);
                    TemperatureStatistics(1, ((double)(sample2 >> 2)) / 10, index);
                    index++;
                }

                Data.Add(ch1List);
                Data.Add(ch2List);
                Tag = tagList;
                recordedSamples /= 2;
                FinalizeStatistics(0);
                FinalizeStatistics(1);
            }
            else if (ch1Enabled)
            {
                InitSensorStatisticsField(0);
                for (int i = 0; i < mont2Samples * 2; i++)
                {
                    sample1 = (short)(decodeByte[i] & 0xff);
                    i++;
                    sample1 |= (short)(decodeByte[i] << 8);
                    hasFlag = ((sample1 & (0x0002)) != 0);

                    ch1List.Add(((double)(sample1 >> 2)) / 10);
                    if (hasFlag)
                    {
                        tagList.Add(index);
                    }

                    TemperatureStatistics(0, ((double)(sample1 >> 2)) / 10, index);
                    index++;
                }

                Data.Add(ch1List);
                Tag = tagList;


                FinalizeStatistics(0);
            }
            else if (ch2Enabled)
            {
                InitSensorStatisticsField(1);

                for (int i = 0; i < mont2Samples * 2; i++)
                {
                    sample2 = (short)(decodeByte[i] & 0xff);
                    i++;
                    sample2 |= (short)(decodeByte[i] << 8);
                    hasFlag = ((sample2 & (0x0002)) != 0);

                    ch2List.Add(((double)(sample2 >> 2)) / 10);
                    if (hasFlag)
                    {
                        tagList.Add(index);
                    }

                    TemperatureStatistics(1, ((double)(sample2 >> 2)) / 10, index);
                    index++;
                }
                Data.Add(ch1List);
                Data.Add(ch2List);
                Tag = tagList;

                FinalizeStatistics(1);
            }
        }
        void ReadStoreData(byte[] decodeByte, int memoryStart)
        {
            var MaxReadingLength = (G4MemorySize - (6 * numberChannel) - 2) / numberChannel;

            for (int i = 0; i < numberChannel; i++)
            {
                var tagList = new List<int>();
                var channelList = new List<double>();
                var arrayPointer = 0;
                var dataPointer = (memoryStart + i) & G4MemorySize;                  //& 0x007FFF is to allow buffer rotation
                var totalSameples = 0;

                InitSensorStatisticsField(i);

                while ((dataPointer < decodeByte.Length) && (decodeByte[dataPointer] != 0x7F) && ((decodeByte[dataPointer] & 0xFF) != 0xFF) && (totalSameples < MaxReadingLength))
                {
                    if ((decodeByte[dataPointer] & 0xFF) == 0x80)
                    {
                        if (i == 0)
                        {
                            tagList.Add(arrayPointer);
                            tagNumbers++;
                        }
                    }
                    else
                    {
                        if (decodeByte[dataPointer] > 0x7f) //> 0x7f
                        {
                            sensorStartingValue[i] -= (decodeByte[dataPointer] & 0x7f); //compressionTable[(decodeByte[dataPointer] & 0x7f)];
                        }
                        else
                        {
                            sensorStartingValue[i] += (decodeByte[dataPointer]);
                        }
                        channelList.Add((double)sensorStartingValue[i] / 100);
                        TemperatureStatistics(i, ((double)sensorStartingValue[i] / 100), arrayPointer);
                        totalSameples++;
                    }

                    dataPointer += numberChannel;
                    dataPointer &= G4MemorySize;                                                           //& 0x007FFF is to allow buffer rotation
                }

                FinalizeStatistics(i);

                Data.Add(channelList);
                Tag = tagList;
            }
        }
        string SampleNumberLoggedMonT(byte[] decodeByte)
        {
            if (loopOverwrite)
                return "4681";
            else
            {
                long samplenumber = (((ticksAtLastSample - startDelay) / samplePeriod) + 1);
                return samplenumber.ToString();
            }
        }
        async Task SensorDecoding(byte[] decodeByte)
        {
            var offset = 11;
            var sensorAddressArray = new string[2];
            var hexString = await GetHexFile();
            var hexStringArray = hexString.Split("\r\n");

            for (int i = 0; i < numberChannel; i++)
            {
                var pointer = i * offset;
                sensorType[i] = decodeByte[pointer + 7]; // byte 7 is where the sensorType is stored
                sensorAddressArray[0] = decodeByte[pointer + 10].ToString("x02") + decodeByte[pointer + 9].ToString("x02");
                sensorAddressArray[1] = "21"; // size of the sensor information 

                var sensorInfoArray = ReadHex(sensorAddressArray, hexStringArray);

                if (sensorInfoArray.Length != 0)
                {
                    var sensorData = sensorInfoArray[20] << 16 | sensorInfoArray[19] << 8 | sensorInfoArray[18];
                    if (sensorType[i] == 0 || sensorType[i] == 6) // get yasiru to explain why
                    {
                        sensorStartingValue[i] = Kelvin - sensorData;
                    }
                    else
                    {
                        sensorStartingValue[i] = 0x1000000 - sensorData;
                    }
                }
            }
        }
        string String(byte[] decodeByte)
        {
            string UserDataString = string.Empty;

            for (int i = 0; i < decodeByte.Length; i++)
            {
                if (decodeByte[i] > 12 && decodeByte[i] < 127)
                {
                    if (decodeByte[i] == 13)
                        decodeByte[i] = 32;
                    UserDataString += Convert.ToChar(decodeByte[i]);
                }
            }
            return UserDataString;
        }
        string TimeFirstSampleMonT(byte[] decodeByte)
        {
            if (loopOverwrite)
            {
                timeAtFirstSameple = ((utcReferenceTime) - (4680 * samplePeriod) - (ticksSinceStart - ticksAtLastSample));
                return timeAtFirstSameple.ToString();
            }
            else
            {
                timeAtFirstSameple = ((utcReferenceTime - ticksSinceStart) + startDelay);
                return timeAtFirstSameple.ToString();
            }
        }
        string TimeFirstSampleMonT2(byte[] decodeByte)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            if (decodeByte[0] == (byte)0xFF && decodeByte[1] == (byte)0xFF && decodeByte[2] == (byte)0xFF && decodeByte[3] == (byte)0xFF && decodeByte[4] == (byte)0xFF && decodeByte[5] == (byte)0xFF)
            {
                return "0";
            }
            else
            {
                int year = (decodeByte[0] >> 1) + 2000;
                int month = (((decodeByte[1] & 0xff) >> 4) - 1);
                var dateTime = new DateTime(year, month, decodeByte[2], decodeByte[3], decodeByte[4], decodeByte[5]);

                //Console.WriteLine("date time : " + dateTime);

                if (!notOverflown && loopOverwrite)
                {
                    long val;
                    if (loggerState == "Running")
                    {
                        val = ((utcReferenceTime) - ((16384 / sensorNumber) * samplePeriod) - (ticksSinceStart - ticksAtLastSample));
                        mont2Samples = 16384;
                        return val.ToString();
                    }
                    else
                    {
                        int seconds = (int)samplePeriod;
                        int maxMem = 16384;
                        if (sensorNumber == 2)
                        {
                            maxMem = maxMem / 2;
                        }

                        maxMem = ((int)mont2Samples / sensorNumber) - maxMem;
                        seconds = seconds * maxMem;

                        var date = dateTime.AddSeconds(seconds);
                        date = date.AddSeconds(startDelay);
                        mont2Samples = 16384;
                        return (date - epoch).TotalSeconds.ToString();
                    }
                }
                else
                {
                    dateTime = dateTime.AddSeconds(startDelay);
                    return (dateTime - epoch).TotalSeconds.ToString();
                }

            }
        }
        string TimeLastSampleMonT(byte[] decodeByte)
        {
            var LastSample = (utcReferenceTime - (4294967040 - secondsTimer));
            return LastSample.ToString();
        }
        string ToLittleEndian(byte[] decodebyte)
        {
            var sb = new StringBuilder();
            for (int i = decodebyte.Length - 1; i >= 0; i--)
            {
                sb.Append(decodebyte[i].ToString("x02"));
            }

            return sb.ToString();
        }
        string ToBigEndian(byte[] decodebyte)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < decodebyte.Length; i++)
            {
                sb.Append(decodebyte[i].ToString("x02"));
            }
            return sb.ToString();
        }
        public string UNIXtoUTC(long now)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var date = epoch.AddMilliseconds(now * 1000);
            return date.ToUniversalTime().ToString("dd/MM/yyyy HH:mm:sss UTC");
        }
        public string UNIXtoUTCDate(long now)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var date = epoch.AddMilliseconds(now * 1000);
            return date.ToUniversalTime().ToString("dd/MM/yyyy");
        }
        public string UNIXtoUTCTime(long now)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var date = epoch.AddMilliseconds(now * 1000);
            return date.ToUniversalTime().ToString("HH:mm:sss");
        }
        void InitSensorStatisticsField(int currentChannel)
        {
            recordedSamples = 0;

            sensorMin[currentChannel] = 0xFFFFFFFFFFL;
            sensorMax[currentChannel] = -274;

            highestPosition[currentChannel] = 0;
            lowestPosition[currentChannel] = 0;

            mean[currentChannel] = 0;
            mkt[currentChannel] = 0;

            withinLimit[currentChannel] = 0;
            belowLimit[currentChannel] = 0;
            aboveLimit[currentChannel] = 0;
        }
        void TemperatureStatistics(int currentChannel, double Value, int index)
        {
            if (Value < sensorMin[currentChannel])
            {
                lowestPosition[currentChannel] = index + 1;// Cause it starts from zero
                sensorMin[currentChannel] = Value;
            }

            if (Value > sensorMax[currentChannel])
            {
                highestPosition[currentChannel] = index + 1;
                sensorMax[currentChannel] = Value;
            }

            if (Value > upperLimit[currentChannel])
            {
                aboveLimit[currentChannel]++;
            }
            else if (Value < lowerLimit[currentChannel])
            {
                belowLimit[currentChannel]++;
            }
            else
            {
                withinLimit[currentChannel]++;
            }

            mkt[currentChannel] += Math.Exp((-Delta_H) / ((Value + KelvinDec) * R));
            mean[currentChannel] += Value;

            recordedSamples++;
        }
        void FinalizeStatistics(int currentChannel)
        {
            mean[currentChannel] /= recordedSamples;
            mkt[currentChannel] = (Delta_H / R) / (-Math.Log(mkt[currentChannel] / recordedSamples));
        }
        #endregion
    }
}


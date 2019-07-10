using System;

namespace TRL
{
    public class DecodeConstant
    {
        public const string G4 = "G4";
        public const string MonT = "Mon T";
        public const string NumberOfChannels = "SENSOR,SensorNumber";
        public const string UserDataLength = "USER_DATA,UserDataLen";
        public const string EmailID = "USER_SETTINGS,Owner";
        public const string UserData = "USER_DATA,UserData";
        public const string LoggerState = "HEADER,State";
        public const string BatteryPercentage = "BATTERY_INFO,Battery";
        public const string LoopOverwriteAddress = "LoopOverWriteAddress";
        public const string IsFahrenhiet = "USER_SETTINGS,Fahrenheit";
        public const string UTCReferenceTime = "UTCReferenceTime";
        public const string TotalRTCTicks = "HEADER,TotalRTCTicks";
        public const string ManufactureDate = "HEADER,ManufactureDate";
        public const string IsLoopOverwrite = "DATA_INFO,Overwritten";
        public const string TotalSamplingEvents = "HEADER,TotalSamplingEvents";
        public const string TotalUses = "HEADER,TotalUses";
        public const string SecondTimer = "SecondsTimer";
        public const string StartDelay = "USER_SETTINGS,StartDelay";
        public const string SamplePeriod = "USER_SETTINGS,SamplingPeriod";
        public const string TicksSinceStart = "DATA_INFO,TicksSinceArousal";
        public const string TicksSinceLastSample = "DATA_INFO,TicksAtLastSample";
        public const string TicksAtLastSample = "DATA_INFO,LastSampleAt";
        public const string TotalRecordedSamples = "DATA_INFO,SamplesNumber";
        public const string TimeAtFirstSample = "DATA_INFO,TimeStarted";
        public const string MonTTimeAtFirstSample = "DATA_INFO,Time_FirstSample_MonT";
        public const string LowestTemp = "USER_SETTINGS,LowestTemp";
        public const string ResolutionRatio = "USER_SETTINGS,ResolutionRatio";
        public const string CompressionTable = "TABLE,CompressionTable";
        public const string Sensor = "SENSOR,SENSOR";
        public const string DataEndPointer = "DATA_INFO,DataEndPointer";
        public const string LowerLimit = "CHANNEL_INFO,LowerLimit";
        public const string UpperLimit = "CHANNEL_INFO,UpperLimit";
        public const string Data = "SENSOR,Decode_Delta_Data";
        public const string MonTData = "SENSOR,Decode_MonT_Data";

        public const string Celcius = "°C"; //°
        public const string Farenhiet = "°F";
        public const string Percentage = " %";
        public const string Breached = " (breached)";


        
    }
}

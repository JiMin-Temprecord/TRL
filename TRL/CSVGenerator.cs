using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TRL.Constant;
using Windows.Storage;

namespace TRL
{
    public class CSVGenerator
    {
        int row = 5;
        List<List<string>> csvStringList = new List<List<string>>();

        public async void CreateCSV(LoggerInformation loggerInformation)
        {
            var hexDecoder = new HexFileDecoder(loggerInformation);
            await hexDecoder.SetUpDecoderUsingJsonFile();
            var loggerVariables = await hexDecoder.AssignLoggerValue();
            
            BuildCSV(loggerInformation, loggerVariables);
            SaveCSV(loggerInformation);
        }

        private void BuildCSV(LoggerInformation loggerInformation, LoggerVariables loggerVariables)
        {
            var decoder = new HexFileDecoder(loggerInformation);
            var channelTwoEnabled = loggerVariables.IsChannelTwoEnabled;
            var channelOne = loggerVariables.ChannelOne;
            var channelTwo = loggerVariables.ChannelTwo;

            WritetoList(1, 5,DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:sss UTC"));
            WritetoList(2, 1,LabelConstant.Title);
            WritetoList(2, 5,LabelConstant.SerialNumber + loggerVariables.SerialNumber);

            WriteLoggerInfotoList(LabelConstant.Model, loggerInformation.LoggerName);
            WriteLoggerInfotoList(LabelConstant.LoggerState, loggerVariables.LoggerState);
            WriteLoggerInfotoList(LabelConstant.Battery, loggerVariables.BatteryPercentage);
            WriteLoggerInfotoList(LabelConstant.SamplePeriod, loggerVariables.SameplePeriod);
            WriteLoggerInfotoList(LabelConstant.StartDelay, loggerVariables.StartDelay);
            WriteLoggerInfotoList(LabelConstant.FirstSample, loggerVariables.FirstSample);
            WriteLoggerInfotoList(LabelConstant.LastSample, loggerVariables.LastSample);
            WriteLoggerInfotoList(LabelConstant.RecordedSample, loggerVariables.RecordedSamples.ToString());
            WriteLoggerInfotoList(LabelConstant.TagsPlaced, loggerVariables.TagsPlaced.ToString());
            row++;

            WritetoList(row, 2,LabelConstant.StatChannelOneLabel);
            if (channelTwoEnabled)
                WritetoList(row, 3,LabelConstant.StatChannelTwoLabel);
            row++;

            WriteChannelStattoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.PresentUpperLimit, c => c.PresetUpperLimit.ToString("N2"));
            WriteChannelStattoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.PresentLowerLimit, c => c.PresetLowerLimit.ToString("N2"));
            WriteChannelStattoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.Mean, c => c.Mean.ToString("N2"));
            WriteChannelStattoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.MKT, c => c.MKT_C.ToString("N2"));
            WriteChannelStattoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.Max, c => c.Max.ToString("N2"));
            WriteChannelStattoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.Min, c => c.Min.ToString("N2"));
            WriteChannelLimitstoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.SampleWithinLimits, c => c.WithinLimits.ToString("N1"));
            WriteChannelLimitstoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.TimeWithinLimits, c => c.TimeWithinLimits);
            WriteChannelLimitstoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.SampleOutofLimits, c => c.OutsideLimits.ToString("N1"));
            WriteChannelLimitstoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.TimeOutOfLimits, c => c.TimeOutLimits);
            WriteChannelLimitstoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.SampleAboveLimit, c => c.AboveLimits.ToString("N1"));
            WriteChannelLimitstoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.TimeAboveLimit, c => c.TimeAboveLimits);
            WriteChannelLimitstoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.SampleBelowLimit, c => c.BelowLimits.ToString("N1"));
            WriteChannelLimitstoList(channelOne, channelTwo, channelTwoEnabled, LabelConstant.TimeBelowLimit, c => c.TimeBelowLimits);

            WriteLoggerInfotoList(LabelConstant.UserComment, string.Empty);

            if (loggerVariables.UserData.Length > 120)
            {
                var firstLine = loggerVariables.UserData.Substring(0, loggerVariables.UserData.Length / 2);
                var secondLine = loggerVariables.UserData.Substring(loggerVariables.UserData.Length / 2);

                WriteLoggerInfotoList(firstLine, string.Empty);
                WriteLoggerInfotoList(secondLine, string.Empty);
            }
            else
            {
                WriteLoggerInfotoList(loggerVariables.UserData, string.Empty);
            }

            row++;
            if (channelTwoEnabled)
                WritetoList(row, 3,LabelConstant.GraphChannelTwoLabel);
            WriteLoggerInfotoList(LabelConstant.DateTime, LabelConstant.GraphChannelOneLabel);
            row++;

            var startRange = row;
            var endRange = (row + loggerVariables.RecordedSamples);
            FillValueRange(decoder, loggerVariables, channelOne, channelTwo, startRange, endRange);
        }

        void WriteLoggerInfotoList(string label, string value)
        {
            WritetoList(row, 1,label);
            WritetoList(row, 2,value);
            row++;
        }

        void WriteChannelLimitstoList(ChannelConfig channelOne, ChannelConfig channelTwo, bool channelTwoEnabled, string label, Func<ChannelConfig, string> getString)
        {
            WritetoList(row, 1,label);
            WritetoList(row, 2,getString(channelOne));
            if (channelTwoEnabled)
            {
                WritetoList(row, 3, getString(channelTwo));
            }
            row++;
        }

        void WriteChannelStattoList(ChannelConfig channelOne, ChannelConfig channelTwo, bool channelTwoEnabled, string label, Func<ChannelConfig, string> getString)
        {
            WritetoList(row, 1,label);
            WritetoList(row, 2,getString(channelOne) + channelOne.Unit);
            if (channelTwoEnabled && label != LabelConstant.MKT)
            {
                WritetoList(row, 3,getString(channelTwo) + channelTwo.Unit);
            }
            row++;
        }

        void FillValueRange(HexFileDecoder decoder, LoggerVariables pdfVariables, ChannelConfig channelOne, ChannelConfig channelTwo, int start, int end)
        {
            for (int i = 0; i < pdfVariables.RecordedSamples; i++)
            {
                WritetoList(start + i, 1,decoder.UNIXtoUTC(Convert.ToInt32(pdfVariables.Time[i])));
                WritetoList(start + i, 2,channelOne.Data[i].ToString());

                if (pdfVariables.IsChannelTwoEnabled == true)
                    WritetoList(start + i, 3,channelTwo.Data[i].ToString());
            }

        }

        void WritetoList(int row, int coloum, string value)
        {
            if (row <= csvStringList.Count)
            {
                if (coloum <= csvStringList[row - 1].Count)
                {
                    csvStringList[row - 1][coloum - 1] = value +",";
                }

                else
                {
                    while (coloum > csvStringList[row - 1].Count)
                        csvStringList[row - 1].Add(",");

                    csvStringList[row - 1][coloum - 1] = value + ",";
                }
            }
            else
            {
                while (row > csvStringList.Count)
                {
                    var temp = new List<string>();
                    csvStringList.Add(temp);
                }

                while (coloum > csvStringList[row - 1].Count)
                    csvStringList[row - 1].Add(",");

                csvStringList[row - 1][coloum - 1] = value + ",";
            }
        }

        async void SaveCSV(LoggerInformation loggerInformation)
        {
            var csvPath = Path.GetTempPath() + "\\" + loggerInformation.SerialNumber + ".csv";
            try
            {
                using (var fs = new FileStream(csvPath, FileMode.Create, FileAccess.Write))
                {
                    for (int i = 0; i < csvStringList.Count; i++)
                    {
                        for (int j = 0; j < csvStringList[i].Count; j++)
                        {
                            var csvContentByte = Encoding.GetEncoding("iso-8859-1").GetBytes(csvStringList[i][j].ToString());
                            fs.Write(csvContentByte, 0, csvContentByte.Length);
                        }
                        var returnArray = new byte[] { 0x0d };
                        fs.Write(returnArray, 0, 1);
                    }
                    fs.Close();
                }

                var csvFile = await StorageFile.GetFileFromPathAsync(csvPath);
            }
            catch { }
        }
    }
}

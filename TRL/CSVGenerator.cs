using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TRL.Constant;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

namespace TRL
{
    public class CSVGenerator
    {
        int row = 5;
        List<List<string>> csvStringList;

        public async void CreateCSV(LoggerInformation loggerInformation)
        {
            var decoder = new HexFileDecoder(loggerInformation);
            await decoder.ReadIntoJsonFileAndSetupDecoder();
            var loggerVariables = await decoder.AssignLoggerValue();

            var csvPath = Path.GetTempPath() + "\\" + loggerInformation.SerialNumber + ".csv";

            try
            {
                using (var sw = new StreamWriter(csvPath))
                {
                    sw.Write("hello");
                    sw.Write(",");
                    sw.Write(",");
                    sw.Write(",");
                    sw.Write("hello");
                    sw.Close();
                }

                var csvFile = await StorageFile.GetFileFromPathAsync(csvPath);
            }
            catch (Exception e) { }
        }

        private void CreateLayout(LoggerInformation loggerInformation, LoggerVariables pdfVariables)
        {
            var decoder = new HexFileDecoder(loggerInformation);
            var channelTwoEnabled = pdfVariables.IsChannelTwoEnabled;
            var channelOne = pdfVariables.ChannelOne;
            var channelTwo = pdfVariables.ChannelTwo;

            workSheet.Range[1, 5].Value = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:sss UTC");
            workSheet.Range[2, 1].Value = LabelConstant.Title;
            workSheet.Range[2, 5].Value = LabelConstant.SerialNumber + pdfVariables.SerialNumber;

            FillRange(LabelConstant.Model, loggerInformation.LoggerName);
            FillRange(LabelConstant.LoggerState, pdfVariables.LoggerState);
            FillRange(LabelConstant.Battery, pdfVariables.BatteryPercentage);
            FillRange(LabelConstant.SamplePeriod, pdfVariables.SameplePeriod);
            FillRange(LabelConstant.StartDelay, pdfVariables.StartDelay);
            FillRange(LabelConstant.FirstSample, pdfVariables.FirstSample);
            FillRange(LabelConstant.LastSample, pdfVariables.LastSample);
            FillRange(LabelConstant.RecordedSample, pdfVariables.RecordedSamples.ToString());
            FillRange(LabelConstant.TagsPlaced, pdfVariables.TagsPlaced.ToString());
            row++;

            workSheet.Range[row, 2].Value = LabelConstant.StatChannelOneLabel;
            if (channelTwoEnabled)
                workSheet.Range[row, 3].Value = LabelConstant.StatChannelTwoLabel;
            row++;

            FillChannelStatRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.PresentUpperLimit, c => c.PresetUpperLimit.ToString("N2"));
            FillChannelStatRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.PresentLowerLimit, c => c.PresetLowerLimit.ToString("N2"));
            FillChannelStatRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.Mean, c => c.Mean.ToString("N2"));
            FillChannelStatRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.MKT, c => c.MKT_C.ToString("N2"));
            FillChannelStatRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.Max, c => c.Max.ToString("N2"));
            FillChannelStatRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.Min, c => c.Min.ToString("N2"));
            FillChannelRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.SampleWithinLimits, c => c.WithinLimits.ToString("N1"));
            FillChannelRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.TimeWithinLimits, c => c.TimeWithinLimits);
            FillChannelRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.SampleOutofLimits, c => c.OutsideLimits.ToString("N1"));
            FillChannelRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.TimeOutOfLimits, c => c.TimeOutLimits);
            FillChannelRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.SampleAboveLimit, c => c.AboveLimits.ToString("N1"));
            FillChannelRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.TimeAboveLimit, c => c.TimeAboveLimits);
            FillChannelRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.SampleBelowLimit, c => c.BelowLimits.ToString("N1"));
            FillChannelRange(channelOne, channelTwo, channelTwoEnabled, LabelConstant.TimeBelowLimit, c => c.TimeBelowLimits);

            FillRange(LabelConstant.UserComment, string.Empty);

            if (pdfVariables.UserData.Length > 120)
            {
                var firstLine = pdfVariables.UserData.Substring(0, pdfVariables.UserData.Length / 2);
                var secondLine = pdfVariables.UserData.Substring(pdfVariables.UserData.Length / 2);

                FillRange(firstLine, string.Empty);
                FillRange(secondLine, string.Empty);
            }

            row = 50;
            if (channelTwoEnabled)
                workSheet.Range[row, 3].Value = LabelConstant.GraphChannelTwoLabel;
            FillRange(workSheet, LabelConstant.DateTime, LabelConstant.GraphChannelOneLabel);
            row++;

            var startRange = row;
            var endRange = (row + pdfVariables.RecordedSamples);
            FillValueRange(workSheet, decoder, pdfVariables, channelOne, channelTwo, startRange, endRange);
            workSheet.UsedRange.AutofitColumns();
        }

        void FillRange(string label, string value)
        {
            worksheet.Range[row, 1].Value = label;
            worksheet.Range[row, 2].Value = value;
            row++;
        }

        void FillChannelRange(ChannelConfig channelOne, ChannelConfig channelTwo, bool channelTwoEnabled, string label, Func<ChannelConfig, string> getString)
        {
            worksheet.Range[row, 1].Value = label;
            worksheet.Range[row, 2].Value = getString(channelOne);
            if (channelTwoEnabled)
            {
                writeToList(row, 3,getString(channelTwo));
            }
            row++;
        }

        void FillChannelStatRange(IWorksheet worksheet, ChannelConfig channelOne, ChannelConfig channelTwo, bool channelTwoEnabled, string label, Func<ChannelConfig, string> getString)
        {
            worksheet.Range[row, 1].Value = label;
            worksheet.Range[row, 2].Value = getString(channelOne) + channelOne.Unit;
            if (channelTwoEnabled && label!=LabelConstant.MKT)
            {
                worksheet.Range[row, 3].Value = getString(channelTwo) + channelTwo.Unit;
            }
            row++;
        }

        void FillValueRange(IWorksheet worksheet, HexFileDecoder decoder, LoggerVariables pdfVariables, ChannelConfig channelOne, ChannelConfig channelTwo, int start, int end)
        {
            for (int i = 0; i < pdfVariables.RecordedSamples; i++)
            {
                worksheet.Range[start+i,1].Value = decoder.UNIXtoUTC(Convert.ToInt32(pdfVariables.Time[i]));
                worksheet.Range[start +i,2].Value = channelOne.Data[i].ToString();

                if (pdfVariables.IsChannelTwoEnabled == true)
                    worksheet.Range[start + i, 3].Value = channelTwo.Data[i].ToString();
            }
            
        }

        void writeToList(int row, int coloum, string value)
        {
            if(row <= csvStringList.Count)
            {

            }
        }
    }
}

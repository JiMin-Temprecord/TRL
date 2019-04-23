using Syncfusion.XlsIO;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI;
using TRL.Constant;
using System.IO;
using Windows.System;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace TRL
{
    public class ExcelGenerator
    {
        int row = 5;

        public async void CreateExcelAsync(LoggerInformation loggerInformation)
        {
            var decoder = new HexFileDecoder(loggerInformation);
            decoder.ReadIntoJsonFileAndSetupDecoder();
            var loggerVariables = decoder.AssignLoggerValue();

            var excelPath = Path.GetTempPath() + "\\" + loggerInformation.SerialNumber + ".xlsx";

            using (var excel = new ExcelEngine())
            {
                var workbook = excel.Excel.Workbooks.Create(1);
                var workSheet = workbook.Worksheets[0];
                CreateLayout(workSheet, loggerInformation, loggerVariables);

                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.SuggestedFileName = loggerInformation.SerialNumber;
                savePicker.FileTypeChoices.Add("Excel Files", new List<string>() { ".xlsx" });
                StorageFile storageFile = await savePicker.PickSaveFileAsync();

                workbook.SaveAsAsync(storageFile);
                Launcher.LaunchFileAsync(storageFile);
                //Console.WriteLine("EXCEL Created !");
            }
        }

        private void CreateLayout(IWorksheet workSheet, LoggerInformation loggerInformation, LoggerVariables pdfVariables)
        {
            var decoder = new HexFileDecoder(loggerInformation);
            var channelTwoEnabled = pdfVariables.IsChannelTwoEnabled;
            var channelOne = pdfVariables.ChannelOne;
            var channelTwo = pdfVariables.ChannelTwo;
            var path = Windows.ApplicationModel.Package.Current.InstalledLocation + "\\Images\\";
            var assembly = typeof(App).GetTypeInfo().Assembly;
            
            if (channelOne.OutsideLimits == 0 && channelTwo.OutsideLimits == 0)
            {
                var tickImage = assembly.GetManifestResourceStream(LabelConstant.WithinLimitImage);
                var setPosition = workSheet.Pictures.AddPicture(5,5, tickImage);
                setPosition.Height = 145;
                setPosition.Width = 128;
                setPosition.Top = 80;
                setPosition.Left = 275;
                workSheet.Range[12, 5].Value = LabelConstant.WithinLimit;
                workSheet.Range[12, 5].CellStyle.Font.Bold = true;
                workSheet.Range[12, 5].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;

            }
            else
            {
                var warningImage = assembly.GetManifestResourceStream(LabelConstant.LimitsExceededImage);
                var setPosition = workSheet.Pictures.AddPicture(5, 5, warningImage);
                setPosition.Height = 145;
                setPosition.Width = 128;
                setPosition.Top = 80;
                setPosition.Left = 275;
                workSheet.Range[12, 5].Value = LabelConstant.LimitsExceeded;
                workSheet.Range[12, 5].CellStyle.Font.Bold = true;
                workSheet.Range[12, 5].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }
            
            var logoImage = assembly.GetManifestResourceStream(LabelConstant.LogoIcon);
            var setLogoPosition = workSheet.Pictures.AddPicture(1,3, logoImage);
            setLogoPosition.Height = 103;
            setLogoPosition.Width = 63;
            setLogoPosition.Top = 10;
            setLogoPosition.Left = 130;
            workSheet.Range[15,2,15,3].CellStyle.Font.Bold = true;
            workSheet.Range[50,2,50,3].CellStyle.Font.Bold = true;
            workSheet.Range[2,1,2,5].CellStyle.Font.Size = 20;
            workSheet.Range[2,1,2,5].CellStyle.Font.Color = ExcelKnownColors.Blue;
            workSheet.Range[2,1 ,5,2].CellStyle.Font.Bold = true;
            workSheet.Range[4,1,4,5].CellStyle.Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Double;
            workSheet.Range[4,1,4,5].CellStyle.Borders[ExcelBordersIndex.EdgeTop].Color = ExcelKnownColors.Blue;

            workSheet.Range[1, 5].Value = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:sss UTC");
            workSheet.Range[2, 1].Value = LabelConstant.Title;
            workSheet.Range[2, 5].Value = LabelConstant.SerialNumber + pdfVariables.SerialNumber;

            FillRange(workSheet, LabelConstant.Model, loggerInformation.LoggerName);
            FillRange(workSheet, LabelConstant.LoggerState, pdfVariables.LoggerState);
            FillRange(workSheet, LabelConstant.Battery, pdfVariables.BatteryPercentage);
            FillRange(workSheet, LabelConstant.SamplePeriod, pdfVariables.SameplePeriod);
            FillRange(workSheet, LabelConstant.StartDelay, pdfVariables.StartDelay);
            FillRange(workSheet, LabelConstant.FirstSample, pdfVariables.FirstSample);
            FillRange(workSheet, LabelConstant.LastSample, pdfVariables.LastSample);
            FillRange(workSheet, LabelConstant.RecordedSample, pdfVariables.RecordedSamples.ToString());
            FillRange(workSheet, LabelConstant.TagsPlaced, pdfVariables.TagsPlaced.ToString());
            row++;

            workSheet.Range[row, 2].Value = LabelConstant.StatChannelOneLabel;
            if (channelTwoEnabled)
                workSheet.Range[row, 3].Value = LabelConstant.StatChannelTwoLabel;
            row++;

            FillChannelStatRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.PresentUpperLimit, c => c.PresetUpperLimit.ToString("N2"));
            FillChannelStatRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.PresentLowerLimit, c => c.PresetLowerLimit.ToString("N2"));
            FillChannelStatRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.Mean, c => c.Mean.ToString("N2"));
            FillChannelStatRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.MKT, c => c.MKT_C.ToString("N2"));
            FillChannelStatRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.Max, c => c.Max.ToString("N2"));
            FillChannelStatRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.Min, c => c.Min.ToString("N2"));
            FillChannelRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.SampleWithinLimits, c => c.WithinLimits.ToString("N1"));
            FillChannelRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.TimeWithinLimits, c => c.TimeWithinLimits);
            FillChannelRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.SampleOutofLimits, c => c.OutsideLimits.ToString("N1"));
            FillChannelRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.TimeOutOfLimits, c => c.TimeOutLimits);
            FillChannelRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.SampleAboveLimit, c => c.AboveLimits.ToString("N1"));
            FillChannelRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.TimeAboveLimit, c => c.TimeAboveLimits);
            FillChannelRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.SampleBelowLimit, c => c.BelowLimits.ToString("N1"));
            FillChannelRange(workSheet, channelOne, channelTwo, channelTwoEnabled, LabelConstant.TimeBelowLimit, c => c.TimeBelowLimits);

            FillRange(workSheet, LabelConstant.UserComment, string.Empty);

            if (pdfVariables.UserData.Length > 120)
            {
                var firstLine = pdfVariables.UserData.Substring(0, pdfVariables.UserData.Length / 2);
                var secondLine = pdfVariables.UserData.Substring(pdfVariables.UserData.Length / 2);

                FillRange(workSheet, firstLine, string.Empty);
                FillRange(workSheet, secondLine, string.Empty);
            }

            row = 50;
            if (channelTwoEnabled)
                workSheet.Range[row, 3].Value = LabelConstant.GraphChannelTwoLabel;
            FillRange(workSheet, LabelConstant.DateTime, LabelConstant.GraphChannelOneLabel);
            row++;

            var startRange = row;
            var endRange = (row + pdfVariables.RecordedSamples);
            FillValueRange(workSheet, decoder, pdfVariables, channelOne, channelTwo, startRange, endRange);
            CreateGraph(workSheet, pdfVariables, channelOne, channelTwo, startRange, endRange);
            workSheet.UsedRange.AutofitColumns();
        }

        void FillRange(IWorksheet worksheet, string label, string value)
        {
            worksheet.Range[row, 1].CellStyle.Font.Bold = true;
            worksheet.Range[row, 2].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[row, 1].Value = label;
            worksheet.Range[row, 2].Value = value;
            row++;
        }

        void FillChannelRange(IWorksheet worksheet, ChannelConfig channelOne, ChannelConfig channelTwo, bool channelTwoEnabled, string label, Func<ChannelConfig, string> getString)
        {
            worksheet.Range[row, 1].CellStyle.Font.Bold = true;
            worksheet.Range[row, 1].Value = label;
            worksheet.Range[row, 2].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
            worksheet.Range[row, 2].Value = getString(channelOne);
            if (channelTwoEnabled)
            {
                worksheet.Range[row, 3].Value = getString(channelTwo);
            }
            row++;
        }

        void FillChannelStatRange(IWorksheet worksheet, ChannelConfig channelOne, ChannelConfig channelTwo, bool channelTwoEnabled, string label, Func<ChannelConfig, string> getString)
        {
            worksheet.Range[row, 1].CellStyle.Font.Bold = true;
            worksheet.Range[row, 1].Value = label;
            worksheet.Range[row, 2].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignLeft;
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

        void CreateGraph(IWorksheet worksheet, LoggerVariables pdfVariables, ChannelConfig channelOne, ChannelConfig channelTwo, int start, int end)
        {
            IChart graph = worksheet.Charts.Add();
            graph.ChartType = ExcelChartType.Line;
            graph.XPos = 0;
            graph.YPos = 675;
            graph.Height = 500;
            graph.Width = 500;

            var xSeries = worksheet.Range[start,1,end-1,1];
            var ySeries = worksheet.Range[start,2,end-1,2];

            var seriesOne = graph.Series.Add(LabelConstant.GraphChannelOneLabel + channelOne.Unit);
            seriesOne.Values = ySeries;
            seriesOne.CategoryLabels = xSeries;

            if (pdfVariables.IsChannelTwoEnabled)
            {
                var ySeries2 = worksheet.Range[start, 3, end - 1, 3];
                var seriesTwo = graph.Series.Add(LabelConstant.GraphChannelOneLabel + channelOne.Unit);
                seriesTwo.Values = ySeries;
                seriesTwo.CategoryLabels = xSeries;
            }
        }
    }
}

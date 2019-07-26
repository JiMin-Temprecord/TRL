using PDF;
using PDF.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TRL.Constant;
using Windows.UI;

namespace TRL
{
    public class PDFGenerator
    {
        PDFDocument pdfDocument = new PDFDocument();
        
        public async Task<bool> CreatePDF(LoggerInformation loggerInformation)
        {
            double lineCounter = 80;

            var decoder = new HexFileDecoder(loggerInformation);
            var loggerVariables = await decoder.AssignLoggerValue();
            
            var channelOne = loggerVariables.ChannelOne;
            var channelTwo = loggerVariables.ChannelTwo;
            var channelTwoEnabled = loggerVariables.IsChannelTwoEnabled;

            var pen = new PDFPen(Colors.Black, 1);
            var font = new PDFFont(FontType.Helvetica, 10, FontStyle.Regular);
            var boldFont = new PDFFont(FontType.Helvetica, 10, FontStyle.Bold);

            if (loggerVariables.LoggerState != "Logging" && loggerVariables.LoggerState != "Stopped")
                return false;

            var pdfPage = CreateNewPage(font, loggerInformation.SerialNumber);

            void DrawChannelStatistics(string Label, Func<ChannelConfig, string> getString, double lineConterMultiplier = 1.0)
            {
                pdfPage.DrawString(Label, boldFont, Colors.Black, PDFcoordinates.first_column, (float)lineCounter);
                pdfPage.DrawString(getString(channelOne) + channelOne.Unit, font, Colors.Black, PDFcoordinates.second_column, (float)lineCounter);
                if ((channelTwoEnabled) && Label != LabelConstant.MKT)
                    pdfPage.DrawString(getString(channelTwo) + channelTwo.Unit, font, Colors.Black, PDFcoordinates.third_column, (float)lineCounter);

                lineCounter += PDFcoordinates.line_inc * lineConterMultiplier;
            }

            void DrawChannelLimits(string Label, Func<ChannelConfig, string> getString, double lineConterMultiplier = 1.0)
            {
                pdfPage.DrawString(Label, boldFont, Colors.Black, PDFcoordinates.first_column, (float)lineCounter);
                pdfPage.DrawString(getString(channelOne), font, Colors.Black, PDFcoordinates.second_column, (float)lineCounter);
                if (channelTwoEnabled)
                    pdfPage.DrawString(getString(channelTwo), font, Colors.Black, PDFcoordinates.third_column, (float)lineCounter);

                lineCounter += PDFcoordinates.line_inc * lineConterMultiplier;
            }

            void DrawSection(string firstColoumString, string secondColoumString, double lineConterMultiplier = 1.0)
            {
                pdfPage.DrawString(firstColoumString, boldFont, Colors.Black, PDFcoordinates.first_column, (float)lineCounter);
                pdfPage.DrawString(secondColoumString, font, Colors.Black, PDFcoordinates.second_column, (float)lineCounter);
                lineCounter += PDFcoordinates.line_inc * lineConterMultiplier;
            }

            if ((int)channelOne.OutsideLimits == 0 && (int)channelTwo.OutsideLimits == 0)
            {
                var greenPen = new PDFPen(Colors.LimeGreen,15);
                pdfPage.DrawLine(greenPen, 560, 163, 580, 180);
                pdfPage.DrawLine(greenPen, 580, 190,642,120);
                pdfPage.DrawString(LabelConstant.WithinLimit, font, Colors.Black, PDFcoordinates.limitinfo_startX, PDFcoordinates.limitinfo_startY);
            }
            else
            {
                var redPen = new PDFPen(Colors.OrangeRed, 15);
                pdfPage.DrawLine(redPen, 560, 125, 620, 190);
                pdfPage.DrawLine(redPen, 560, 190, 620, 125);
                pdfPage.DrawString(LabelConstant.LimitsExceeded, font, Colors.Black, PDFcoordinates.limitinfo_startX, PDFcoordinates.limitinfo_startY);
            }

            //Draw the boxes
            pdfPage.DrawRectangle(pen, PDFcoordinates.box1_X1, PDFcoordinates.box1_Y1, PDFcoordinates.box1_X2 - PDFcoordinates.box1_X1, PDFcoordinates.box1_Y2 - PDFcoordinates.box1_Y1);
            pdfPage.DrawRectangle(pen, PDFcoordinates.box2_X1, PDFcoordinates.box2_Y1, PDFcoordinates.box2_X2 - PDFcoordinates.box2_X1, PDFcoordinates.box2_Y2 - PDFcoordinates.box2_Y1);
            pdfPage.DrawRectangle(pen, PDFcoordinates.box3_X1, PDFcoordinates.box3_Y1, PDFcoordinates.box3_X2 - PDFcoordinates.box3_X1, PDFcoordinates.box3_Y2 - PDFcoordinates.box3_Y1);

            //Draw the Text
            DrawSection(LabelConstant.Model, loggerInformation.LoggerName);
            DrawSection(LabelConstant.LoggerState, loggerVariables.LoggerState);
            DrawSection(LabelConstant.Battery, loggerVariables.BatteryPercentage);
            DrawSection(LabelConstant.SamplePeriod, loggerVariables.SameplePeriod + LabelConstant.TimeSuffix);
            DrawSection(LabelConstant.StartDelay, loggerVariables.StartDelay + LabelConstant.TimeSuffix);
            DrawSection(LabelConstant.FirstSample, loggerVariables.FirstSample);
            DrawSection(LabelConstant.LastSample, loggerVariables.LastSample);
            DrawSection(LabelConstant.RecordedSample, loggerVariables.RecordedSamples.ToString());
            DrawSection(LabelConstant.TotalTrips, loggerVariables.TotalTrip.ToString());
            DrawSection(LabelConstant.TagsPlaced, loggerVariables.TagsPlaced.ToString());

            lineCounter -= PDFcoordinates.line_inc * 0.75;
            pdfPage.DrawLine(pen, 10, lineCounter, 680,lineCounter);
            lineCounter += PDFcoordinates.line_inc * 0.75;

            pdfPage.DrawString(LabelConstant.Channel, boldFont, Colors.Black, PDFcoordinates.first_column, lineCounter);
            pdfPage.DrawString(LabelConstant.ChannelOneLabel, boldFont, Colors.Black, PDFcoordinates.second_column, lineCounter);
            if (channelTwoEnabled) pdfPage.DrawString(LabelConstant.ChannelTwoLabel, boldFont, Colors.Black, PDFcoordinates.third_column, (float)lineCounter);
            lineCounter += PDFcoordinates.line_inc;

            if (channelOne.AboveLimits > 0) pdfPage.DrawString(DecodeConstant.Breached, font, Colors.Black, PDFcoordinates.second_column + 50, (float)lineCounter);
            if (channelTwo.AboveLimits > 0) pdfPage.DrawString(DecodeConstant.Breached, font, Colors.Black, PDFcoordinates.third_column + 50, (float)lineCounter);
            DrawChannelStatistics(LabelConstant.PresentUpperLimit, c => c.PresetUpperLimit.ToString("N2"));
            if (channelOne.BelowLimits > 0) pdfPage.DrawString(DecodeConstant.Breached, font, Colors.Black, PDFcoordinates.second_column + 50, (float)lineCounter);
            if (channelTwo.BelowLimits > 0) pdfPage.DrawString(DecodeConstant.Breached, font, Colors.Black, PDFcoordinates.third_column + 50, (float)lineCounter);
            DrawChannelStatistics(LabelConstant.PresentLowerLimit, c => c.PresetLowerLimit.ToString("N2"));
            DrawChannelStatistics(LabelConstant.Mean, c => c.Mean.ToString("N2"));
            DrawChannelStatistics(LabelConstant.MKT, c => c.MKT_C.ToString("N2"));
            DrawChannelStatistics(LabelConstant.Max, c => c.Max.ToString("N2"));
            DrawChannelStatistics(LabelConstant.Min, c => c.Min.ToString("N2"));
            lineCounter += (PDFcoordinates.line_inc * 0.5);
            DrawChannelLimits(LabelConstant.SampleWithinLimits, c => c.WithinLimits.ToString("N1"));
            DrawChannelLimits(LabelConstant.TimeWithinLimits, c => c.TimeWithinLimits);
            lineCounter += (PDFcoordinates.line_inc * 0.5);
            DrawChannelLimits(LabelConstant.SampleOutofLimits, c => c.OutsideLimits.ToString("N1"));
            DrawChannelLimits(LabelConstant.TimeOutOfLimits, c => c.TimeOutLimits);
            lineCounter += (PDFcoordinates.line_inc * 0.5);
            DrawChannelLimits(LabelConstant.SampleAboveLimit, c => c.AboveLimits.ToString("N1"));
            DrawChannelLimits(LabelConstant.TimeAboveLimit, c => c.TimeAboveLimits);
            lineCounter += (PDFcoordinates.line_inc * 0.5);
            DrawChannelLimits(LabelConstant.SampleBelowLimit, c => c.BelowLimits.ToString("N1"));
            DrawChannelLimits(LabelConstant.TimeBelowLimit, c => c.TimeBelowLimits);
            DrawSection(LabelConstant.UserComment, string.Empty);

            if (loggerVariables.UserData.Length > 280)
            {
                var firstLine = loggerVariables.UserData.Substring(0, loggerVariables.UserData.Length / 2);
                var secondLine = loggerVariables.UserData.Substring(loggerVariables.UserData.Length / 2);

                pdfPage.DrawString(firstLine, font, Colors.Black, PDFcoordinates.first_column, lineCounter);
                lineCounter += PDFcoordinates.line_inc;
                pdfPage.DrawString(secondLine, font, Colors.Black, PDFcoordinates.first_column, lineCounter);
                lineCounter += PDFcoordinates.line_inc * 0.5;
            }
            else
            {
                pdfPage.DrawString(loggerVariables.UserData, font, Colors.Black, PDFcoordinates.first_column, lineCounter);
                lineCounter += PDFcoordinates.line_inc;
            }

            pdfPage.DrawLine(pen, 10, lineCounter, 680, lineCounter);
            lineCounter += PDFcoordinates.line_inc * 0.75;

            pdfPage.DrawString(LabelConstant.ChannelOneLabel + channelOne.Unit, font, Colors.DarkOliveGreen, PDFcoordinates.second_column, lineCounter);
            if (channelTwoEnabled) pdfPage.DrawString(LabelConstant.ChannelTwoLabel + channelTwo.Unit, font, Colors.MediumPurple, PDFcoordinates.second_column + 120, lineCounter);
            lineCounter += PDFcoordinates.line_inc;

            //Draw graph
            DrawGraph(decoder, loggerVariables, pdfPage, pen, font);
            FillInValues(decoder, loggerVariables, loggerInformation.SerialNumber);

            string filename = Path.GetTempPath() + loggerInformation.SerialNumber + ".pdf";

            await pdfDocument.Save(filename);
            return true;
        }

        void DrawGraph(HexFileDecoder decoder, LoggerVariables pdfVariables, PDFPage pdfPage, PDFPen pen, PDFFont font)
        {
            var ch0 = new PDFPen(Colors.DarkGreen, 1);
            var ch1 = new PDFPen(Colors.MediumPurple, 1);
            var ch1Limits = new PDFPen(Colors.Lavender, 1, PenStyle.ShortDash);
            var withinlimits = new PDFPen(Colors.ForestGreen, 1, PenStyle.ShortDash);
            var abovelimit = new PDFPen(Colors.Coral, 1, PenStyle.ShortDash);
            var belowlimit = new PDFPen(Colors.CornflowerBlue, 1, PenStyle.ShortDash);
            var abovelimitData = new PDFPen(Colors.Coral,1);
            var belowlimitData = new PDFPen(Colors.CornflowerBlue,1);

            var chUpperLimit = new double[8];
            var chLowerLimit = new double[8];
            var chUpperYLimit = new double[8];
            var chLowerYLimit = new double[8];

            var chMax = new double[8];
            var chMin = new double[8];
            var chYMax = new double[8];
            var chYMin = new double[8];

            float yCH0 = 0;
            float yCH1 = 0;
            float xGraphMaximum = 55;
            float xGraphDate = 0;
            float xGraphScale = 0;
            float yGraphScale = 0;

            int numberofDates = pdfVariables.RecordedSamples / 5;
            int dateGap;

            if (numberofDates <= 0)
            {
                dateGap = 1;
            }
            else
            {
                dateGap = numberofDates;
            }

            chUpperLimit[0] = pdfVariables.ChannelOne.PresetUpperLimit;
            chLowerLimit[0] = pdfVariables.ChannelOne.PresetLowerLimit;
            chMax[0] = pdfVariables.ChannelOne.Max;
            chMin[0] = pdfVariables.ChannelOne.Min;

            var yHighest = pdfVariables.ChannelOne.Max;
            var yLowest = pdfVariables.ChannelOne.Min;

            if (pdfVariables.IsChannelTwoEnabled) //Second Sensor
            {
                chUpperLimit[1] = pdfVariables.ChannelTwo.PresetUpperLimit;
                chLowerLimit[1] = pdfVariables.ChannelTwo.PresetLowerLimit;
                chMax[1] = pdfVariables.ChannelTwo.Max;
                chMin[1] = pdfVariables.ChannelTwo.Min;

                if (pdfVariables.ChannelTwo.Max > yHighest)
                    yHighest = pdfVariables.ChannelTwo.Max;

                if (pdfVariables.ChannelTwo.Min < yLowest)
                    yLowest = pdfVariables.ChannelTwo.Min;
            }

            //draw graph
            pdfPage.DrawLine(pen, PDFcoordinates.G_axis_startX, PDFcoordinates.G_axis_startY, PDFcoordinates.G_axis_meetX, PDFcoordinates.G_axis_meetY);
            pdfPage.DrawLine(pen, PDFcoordinates.G_axis_meetX, PDFcoordinates.G_axis_meetY, PDFcoordinates.G_axis_endX, PDFcoordinates.G_axis_endY);
            yGraphScale = (float)((PDFcoordinates.graph_H - 20) / (yHighest - yLowest));
            xGraphScale = (float)PDFcoordinates.graph_W / pdfVariables.RecordedSamples;

            while (numberofDates < pdfVariables.RecordedSamples)
            {
                xGraphDate = (xGraphScale * numberofDates) + xGraphMaximum;
                pdfPage.DrawString(decoder.UNIXtoUTCDate(Convert.ToInt32(pdfVariables.Time[numberofDates])), font, Colors.Black, xGraphDate - 40, PDFcoordinates.G_axis_meetY + 15);
                pdfPage.DrawString(decoder.UNIXtoUTCTime(Convert.ToInt32(pdfVariables.Time[numberofDates])), font, Colors.Black, xGraphDate - 45, PDFcoordinates.G_axis_meetY + 28);
                numberofDates += dateGap;
            }

            if (pdfVariables.IsChannelTwoEnabled && pdfVariables.RecordedSamples > 0)
            {
                yCH1 = (float)(PDFcoordinates.graph_H - (pdfVariables.ChannelTwo.Data[0] - yLowest) * yGraphScale) + PDFcoordinates.graph_topY;
                chUpperYLimit[1] = (float)(PDFcoordinates.graph_H - ((chUpperLimit[1] - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;
                chLowerYLimit[1] = (float)(PDFcoordinates.graph_H - ((chLowerLimit[1] - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;
                chYMax[1] = (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelTwo.Max - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;
                chYMin[1] = (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelTwo.Min - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;

                pdfPage.DrawLine(ch1Limits, PDFcoordinates.graph_l_lineX_start, chYMax[1], PDFcoordinates.graph_l_lineX_end, chYMax[1]);
                pdfPage.DrawLine(ch1Limits, PDFcoordinates.graph_l_lineX_start, chYMin[1], PDFcoordinates.graph_l_lineX_end, chYMin[1]);
                pdfPage.DrawString(chMin[1].ToString("N2"), font, Colors.Black, PDFcoordinates.first_column, chYMin[1]);
                pdfPage.DrawString(chMax[1].ToString("N2"), font, Colors.Black, PDFcoordinates.first_column, chYMax[1]);

                if ((chUpperLimit[1] < chMax[1]) && (chUpperLimit[1] > chMin[1]))
                {
                    pdfPage.DrawString(pdfVariables.ChannelTwo.Unit + LabelConstant.UpperLimit, font, Colors.Coral, PDFcoordinates.third_column, chUpperYLimit[1] - 5);
                    pdfPage.DrawString(chUpperLimit[1].ToString("N2"), font, Colors.Black, PDFcoordinates.first_column, chUpperYLimit[1]);
                    pdfPage.DrawLine(abovelimit, PDFcoordinates.graph_l_lineX_start, chUpperYLimit[1], PDFcoordinates.graph_l_lineX_end, chUpperYLimit[1]);
                }

                if ((chLowerLimit[1] > chMin[1]) && (chLowerLimit[1] < chMax[1]))
                {
                    pdfPage.DrawString(pdfVariables.ChannelTwo.Unit + LabelConstant.LowerLimit, font, Colors.CornflowerBlue, PDFcoordinates.third_column, chLowerYLimit[1] + 5);
                    pdfPage.DrawString(chLowerLimit[1].ToString("N2"), font, Colors.Black, PDFcoordinates.first_column, chLowerYLimit[1]);
                    pdfPage.DrawLine(belowlimit, PDFcoordinates.graph_l_lineX_start, chLowerYLimit[1], PDFcoordinates.graph_l_lineX_end, chLowerYLimit[1]);
                }
            }


            if (pdfVariables.ChannelOne.Data != null)
            {
                yCH0 = (float)(PDFcoordinates.graph_H - (pdfVariables.ChannelOne.Data[0] - yLowest) * yGraphScale) + PDFcoordinates.graph_topY;
                chUpperYLimit[0] = (float)(PDFcoordinates.graph_H - ((chUpperLimit[0] - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;
                chLowerYLimit[0] = (float)(PDFcoordinates.graph_H - ((chLowerLimit[0] - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;
                chYMax[0] = (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelOne.Max - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;
                chYMin[0] = (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelOne.Min - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;

                pdfPage.DrawLine(withinlimits, PDFcoordinates.graph_l_lineX_start, chYMax[0], PDFcoordinates.graph_l_lineX_end, chYMax[0]);
                pdfPage.DrawLine(withinlimits, PDFcoordinates.graph_l_lineX_start, chYMin[0], PDFcoordinates.graph_l_lineX_end, chYMin[0]);
                pdfPage.DrawString(chMin[0].ToString("N2"), font, Colors.Black, PDFcoordinates.first_column, chYMin[0]);
                pdfPage.DrawString(chMax[0].ToString("N2"), font, Colors.Black, PDFcoordinates.first_column, chYMax[0]);

                if ((chUpperLimit[0] < chMax[0]) && (chUpperLimit[0] > chMin[0]))
                {
                    pdfPage.DrawString(pdfVariables.ChannelOne.Unit + LabelConstant.UpperLimit, font, Colors.Coral, PDFcoordinates.third_column, chUpperYLimit[0] - 5);
                    pdfPage.DrawString(chUpperLimit[0].ToString("N2"), font, Colors.Black, PDFcoordinates.first_column, chUpperYLimit[0]);
                    pdfPage.DrawLine(abovelimit, PDFcoordinates.graph_l_lineX_start, chUpperYLimit[0], PDFcoordinates.graph_l_lineX_end, chUpperYLimit[0]);
                }

                if ((chLowerLimit[0] > chMin[0]) && (chLowerLimit[0] < chMax[0]))
                {
                    pdfPage.DrawString(pdfVariables.ChannelOne.Unit + LabelConstant.LowerLimit, font, Colors.CornflowerBlue, PDFcoordinates.third_column, chLowerYLimit[0] + 5);
                    pdfPage.DrawString(chLowerLimit[0].ToString("N2"), font, Colors.Black, PDFcoordinates.first_column, chLowerYLimit[0]);
                    pdfPage.DrawLine(belowlimit, PDFcoordinates.graph_l_lineX_start, chLowerYLimit[0], PDFcoordinates.graph_l_lineX_end, chLowerYLimit[0]);
                }
            }

            int i = 0;
            while (i < pdfVariables.RecordedSamples && (pdfVariables.ChannelOne.Data != null))
            {
                if (pdfVariables.ChannelOne.Data[i] > pdfVariables.ChannelOne.PresetUpperLimit)
                    pdfPage.DrawLine(abovelimitData, xGraphMaximum, yCH0, xGraphMaximum + xGraphScale, (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelOne.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY);
                else if (pdfVariables.ChannelOne.Data[i] < pdfVariables.ChannelOne.PresetLowerLimit)
                    pdfPage.DrawLine(belowlimitData, xGraphMaximum, yCH0, xGraphMaximum + xGraphScale, (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelOne.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY);
                else
                    pdfPage.DrawLine(ch0, xGraphMaximum, yCH0, xGraphMaximum + xGraphScale, (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelOne.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY);

                yCH0 = (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelOne.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY;

                if (pdfVariables.IsChannelTwoEnabled)
                {
                    if (pdfVariables.ChannelTwo.Data[i] > pdfVariables.ChannelTwo.PresetUpperLimit)
                        pdfPage.DrawLine(abovelimitData, xGraphMaximum, yCH1, xGraphMaximum + xGraphScale, (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelTwo.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY);
                    else if (pdfVariables.ChannelTwo.Data[i] < pdfVariables.ChannelTwo.PresetLowerLimit)
                        pdfPage.DrawLine(belowlimitData, xGraphMaximum, yCH1, xGraphMaximum + xGraphScale, (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelTwo.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY);
                    else
                        pdfPage.DrawLine(ch1, xGraphMaximum, yCH1, xGraphMaximum + xGraphScale, (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelTwo.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY);

                    yCH1 = (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelTwo.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY;
                }

                xGraphMaximum += xGraphScale;
                i++;
            }
        }

        void FillInValues(HexFileDecoder decoder, LoggerVariables pdfVariables, String serialNumber)
        {
            var dateColumn = 20;
            var timeColumn = 30;

            var columnStart = 75;
            var currentColumn = columnStart;
            var maxColumnValue = 650;
            var columnIncrement = 28;

            var rowStart = 65;
            var row = rowStart;
            var rowIncrement = 12;

            var pageFont = new PDFFont(FontType.Helvetica, 10, FontStyle.Regular);
            var font = new PDFFont(FontType.Helvetica, 6, FontStyle.Regular);
            var boldFont = new PDFFont(FontType.Helvetica, 6, FontStyle.Bold);
            var tempPen = new PDFPen(Colors.Black, 1);
            var humPen = new PDFPen(Colors.Gray, 1);

            PDFPen abovelimit = new PDFPen(Colors.Coral,1);
            PDFPen belowlimit = new PDFPen(Colors.CornflowerBlue,1);


            var time = new List<string>();
            var date = new List<string>();

            var valuePage = CreateNewPage(pageFont, serialNumber);

            for (int i = 0; i < pdfVariables.Time.Count; i++)
            {
                time.Add(decoder.UNIXtoUTCTime(Convert.ToInt32(pdfVariables.Time[i])));
                date.Add(decoder.UNIXtoUTCDate(Convert.ToInt32(pdfVariables.Time[i])));
            }

            for (int i = 0; i < pdfVariables.RecordedSamples; i++)
            {

                if ((i == 0) || (date[i - 1] != date[i]))
                {
                    if (pdfVariables.IsChannelTwoEnabled && i != 0)
                        row = row + rowIncrement * 2;
                    else
                        row += rowIncrement;

                    valuePage.DrawString(date[i], boldFont, Colors.Black, dateColumn, row);
                    row += rowIncrement;
                    valuePage.DrawString(time[i], boldFont, Colors.Black, timeColumn, row);
                    currentColumn = columnStart;
                }

                if ((currentColumn > maxColumnValue))
                {
                    currentColumn = columnStart;

                    if (pdfVariables.IsChannelTwoEnabled)
                        row = row + rowIncrement * 2;
                    else
                        row += rowIncrement;

                    if (row > 950)
                    {
                        valuePage = CreateNewPage(pageFont, serialNumber);
                        row = rowStart + rowIncrement;
                        currentColumn = columnStart;
                    }

                    valuePage.DrawString(time[i], boldFont, Colors.Black, timeColumn, row);
                }

                if (pdfVariables.ChannelOne.Data[i] > pdfVariables.ChannelOne.PresetUpperLimit)
                    valuePage.DrawString(pdfVariables.ChannelOne.Data[i].ToString("N2") + pdfVariables.ChannelOne.Unit, font, Colors.Coral, currentColumn, row);
                else if (pdfVariables.ChannelOne.Data[i] < pdfVariables.ChannelOne.PresetLowerLimit)
                    valuePage.DrawString(pdfVariables.ChannelOne.Data[i].ToString("N2") + pdfVariables.ChannelOne.Unit, font, Colors.CornflowerBlue, currentColumn, row);
                else
                    valuePage.DrawString(pdfVariables.ChannelOne.Data[i].ToString("N2") + pdfVariables.ChannelOne.Unit, font, Colors.Black, currentColumn, row);

                if (pdfVariables.IsChannelTwoEnabled)
                {
                    if (pdfVariables.ChannelTwo.Data[i] > pdfVariables.ChannelTwo.PresetUpperLimit)
                        valuePage.DrawString(pdfVariables.ChannelTwo.Data[i].ToString("N2") + pdfVariables.ChannelTwo.Unit, font, Colors.Coral, currentColumn, row + rowIncrement);
                    else if (pdfVariables.ChannelTwo.Data[i] < pdfVariables.ChannelTwo.PresetLowerLimit)
                        valuePage.DrawString(pdfVariables.ChannelTwo.Data[i].ToString("N2") + pdfVariables.ChannelTwo.Unit, font, Colors.CornflowerBlue, currentColumn, row + rowIncrement);
                    else
                        valuePage.DrawString(pdfVariables.ChannelTwo.Data[i].ToString("N2") + pdfVariables.ChannelTwo.Unit, font, Colors.Black, currentColumn, row + rowIncrement);
                }

                currentColumn += columnIncrement;
            }
        }

        int pageNumber = 0;
        PDFPage CreateNewPage(PDFFont font, string serialNumber)
        {
            var headerPen = new PDFPen(Colors.DarkSlateBlue, 120);
            var serialfont = new PDFFont(FontType.Helvetica, 16, FontStyle.Regular);
            var companyNameFont = new PDFFont(FontType.Helvetica, 30, FontStyle.Bold);

            pageNumber++;

            PDFPage page = pdfDocument.AddPage();
            page.Height = 1000;
            page.Width = 700;
            
            page.DrawLine(headerPen, 0, 0, 700, 0);
            page.DrawString(LabelConstant.Title, serialfont, Colors.White, 10, 50);
            page.DrawString(LabelConstant.SerialNumber + serialNumber, serialfont, Colors.White, 550, 50);
            page.DrawString(LabelConstant.CompanyName, companyNameFont, Colors.White, 230, 40);
            page.DrawString(LabelConstant.TradeMark, font, Colors.White, 450, 20);
            page.DrawString(LabelConstant.Page + pageNumber, font, Colors.Black, 600, 980);
            page.DrawString(LabelConstant.Website, font, Colors.Black, PDFcoordinates.siteX, PDFcoordinates.siteY);
            page.DrawString(DateTime.UtcNow.ToString("dd/MM/yyy HH:mm:sss UTC"), font, Colors.White, PDFcoordinates.dateX, PDFcoordinates.dateY);
            page.DrawString("0.1.9.1", font, Colors.Black, PDFcoordinates.versionX, PDFcoordinates.versionY);

            return page;
        }
    }
}

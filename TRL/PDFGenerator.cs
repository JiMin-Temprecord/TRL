using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TRL.Constant;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace TRL
{
    public class PDFGenerator
    {
        PdfDocument pdfDocument = new PdfDocument();
        
        string path = Windows.ApplicationModel.Package.Current.InstalledLocation + "\\Images\\";
        int pageNumber = 0;
        
        public async void CreatePDF(LoggerInformation loggerInformation)
        {
            var decoder = new HexFileDecoder(loggerInformation);
            await decoder.ReadIntoJsonFileAndSetupDecoder();
            var loggerVariables = await decoder.AssignLoggerValue();

            double lineCounter = 80;
            var channelTwoEnabled = loggerVariables.IsChannelTwoEnabled;
            var channelOne = loggerVariables.ChannelOne;
            var channelTwo = loggerVariables.ChannelTwo;

            var pen = new PdfPen(PdfBrushes.Black, 1);
            var font = new PdfStandardFont(PdfFontFamily.Courier, 11, PdfFontStyle.Regular);
            var boldFont = new PdfStandardFont(PdfFontFamily.Courier, 11, PdfFontStyle.Bold);

            //if (loggerVariables.LoggerState == "Ready" || loggerVariables.LoggerState == "Delay")
                //return false;
            
            var newPage = CreateNewPage(font, loggerInformation.SerialNumber);
            var pdfPage = newPage.Graphics;

            void DrawChannelStatistics(string Label, Func<ChannelConfig, string> getString, double lineConterMultiplier = 1.0)
            {
                pdfPage.DrawString(Label, boldFont, PdfBrushes.Black, PDFcoordinates.first_column, (float)lineCounter);
                pdfPage.DrawString(getString(channelOne) + channelOne.Unit , font, PdfBrushes.Black,PDFcoordinates.second_column, (float)lineCounter);
                if ((channelTwoEnabled) && Label != LabelConstant.MKT)
                    pdfPage.DrawString(getString(channelTwo) + channelTwo.Unit, font, PdfBrushes.Black, PDFcoordinates.third_column, (float)lineCounter);

                lineCounter += PDFcoordinates.line_inc * lineConterMultiplier;
            }

            void DrawChannelLimits(string Label, Func<ChannelConfig, string> getString, double lineConterMultiplier = 1.0)
            {
                pdfPage.DrawString(Label, boldFont, PdfBrushes.Black,PDFcoordinates.first_column, (float)lineCounter);
                pdfPage.DrawString(getString(channelOne), font, PdfBrushes.Black, PDFcoordinates.second_column, (float)lineCounter);
                if (channelTwoEnabled)
                    pdfPage.DrawString(getString(channelTwo), font, PdfBrushes.Black,PDFcoordinates.third_column, (float)lineCounter);

                lineCounter += PDFcoordinates.line_inc * lineConterMultiplier;
            }

            void DrawSection(string firstColoumString, string secondColoumString, double lineConterMultiplier = 1.0)
            {
                pdfPage.DrawString(firstColoumString, boldFont, PdfBrushes.Black, PDFcoordinates.first_column, (float)lineCounter);
                pdfPage.DrawString(secondColoumString, font, PdfBrushes.Black, PDFcoordinates.second_column, (float)lineCounter);
                lineCounter += PDFcoordinates.line_inc * lineConterMultiplier;
            }

            /*if ((int)channelOne.OutsideLimits == 0 && (int)channelTwo.OutsideLimits == 0)
            {
                var imageStream = typeof(MainPage).GetTypeInfo().Assembly.GetManifestResourceStream(LabelConstant.WithinLimitImage);
                var greentick = new PdfBitmap(imageStream);
                pdfPage.DrawImage(greentick, PDFcoordinates.sign_left, PDFcoordinates.sign_top, 90, 80);
                pdfPage.DrawString(LabelConstant.WithinLimit, font, PdfBrushes.Black, PDFcoordinates.limitinfo_startX, PDFcoordinates.limitinfo_startY);
            }
            else
            {
                var imageStream = typeof(MainPage).GetTypeInfo().Assembly.GetManifestResourceStream(LabelConstant.LimitsExceededImage);
                var redwarning = new PdfBitmap(imageStream);
                pdfPage.DrawImage(redwarning, PDFcoordinates.sign_left, PDFcoordinates.sign_top, 90, 80);
                pdfPage.DrawString(LabelConstant.LimitsExceeded, font, PdfBrushes.Black, PDFcoordinates.limitinfo_startX, PDFcoordinates.limitinfo_startY);
            }*/
            
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
            //PointF[] break1 = { new PointF(PDFcoordinates.partitionXstart, lineCounter), new PointF(PDFcoordinates.partitionXstart, lineCounter + 1), new PointF(PDFcoordinates.partitionXend, lineCounter), new PointF(PDFcoordinates.partitionXend, lineCounter + 1) }
            //pdfPage.DrawPolygon(pen, PdfBrushes.Black, break1);
            lineCounter += PDFcoordinates.line_inc * 0.75;

            pdfPage.DrawString(LabelConstant.StatChannelOneLabel, font, PdfBrushes.Black, PDFcoordinates.second_column - 25, (float)lineCounter);
            if (channelTwoEnabled) pdfPage.DrawString(LabelConstant.StatChannelTwoLabel, font, PdfBrushes.Black, PDFcoordinates.third_column - 25, (float)lineCounter);
            lineCounter += PDFcoordinates.line_inc;

            if (channelOne.AboveLimits > 0) pdfPage.DrawString(DecodeConstant.Breached, font, PdfBrushes.Black, PDFcoordinates.second_column + 50, (float)lineCounter);
            if (channelTwo.AboveLimits > 0) pdfPage.DrawString(DecodeConstant.Breached, font, PdfBrushes.Black, PDFcoordinates.third_column + 50, (float)lineCounter);
            DrawChannelStatistics(LabelConstant.PresentUpperLimit, c => c.PresetUpperLimit.ToString("N2")); 
            if (channelOne.BelowLimits > 0) pdfPage.DrawString(DecodeConstant.Breached, font, PdfBrushes.Black, PDFcoordinates.second_column + 50, (float)lineCounter);
            if (channelTwo.BelowLimits > 0) pdfPage.DrawString(DecodeConstant.Breached, font, PdfBrushes.Black, PDFcoordinates.third_column + 50, (float)lineCounter);
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

            if (loggerVariables.UserData.Length > 120)
            {
                var firstLine = loggerVariables.UserData.Substring(0, loggerVariables.UserData.Length/2);
                var secondLine = loggerVariables.UserData.Substring(loggerVariables.UserData.Length / 2);

                pdfPage.DrawString(firstLine, font, PdfBrushes.Black, PDFcoordinates.first_column, (float)lineCounter);
                lineCounter += PDFcoordinates.line_inc;
                pdfPage.DrawString(secondLine, font, PdfBrushes.Black, PDFcoordinates.first_column, (float)lineCounter);
                lineCounter += PDFcoordinates.line_inc*0.5;
            }
            else
            {
                pdfPage.DrawString(loggerVariables.UserData, font, PdfBrushes.Black,PDFcoordinates.first_column, (float)lineCounter);
                lineCounter += PDFcoordinates.line_inc;
            }

            //PointF[] break2 = { new PointF(PDFcoordinates.partitionXstart, lineCounter), new PointF(PDFcoordinates.partitionXstart, lineCounter+1), new PointF(PDFcoordinates.partitionXend, lineCounter), new PointF(PDFcoordinates.partitionXend, lineCounter+1) }
            //pdfPage.DrawPolygon(pen, PdfBrushes.Blue , break2);
            lineCounter += PDFcoordinates.line_inc * 0.75;

            pdfPage.DrawString(LabelConstant.GraphChannelOneLabel+ channelOne.Unit, font, PdfBrushes.DarkOliveGreen,PDFcoordinates.second_column, (float)lineCounter);
            if (channelTwoEnabled) pdfPage.DrawString(LabelConstant.GraphChannelTwoLabel + channelTwo.Unit, font, PdfBrushes.MediumPurple, PDFcoordinates.second_column + 120, (float)lineCounter);
            lineCounter += PDFcoordinates.line_inc;

            //Draw graph
            DrawGraph(decoder, loggerVariables, pdfPage, pen, font);
            FillInValues(decoder, loggerVariables, loggerInformation.SerialNumber);
            
            string filename = Path.GetTempPath() + "\\" + loggerInformation.SerialNumber + ".pdf";

            var stream = new MemoryStream(); 
            pdfDocument.Save(stream);
            pdfDocument.Close(true);

            Save(loggerInformation.SerialNumber,stream, filename);
            //return true;
        }
        
        void DrawGraph(HexFileDecoder decoder, LoggerVariables pdfVariables, PdfGraphics draw, PdfPen pen, PdfFont font)
        {
            var ch0 = new PdfPen(PdfBrushes.DarkGreen);
            var ch1 = new PdfPen(PdfBrushes.MediumPurple);
            var ch1Limits = new PdfPen(PdfBrushes.Lavender);
            var withinlimits = new PdfPen(PdfBrushes.ForestGreen);
            var abovelimit = new PdfPen(PdfBrushes.Coral);
            var belowlimit = new PdfPen(PdfBrushes.CornflowerBlue);

            ch1Limits.DashStyle = PdfDashStyle.Dash;
            withinlimits.DashStyle = PdfDashStyle.Dash;
            abovelimit.DashStyle = PdfDashStyle.Dash;
            belowlimit.DashStyle = PdfDashStyle.Dash;

            var chUpperLimit = new float[8];
            var chLowerLimit = new float[8];
            var chUpperYLimit = new float[8];
            var chLowerYLimit = new float[8];

            var chMax = new float[8];
            var chMin = new float[8];
            var chYMax = new float[8];
            var chYMin = new float[8];
            
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

            chUpperLimit[0] = (float)pdfVariables.ChannelOne.PresetUpperLimit;
            chLowerLimit[0] = (float)pdfVariables.ChannelOne.PresetLowerLimit;
            chMax[0] = (float)pdfVariables.ChannelOne.Max;
            chMin[0] = (float)pdfVariables.ChannelOne.Min;

            var yHighest = pdfVariables.ChannelOne.Max;
            var yLowest = pdfVariables.ChannelOne.Min;

            if (pdfVariables.IsChannelTwoEnabled) //Second Sensor
            {
                chUpperLimit[1] = (float)pdfVariables.ChannelTwo.PresetUpperLimit;
                chLowerLimit[1] = (float)pdfVariables.ChannelTwo.PresetLowerLimit;
                chMax[1] = (float)pdfVariables.ChannelTwo.Max;
                chMin[1] = (float)pdfVariables.ChannelTwo.Min;

                if (pdfVariables.ChannelTwo.Max > yHighest)
                    yHighest = pdfVariables.ChannelTwo.Max;

                if(pdfVariables.ChannelTwo.Min < yLowest)
                    yLowest = pdfVariables.ChannelTwo.Min;
            }
            
            //draw graph
            draw.DrawLine(pen, PDFcoordinates.G_axis_startX, PDFcoordinates.G_axis_startY, PDFcoordinates.G_axis_meetX, PDFcoordinates.G_axis_meetY);
            draw.DrawLine(pen, PDFcoordinates.G_axis_meetX, PDFcoordinates.G_axis_meetY, PDFcoordinates.G_axis_endX, PDFcoordinates.G_axis_endY);
            yGraphScale = (float)((PDFcoordinates.graph_H - 20) / (yHighest - yLowest));
            xGraphScale = (float)PDFcoordinates.graph_W / pdfVariables.RecordedSamples;

            while (numberofDates < pdfVariables.RecordedSamples)
            {
                xGraphDate = (xGraphScale * numberofDates) + xGraphMaximum;
                draw.DrawString(decoder.UNIXtoUTCDate(Convert.ToInt32(pdfVariables.Time[numberofDates])), font, PdfBrushes.Black, xGraphDate - 40, PDFcoordinates.G_axis_meetY + 15);
                draw.DrawString(decoder.UNIXtoUTCTime(Convert.ToInt32(pdfVariables.Time[numberofDates])), font, PdfBrushes.Black, xGraphDate - 45, PDFcoordinates.G_axis_meetY + 28);
                numberofDates += dateGap;
            }

            if (pdfVariables.IsChannelTwoEnabled && pdfVariables.RecordedSamples > 0)
            {
                yCH1 = (float)(PDFcoordinates.graph_H - (pdfVariables.ChannelTwo.Data[0] - yLowest) * yGraphScale) + PDFcoordinates.graph_topY;
                chUpperYLimit[1] = (float)(PDFcoordinates.graph_H - ((chUpperLimit[1] - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;
                chLowerYLimit[1] = (float)(PDFcoordinates.graph_H - ((chLowerLimit[1] - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;
                chYMax[1] = (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelTwo.Max - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;
                chYMin[1] = (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelTwo.Min - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;

                draw.DrawLine(ch1Limits, PDFcoordinates.graph_l_lineX_start, chYMax[1], PDFcoordinates.graph_l_lineX_end, chYMax[1]);
                draw.DrawLine(ch1Limits, PDFcoordinates.graph_l_lineX_start, chYMin[1], PDFcoordinates.graph_l_lineX_end, chYMin[1]);
                draw.DrawString(chMin[1].ToString("N2"), font, PdfBrushes.Black, PDFcoordinates.first_column, chYMin[1]);
                draw.DrawString(chMax[1].ToString("N2"), font, PdfBrushes.Black, PDFcoordinates.first_column, chYMax[1]);

                if (chUpperLimit[1] < chMax[1])
                {
                    draw.DrawString(pdfVariables.ChannelTwo.Unit + LabelConstant.UpperLimit, font, PdfBrushes.Coral, PDFcoordinates.third_column, chUpperYLimit[1] - 5);
                    draw.DrawString(chUpperLimit[1].ToString("N2"), font, PdfBrushes.Black, PDFcoordinates.first_column, chUpperYLimit[1]);
                    draw.DrawLine(abovelimit, PDFcoordinates.graph_l_lineX_start, chUpperYLimit[1], PDFcoordinates.graph_l_lineX_end, chUpperYLimit[1]);
                }

                if (chLowerLimit[1] > chMin[1])
                {
                    draw.DrawString(pdfVariables.ChannelTwo.Unit + LabelConstant.LowerLimit, font, PdfBrushes.CornflowerBlue, PDFcoordinates.third_column, chLowerYLimit[1] + 5); 
                    draw.DrawString(chLowerLimit[1].ToString("N2"), font, PdfBrushes.Black, PDFcoordinates.first_column, chLowerYLimit[1]);
                    draw.DrawLine(belowlimit, PDFcoordinates.graph_l_lineX_start, chLowerYLimit[1], PDFcoordinates.graph_l_lineX_end, chLowerYLimit[1]);
                }
            }


            if (pdfVariables.ChannelOne.Data != null)
            {
                yCH0 = (float)(PDFcoordinates.graph_H - (pdfVariables.ChannelOne.Data[0] -yLowest) * yGraphScale) + PDFcoordinates.graph_topY;
                chUpperYLimit[0] = (float)(PDFcoordinates.graph_H - ((chUpperLimit[0] - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;
                chLowerYLimit[0] = (float)(PDFcoordinates.graph_H - ((chLowerLimit[0] - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;
                chYMax[0] = (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelOne.Max - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;
                chYMin[0] = (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelOne.Min - yLowest) * yGraphScale)) + PDFcoordinates.graph_topY;

                draw.DrawLine(withinlimits, PDFcoordinates.graph_l_lineX_start, chYMax[0], PDFcoordinates.graph_l_lineX_end, chYMax[0]);
                draw.DrawLine(withinlimits, PDFcoordinates.graph_l_lineX_start, chYMin[0], PDFcoordinates.graph_l_lineX_end, chYMin[0]);
                draw.DrawString(chMin[0].ToString("N2"), font, PdfBrushes.Black, PDFcoordinates.first_column, chYMin[0]);
                draw.DrawString(chMax[0].ToString("N2"), font, PdfBrushes.Black, PDFcoordinates.first_column, chYMax[0]);

                if (chUpperLimit[0] < chMax[0])
                {
                    draw.DrawString(pdfVariables.ChannelOne.Unit + LabelConstant.UpperLimit, font, PdfBrushes.Coral, PDFcoordinates.third_column, chUpperYLimit[0] - 5);
                    draw.DrawString(chUpperLimit[0].ToString("N2"), font, PdfBrushes.Black, PDFcoordinates.first_column, chUpperYLimit[0]);
                    draw.DrawLine(abovelimit, PDFcoordinates.graph_l_lineX_start, chUpperYLimit[0], PDFcoordinates.graph_l_lineX_end, chUpperYLimit[0]);
                }

                if (chLowerLimit[0] > chMin[0])
                {
                    draw.DrawString(pdfVariables.ChannelOne.Unit + LabelConstant.LowerLimit, font, PdfBrushes.CornflowerBlue, PDFcoordinates.third_column, chLowerYLimit[0] + 5);
                    draw.DrawString(chLowerLimit[0].ToString("N2"), font, PdfBrushes.Black, PDFcoordinates.first_column, chLowerYLimit[0]);
                    draw.DrawLine(belowlimit, PDFcoordinates.graph_l_lineX_start, chLowerYLimit[0], PDFcoordinates.graph_l_lineX_end, chLowerYLimit[0]);
                }
            }

            int i = 0;
            while (i < pdfVariables.RecordedSamples && (pdfVariables.ChannelOne.Data != null))
            {
                if(pdfVariables.ChannelOne.Data[i] > pdfVariables.ChannelOne.AboveLimits)
                    draw.DrawLine(abovelimit, xGraphMaximum, yCH0, xGraphMaximum + xGraphScale, (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelOne.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY);
                else if (pdfVariables.ChannelOne.Data[i] < pdfVariables.ChannelOne.BelowLimits)
                    draw.DrawLine(belowlimit, xGraphMaximum, yCH0, xGraphMaximum + xGraphScale, (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelOne.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY);
                else
                    draw.DrawLine(ch0, xGraphMaximum, yCH0, xGraphMaximum + xGraphScale, (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelOne.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY);

                yCH0 = (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelOne.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY;

                if (pdfVariables.IsChannelTwoEnabled)
                {
                    if(pdfVariables.ChannelTwo.Data[i] > pdfVariables.ChannelTwo.AboveLimits)
                        draw.DrawLine(abovelimit, xGraphMaximum, yCH1, xGraphMaximum + xGraphScale, (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelTwo.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY);
                    else if(pdfVariables.ChannelTwo.Data[i] < pdfVariables.ChannelTwo.BelowLimits)
                        draw.DrawLine(belowlimit, xGraphMaximum, yCH1, xGraphMaximum + xGraphScale, (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelTwo.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY);
                    else
                        draw.DrawLine(ch1, xGraphMaximum, yCH1, xGraphMaximum + xGraphScale, (float)(PDFcoordinates.graph_H - ((pdfVariables.ChannelTwo.Data[i] - (yLowest)) * yGraphScale)) + PDFcoordinates.graph_topY);

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

            var columnStart = 85;
            var currentColumn = columnStart;
            var maxColumnValue = 650;
            var columnIncrement = 45;

            var rowStart = 65;
            var row = rowStart;
            var rowIncrement = 16;

            var font = new PdfStandardFont(PdfFontFamily.Courier, 10, PdfFontStyle.Regular);
            var boldFont = new PdfStandardFont(PdfFontFamily.Courier, 10, PdfFontStyle.Bold);
            var tempPen = new PdfPen(PdfBrushes.Black, 1);
            var humPen = new PdfPen(PdfBrushes.Gray, 1);

            PdfPen abovelimit = new PdfPen(PdfBrushes.Coral);
            PdfPen belowlimit = new PdfPen(PdfBrushes.CornflowerBlue);


            var time = new List<string>();
            var date = new List<string>();

            var valuePage = CreateNewPage(font, serialNumber);
            var valueDraw = valuePage.Graphics;
            
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

                    valueDraw.DrawString(date[i], boldFont, PdfBrushes.Black, dateColumn, row);
                    row += rowIncrement;
                    valueDraw.DrawString(time[i], boldFont, PdfBrushes.Black, timeColumn, row);
                    currentColumn = columnStart;
                }

                if ((currentColumn > maxColumnValue))
                {
                    currentColumn = columnStart;

                    if (pdfVariables.IsChannelTwoEnabled)
                        row = row + rowIncrement * 2;
                    else
                        row += rowIncrement;

                    if (row > 970)
                    {
                        valuePage = CreateNewPage(font, serialNumber);
                        valueDraw = valuePage.Graphics;
                        row = rowStart + rowIncrement;
                        currentColumn = columnStart;
                    }

                    valueDraw.DrawString(time[i], boldFont, PdfBrushes.Black, timeColumn, row);
                }

                if(pdfVariables.ChannelOne.Data[i] > pdfVariables.ChannelOne.PresetUpperLimit)
                    valueDraw.DrawString(pdfVariables.ChannelOne.Data[i].ToString("N2")+ pdfVariables.ChannelOne.Unit, font, PdfBrushes.Coral, currentColumn, row);
                else if (pdfVariables.ChannelOne.Data[i] < pdfVariables.ChannelOne.PresetLowerLimit)
                    valueDraw.DrawString(pdfVariables.ChannelOne.Data[i].ToString("N2") + pdfVariables.ChannelOne.Unit, font, PdfBrushes.CornflowerBlue, currentColumn, row);
                else
                    valueDraw.DrawString(pdfVariables.ChannelOne.Data[i].ToString("N2") + pdfVariables.ChannelOne.Unit, font, PdfBrushes.Black, currentColumn, row);

                if (pdfVariables.IsChannelTwoEnabled)
                {
                    if (pdfVariables.ChannelTwo.Data[i] > pdfVariables.ChannelTwo.PresetUpperLimit)
                        valueDraw.DrawString(pdfVariables.ChannelTwo.Data[i].ToString("N2") + pdfVariables.ChannelTwo.Unit, font, PdfBrushes.Coral, currentColumn, row + rowIncrement);
                    else if (pdfVariables.ChannelTwo.Data[i] < pdfVariables.ChannelTwo.PresetLowerLimit)
                        valueDraw.DrawString(pdfVariables.ChannelTwo.Data[i].ToString("N2") + pdfVariables.ChannelTwo.Unit, font, PdfBrushes.CornflowerBlue, currentColumn, row + rowIncrement);
                    else
                        valueDraw.DrawString(pdfVariables.ChannelTwo.Data[i].ToString("N2") + pdfVariables.ChannelTwo.Unit, font, PdfBrushes.Black, currentColumn, row + rowIncrement);
                }

                currentColumn += columnIncrement;
            }
        }

        PdfPage CreateNewPage (PdfFont font, string serialNumber)
        {
            var serialPen = new PdfPen(PdfBrushes.Blue);
            var serialfont = new PdfStandardFont(PdfFontFamily.Courier, 18);

            //var imageStream = typeof(MainPage).GetTypeInfo().Assembly.GetManifestResourceStream("TRL.Images."+LabelConstant.LogoIcon);
            //PdfBitmap logo = new PdfBitmap(imageStream);

            pageNumber++;

            PdfPage page = pdfDocument.Pages.Add();
            //pdfDocument.PageSettings.Size = new SizeF(1000, 700);
            
            var draw = page.Graphics;
            draw.DrawString(LabelConstant.Title, serialfont, PdfBrushes.Blue, 10, 50);
            draw.DrawString(LabelConstant.SerialNumber+ serialNumber, serialfont, PdfBrushes.Blue, 550, 50);
            draw.DrawLine(serialPen, 10, 60, 690, 60);
            draw.DrawString(LabelConstant.Page + pageNumber , font, PdfBrushes.Black, 600, 980);
            draw.DrawString(LabelConstant.Website, font, PdfBrushes.Black, PDFcoordinates.siteX, PDFcoordinates.siteY);
            draw.DrawString(DateTime.UtcNow.ToString("dd/MM/yyy HH:mm:sss UTC"), font, PdfBrushes.Black, PDFcoordinates.dateX, PDFcoordinates.dateY);
            draw.DrawString("0.1.9.1", font, PdfBrushes.Black, PDFcoordinates.versionX, PDFcoordinates.versionY);
            //draw.DrawImage(logo, 320, 10, 65, 40);

            return page;
        }

        #region Helper Methods
        public async void Save(String serialNumber, Stream stream, string filename)
        {
            stream.Position = 0;
            StorageFile stFile;
            if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons")))
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.DefaultFileExtension = ".pdf";
                savePicker.SuggestedFileName = serialNumber;
                savePicker.FileTypeChoices.Add("Adobe PDF Document", new List<string>() { ".pdf" });
                stFile = await savePicker.PickSaveFileAsync();
            }
            else
            {
                StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
                stFile = await local.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            }
            if (stFile != null)
            {
                Windows.Storage.Streams.IRandomAccessStream fileStream = await stFile.OpenAsync(FileAccessMode.ReadWrite);
                Stream st = fileStream.AsStreamForWrite();
                st.Write((stream as MemoryStream).ToArray(), 0, (int)stream.Length);
                st.Flush();
                st.Dispose();
                fileStream.Dispose();
                MessageDialog messageDialog = new MessageDialog("Do you want to view the Document?", "File created.");
                UICommand yesCmd = new UICommand("Yes");
                messageDialog.Commands.Add(yesCmd);
                UICommand noCmd = new UICommand("No");
                messageDialog.Commands.Add(noCmd);
                IUICommand cmd = await messageDialog.ShowAsync();
                if (cmd == yesCmd)
                {
                    // Launch the retrieved file
                    bool success = await Windows.System.Launcher.LaunchFileAsync(stFile);
                }
            }
        }
        #endregion
    }
}

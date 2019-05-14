using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TRL
{
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// An empty page that can be used on its own or navigated to within a Frame.
        /// </summary>
        LoggerInformation loggerInformation = new LoggerInformation();

        BackgroundWorker readerBW;
        BackgroundWorker loggerBW;
        BackgroundWorker progressBarBW;
        BackgroundWorker documentBW;
        BackgroundWorker sendingEmailBW;
        BackgroundWorker previewPanelBW;

        bool errorDectected = false;
        bool loggerHasStarted = true;

        public MainPage()
        {
            this.InitializeComponent();
        }

        async void PDFPreview_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
                
        }

        async void PDFPreview_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
        }

        void ExcelPreview_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var excelGenerator = new ExcelGenerator();
            excelGenerator.CreateExcelAsync(loggerInformation);

            var filePath = Path.GetTempPath() + "\\" + loggerInformation.SerialNumber + ".xlsx";
        }

        void ExcelPreview_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var excelGenerator = new ExcelGenerator();
            excelGenerator.CreateExcelAsync(loggerInformation);

            var filePath = Path.GetTempPath() + "\\" + loggerInformation.SerialNumber + ".xlsx";
        }

        private async void Grid_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var reader = new Reader();
            var communication = new Communication();
            var loggerInformation = new LoggerInformation();
            var stopWatch = new Stopwatch();

            ReaderPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            var usbDevice = reader.FindReader();
            while (usbDevice == null)
                usbDevice = reader.FindReader();
            ReaderPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            await communication.FindLogger(usbDevice);
            
            ReadingLoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            var errorDectected = await communication.GenerateHexFile(usbDevice, loggerInformation);
            ReadingLoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (errorDectected)
            {
                ErrorReadingPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                var pdfGenerator = new PDFGenerator();
                var isData = await pdfGenerator.CreatePDF(loggerInformation);
                if (isData)
                {
                    var filePath = Path.GetTempPath() + "\\" + loggerInformation.SerialNumber + ".pdf";
                }
                else
                    Debug.WriteLine("LOGGER IS IN ERROR//READY STATE");
            }
        }
    }
}

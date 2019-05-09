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
            var reader = new Reader();
            var communication = new Communication();
            var loggerInformation = new LoggerInformation();

            var usbDevice = reader.FindReader();
            while (usbDevice == null)
                usbDevice = reader.FindReader();

            await communication.FindLogger(usbDevice);
            var errorDectected = await communication.GenerateHexFile(usbDevice, loggerInformation);

            if (!errorDectected)
            {
                var pdfGenerator = new PDFGenerator();
                pdfGenerator.CreatePDF(loggerInformation);
                var filePath = Path.GetTempPath() + "\\" + loggerInformation.SerialNumber + ".pdf";
            }
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
    }
}

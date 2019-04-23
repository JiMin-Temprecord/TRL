using System;
using System.ComponentModel;
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

        private void PDFPreview_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var pdfGenerator = new PDFGenerator();
            pdfGenerator.CreatePDF(loggerInformation);

            var filePath = Path.GetTempPath() + "\\" + loggerInformation.SerialNumber + ".pdf";
        }

        private void PDFPreview_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var pdfGenerator = new PDFGenerator();
            pdfGenerator.CreatePDF(loggerInformation);

            var filePath = Path.GetTempPath() + "\\" + loggerInformation.SerialNumber + ".pdf";

        }

        private void ExcelPreview_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var excelGenerator = new ExcelGenerator();
            excelGenerator.CreateExcelAsync(loggerInformation);

            var filePath = Path.GetTempPath() + "\\" + loggerInformation.SerialNumber + ".xlsx";
        }

        private void ExcelPreview_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var excelGenerator = new ExcelGenerator();
            excelGenerator.CreateExcelAsync(loggerInformation);

            var filePath = Path.GetTempPath() + "\\" + loggerInformation.SerialNumber + ".xlsx";
        }
    }
}

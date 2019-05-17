using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Windows.Devices.Usb;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TRL
{
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// An empty page that can be used on its own or navigated to within a Frame.
        /// </summary>

        Communication communication = new Communication();
        LoggerInformation loggerInformation = new LoggerInformation();
        Reader reader = new Reader();
        UsbDevice usbDevice = null;


        bool errorDectected = false;
        bool loggerHasStarted = true;
        
        public MainPage()
        {
            this.InitializeComponent();
        }

        void PDFPreview_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var filePath = Path.GetTempPath() + "\\" + loggerInformation.SerialNumber + ".pdf";
            try
            {
                Process.Start(filePath);
            }
            catch { }
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
        }

        private void ReadLogger_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ReadyStateErrorPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ErrorReadingPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ReadLoggerButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            usbDevice.Dispose();
            ApplicationProcess(0);
        }

        async void ApplicationProcess(int stage)
        {
            switch (stage)
            {
                // Find Reader 
                case 0:
                    ReaderPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    usbDevice = reader.FindReader();
                    while (usbDevice == null)
                        usbDevice = reader.FindReader();
                    ApplicationProcess(1);
                    break;

                // Find Logger
                case 1:
                    ReaderPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    await communication.FindLogger(usbDevice);
                    ApplicationProcess(2);
                    break;

                //Reading Logger
                case 2:
                    ReadingLoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    errorDectected = await communication.GenerateHexFile(usbDevice, loggerInformation);
                    ApplicationProcess(3);
                    break;

                //Generating Documents
                case 3:
                    ReadingLoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    
                    if (errorDectected)
                    {
                        LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        ReadingLoggerProgressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        ErrorReadingPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        ReadLoggerButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                    else
                    {
                        var pdfGenerator = new PDFGenerator();
                        var excelGenerator = new ExcelGenerator();

                        loggerHasStarted = await pdfGenerator.CreatePDF(loggerInformation);
                        if (loggerHasStarted)
                            excelGenerator.CreateExcelAsync(loggerInformation);

                        ApplicationProcess(4);
                    }
                    break;

                //Changing state
                case 4:
                    if(loggerHasStarted)
                    {
                        if (loggerInformation.EmailId == string.Empty)
                        {
                            GeneratingDocumentsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            ReadLoggerButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
                        else if (loggerInformation.EmailId == null)
                        {
                            LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            GeneratingDocumentsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            ErrorReadingPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                            ReadLoggerButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
                        else
                        {
                            GeneratingDocumentsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            SendingEmailPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                            ApplicationProcess(5);
                        }
                        break;
                    }
                    else
                    {
                        GeneratingDocumentsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        ReadyStateErrorPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        ReadLoggerButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                    break;

                //Sending Email
                case 5:
                    SendingEmailPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    var email = new Email();
                    email.SetUpEmail(loggerInformation.SerialNumber, loggerInformation.EmailId);
                    LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    SendingEmailPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    SentEmailPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    ReadLoggerButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    break;
            }
        }

        private void ReaderPanel_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ApplicationProcess(0);
        }

        private async void PDFEmail_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var email = new Email();
            await email.OpenEmailApplication(loggerInformation.SerialNumber, 0);
        }

        private async void ExcelEmail_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var email = new Email();
            await email.OpenEmailApplication(loggerInformation.SerialNumber, 1);
        }
    }
}

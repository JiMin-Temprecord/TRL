using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Windows.Devices.Usb;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TRL
{
    public sealed partial class MainPage : Page
    {
        Communication communication = new Communication();
        LoggerInformation loggerInformation = new LoggerInformation();
        Reader reader;
        UsbDevice usbDevice = null;

        BackgroundWorker readerBW;

        bool errorDectected = false;
        bool loggerHasStarted = true;

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(710, 760);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(710, 760));

        }

        async void PDFPreview_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var pdfPath = Path.GetTempPath() + loggerInformation.SerialNumber + ".pdf";
            var pdfFile = await StorageFile.GetFileFromPathAsync(pdfPath);

            try
            {
                await Launcher.LaunchFileAsync(pdfFile);
            }
            catch { }
        }

        async void PDFPreview_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
        }

        async void ExcelPreview_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var excelPath = Path.GetTempPath() + loggerInformation.SerialNumber + ".csv";
            var excelFile = await StorageFile.GetFileFromPathAsync(excelPath);
            try
            {
                await Launcher.LaunchFileAsync(excelFile);
            }
            catch { }
        }

        void ExcelPreview_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
        }

        private void ReadLogger_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PreviewGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ReadyStateErrorPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ErrorReadingPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            ReadLoggerButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
            ApplicationProcess(0);
        }

        async void ApplicationProcess(int stage)
        {
            switch (stage)
            {
                // Find Reader 
                case 0:
                    SentEmailPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    ReaderPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    readerBW = new BackgroundWorker();
                    readerBW.DoWork += readerBackgroundWorker_DoWork;
                    readerBW.RunWorkerCompleted += readerBackgroundWorker_RunWorkerCompleted;
                    readerBW.WorkerReportsProgress = true;
                    readerBW.WorkerSupportsCancellation = true;
                    readerBW.RunWorkerAsync();
                    break;

                // Find Logger
                case 1:
                    ReaderPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    await communication.FindLogger(usbDevice, reader);
                    ApplicationProcess(2);
                    break;

                //Reading Logger
                case 2:
                    ReadingLoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;

                    errorDectected = await communication.GenerateHexFile(usbDevice, loggerInformation, reader);
                    ApplicationProcess(3);
                    break;

                //Generating Documents
                case 3:
                    ReadingLoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    if (errorDectected)
                    {
                        LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        ErrorReadingPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        ReadLoggerButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                    else if (!reader.UsbExist)
                    {
                        LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        ReaderPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        usbDevice.Dispose();
                        ApplicationProcess(0);
                    }
                    else
                    {
                        GeneratingDocumentsPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        var pdfGenerator = new PDFGenerator();
                        var excelGenerator = new CSVGenerator();

                        loggerHasStarted = await pdfGenerator.CreatePDF(loggerInformation);
                        if (loggerHasStarted)
                            excelGenerator.CreateCSV(loggerInformation);
                        ApplicationProcess(4);
                    }
                    break;

                //Changing state
                case 4:
                    if (loggerHasStarted)
                    {
                        Debug.WriteLine("EMAIL : " + loggerInformation.EmailId);
                        if (loggerInformation.EmailId == string.Empty)
                        {
                            GeneratingDocumentsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            ReadLoggerButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                            PreviewGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
                            LoggerPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            GeneratingDocumentsPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            SendingEmailPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                            ApplicationProcess(5);
                        }
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

        #region Find Reader
        void readerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            usbDevice = reader.GetUSBDevice();
            while (usbDevice == null)
            {
                usbDevice = reader.FindReader();

            }
        }
        void readerBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ApplicationProcess(1);
            readerBW.Dispose();
        }
        #endregion

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

        private void Grid_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            reader = new Reader();
            ApplicationProcess(0);
        }
    }
}

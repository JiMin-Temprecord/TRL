﻿#pragma checksum "C:\Users\JiMin\source\repos\TRL\TRL\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "2585BF8E0D29E827478340990AB56485"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TRL
{
    partial class MainPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1: // MainPage.xaml line 1
                {
                    this.TRL = (global::Windows.UI.Xaml.Controls.Page)(target);
                }
                break;
            case 2: // MainPage.xaml line 11
                {
                    this.bg = (global::Windows.UI.Xaml.Controls.Grid)(target);
                    ((global::Windows.UI.Xaml.Controls.Grid)this.bg).Loaded += this.Grid_Loaded;
                }
                break;
            case 3: // MainPage.xaml line 12
                {
                    this.PreviewGrid = (global::Windows.UI.Xaml.Controls.Grid)(target);
                }
                break;
            case 4: // MainPage.xaml line 38
                {
                    this.ReaderPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 5: // MainPage.xaml line 45
                {
                    this.LoggerPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 6: // MainPage.xaml line 52
                {
                    this.ReadingLoggerPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 7: // MainPage.xaml line 57
                {
                    this.ReadyStateErrorPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 8: // MainPage.xaml line 64
                {
                    this.ErrorReadingPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 9: // MainPage.xaml line 71
                {
                    this.GeneratingDocumentsPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 10: // MainPage.xaml line 75
                {
                    this.SendingEmailPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 11: // MainPage.xaml line 85
                {
                    this.SentEmailPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 12: // MainPage.xaml line 91
                {
                    this.ReadLoggerButton = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ReadLoggerButton).Click += this.ReadLogger_Click;
                }
                break;
            case 13: // MainPage.xaml line 89
                {
                    this.SentEmailTextBox = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 14: // MainPage.xaml line 83
                {
                    this.SendingEmailProgressBar = (global::Windows.UI.Xaml.Controls.ProgressBar)(target);
                }
                break;
            case 15: // MainPage.xaml line 72
                {
                    this.GeneratingDocumentLabel = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 16: // MainPage.xaml line 73
                {
                    this.GeneratingDocumentProgressBar = (global::Windows.UI.Xaml.Controls.ProgressBar)(target);
                }
                break;
            case 17: // MainPage.xaml line 59
                {
                    this.readyStateImage = (global::Windows.UI.Xaml.Controls.Image)(target);
                }
                break;
            case 18: // MainPage.xaml line 60
                {
                    this.readyStateLabel = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 19: // MainPage.xaml line 61
                {
                    this.readyStateText = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 20: // MainPage.xaml line 55
                {
                    this.ReadingLoggerProgressBar = (global::Windows.UI.Xaml.Controls.ProgressBar)(target);
                }
                break;
            case 21: // MainPage.xaml line 40
                {
                    this.ReaderImage = (global::Windows.UI.Xaml.Controls.Image)(target);
                }
                break;
            case 22: // MainPage.xaml line 41
                {
                    this.ReaderLabel = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 23: // MainPage.xaml line 42
                {
                    this.ReaderTextBox = (global::Windows.UI.Xaml.Controls.TextBox)(target);
                }
                break;
            case 24: // MainPage.xaml line 13
                {
                    this.PreviewPDF = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.PreviewPDF).Click += this.PDFPreview_Click;
                }
                break;
            case 25: // MainPage.xaml line 19
                {
                    this.PDFEmail = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.PDFEmail).Click += this.PDFEmail_Click;
                    ((global::Windows.UI.Xaml.Controls.Button)this.PDFEmail).Tapped += this.PDFPreview_Tapped;
                }
                break;
            case 26: // MainPage.xaml line 25
                {
                    this.ExcelPreview = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.ExcelPreview).Click += this.ExcelPreview_Click;
                    ((global::Windows.UI.Xaml.Controls.Button)this.ExcelPreview).Tapped += this.ExcelPreview_Tapped;
                }
                break;
            case 27: // MainPage.xaml line 31
                {
                    this.emailExcel = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)this.emailExcel).Click += this.ExcelEmail_Click;
                    ((global::Windows.UI.Xaml.Controls.Button)this.emailExcel).Tapped += this.ExcelPreview_Tapped;
                }
                break;
            case 28: // MainPage.xaml line 32
                {
                    this.emailExcelPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 29: // MainPage.xaml line 33
                {
                    this.emailExcelImage = (global::Windows.UI.Xaml.Controls.Image)(target);
                }
                break;
            case 30: // MainPage.xaml line 34
                {
                    this.emailExcelLabel = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 31: // MainPage.xaml line 26
                {
                    this.previewExcelPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 32: // MainPage.xaml line 27
                {
                    this.previewExcelImage = (global::Windows.UI.Xaml.Controls.Image)(target);
                }
                break;
            case 33: // MainPage.xaml line 28
                {
                    this.previewExcelLabel = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 34: // MainPage.xaml line 20
                {
                    this.emailPDFPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 35: // MainPage.xaml line 21
                {
                    this.emailPDFImage = (global::Windows.UI.Xaml.Controls.Image)(target);
                }
                break;
            case 36: // MainPage.xaml line 22
                {
                    this.emailPDFLabel = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            case 37: // MainPage.xaml line 14
                {
                    this.PreviewPDFPanel = (global::Windows.UI.Xaml.Controls.StackPanel)(target);
                }
                break;
            case 38: // MainPage.xaml line 15
                {
                    this.previewPDFImage = (global::Windows.UI.Xaml.Controls.Image)(target);
                }
                break;
            case 39: // MainPage.xaml line 16
                {
                    this.previewPDFLabel = (global::Windows.UI.Xaml.Controls.TextBlock)(target);
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.18362.1")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            return returnValue;
        }
    }
}

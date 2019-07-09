using MimeKit;
using System.IO;
using System;
using Windows.Storage;
using System.Threading.Tasks;

namespace TRL
{
    public class Email
    {
        public void SetUpEmail(string serialNumber, string emailID)
        {
            var message = new MimeMessage();
            var builder = new BodyBuilder();
            var emailTo = GetSenderEmail(emailID);
            var emailSubject = "Temprecord Logger " + serialNumber;

            message.From.Add(new MailboxAddress(emailSubject, "temprecordapp@temprecord.com"));
            message.To.Add(new MailboxAddress(emailTo));
            message.Subject = emailSubject;
            
            var PDF = Path.GetTempPath() + serialNumber + ".pdf";
            var EXCEL = Path.GetTempPath() + serialNumber + ".csv";

            if (File.Exists(PDF) && (File.Exists(EXCEL)))
            {
                builder.Attachments.Add(PDF);
                builder.Attachments.Add(EXCEL);
            }
            
            message.Body = builder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Timeout = 10000;
                client.Connect("smtp.siteprotect.com", 587, false);
                client.Authenticate("temprecordapp@temprecord.com", "bEw7a!EtYRv@z6Q");
                client.Send(message);
                client.Disconnect(true);
            }
        }

        public async Task OpenEmailApplication(string serialNumber, int file = 2)
        {
            var emailSubject = "Temprecord Logger " + serialNumber;
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.Subject = emailSubject;

            if (file == 0)
            {
                var pdfPath = Path.GetTempPath() + serialNumber + ".pdf";
                var pdfFile = await StorageFile.GetFileFromPathAsync(pdfPath);
                var pdfStream = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(pdfFile);
                var pdfAttachment = new Windows.ApplicationModel.Email.EmailAttachment(serialNumber + ".pdf", pdfStream);
                emailMessage.Attachments.Add(pdfAttachment);
            }
            else if (file == 1)
            {
                var excelPath = Path.GetTempPath() + serialNumber + ".csv";
                var excelFile = await StorageFile.GetFileFromPathAsync(excelPath);
                var excelStream = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(excelFile);
                var excelAttachment = new Windows.ApplicationModel.Email.EmailAttachment(serialNumber + ".csv", excelStream);
                emailMessage.Attachments.Add(excelAttachment);
            }
            else
            {
                var pdfPath = Path.GetTempPath() + serialNumber + ".pdf";
                var pdfFile = await StorageFile.GetFileFromPathAsync(pdfPath);
                var pdfStream = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(pdfFile);
                var pdfAttachment = new Windows.ApplicationModel.Email.EmailAttachment(serialNumber + ".pdf", pdfStream);

                var excelPath = Path.GetTempPath() + serialNumber + ".csv";
                var excelFile = await StorageFile.GetFileFromPathAsync(excelPath);
                var excelStream = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(excelFile);
                var excelAttachment = new Windows.ApplicationModel.Email.EmailAttachment(serialNumber + ".csv", excelStream);

                emailMessage.Attachments.Add(pdfAttachment);
                emailMessage.Attachments.Add(excelAttachment);
            }

            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        string GetSenderEmail (string emailID)
        {
            switch (emailID)
            {
                case "TBS-NSW":
                    return "NSWDataLoggerCommunication@arcbs.redcross.org.au";
                case "TBS-SA":
                    return "SADataLoggerCommunication@arcbs.redcross.org.au";
                case "TBS-TAS":
                    return "TASDataLoggerCommunication@arcbs.redcross.org.au";
                case "TBS-VIC":
                    return "VICDataLoggerCommunication@arcbs.redcross.org.au";
                case "TBS-QLD":
                    return "QLDDataLoggerCommunication@arcbs.redcross.org.au";
                case "TBS-WA":
                    return "WADataLoggerCommunication@arcbs.redcross.org.au";
                case "TBS-NT":
                    return "NSWDataLoggerCommunication@arcbs.redcross.org.au";
                case "TBS-TEST":
                    return "jimin@temprecord.com";
                default:
                    return "jimin@temprecord.com";
            }
        }
    }
}

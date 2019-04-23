using MimeKit;
using System.IO;

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
            var EXCEL = Path.GetTempPath() + serialNumber + ".xlsx";

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

        public void OpenEmailApplication (string serialNumber, string emailID, int file = 2)
        {
            /*var PDF = Path.GetTempPath() + serialNumber + ".pdf";
            var EXCEL = Path.GetTempPath() + serialNumber + ".xlsx";
            var emailSubject = "Temprecord Logger " + serialNumber;
            
            try
            {
                var outlookApp = new Application();
                var outlookMail = outlookApp.CreateItem(OlItemType.olMailItem);
                outlookMail.Subject = emailSubject;
                
                if(file == 0)
                    outlookMail.Attachments.Add(PDF);
                else if(file == 1)
                    outlookMail.Attachments.Add(EXCEL);
                else
                {
                    outlookMail.Attachments.Add(PDF);
                    outlookMail.Attachments.Add(EXCEL);
                }
                outlookMail.Display(true);
            }
            catch
            {
                MessageBox.Show("Unable to Detect Outlook. Please download Outlook and try again.");
            }*/
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

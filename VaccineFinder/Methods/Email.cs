using MailKit.Net.Smtp;
using MimeKit;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaccineFinder.Private;

namespace VaccineFinder
{
    public class Email
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        internal static string DeveloperEmail = PrivateData.DeveloperEmail;
        internal static string DeveloperName = PrivateData.DeveloperName;
        private static string FromEmail = PrivateData.FromEmail;
        private static string FromName = "Vaccine Finder";
        private static string Password = PrivateData.Pass;

        public static void SendEmail(string message, string subject, string mailIdsTo, string fullNameTo, bool isHTML = false)
        {
            string stInfo = string.Empty;
            try
            {
                logger.Info("SendEmail start.");
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress(FromName, FromEmail));
                foreach (var emailTo in GetEmailIDs(mailIdsTo))
                {
                    mailMessage.To.Add(new MailboxAddress(fullNameTo, emailTo));
                }
                mailMessage.Bcc.Add(new MailboxAddress(DeveloperName, DeveloperEmail));
                mailMessage.Bcc.Add(new MailboxAddress(DeveloperName, FromEmail));

                mailMessage.Subject = subject;
                mailMessage.Body = new TextPart(!isHTML ? "plain" : "html")
                {
                    Text = message
                };

                using (var smtpClient = new SmtpClient())
                {
                    //config settings should be picked from web.config
                    smtpClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    smtpClient.Authenticate(FromEmail, Password);
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }
                stInfo = "Mail Sent Successfully!";
                ConsoleMethods.PrintSuccess(stInfo);
                logger.Info(stInfo);
            }
            catch (Exception ex)
            {
                // Write o the event log.
                stInfo = "Unable to send email.";
                ConsoleMethods.PrintError(stInfo);
                logger.Error(stInfo + "\nException details: " + ex + "\nInner Exception: " + ex.InnerException);
            }
        }

        public static List<string> GetEmailIDs(string emailIds)
        {
            return emailIds.Trim().Replace(" ", "").Split(',').ToList();
        }
    }
}

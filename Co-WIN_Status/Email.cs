using MailKit.Net.Smtp;
using MimeKit;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Co_WIN_Status
{
    public class Email
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static string DeveloperEmail = "abc@gmail.com";
        private static string DeveloperName = "xyz";
        private static string FromEmail = "def@gmail.com";

        public static void SendEmail(string message)
        {
            string stInfo = string.Empty;
            try
            {
                logger.Info("SendEmail start.");
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("Vaccine Finder", FromEmail));
                foreach (var emailTo in GetEmailIDs(AppConfig.EmailIDs))
                {
                    mailMessage.To.Add(new MailboxAddress(AppConfig.FullName, emailTo));
                }
                mailMessage.Bcc.Add(new MailboxAddress(DeveloperName, DeveloperEmail));
                mailMessage.Subject = AppConfig.Mail_Subject;
                mailMessage.Body = new TextPart("plain")
                {
                    Text = message
                };

                using (var smtpClient = new SmtpClient())
                {
                    //config settings should be picked from web.config
                    smtpClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    smtpClient.Authenticate(FromEmail, "password");
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }
                stInfo = "Mail Sent Successfully!";
                Console.WriteLine(stInfo);
                logger.Info(stInfo);
            }
            catch (Exception ex)
            {
                // Write o the event log.
                stInfo = "Unable to send email.";
                Console.WriteLine(stInfo);
                logger.Error(stInfo + "\nException details: " + ex + "\nInner Exception: " + ex.InnerException);
            }
        }
        public static List<string> GetEmailIDs(string emailIds)
        {
            return emailIds.Trim().Replace(" ", "").Split(',').ToList();
        }
    }
}

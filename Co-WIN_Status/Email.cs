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
        public static void SendEmail(string message)
        {
            try
            {
                var mailMessage = new MimeMessage();
                mailMessage.From.Add(new MailboxAddress("abc", "abc@gmail.com"));
                mailMessage.To.Add(new MailboxAddress(AppConfig.FullName, AppConfig.Email));
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
                    smtpClient.Authenticate("username", "password");
                    smtpClient.Send(mailMessage);
                    smtpClient.Disconnect(true);
                }
                logger.Error("Mail Sent Successfully!");
            }
            catch (Exception ex)
            {
                // Write o the event log.
                logger.Error("Unable to send email.\nException details: " + ex + "\nInner Exception: " + ex.InnerException);
            }
        }
    }
}

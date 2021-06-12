using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using VaccineFinder.Private;

namespace VaccineFinder.Models
{
    public class TelegramNotifier : INotifier
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public TelegramNotifier(string channelOrChatID)
        {
            ChannelOrChatID = channelOrChatID;
        }

        private string ChannelOrChatID { get; set; }

        public void Notify(string message)
        {
            string stInfo = string.Empty;
            try
            {
                logger.Info("SendTelegramNotification start.");

                int max_size = 4096;
                var partitions = message.Length / max_size;
                var start = 0;

                List<string> messagesList = new List<string>();
                for (int i = 0; i < partitions; i++)
                {
                    var end = Math.Min(max_size, message.Length - start);
                    messagesList.Add(message.Substring(start, end));
                    start = start + max_size;
                }
                var botClient = new TelegramBotClient(PrivateData.TelegramBotToken);
                foreach (var msg in messagesList) //Push one by one
                {
                    var output = botClient.SendTextMessageAsync(ChannelOrChatID, msg, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2).Result;
                }
                stInfo = "Telegram Notification Sent Successfully!";
                ConsoleMethods.PrintSuccess(stInfo);
                logger.Info(stInfo);
            }
            catch (Exception ex)
            {
                stInfo = $"[ERROR] Issue faced in Telegram API: Unable to Send Notification.";
                ConsoleMethods.PrintError(stInfo);
                logger.Error(stInfo + "\nException details: " + ex + "\nInner Exception: " + ex.InnerException);
            }
        }
    }
}

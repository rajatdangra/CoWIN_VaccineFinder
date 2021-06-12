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
        public TelegramNotifier(string channelOrChatID)
        {
            ChannelOrChatID = channelOrChatID;
        }

        private string ChannelOrChatID { get; set; }

        public void Notify(string message)
        {
            try
            {
                var botClient = new TelegramBotClient(PrivateData.TelegramBotToken);

                var output = botClient.SendTextMessageAsync(ChannelOrChatID, message, Telegram.Bot.Types.Enums.ParseMode.Html).Result;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Issue faced in Telegram API {e}");
                Console.ResetColor();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Configurations.Commands
{
    public class Comamnd
    {
        public static async Task SetBotCommands(ITelegramBotClient botClient)
        {
            var commands = new[]
            {
                 new BotCommand { Command = "/help", Description = "💬 پشتیبانی" },
                 new BotCommand { Command = "/menu", Description = "🏠 صفحه نخست" },
                 new BotCommand { Command = "/aboutus", Description = "ℹ️ " },
             };

            // Set bot commands
            await botClient.SetMyCommandsAsync(commands);
        }
    }   
}

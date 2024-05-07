using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.Configurations.Commands
{
    public class Command
    {
        public static async Task SetBotCommands(ITelegramBotClient botClient)
        {
            var commands = new[]
            {
                new BotCommand { Command = "/menu", Description = "منو 🏠" },
                new BotCommand { Command = "/supporter", Description = "پشتیبانی 💬" },
                new BotCommand { Command = "/intro", Description = "ℹ️" },
             };

            // Set bot commands
            await botClient.SetMyCommandsAsync(commands);
        }
    }
}

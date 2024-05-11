using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.ConstMessages;

namespace TelegramBot.Configurations
{
    public class GlobalConfigs
    {
        private readonly ITelegramBotClient _client;

        public GlobalConfigs(ITelegramBotClient client)
        {
            _client = client;
        }


        //errors
        public async Task SendErrorToUser(long chatId)
        {
            await _client.SendTextMessageAsync(chatId, ConstMessage.Error, parseMode: ParseMode.Html);
        }


        //validations
        public async Task<Message> SendAmountValidationErrorMessageToUser(long chatId)
        {
           return  await _client.SendTextMessageAsync(chatId, ConstMessage.AmountValidationErrorMEssage, parseMode: ParseMode.Html);
        }
    }
}

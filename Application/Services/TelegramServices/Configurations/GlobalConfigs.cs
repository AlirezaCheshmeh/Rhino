using Application.Services.TelegramServices.ConstVariable;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Application.Services.TelegramServices.Configurations
{
    public class GlobalConfigs
    {
        private readonly ITelegramBotClient _client;

        public GlobalConfigs(ITelegramBotClient client)
        {
            _client = client;
        }


        //errors
        public async Task<Message> SendErrorToUser(long chatId)
        {
            return await _client.SendTextMessageAsync(chatId, ConstMessage.Error, parseMode: ParseMode.Html);
        }


        //validations
        public async Task<Message> SendAmountValidationErrorMessageToUser(long chatId)
        {
            return await _client.SendTextMessageAsync(chatId, ConstMessage.AmountValidationErrorMEssage, parseMode: ParseMode.Html);
        }


        //buy Account Error
        public async Task<Message> SendYouDontHaveActiveAccount(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                             {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu),
                                    InlineKeyboardButton.WithCallbackData("💳 خرید اشتراک", ConstCallBackData.Menu.BuyAccount)
                                },
                            });
            return await _client
                .SendTextMessageAsync(chatId, ConstMessage.AccountError, parseMode: ParseMode.Html,
                    replyMarkup: inlineKeyboard);
        }
    }
}

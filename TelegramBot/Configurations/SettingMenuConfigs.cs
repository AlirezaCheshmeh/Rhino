using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.ConstMessages;
using TelegramBot.ConstVariable;

namespace TelegramBot.Configurations
{
    public class SettingMenuConfigs
    {
        private readonly ITelegramBotClient _client;

        public SettingMenuConfigs(ITelegramBotClient client)
        {
            _client = client;
        }

        //setting 
        public async Task SendSettingMenuToUser(long chatId)
        {
            var inlineKeyboards = new InlineKeyboardMarkup(new[]
                        {
                            new[] {InlineKeyboardButton.WithCallbackData("🆕 ایجاد بانک جدید", ConstCallBackData.BankMenu.InsertNewBank)},
                            new[]
                            {
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.BackToMenu, ConstCallBackData.Global.BackToMenu),
                                 InlineKeyboardButton.WithCallbackData(ConstMessage.CancelButton, ConstCallBackData.OutboundTransactionPreview.Cancel),
                            },
                        });
            await _client.SendTextMessageAsync(
            chatId: chatId,
            text: ConstMessage.Settings,
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboards);
        }


    }
}

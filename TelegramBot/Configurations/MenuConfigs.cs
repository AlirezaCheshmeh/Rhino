using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.ConstMessages;
using TelegramBot.ConstVariable;

namespace TelegramBot.Configurations
{
    public class MenuConfigs
    {
        private readonly ITelegramBotClient _client;

        public MenuConfigs(ITelegramBotClient client)
        {
            _client = client;
        }
        public async Task SendMenuToUserAsync(long chatId)
        {
            var inlineKeyboards = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                 InlineKeyboardButton
                                     .WithCallbackData("🆕 ایجاد پرداختی جدید", ConstCallBackData.Menu.OutboundTransaction),
                            },
                            new[]
                            {
                                InlineKeyboardButton
                                    .WithCallbackData("🗞 ایجاد دریافتی جدید", ConstCallBackData.Menu.InboundTransaction),
                            },
                            new[]
                            {
                                InlineKeyboardButton
                                    .WithCallbackData("🔔 یادآوری رویداد یکروز ", ConstCallBackData.Menu.OnceReminder),
                                InlineKeyboardButton
                                    .WithCallbackData("🗓 یادآوری رویداد روزانه", ConstCallBackData.Menu.PeriodicReminder),

                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("⚙ تنظیمات",ConstCallBackData.Menu.Settings),
                                InlineKeyboardButton.WithCallbackData("🧮 ماشین حساب", ConstCallBackData.Menu.Calculator),

                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("⁉️ راهنما", ConstCallBackData.Menu.Guide),
                                InlineKeyboardButton.WithCallbackData("💬 پشتیبانی",ConstCallBackData.Menu.Supporter),

                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("💳 خرید اشتراک", ConstCallBackData.Menu.BuyAccount),
                            }
                        });
            await _client.SendTextMessageAsync(
            chatId: chatId,
            text: ConstMessage.Menu,
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboards);
        }

    }
}

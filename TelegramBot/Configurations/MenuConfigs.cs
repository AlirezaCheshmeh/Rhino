using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

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
                                 InlineKeyboardButton.WithCallbackData("ایجاد پرداختی جدید", "InsertTransactionInbound"),
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("ایجاد دریافتی جدید", "InsertTransactionOutbound"),
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("📅یادآوری رویداد تکی ", "ReminderOnce"),
                                InlineKeyboardButton.WithCallbackData("📅 یادآوری رویداد دوره ای", "ReminderPeriodic"),

                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("تنظیمات", "Settings"),
                                InlineKeyboardButton.WithCallbackData("ماشین حساب", "Calculator"),

                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("راهنما", "Guide"),
                                InlineKeyboardButton.WithCallbackData("پشتیبانی", "Supporter"),

                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("خرید اشتراک", "BuyAccount"),
                            }
                        });
            await _client.SendTextMessageAsync(
            chatId: chatId,
            text: $"فعالیت مورد نظر خود را انتخاب کنید",
            replyMarkup: inlineKeyboards);
        }

    }
}
